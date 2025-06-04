#define ENABLE_LOG

using CadDataTypes;
using System;
using System.Collections.Generic;
using TCad.Controls.CadConsole;
using TCad.MathFunctions;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Settings;
using TCad.Plotter.undo;
using TCad.ViewModel;

namespace TCad.Plotter.Controller;

public class ControllerState
{
    public virtual ControllerStateID ID
    {
        get => ControllerStateID.NONE;
    }

    protected IPlotterController Controller
    {
        get => Context.Controller;
    }

    protected StateContext Context;

    public bool isStart;

    public ControllerState(StateContext context)
    {
        Context = context;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void Draw(DrawContext dc) { }

    public virtual void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void RButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void RButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void MButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        Context.StateMachine.PushState(ControllerStateID.DRAGING_VIEW_ORG);
        Context.StateMachine.CurrentState.MButtonDown(pointer, dc, x, y);
    }

    public virtual void MButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void Cancel() { }

    public virtual void MoveKeyDown(MoveInfo moveInfo, bool isStart) { }

    public virtual void MoveKeyUp() { }
}

public class NoneState : ControllerState
{
    public override ControllerStateID ID
    {
        get => ControllerStateID.NONE;
    }

    public NoneState(StateContext context) : base(context)
    {
    }
}


public class CreateFigureState : ControllerState
{
    public override ControllerStateID ID
    {
        get => ControllerStateID.CREATE_FIGURE;
    }

    public CreateFigureState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
        isStart = true;
    }

    public override void Exit()
    {
    }

    public override void Draw(DrawContext dc)
    {
        if (isStart)
        {
            return;
        }

        FigCreator creator = Controller.FigureCreator;

        if (creator != null)
        {
            vector3_t p = dc.DevPointToWorldPoint(Controller.Input.CrossCursor.Pos);
            creator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
        }
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        if (isStart)
        {
            Controller.Input.LastDownPoint = Controller.Input.SnapPoint;

            CadFigure fig = Controller.DB.NewFigure(Controller.CreatingFigType);

            Controller.FigureCreator = FigCreator.Get(Controller.CreatingFigType, fig);

            isStart = false;

            Controller.FigureCreator.StartCreate(dc);

            SetPointInCreating(dc, (CadVertex)Controller.Input.SnapPoint);
        }
        else
        {
            Controller.Input.LastDownPoint = Controller.Input.SnapPoint;

            SetPointInCreating(dc, (CadVertex)Controller.Input.SnapPoint);
        }
    }

    public override void Cancel()
    {
        if (Controller.FigureCreator?.Figure.PointCount > 0)
        {
            Controller.UpdateObjectTree(true);
        }

        Context.ChangeState(ControllerStateID.SELECT);

        Controller.CreatingFigType = CadFigure.Types.NONE;
        Controller.NotifyStateChange(
            new StateChangedParam(StateChangedType.CREATING_FIG_TYPE_CHANGED));
    }

    private FigCreator FigureCreator
    {
        get => Controller.FigureCreator;
    }

    private CadLayer CurrentLayer
    {
        get => Controller.CurrentLayer;
    }

    private HistoryManager HistoryMan
    {
        get => Controller.HistoryMan;
    }

    private void SetPointInCreating(DrawContext dc, CadVertex p)
    {
        FigureCreator.AddPointInCreating(dc, p);

        FigCreator.State state = FigureCreator.GetCreateState();

        if (state == FigCreator.State.FULL)
        {
            FigureCreator.EndCreate(dc);

            CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, FigureCreator.Figure.ID);
            HistoryMan.foward(ope);
            CurrentLayer.AddFigure(FigureCreator.Figure);

            Controller.NextState();
        }
        else if (state == FigCreator.State.ENOUGH)
        {
            CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, FigureCreator.Figure.ID);
            HistoryMan.foward(ope);
            CurrentLayer.AddFigure(FigureCreator.Figure);
        }
        else if (state == FigCreator.State.WAIT_NEXT_POINT)
        {
            CadOpe ope = new CadOpeAddPoint(
                CurrentLayer.ID,
                FigureCreator.Figure.ID,
                FigureCreator.Figure.PointCount - 1,
                ref p
                );

            HistoryMan.foward(ope);
        }
    }
}

public class SelectingState : ControllerState
{
    private bool EditStarted = false;
    private List<CadFigure> EditFigList = null;


    public override ControllerStateID ID
    {
        get => ControllerStateID.SELECT;
    }

    public SelectingState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Draw(DrawContext dc)
    {
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t pixp = new(x, y, 0);


        if (Controller.Input.SelectNearest(dc, Controller.Input.CrossCursor.Pos))
        {
            if (!Controller.Input.CursorLocked)
            {
                Context.ChangeState(ControllerStateID.DRAGING_POINTS);
                Context.CurrentState.LButtonDown(pointer, dc, x, y);
            }

            Controller.Input.CrossCursorOffset = pixp - Controller.Input.CrossCursor.Pos;

            Context.StoredObjDownPoint = Controller.Input.ObjDownPoint;
        }
        else
        {
            Context.ChangeState(ControllerStateID.RUBBER_BAND_SELECT);
            Context.CurrentState.LButtonDown(pointer, dc, x, y);
        }
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        Controller.NotifyStateChange(
            new StateChangedParam(StateChangedType.SELECTION_CHANGED));
    }

    public override void Cancel()
    {
    }

    public override void MoveKeyDown(MoveInfo moveInfo, bool isStart)
    {
        if (isStart)
        {
            EditFigList = Controller.DB.GetSelectedFigList();
            if (EditFigList != null && EditFigList.Count > 0)
            {
                EditStarted = true;
                Controller.EditManager.StartEdit(EditFigList);
            }
        }


        if (EditStarted)
        {
            Controller.CommandProc.MovePointsFromStored(EditFigList, moveInfo);
            Controller.Redraw();
        }
        else
        {
            vector3_t p = Controller.Input.GetCursorPos();
            Controller.Input.SetCursorWoldPos(p + moveInfo.Delta);
            Controller.Redraw();
        }
    }

    public override void MoveKeyUp()
    {
        if (EditStarted)
        {
            Controller.EditManager.EndEdit(EditFigList);

            EditFigList = null;
            EditStarted = false;
        }
        Controller.Redraw();
    }
}

public class RubberBandSelectState : ControllerState
{
    private vector3_t RubberBandScrnPoint0 = VectorExt.InvalidVector3;
    private vector3_t RubberBandScrnPoint1 = default;

    public override ControllerStateID ID
    {
        get => ControllerStateID.RUBBER_BAND_SELECT;
    }

    public RubberBandSelectState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
        isStart = true;
    }

    public override void Exit()
    {
    }

    public override void Draw(DrawContext dc)
    {
        dc.Drawing.DrawRectScrn(dc.GetPen(DrawTools.PEN_TEMP_FIGURE),
            RubberBandScrnPoint0, RubberBandScrnPoint1);
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t pixp = new vector3_t(x, y, 0);

        RubberBandScrnPoint0 = pixp;
        RubberBandScrnPoint1 = pixp;
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t pixp = new vector3_t(x, y, 0);

        RubberBandSelect(dc, RubberBandScrnPoint0, pixp);

        RubberBandScrnPoint0 = VectorExt.InvalidVector3;

        Controller.NotifyStateChange(
            new StateChangedParam(StateChangedType.SELECTION_CHANGED));

        Context.ChangeState(ControllerStateID.SELECT);
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        RubberBandScrnPoint1.X = x;
        RubberBandScrnPoint1.Y = y;
        RubberBandScrnPoint1.Z = 0;
    }

    public override void Cancel()
    {
    }

    private void RubberBandSelect(DrawContext dc, vector3_t p0, vector3_t p1)
    {
        Controller.Input.LastSelPoint = null;
        Controller.Input.LastSelSegment = null;

        vector3_t minp = VectorExt.Min(p0, p1);
        vector3_t maxp = VectorExt.Max(p0, p1);
        Controller.DB.ForEachEditableFigure(
            (layer, fig) =>
            {
                SelectIfContactRect(dc, minp, maxp, fig);
            });
    }

    private static void SelectIfContactRect(DrawContext dc, vector3_t minp, vector3_t maxp, CadFigure fig)
    {
        for (int i = 0; i < fig.PointCount; i++)
        {
            vector3_t p = dc.WorldPointToDevPoint(fig.PointList[i].vector);

            if (CadUtil.IsInRect2D(minp, maxp, p))
            {
                fig.SelectPointAt(i, true);
            }
        }
        return;
    }
}

public class DragingPointsState : ControllerState
{
    vector3_t StartPos;

    public override ControllerStateID ID
    {
        get => ControllerStateID.DRAGING_POINTS;
    }

    public DragingPointsState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
        isStart = true;
        StartPos = Controller.Input.CrossCursor.Pos;
    }

    public override void Exit()
    {
    }

    public override void Draw(DrawContext dc)
    {
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        //Ctrl.mPointSearcher.SetIgnoreList(null);
        //Ctrl.mSegSearcher.SetIgnoreList(null);
        //Ctrl.mSegSearcher.SetIgnoreSeg(null);

        //DOut.pl("LButtonUp isStart:" + isStart);
        if (!isStart)
        {
            Controller.EditManager.EndEdit();
        }

        Context.ChangeState(ControllerStateID.SELECT);
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        if (isStart)
        {
            //
            // 選択時に思わずずらしてしまうことを防ぐため、
            // 最初だけある程度ずらさないと移動しないようにする
            //
            CadVertex v = CadVertex.Create(x, y, 0);
            vcompo_t d = (Controller.Input.RawDownPoint - v).Norm();

            if (d > SettingsHolder.Settings.InitialMoveLimit)
            {
                isStart = false;
                Controller.EditManager.StartEdit();
            }
        }
        else
        {
            vector3_t p0 = dc.DevPointToWorldPoint(StartPos);
            vector3_t p1 = dc.DevPointToWorldPoint(Controller.Input.CrossCursor.Pos);

            vector3_t delta = p1 - p0;

            Controller.Editor.MoveSelectedPoints(dc, new MoveInfo(p0, p1, Controller.Input.CrossCursor.Pos));

            Controller.Input.ObjDownPoint = Context.StoredObjDownPoint + delta;
        }
    }

    public override void Cancel()
    {
        Controller.EditManager.CancelEdit();
        Context.ChangeState(ControllerStateID.SELECT);
        Controller.Input.ClearSelection();
    }
}

public class MeasuringState : ControllerState
{
    public override ControllerStateID ID
    {
        get => ControllerStateID.MEASURING;
    }

    public MeasuringState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Draw(DrawContext dc)
    {
        if (Controller.MeasureFigureCreator != null)
        {
            vector3_t p = dc.DevPointToWorldPoint(Controller.Input.CrossCursor.Pos);
            Controller.MeasureFigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
        }
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        Controller.Input.LastDownPoint = Controller.Input.SnapPoint;

        CadVertex p;

        if (Controller.Input.CurrentSnapInfo.IsPointMatch)
        {
            p = new CadVertex(Controller.Input.SnapPoint);
        }
        else
        {
            p = (CadVertex)dc.DevPointToWorldPoint(Controller.Input.CrossCursor.Pos);
        }

        SetPointInMeasuring(dc, p);
        PutMeasure();
    }

    public override void Cancel()
    {
        Context.ChangeState(ControllerStateID.SELECT);
        Controller.MeasureMode = MeasureModes.NONE;
        Controller.MeasureFigureCreator = null;

        Controller.NotifyStateChange(
            new StateChangedParam(StateChangedType.MESURE_MODE_CHANGED));
    }

    private FigCreator MeasureFigureCreator
    {
        get => Controller.MeasureFigureCreator;
    }

    private void SetPointInMeasuring(DrawContext dc, CadVertex p)
    {
        MeasureFigureCreator.AddPointInCreating(dc, p);
    }

    private void PutMeasure()
    {
        int pcnt = MeasureFigureCreator.Figure.PointCount;

        vcompo_t currentD = 0;

        if (pcnt > 1)
        {
            CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
            CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

            currentD = (p1 - p0).Norm();
            currentD = (vcompo_t)Math.Round(currentD, 4);
        }

        vcompo_t a = 0;

        if (pcnt > 2)
        {
            CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
            CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 3);
            CadVertex p2 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

            vector3_t v1 = p1.vector - p0.vector;
            vector3_t v2 = p2.vector - p0.vector;

            vcompo_t t = CadMath.AngleOfVector(v1, v2);
            a = CadMath.Rad2Deg(t);
            a = (vcompo_t)Math.Round(a, 4);
        }

        vcompo_t totalD = CadUtil.AroundLength(MeasureFigureCreator.Figure);

        totalD = (vcompo_t)Math.Round(totalD, 4);

        int cnt = MeasureFigureCreator.Figure.PointCount;

        ItConsole.println("[" + cnt.ToString() + "]" +
            AnsiEsc.Reset + " LEN:" + AnsiEsc.BGreen + currentD.ToString() +
            AnsiEsc.Reset + " ANGLE:" + AnsiEsc.BBlue + a.ToString() +
            AnsiEsc.Reset + " TOTAL:" + totalD.ToString());
    }
}

public class DragingViewOrgState : ControllerState
{
    private vector3_t StoreViewOrg = default;
    private vector3_t StoreSnapPoint = default;

    private vector3_t MDownPoint = default;

    public override ControllerStateID ID
    {
        get => ControllerStateID.DRAGING_VIEW_ORG;
    }

    public DragingViewOrgState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
        StoreSnapPoint = Context.Controller.Input.SnapPoint;
    }

    public override void Exit()
    {
    }

    public override void Draw(DrawContext dc)
    {
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    public override void MButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        MDownPoint = dc.WorldPointToDevPoint(StoreSnapPoint);

        Controller.Input.UnlockCursor();

        Controller.Input.CrossCursor.Store();

        Controller.ChangeMouseCursor(UITypes.MouseCursorType.HAND);

        StoreViewOrg = dc.ViewOrg;
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        ViewOrgDrag(pointer, dc, x, y);
    }

    public override void MButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t p = dc.WorldPointToDevPoint(StoreSnapPoint);

        if (MDownPoint.X == p.X && MDownPoint.Y == p.Y)
        {
            ViewPortUtil.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
        }

        Context.StateMachine.PopState();

        Controller.Input.CrossCursor.Pos = new vector3_t(x, y, 0);

        Controller.ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
    }

    public override void Cancel()
    {
        Context.StateMachine.PopState();

        vector3_t op = StoreViewOrg;

        ViewPortUtil.SetOrigin(Context.Controller.DC, (int)op.X, (int)op.Y);

        Controller.ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
    }

    private void ViewOrgDrag(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t cp = new vector3_t(x, y, 0);

        vector3_t d = cp - MDownPoint;

        vector3_t op = StoreViewOrg + d;

        ViewPortUtil.SetOrigin(dc, (int)op.X, (int)op.Y);

        Controller.Input.CrossCursor.Pos = Controller.Input.CrossCursor.StorePos + d;
    }
}

