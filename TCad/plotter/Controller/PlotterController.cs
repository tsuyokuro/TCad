//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using Plotter.Controller.TaskRunner;
using Plotter.Scripting;
using Plotter.Settings;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using TCad.ViewModel;



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

public partial class PlotterController
{
    public CadObjectDB DB
    {
        get;
        private set;
    } = new CadObjectDB();

    public PaperPageSize PageSize
    {
        get;
        set;
    } = new PaperPageSize(PaperKind.A4, false);

    public SelectModes SelectMode
    {
        set;
        get;
    } = SelectModes.OBJECT;

    public CadLayer CurrentLayer
    {
        get => DB.CurrentLayer;
        set
        {
            DB.CurrentLayer = value;
            UpdateObjectTree(true);
        }
    }

    public CadFigure.Types CreatingFigType
    {
        get;
        set;
    } = CadFigure.Types.NONE;

    public MeasureModes MeasureMode
    {
        get;
        set;
    } = MeasureModes.NONE;

    public FigCreator FigureCreator
    {
        get;
        set;
    } = null;

    public FigCreator MeasureFigureCreator
    {
        get;
        set;
    } = null;

    public HistoryManager HistoryMan
    {
        get;
        private set;
    } = null;


    public IPlotterViewModel ViewModelIF
    {
        get;
        private set;
    } = IPlotterViewModel.Dummy;

    public List<CadFigure> TempFigureList
    {
        get;
        private set;
    } = new List<CadFigure>();

    public DrawContext DC
    {
        get;
        set;
    }

    public ScriptEnvironment ScriptEnv
    {
        get;
        private set;
    }

    private PlotterTaskRunner PlotterTaskRunner
    {
        get;
        set;
    }

    private Vector3List ExtendSnapPointList
    {
        get; set;
    } = new Vector3List(20);

    public ContextMenuManager ContextMenuMan
    {
        get;
        private set;
    }

    public string CurrentFileName
    {
        get;
        set;
    } = null;

    private ControllerStateMachine StateMachine;

    public ControllerStates StateID
    {
        get => StateMachine.CurrentStateID;
    }

    public ControllerState CurrentState
    {
        get => StateMachine.CurrentState;
    }

    public PlotterController(IPlotterViewModel vm)
    {
        Log.plx("in");

        if (vm == null)
        {
            throw new System.ArgumentNullException(nameof(vm));
        }

        ViewModelIF = vm;

        StateMachine = new ControllerStateMachine(this);
        ChangeState(ControllerStates.SELECT);

        CadLayer layer = DB.NewLayer();
        DB.LayerList.Add(layer);
        CurrentLayer = layer;

        HistoryMan = new HistoryManager(this);

        ScriptEnv = new ScriptEnvironment(this);

        ContextMenuMan = new ContextMenuManager(this);

        PlotterTaskRunner = new PlotterTaskRunner(this);

        ObjDownPoint = VectorExt.InvalidVector3;

        InitHid();

        Log.plx("out");
    }

    public void ChangeState(ControllerStates state)
    {
        StateMachine.ChangeState(state);
    }

    #region ObjectTree handling
    public void UpdateObjectTree(bool remakeTree)
    {
        ViewModelIF.UpdateTreeView(remakeTree);
    }

    public void SetObjectTreePos(int index)
    {
        ViewModelIF.SetTreeViewPos(index);
    }

    public int FindObjectTreeItem(uint id)
    {
        return ViewModelIF.FindTreeViewItemIndex(id);
    }
    #endregion ObjectTree handling


    #region Notify
    public void UpdateLayerList()
    {
        ViewModelIF.LayerListChanged(GetLayerListInfo());
    }

    private LayerListInfo GetLayerListInfo()
    {
        LayerListInfo layerInfo = default(LayerListInfo);
        layerInfo.LayerList = DB.LayerList;
        layerInfo.CurrentID = CurrentLayer.ID;

        return layerInfo;
    }

    public void NotifyStateChange(StateChangedParam param)
    {
        ViewModelIF.StateChanged(param);
    }
    #endregion Notify

    #region Start and End creating figure
    public void StartCreateFigure(CadFigure.Types type)
    {
        ChangeState(ControllerStates.CREATE_FIGURE);
        CreatingFigType = type;
    }

    public void EndCreateFigure()
    {
        if (FigureCreator != null)
        {
            FigureCreator.EndCreate(DC);
            FigureCreator = null;
        }

        NextState();
    }

    public void CloseFigure()
    {
        if (FigureCreator != null)
        {
            FigureCreator.Figure.IsLoop = true;

            CadOpe ope = new CadOpeSetClose(CurrentLayer.ID, FigureCreator.Figure.ID, true);
            HistoryMan.foward(ope);

            FigureCreator.EndCreate(DC);
        }

        NextState();
    }

    public void NextState()
    {
        if (StateID == ControllerStates.CREATE_FIGURE)
        {
            if (SettingsHolder.Settings.ContinueCreateFigure)
            {
                FigureCreator = null;
                StartCreateFigure(CreatingFigType);
                UpdateObjectTree(true);
            }
            else
            {
                FigureCreator = null;
                CreatingFigType = CadFigure.Types.NONE;
                ChangeState(ControllerStates.SELECT);

                UpdateObjectTree(true);
                NotifyStateChange(
                    new StateChangedParam(StateChangedType.CREATING_FIG_TYPE_CHANGED));
            }
        }
    }

    public void StartMeasure(MeasureModes mode)
    {
        ChangeState(ControllerStates.MEASURING);
        MeasureMode = mode;
        MeasureFigureCreator =
            FigCreator.Get(
                CadFigure.Types.POLY_LINES,
                CadFigure.Create(CadFigure.Types.POLY_LINES)
                );
    }

    public void EndMeasure()
    {
        ChangeState(ControllerStates.SELECT);
        MeasureMode = MeasureModes.NONE;
        MeasureFigureCreator = null;
    }
    #endregion Start and End creating figure

    public void Undo()
    {
        ClearSelection();
        HistoryMan.undo();
        UpdateObjectTree(true);
        UpdateLayerList();
    }

    public void Redo()
    {
        ClearSelection();
        HistoryMan.redo();
        UpdateObjectTree(true);
        UpdateLayerList();
    }

    #region Getting selection
    public bool HasSelect()
    {
        foreach (CadLayer layer in DB.LayerList)
        {
            foreach (CadFigure fig in layer.FigureList )
            {
                if (fig.HasSelectedPointInclueChild())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public List<CadFigure> GetSelectedFigureList()
    {
        List<CadFigure> figList = new List<CadFigure>();

        foreach (CadLayer layer in DB.LayerList)
        {
            layer.ForEachFig(fig =>
            {
                if (fig.HasSelectedPoint())
                {
                    figList.Add(fig);
                }
            });
        }

        return figList;
    }

    public List<CadFigure> GetSelectedRootFigureList()
    {
        List<CadFigure> figList = new List<CadFigure>();

        foreach (CadLayer layer in DB.LayerList)
        {
            layer.ForEachRootFig(fig =>
            {
                if (fig.HasSelectedPointInclueChild())
                {
                    figList.Add(fig);
                }
            });
        }

        return figList;
    }
    #endregion Getting selection

    public void SetDB(CadObjectDB db, bool clearHistory)
    {
        DB = db;

        if (clearHistory)
        {
            HistoryMan.Clear();
        }

        UpdateLayerList();

        UpdateObjectTree(true);

        //Redraw();
    }

    public void SetDB(CadObjectDB db)
    {
        SetDB(db, true);
    }

    public void SetCurrentLayer(uint id)
    {
        DB.CurrentLayerID = id;
        UpdateObjectTree(true);
    }

    public void EvalTextCommand(string s)
    {
        //ScriptEnv.ExecuteCommandSync(s);
        ScriptEnv.ExecuteCommandAsync(s);
    }

    public void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
    {
        PlotterPrinter printer = new PlotterPrinter();
        printer.PrintPage(this, printerGraphics, pageSize, deviceSize);
    }
}
