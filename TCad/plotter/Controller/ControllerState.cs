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
        ControllerState[] StateList = new ControllerState[(int)States.EOD];
        ControllerState CurrentState = null;

        private void InitState()
        {
            StateList[(int)States.SELECT] = null;
            StateList[(int)States.RUBBER_BAND_SELECT] = null;
            StateList[(int)States.START_DRAGING_POINTS] = null;
            StateList[(int)States.DRAGING_POINTS] = null;
            StateList[(int)States.DRAGING_VIEW_ORG] = null;
            StateList[(int)States.START_CREATE] = new CreateFigureState(this);
            StateList[(int)States.CREATING] = null;
            StateList[(int)States.MEASURING] = null;
        }

        private void ChangeState(States state)
        {
            State = state;
            CurrentState = StateList[(int)state];
            if (CurrentState != null)
            {
                CurrentState.Enter();
            }
        }

        public class ControllerState
        {
            protected PlotterController Ctrl;

            public ControllerState(PlotterController controller)
            {
                Ctrl = controller;
            }

            public virtual void Enter()
            {

            }

            public virtual void Exit()
            {

            }

            public virtual void Draw(DrawContext dc)
            {

            }

            public virtual void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
            {

            }

            public virtual void Cancel()
            {

            }

            public virtual void CloseFigure()
            {

            }
        }

        public class CreateFigureState : ControllerState
        {
            bool isCreating = false;

            public CreateFigureState(PlotterController controller) : base(controller)
            {
                Ctrl = controller;
            }

            public override void Enter()
            {
                isCreating = false;
            }

            public override void Exit()
            {

            }

            public override void Draw(DrawContext dc)
            {
                FigCreator creator = Ctrl.FigureCreator;

                if (creator != null)
                {
                    Vector3d p = dc.DevPointToWorldPoint(Ctrl.CrossCursor.Pos);
                    creator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
                }
            }

            public override void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
            {
                if (!isCreating)
                {
                    Ctrl.LastDownPoint = Ctrl.SnapPoint;

                    CadFigure fig = Ctrl.mDB.NewFigure(Ctrl.CreatingFigType);

                    Ctrl.mFigureCreator = FigCreator.Get(Ctrl.CreatingFigType, fig);

                    // TODO Remove States.CREATING state
                    //Ctrl.State = States.CREATING;

                    isCreating = true;

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
                Ctrl.ChangeState(States.SELECT);
                Ctrl.CreatingFigType = CadFigure.Types.NONE;
                Ctrl.NotifyStateChange();
            }

            public override void CloseFigure()
            {
                if (Ctrl.FigureCreator != null)
                {
                    Ctrl.FigureCreator.Figure.IsLoop = true;

                    CadOpe ope = new CadOpeSetClose(Ctrl.CurrentLayer.ID, Ctrl.FigureCreator.Figure.ID, true);
                    Ctrl.HistoryMan.foward(ope);

                    Ctrl.FigureCreator.EndCreate(Ctrl.DC);
                }

                NextState();
            }

            private void NextState()
            {
                if (SettingsHolder.Settings.ContinueCreateFigure)
                {
                    Ctrl.mFigureCreator = null;
                    Ctrl.StartCreateFigure(Ctrl.CreatingFigType);
                    Ctrl.UpdateObjectTree(true);
                }
                else
                {
                    Ctrl.mFigureCreator = null;
                    Ctrl.CreatingFigType = CadFigure.Types.NONE;
                    
                    Ctrl.ChangeState(States.SELECT);

                    Ctrl.UpdateObjectTree(true);
                    Ctrl.NotifyStateChange();
                }
            }
        }
    }
}