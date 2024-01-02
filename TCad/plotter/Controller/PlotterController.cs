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
    private CadObjectDB mDB = new CadObjectDB();
    public CadObjectDB DB => mDB;

    private PaperPageSize mPageSize = new PaperPageSize(PaperKind.A4, false);
    public PaperPageSize PageSize
    {
        get => mPageSize;
        set => mPageSize = value;
    }

    public SelectModes SelectMode
    {
        set;
        get;
    } = SelectModes.OBJECT;

    public CadLayer CurrentLayer
    {
        get => mDB.CurrentLayer;

        set
        {
            mDB.CurrentLayer = value;
            UpdateObjectTree(true);
        }
    }


    CadFigure.Types mCreatingFigType = CadFigure.Types.NONE;

    public CadFigure.Types CreatingFigType
    {
        set => mCreatingFigType = value;
        get => mCreatingFigType;
    }

    private MeasureModes mMeasureMode = MeasureModes.NONE;
    public MeasureModes MeasureMode
    {
        get => mMeasureMode;
        set => mMeasureMode = value;
    }

    private FigCreator mFigureCreator = null;
    public FigCreator FigureCreator
    {
        get => mFigureCreator;
        set => mFigureCreator = value;
    }

    public FigCreator MeasureFigureCreator = null;


    public HistoryManager HistoryMan = null;


    public bool ContinueCreate { set; get; } = true;

    private IPlotterViewModel mPlotterVM = IPlotterViewModel.Dummy;
    public IPlotterViewModel ViewModelIF
    {
        get => mPlotterVM;
        private set => mPlotterVM = value;
    }

    public List<CadFigure> TempFigureList = new List<CadFigure>();

    private DrawContext mDC;
    public DrawContext DC
    {
        set => mDC = value; 
        get => mDC;
    }

    public ScriptEnvironment ScriptEnv;

    public PlotterTaskRunner mPlotterTaskRunner;

    private Vector3List ExtendSnapPointList = new Vector3List(20);

    private ContextMenuManager mContextMenuMan;
    public ContextMenuManager ContextMenuMan
    {
        get => mContextMenuMan;
    }

    public string CurrentFileName
    {
        get;
        set;
    } = null;

    private ControllerStateMachine StateMachine;

    public ControllerStates State
    {
        get => StateMachine.CurrentStateID;
    }

    public ControllerState CurrentState
    {
        get => StateMachine.CurrentState;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="vm"> Interface of Plotter ViewModel</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// 
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

        CadLayer layer = mDB.NewLayer();
        mDB.LayerList.Add(layer);
        CurrentLayer = layer;

        HistoryMan = new HistoryManager(this);

        ScriptEnv = new ScriptEnvironment(this);

        mContextMenuMan = new ContextMenuManager(this);

        mPlotterTaskRunner = new PlotterTaskRunner(this);

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
        layerInfo.LayerList = mDB.LayerList;
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
        ChangeState(ControllerStates.CREATING);
        CreatingFigType = type;
    }

    public void EndCreateFigure()
    {
        if (mFigureCreator != null)
        {
            mFigureCreator.EndCreate(DC);
            mFigureCreator = null;
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
        if (State == ControllerStates.CREATING)
        {
            if (SettingsHolder.Settings.ContinueCreateFigure)
            {
                mFigureCreator = null;
                StartCreateFigure(CreatingFigType);
                UpdateObjectTree(true);
            }
            else
            {
                mFigureCreator = null;
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
        mMeasureMode = mode;
        MeasureFigureCreator =
            FigCreator.Get(
                CadFigure.Types.POLY_LINES,
                CadFigure.Create(CadFigure.Types.POLY_LINES)
                );
    }

    public void EndMeasure()
    {
        ChangeState(ControllerStates.SELECT);
        mMeasureMode = MeasureModes.NONE;
        MeasureFigureCreator = null;
    }
    #endregion Start and End creating figure

    #region UnDo ReDo
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
    #endregion UnDo ReDo

    #region Getting selection
    public bool HasSelect()
    {
        foreach (CadLayer layer in mDB.LayerList)
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

        foreach (CadLayer layer in mDB.LayerList)
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

        foreach (CadLayer layer in mDB.LayerList)
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
        mDB = db;

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
        mDB.CurrentLayerID = id;
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
