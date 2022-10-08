using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Settings;
using System;
using TCad.Controls;

namespace Plotter.Controller;

public partial class PlotterController
{
    public enum States
    {
        NONE,
        SELECT,
        RUBBER_BAND_SELECT,
        DRAGING_POINTS,
        DRAGING_VIEW_ORG,
        CREATING,
        MEASURING,
    }

    private ControllerStateMachine StateMachine;

    public States State
    {
        get => StateMachine.CurrentStateID;
    }

    private States mBackState;

    private ControllerState CurrentState
    {
        get => StateMachine.CurrentState;
    }

    public void ChangeState(States state)
    {
        StateMachine.ChangeState(state);
    }
}

public class ControllerStateContext
{
    public Vector3d StoredObjDownPoint = default;
    public PlotterController Controller;

    public ControllerStateContext(PlotterController controller)
    {
        Controller = controller;
    }
}

public class ControllerStateMachine
{
    ControllerState[] StateList = new ControllerState[(int)PlotterController.States.MEASURING + 1];

    private ControllerState mCurrentState = null;

    public ControllerState CurrentState
    {
        get => mCurrentState;
    }

    public PlotterController.States CurrentStateID
    {
        get
        {
            if (mCurrentState == null)
            {
                return PlotterController.States.NONE;
            }

            return mCurrentState.State;
        }
    }

    private ControllerStateContext Context;

    private PlotterController Controller;

    public ControllerStateMachine(PlotterController controller)
    {
        Controller = controller;
        Context = new ControllerStateContext(controller);

        StateList[(int)PlotterController.States.SELECT] = new SelectingState(Context);
        StateList[(int)PlotterController.States.RUBBER_BAND_SELECT] = new RubberBandSelectState(Context);
        StateList[(int)PlotterController.States.DRAGING_POINTS] = new DragingPointsState(Context);
        StateList[(int)PlotterController.States.DRAGING_VIEW_ORG] = new DragingViewOrgState(Context);
        StateList[(int)PlotterController.States.CREATING] = new CreateFigureState(Context);
        StateList[(int)PlotterController.States.MEASURING] = new MeasuringState(Context);
    }

    public void ChangeState(PlotterController.States state)
    {
        if (mCurrentState != null)
        {
            DOut.pl(mCurrentState.GetType().Name + " Exit");
            mCurrentState.Exit();
        }

        mCurrentState = StateList[(int)state];

        if (mCurrentState != null)
        {
            DOut.pl(mCurrentState.GetType().Name + " Enter");
            mCurrentState.Enter();
        }

        if (Controller.InteractCtrl.IsActive)
        {
            Controller.InteractCtrl.Cancel();
        }
    }
}

public class ControllerState
{
    public virtual PlotterController.States State
    {
        get => PlotterController.States.NONE;
    }

    protected PlotterController Ctrl;

    protected ControllerStateContext Context;

    public bool isStart;

    public ControllerState(ControllerStateContext context)
    {
        Context = context;
        Ctrl = Context.Controller;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void Draw(DrawContext dc) { }

    public virtual void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y) { }

    public virtual void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y) { }

    public virtual void MButtonDown(CadMouse pointer, DrawContext dc, double x, double y) { }

    public virtual void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y) { }

    public virtual void MouseMove(CadMouse pointer, DrawContext dc, double x, double y) { }

    public virtual void Cancel() { }
}

public class CreateFigureState : ControllerState
{
    public override PlotterController.States State
    {
        get => PlotterController.States.CREATING;
    }

    public CreateFigureState(ControllerStateContext context) : base(context)
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
            Vector3d p = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);
            creator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
        }
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
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

            CadVertex p = (CadVertex)dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

            SetPointInCreating(dc, (CadVertex)Ctrl.SnapPoint);
        }
        else
        {
            Ctrl.LastDownPoint = Ctrl.SnapPoint;

            CadVertex p = (CadVertex)dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

            SetPointInCreating(dc, (CadVertex)Ctrl.SnapPoint);
        }
    }

    public override void Cancel()
    {
        if (Ctrl.FigureCreator?.Figure.PointCount > 0)
        {
            Ctrl.UpdateObjectTree(true);
        }

        Ctrl.ChangeState(PlotterController.States.SELECT);
        Ctrl.CreatingFigType = CadFigure.Types.NONE;
        Ctrl.NotifyStateChange();
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

public class SelectingState : ControllerState
{
    public override PlotterController.States State
    {
        get => PlotterController.States.SELECT;
    }

    public SelectingState(ControllerStateContext context) : base(context)
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

    public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
        Vector3d pixp = new Vector3d(x, y, 0);


        if (Ctrl.SelectNearest(dc, (Vector3d)Ctrl.CrossCursor.Pos))
        {
            if (!Ctrl.CursorLocked)
            {
                Ctrl.ChangeState(PlotterController.States.DRAGING_POINTS);
            }

            Ctrl.CrossCursorOffset = pixp - Ctrl.CrossCursor.Pos;

            Context.StoredObjDownPoint = Ctrl.ObjDownPoint;
        }
        else
        {
            Ctrl.ChangeState(PlotterController.States.RUBBER_BAND_SELECT);
        }
    }

    public override void Cancel()
    {
    }
}

public class RubberBandSelectState : ControllerState
{
    public override PlotterController.States State
    {
        get => PlotterController.States.RUBBER_BAND_SELECT;
    }

    public RubberBandSelectState(ControllerStateContext context) : base(context)
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
        Ctrl.DrawSelRect(dc);
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
        Vector3d pixp = new Vector3d(x, y, 0);


        if (Ctrl.SelectNearest(dc, (Vector3d)Ctrl.CrossCursor.Pos))
        {
            if (!Ctrl.CursorLocked)
            {
                Ctrl.ChangeState(PlotterController.States.DRAGING_POINTS);
            }

            Ctrl.CrossCursorOffset = pixp - Ctrl.CrossCursor.Pos;

            Context.StoredObjDownPoint = Ctrl.ObjDownPoint;
        }
        else
        {
            Ctrl.ChangeState(PlotterController.States.RUBBER_BAND_SELECT);
        }
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
        Ctrl.RubberBandSelect(Ctrl.RubberBandScrnPoint0, Ctrl.RubberBandScrnPoint1);
        Ctrl.ChangeState(PlotterController.States.SELECT);
    }

    public override void Cancel()
    {
    }
}

public class DragingPointsState : ControllerState
{
    public override PlotterController.States State
    {
        get => PlotterController.States.DRAGING_POINTS;
    }

    public DragingPointsState(ControllerStateContext context) : base(context)
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
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
        //Ctrl.mPointSearcher.SetIgnoreList(null);
        //Ctrl.mSegSearcher.SetIgnoreList(null);
        //Ctrl.mSegSearcher.SetIgnoreSeg(null);

        //DOut.pl("LButtonUp isStart:" + isStart);
        if (!isStart)
        {
            Ctrl.EndEdit();
        }

        Ctrl.ChangeState(PlotterController.States.SELECT);
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, double x, double y) 
    {
        if (isStart)
        {
            //
            // 選択時に思わずずらしてしまうことを防ぐため、
            // 最初だけある程度ずらさないと移動しないようにする
            //
            CadVertex v = CadVertex.Create(x, y, 0);
            double d = (Ctrl.RawDownPoint - v).Norm();

            if (d > SettingsHolder.Settings.InitialMoveLimit)
            {
                isStart = false;
                //DOut.pl("MouseMove change isStart:" + isStart);
                Ctrl.StartEdit();
            }
        }
        else
        {
            Vector3d p0 = dc.DevPointToWorldPoint(Ctrl.MoveOrgScrnPoint);
            Vector3d p1 = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

            //p0.dump("p0");
            //p1.dump("p1");

            Vector3d delta = p1 - p0;

            Ctrl.MoveSelectedPoints(dc, new MoveInfo(p0, p1, Ctrl.MoveOrgScrnPoint, Ctrl.CrossCursor.Pos));

            Ctrl.ObjDownPoint = Context.StoredObjDownPoint + delta;
        }
    }

    public override void Cancel()
    {
        Ctrl.CancelEdit();
        Ctrl.ChangeState(PlotterController.States.SELECT);
        Ctrl.ClearSelection();
    }
}
    
public class MeasuringState : ControllerState
{
    public override PlotterController.States State
    {
        get => PlotterController.States.MEASURING;
    }

    public MeasuringState(ControllerStateContext context) : base(context)
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
            Vector3d p = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);
            Ctrl.MeasureFigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
        }
    }

    public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
        Ctrl.LastDownPoint = Ctrl.SnapPoint;

        CadVertex p;

        if (Ctrl.mSnapInfo.IsPointMatch)
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

    public override void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
    {
    }

    public override void Cancel()
    {
        Ctrl.ChangeState(PlotterController.States.SELECT);
        Ctrl.MeasureMode = MeasureModes.NONE;
        Ctrl.MeasureFigureCreator = null;

        Ctrl.NotifyStateChange();
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

        double currentD = 0;

        if (pcnt > 1)
        {
            CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
            CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

            currentD = (p1 - p0).Norm();
            currentD = Math.Round(currentD, 4);
        }

        double a = 0;

        if (pcnt > 2)
        {
            CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
            CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 3);
            CadVertex p2 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

            Vector3d v1 = p1.vector - p0.vector;
            Vector3d v2 = p2.vector - p0.vector;

            double t = CadMath.AngleOfVector(v1, v2);
            a = CadMath.Rad2Deg(t);
            a = Math.Round(a, 4);
        }

        double totalD = CadUtil.AroundLength(MeasureFigureCreator.Figure);

        totalD = Math.Round(totalD, 4);

        int cnt = MeasureFigureCreator.Figure.PointCount;

        ItConsole.println("[" + cnt.ToString() + "]" +
            AnsiEsc.Reset + " LEN:" + AnsiEsc.BGreen + currentD.ToString() +
            AnsiEsc.Reset + " ANGLE:" + AnsiEsc.BBlue + a.ToString() +
            AnsiEsc.Reset + " TOTAL:" + totalD.ToString());
    }
}

public class DragingViewOrgState : ControllerState
{
    public override PlotterController.States State
    {
        get => PlotterController.States.DRAGING_VIEW_ORG;
    }

    public DragingViewOrgState(ControllerStateContext context) : base(context)
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

    public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
    }

    public override void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
    }

    public override void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
    {
        ViewOrgDrag(pointer, dc, x, y);
    }

    public override void Cancel()
    {
    }

    private void ViewOrgDrag(CadMouse pointer, DrawContext dc, double x, double y)
    {
        Vector3d cp = new Vector3d(x, y, 0);

        Vector3d d = cp - pointer.MDownPoint;

        Vector3d op = Ctrl.StoreViewOrg + d;

        ViewUtil.SetOrigin(dc, (int)op.X, (int)op.Y);

        Ctrl.CrossCursor.Pos = Ctrl.CrossCursor.StorePos + d;
    }
}

