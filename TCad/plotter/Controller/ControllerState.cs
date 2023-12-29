//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Settings;
using System;
using System.Collections.Generic;
using TCad.Controls;

using StateContext = Plotter.Controller.ControllerStateMachine.StateContext;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace Plotter.Controller;

/// <summary>
/// ControllerStateMachine
/// 
/// </summary>
public class ControllerStateMachine
{
    public class StateContext
    {
        public vector3_t StoredObjDownPoint = default;
        public PlotterController Controller;

        public ControllerState CurrentState
        {
            get => StateMachine.CurrentState;
        }

        private ControllerStateMachine StateMachine;

        public StateContext(ControllerStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            Controller = stateMachine.Controller;
        }

        public void ChangeState(ControllerStates state)
        {
            StateMachine.ChangeState(state);
        }
    }

    private ControllerState[] StateList = new ControllerState[(int)ControllerStates.MEASURING + 1];

    private ControllerState mCurrentState = null;

    private Stack<ControllerState> StateStack = new(10);

    public ControllerState CurrentState
    {
        get => mCurrentState;
    }

    public ControllerStates CurrentStateID
    {
        get
        {
            if (mCurrentState == null)
            {
                return ControllerStates.NONE;
            }

            return mCurrentState.State;
        }
    }

    private StateContext Context;

    private PlotterController Controller;

    public ControllerStateMachine(PlotterController controller)
    {
        Controller = controller;
        Context = new StateContext(this);

        StateList[(int)ControllerStates.SELECT] = new SelectingState(Context);
        StateList[(int)ControllerStates.RUBBER_BAND_SELECT] = new RubberBandSelectState(Context);
        StateList[(int)ControllerStates.DRAGING_POINTS] = new DragingPointsState(Context);
        StateList[(int)ControllerStates.DRAGING_VIEW_ORG] = new DragingViewOrgState(Context);
        StateList[(int)ControllerStates.CREATING] = new CreateFigureState(Context);
        StateList[(int)ControllerStates.MEASURING] = new MeasuringState(Context);
    }

    public void ChangeState(ControllerStates state)
    {
        if (mCurrentState != null)
        {
            Log.pl(mCurrentState.GetType().Name + " Exit");
            mCurrentState.Exit();
        }

        mCurrentState = StateList[(int)state];

        if (mCurrentState != null)
        {
            Log.pl(mCurrentState.GetType().Name + " Enter");
            mCurrentState.Enter();
        }

        if (Controller.InteractCtrl.IsActive)
        {
            Controller.InteractCtrl.Cancel();
        }
    }

    public void PushState(ControllerStates state)
    {
        StateStack.Push(CurrentState);
        mCurrentState = StateList[(int)state];
        if (mCurrentState != null)
        {
            mCurrentState.Enter();
        }
    }

    public void PopState()
    {
        ControllerState backState;
        if (StateStack.TryPop(out backState))
        {
            mCurrentState = backState;
        }
    }
}

/// <summary>
/// CreateFigureState
/// 
/// </summary>
public class ControllerState
{
    public virtual ControllerStates State
    {
        get => ControllerStates.NONE;
    }

    protected PlotterController Ctrl
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

    public virtual void MButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void MButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }

    public virtual void Cancel() { }
}

/// <summary>
/// CreateFigureState
/// 
/// </summary>
public class CreateFigureState : ControllerState
{
    public override ControllerStates State
    {
        get => ControllerStates.CREATING;
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

        FigCreator creator = Ctrl.FigureCreator;

        if (creator != null)
        {
            vector3_t p = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);
            creator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
        }
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        if (isStart)
        {
            Ctrl.LastDownPoint = Ctrl.SnapPoint;

            CadFigure fig = Ctrl.DB.NewFigure(Ctrl.CreatingFigType);

            Ctrl.FigureCreator = FigCreator.Get(Ctrl.CreatingFigType, fig);

            // TODO Remove States.CREATING state
            //Ctrl.State = States.CREATING;

            isStart = false;

            Ctrl.FigureCreator.StartCreate(dc);

            SetPointInCreating(dc, (CadVertex)Ctrl.SnapPoint);
        }
        else
        {
            Ctrl.LastDownPoint = Ctrl.SnapPoint;

            SetPointInCreating(dc, (CadVertex)Ctrl.SnapPoint);
        }
    }

    public override void Cancel()
    {
        if (Ctrl.FigureCreator?.Figure.PointCount > 0)
        {
            Ctrl.UpdateObjectTree(true);
        }

        Context.ChangeState(ControllerStates.SELECT);

        Ctrl.CreatingFigType = CadFigure.Types.NONE;
        Ctrl.NotifyStateChange(
            new StateChangedParam(StateChangedType.CREATING_FIG_TYPE_CHANGED));
    }

    private FigCreator FigureCreator
    {
        get => Ctrl.FigureCreator;
    }

    private CadLayer CurrentLayer
    {
        get => Ctrl.CurrentLayer;
    }

    private HistoryManager HistoryMan
    {
        get => Ctrl.HistoryMan;
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

            Ctrl.NextState();
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

/// <summary>
/// SelectingState
/// 
/// </summary>
public class SelectingState : ControllerState
{
    public override ControllerStates State
    {
        get => ControllerStates.SELECT;
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


        if (Ctrl.SelectNearest(dc, Ctrl.CrossCursor.Pos))
        {
            if (!Ctrl.CursorLocked)
            {
                Context.ChangeState(ControllerStates.DRAGING_POINTS);
                Context.CurrentState.LButtonDown(pointer, dc, x, y);
            }

            Ctrl.CrossCursorOffset = pixp - Ctrl.CrossCursor.Pos;

            Context.StoredObjDownPoint = Ctrl.ObjDownPoint;
        }
        else
        {
            Context.ChangeState(ControllerStates.RUBBER_BAND_SELECT);
            Context.CurrentState.LButtonDown(pointer, dc, x, y);
        }
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        Ctrl.NotifyStateChange(
            new StateChangedParam(StateChangedType.SELECTION_CHANGED));
    }

    public override void Cancel()
    {
    }
}

/// <summary>
/// RubberBandSelectState
/// 
/// </summary>
public class RubberBandSelectState : ControllerState
{
    private vector3_t RubberBandScrnPoint0 = VectorExt.InvalidVector3;
    private vector3_t RubberBandScrnPoint1 = default;

    public override ControllerStates State
    {
        get => ControllerStates.RUBBER_BAND_SELECT;
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

        if (Ctrl.SelectNearest(dc, (vector3_t)Ctrl.CrossCursor.Pos))
        {
            if (!Ctrl.CursorLocked)
            {
                Context.ChangeState(ControllerStates.DRAGING_POINTS);
            }

            Ctrl.CrossCursorOffset = pixp - Ctrl.CrossCursor.Pos;

            Context.StoredObjDownPoint = Ctrl.ObjDownPoint;
        }
        else
        {
            Context.ChangeState(ControllerStates.RUBBER_BAND_SELECT);
        }
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t pixp = new vector3_t(x, y, 0);

        RubberBandSelect(dc, RubberBandScrnPoint0, pixp);

        RubberBandScrnPoint0 = VectorExt.InvalidVector3;

        Ctrl.NotifyStateChange(
            new StateChangedParam(StateChangedType.SELECTION_CHANGED));

        Context.ChangeState(ControllerStates.SELECT);
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
        Ctrl.LastSelPoint = null;
        Ctrl.LastSelSegment = null;

        vector3_t minp = VectorExt.Min(p0, p1);
        vector3_t maxp = VectorExt.Max(p0, p1);
        Ctrl.DB.ForEachEditableFigure(
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

/// <summary>
/// DragingPointsState
/// 
/// </summary>
public class DragingPointsState : ControllerState
{
    vector3_t StartPos;

    public override ControllerStates State
    {
        get => ControllerStates.DRAGING_POINTS;
    }

    public DragingPointsState(StateContext context) : base(context)
    {
    }

    public override void Enter()
    {
        isStart = true;
        StartPos = Ctrl.CrossCursor.Pos;
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
            Ctrl.EndEdit();
        }

        Context.ChangeState(ControllerStates.SELECT);
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
            vcompo_t d = (Ctrl.RawDownPoint - v).Norm();

            if (d > SettingsHolder.Settings.InitialMoveLimit)
            {
                isStart = false;
                Ctrl.StartEdit();
            }
        }
        else
        {
            vector3_t p0 = dc.DevPointToWorldPoint(StartPos);
            vector3_t p1 = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

            vector3_t delta = p1 - p0;

            Ctrl.MoveSelectedPoints(dc, new MoveInfo(p0, p1, Ctrl.CrossCursor.Pos));

            Ctrl.ObjDownPoint = Context.StoredObjDownPoint + delta;
        }
    }

    public override void Cancel()
    {
        Ctrl.CancelEdit();
        Context.ChangeState(ControllerStates.SELECT);
        Ctrl.ClearSelection();
    }
}

/// <summary>
/// MeasuringState
/// </summary>
public class MeasuringState : ControllerState
{
    public override ControllerStates State
    {
        get => ControllerStates.MEASURING;
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
        if (Ctrl.MeasureFigureCreator != null)
        {
            vector3_t p = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);
            Ctrl.MeasureFigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
        }
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        Ctrl.LastDownPoint = Ctrl.SnapPoint;

        CadVertex p;

        if (Ctrl.CurrentSnapInfo.IsPointMatch)
        {
            p = new CadVertex(Ctrl.SnapPoint);
        }
        else
        {
            p = (CadVertex)dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);
        }

        SetPointInMeasuring(dc, p);
        PutMeasure();
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    public override void Cancel()
    {
        Context.ChangeState(ControllerStates.SELECT);
        Ctrl.MeasureMode = MeasureModes.NONE;
        Ctrl.MeasureFigureCreator = null;

        Ctrl.NotifyStateChange(
            new StateChangedParam(StateChangedType.MESURE_MODE_CHANGED));
    }

    private FigCreator MeasureFigureCreator
    {
        get => Ctrl.MeasureFigureCreator;
    }

    private void SetPointInMeasuring(DrawContext dc, CadVertex p)
    {
        MeasureFigureCreator.AddPointInCreating(dc, p);
    }

    public void PutMeasure()
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

/// <summary>
/// DragingViewOrgState
/// 
/// </summary>
public class DragingViewOrgState : ControllerState
{
    public override ControllerStates State
    {
        get => ControllerStates.DRAGING_VIEW_ORG;
    }

    public DragingViewOrgState(StateContext context) : base(context)
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
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        ViewOrgDrag(pointer, dc, x, y);
    }

    public override void Cancel()
    {
    }

    private void ViewOrgDrag(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t cp = new vector3_t(x, y, 0);

        vector3_t d = cp - pointer.MDownPoint;

        vector3_t op = Ctrl.StoreViewOrg + d;

        ViewUtil.SetOrigin(dc, (int)op.X, (int)op.Y);

        Ctrl.CrossCursor.Pos = Ctrl.CrossCursor.StorePos + d;
    }
}

