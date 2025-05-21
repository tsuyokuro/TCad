#define ENABLE_LOG

using System.Collections.Generic;
using TCad.Logger;

namespace TCad.Plotter.Controller;

public enum ControllerStateID
{
    NONE,
    SELECT,
    RUBBER_BAND_SELECT,
    DRAGING_POINTS,
    DRAGING_VIEW_ORG,
    CREATE_FIGURE,
    MEASURING,
}

public class StateContext
{
    public vector3_t StoredObjDownPoint = default;

    public IPlotterController Controller
    {
        get;
        private set;
    }

    public ControllerState CurrentState
    {
        get => StateMachine.CurrentState;
    }

    public ControllerStateMachine StateMachine
    {
        get;
        private set;
    }

    public StateContext(ControllerStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        Controller = stateMachine.Controller;
    }

    public void ChangeState(ControllerStateID state)
    {
        StateMachine.ChangeState(state);
    }
}

public class ControllerStateMachine
{
    private ControllerState[] StateList = new ControllerState[(int)ControllerStateID.MEASURING + 1];


    private Stack<ControllerState> StateStack = new(10);

    public ControllerState CurrentState
    {
        get;
        private set;
    }

    public ControllerStateID CurrentStateID
    {
        get
        {
            return CurrentState.ID;
        }
    }

    private StateContext Context;

    public IPlotterController Controller
    {
        get;
        private set;
    }

    public ControllerStateMachine(IPlotterController controller, ControllerStateID initialState)
    {
        Controller = controller;
        Context = new StateContext(this);

        StateList[(int)ControllerStateID.NONE] = new NoneState(Context);
        StateList[(int)ControllerStateID.SELECT] = new SelectingState(Context);
        StateList[(int)ControllerStateID.RUBBER_BAND_SELECT] = new RubberBandSelectState(Context);
        StateList[(int)ControllerStateID.DRAGING_POINTS] = new DragingPointsState(Context);
        StateList[(int)ControllerStateID.DRAGING_VIEW_ORG] = new DragingViewOrgState(Context);
        StateList[(int)ControllerStateID.CREATE_FIGURE] = new CreateFigureState(Context);
        StateList[(int)ControllerStateID.MEASURING] = new MeasuringState(Context);

        CurrentState = StateList[(int)ControllerStateID.NONE];
        ChangeState(initialState);
    }

    public void ChangeState(ControllerStateID state)
    {
        // If change to the same state, do nothing
        if (CurrentState.ID == state)
        {
            return;
        }

#if ENABLE_LOG
        Log.pl(CurrentState.GetType().Name + " Exit");
#endif

        CurrentState.Exit();

        CurrentState = StateList[(int)state];

#if ENABLE_LOG
        Log.pl(CurrentState.GetType().Name + " Enter");
#endif

        CurrentState.Enter();

        if (Controller.Input.InteractCtrl.IsActive)
        {
            Controller.Input.InteractCtrl.Cancel();
        }
    }

    public void PushState(ControllerStateID state)
    {
#if ENABLE_LOG
        Log.pl(CurrentState.GetType().Name + " Push");
#endif

        StateStack.Push(CurrentState);

        CurrentState = StateList[(int)state];

#if ENABLE_LOG
        Log.pl(CurrentState.GetType().Name + " Enter");
#endif

        CurrentState.Enter();
    }

    public void PopState()
    {
        ControllerState backState;
        if (StateStack.TryPop(out backState))
        {
#if ENABLE_LOG
            Log.pl(CurrentState.GetType().Name + " Exit");
#endif
            CurrentState.Exit();

            CurrentState = backState;

#if ENABLE_LOG
            Log.pl(CurrentState.GetType().Name + " is Poped");
#endif

        }
    }
}

