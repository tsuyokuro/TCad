using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        ControllerState[] StateList = new ControllerState[(int)States.MEASURING+1];
        ControllerState CurrentState = null;

        private void InitState()
        {
            StateList[(int)States.SELECT] = new SelectingState(this);
            StateList[(int)States.RUBBER_BAND_SELECT] = new RubberBandSelectState(this);
            StateList[(int)States.DRAGING_POINTS] = new DragingPointsState(this);
            StateList[(int)States.DRAGING_VIEW_ORG] = new DragingViewOrgState(this);
            StateList[(int)States.CREATING] = new CreateFigureState(this);
            StateList[(int)States.MEASURING] = new MeasuringState(this);

            State = States.SELECT;
        }

        private void ChangeState(States state)
        {
            if (CurrentState != null)
            {
                DOut.pl(CurrentState.GetType().Name + " Exit");
                CurrentState.Exit();
            }

            CurrentState = StateList[(int)state];
            
            if (CurrentState != null)
            {
                DOut.pl(CurrentState.GetType().Name + " Enter");
                CurrentState.Enter();
            }


            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Cancel();
            }
        }

        public class ControllerState
        {
            public virtual States State
            {
                get => States.NONE;
            }

            protected PlotterController Ctrl;

            public bool isStart;

            public ControllerState(PlotterController controller)
            {
                Ctrl = controller;
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
            public override States State
            {
                get => States.CREATING;
            }

            public CreateFigureState(PlotterController controller) : base(controller)
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

                    CadFigure fig = Ctrl.mDB.NewFigure(Ctrl.CreatingFigType);

                    Ctrl.mFigureCreator = FigCreator.Get(Ctrl.CreatingFigType, fig);

                    // TODO Remove States.CREATING state
                    //Ctrl.State = States.CREATING;

                    isStart = false;

                    Ctrl.FigureCreator.StartCreate(dc);

                    CadVertex p = (CadVertex)dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

                    Ctrl.SetPointInCreating(dc, (CadVertex)Ctrl.SnapPoint);
                }
                else
                {
                    Ctrl.LastDownPoint = Ctrl.SnapPoint;

                    CadVertex p = (CadVertex)dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

                    Ctrl.SetPointInCreating(dc, (CadVertex)Ctrl.SnapPoint);
                }
            }

            public override void Cancel()
            {
                Ctrl.State = States.SELECT;
                Ctrl.CreatingFigType = CadFigure.Types.NONE;
                Ctrl.NotifyStateChange();
            }
        }

        public class SelectingState : ControllerState
        {
            public override States State
            {
                get => States.SELECT;
            }

            public SelectingState(PlotterController controller) : base(controller)
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
                        Ctrl.State = States.DRAGING_POINTS;
                    }

                    Ctrl.OffsetScreen = pixp - Ctrl.CrossCursor.Pos;

                    Ctrl.StoredObjDownPoint = Ctrl.ObjDownPoint;
                }
                else
                {
                    Ctrl.State = States.RUBBER_BAND_SELECT;
                }
            }

            public override void Cancel()
            {
            }
        }

        public class RubberBandSelectState : ControllerState
        {
            public override States State
            {
                get => States.RUBBER_BAND_SELECT;
            }

            public RubberBandSelectState(PlotterController controller) : base(controller)
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
                        Ctrl.State= States.DRAGING_POINTS;
                    }

                    Ctrl.OffsetScreen = pixp - Ctrl.CrossCursor.Pos;

                    Ctrl.StoredObjDownPoint = Ctrl.ObjDownPoint;
                }
                else
                {
                    Ctrl.State = States.RUBBER_BAND_SELECT;
                }
            }

            public override void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
            {
                Ctrl.RubberBandSelect(Ctrl.RubberBandScrnPoint0, Ctrl.RubberBandScrnPoint1);
                Ctrl.State = States.SELECT;
            }

            public override void Cancel()
            {
            }
        }

        public class DragingPointsState : ControllerState
        {
            public override States State
            {
                get => States.DRAGING_POINTS;
            }

            public DragingPointsState(PlotterController controller) : base(controller)
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

                Ctrl.State = States.SELECT;
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

                if (!isStart)
                {
                    Vector3d p0 = dc.DevPointToWorldPoint(Ctrl.MoveOrgScrnPoint);
                    Vector3d p1 = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);

                    //p0.dump("p0");
                    //p1.dump("p1");

                    Vector3d delta = p1 - p0;

                    Ctrl.MoveSelectedPoints(dc, new MoveInfo(p0, p1, Ctrl.MoveOrgScrnPoint, Ctrl.CrossCursor.Pos));

                    Ctrl.ObjDownPoint = Ctrl.StoredObjDownPoint + delta;
                }
            }

            public override void Cancel()
            {
                Ctrl.CancelEdit();
                Ctrl.State = States.SELECT;
                Ctrl.ClearSelection();
            }
        }
        
        public class MeasuringState : ControllerState
        {
            public override States State
            {
                get => States.MEASURING;
            }

            public MeasuringState(PlotterController controller) : base(controller)
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

                Ctrl.SetPointInMeasuring(dc, p);
                Ctrl.PutMeasure();
            }

            public override void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
            {
            }

            public override void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
            {
            }

            public override void Cancel()
            {
                Ctrl.State = States.SELECT;
                Ctrl.mMeasureMode = MeasureModes.NONE;
                Ctrl.MeasureFigureCreator = null;

                Ctrl.NotifyStateChange();
            }
        }

        public class DragingViewOrgState : ControllerState
        {
            public override States State
            {
                get => States.DRAGING_VIEW_ORG;
            }

            public DragingViewOrgState(PlotterController controller) : base(controller)
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
                Ctrl.ViewOrgDrag(pointer, dc, x, y);
            }

            public override void Cancel()
            {
            }
        }
    }
}
