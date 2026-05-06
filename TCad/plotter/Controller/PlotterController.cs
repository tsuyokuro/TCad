using System.Collections.Generic;
using System.Drawing;
using TCad.Logger;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Scripting;
using TCad.Plotter.Settings;
using TCad.Plotter.undo;
using TCad.ViewModel;

namespace TCad.Plotter.Controller;

public class PlotterController : IPlotterController
{
    public bool IsStarted
    {
        get;
        protected set;
    } = false;

    private CadData CadData_ = new CadData();

    public CadObjectDB DB
    {
        get => CadData_.DB;
        set => CadData_.DB = value;
    }

    public PaperPageSize PageSize
    {
        get => CadData_.PageSize;
        set => CadData_.PageSize = value;
    }

    public vcompo_t WorldScale
    {
        get {
            return CadData_.WorldScale;
        }
    }

    public DrawContext DC
    {
        get;
        set;
    }

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


    public IPlotterViewModel ViewModel
    {
        get;
        private set;
    } = null;

    public List<CadFigure> TempFigureList
    {
        get;
        private set;
    } = new List<CadFigure>();

    public ScriptEnvironment ScriptEnv
    {
        get;
        private set;
    }

    public PlotterTaskRunner PlotterTaskRunner
    {
        get;
        set;
    }

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

    public ControllerStateMachine StateMachine
    {
        get;
        private set;
    }

    public ControllerStateID StateID
    {
        get => StateMachine.CurrentStateID;
    }

    public ControllerState CurrentState
    {
        get => StateMachine.CurrentState;
    }

    public PlotterInput Input
    {
        get;
        private set;
    }

    public PlotterDrawer Drawer
    {
        get;
        private set;
    }

    public PlotterCommandProcessor CommandProcessor
    {
        get;
        private set;
    }

    public PlotterEditManager EditManager
    {
        get;
        private set;
    }

    public PlotterEditor Editor
    {
        get;
        private set;
    }

    public PlotterController(CadData cadData)
    {
        Log.plx("in");

        CadData_ = cadData;

        Drawer = new PlotterDrawer(this);

        Input = new PlotterInput(this);

        CommandProcessor = new PlotterCommandProcessor(this);

        EditManager = new PlotterEditManager(this);

        Editor = new PlotterEditor(this);

        HistoryMan = new HistoryManager(this);

        ScriptEnv = new ScriptEnvironment(this);

        ContextMenuMan = new ContextMenuManager(this);

        PlotterTaskRunner = new PlotterTaskRunner(this);

        DB.NewLayer(addLayerList: true, selectCurrent: true);


        StateMachine = new ControllerStateMachine(this, ControllerStateID.SELECT);

        Log.plx("out");
    }

    public void SetViewModel(IPlotterViewModel viewModel)
    {
        ViewModel = viewModel;
        ViewModel.StartUp();
        ViewModel.SetWorldScale(CadData_.WorldScale);
        StartUp();
    }

    public void StartUp()
    {
        if (IsStarted)
        {
            Log.plx("Controller is already started.");
            return;
        }

        Log.plx("in");

        UpdateLayerList();
        UpdateObjectTree(true);

        IsStarted = true;

        Log.plx("out");
    }

    public void ShutDown()
    {
        Log.plx("in");
        DC.Dispose();
        Log.plx("out");
    }

    private void ChangeState(ControllerStateID state)
    {
        StateMachine.ChangeState(state);
    }

    public void StartCreatingFigure(CadFigure.Types type)
    {
        CreatingFigType = type;
        ChangeState(ControllerStateID.CREATE_FIGURE);
    }

    public void EndCreatingFigure()
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
        if (StateID == ControllerStateID.CREATE_FIGURE)
        {
            if (SettingsHolder.Settings.ContinueCreateFigure)
            {
                FigureCreator = null;
                StartCreatingFigure(CreatingFigType);
                UpdateObjectTree(true);
            }
            else
            {
                FigureCreator = null;
                CreatingFigType = CadFigure.Types.NONE;
                ChangeState(ControllerStateID.SELECT);

                UpdateObjectTree(true);
                NotifyStateChange(
                    new StateChangedParam(StateChangedType.CREATING_FIG_TYPE_CHANGED));
            }
        }
    }

    public void StartMeasure(MeasureModes mode)
    {
        ChangeState(ControllerStateID.MEASURING);
        MeasureMode = mode;
        MeasureFigureCreator =
            FigCreator.Get(
                CadFigure.Types.POLY_LINES,
                CadFigure.Create(CadFigure.Types.POLY_LINES)
                );
    }

    public void EndMeasure()
    {
        ChangeState(ControllerStateID.SELECT);
        MeasureMode = MeasureModes.NONE;
        MeasureFigureCreator = null;
    }

    public void Undo()
    {
        Input.ClearSelection();
        HistoryMan.undo();
        UpdateObjectTree(true);
        UpdateLayerList();
    }

    public void Redo()
    {
        Input.ClearSelection();
        HistoryMan.redo();
        UpdateObjectTree(true);
        UpdateLayerList();
    }

    public bool HasSelect()
    {
        foreach (CadLayer layer in DB.LayerList)
        {
            foreach (CadFigure fig in layer.FigureList)
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
        return DB.GetSelectedFigList();
    }

    public List<CadFigure> GetSelectedRootFigureList()
    {
        return DB.GetSelectedRootFigureList();
    }

    public void SetDB(CadObjectDB db, bool clearHistory = true)
    {
        DB = db;

        if (clearHistory)
        {
            HistoryMan.Clear();
        }

        UpdateLayerList();

        UpdateObjectTree(true);
    }

    public void SetPaperPageSize(PaperPageSize paperSize)
    {
        PageSize = paperSize;
    }

    public void SetWorldScale(vcompo_t scale)
    {
        CadData_.WorldScale = scale;
        ViewModel.SetWorldScale(scale);
    }

    public void SetCurrentLayer(uint id)
    {
        if (DB.IsValidLayerID(id))
        {
            DB.CurrentLayerID = id;
            UpdateObjectTree(true);
        }
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

    public void ClearAll()
    {
        PageSize = PaperPageSize.A4Portrate;

        DB.ClearAll();
        HistoryMan.Clear();

        UpdateLayerList();
        UpdateObjectTree(true);
    }

    public void Redraw()
    {
        Drawer.Redraw(DC);
    }

    public void SwapBuffers()
    {
        ViewModel.SwapBuffers();
    }

    public void UpdateObjectTree(bool remakeTree)
    {
        ViewModel.UpdateTreeView(remakeTree);
    }

    public void SetObjectTreePos(int index)
    {
        ViewModel.SetTreeViewPos(index);
    }

    public int FindObjectTreeItem(uint id)
    {
        return ViewModel.FindTreeViewItemIndex(id);
    }

    public void UpdateLayerList()
    {
        LayerListInfo layerListInfo = new(DB.LayerList, CurrentLayer.ID);

        ViewModel.LayerListChanged(layerListInfo);
    }

    public void NotifyStateChange(StateChangedParam param)
    {
        ViewModel.StateChanged(param);
    }

    public void OpenPopupMessage(string text, UITypes.MessageType type)
    {
        ViewModel.OpenPopupMessage(text, type);
    }

    public void ClosePopupMessage()
    {
        ViewModel.ClosePopupMessage();
    }

    public void ShowContextMenu(MenuInfo menuInfo, int x, int y)
    {
        ViewModel.ShowContextMenu(menuInfo, x, y);
    }

    public void UpdateTreeView(bool remakeTree)
    {
        ViewModel.UpdateTreeView(remakeTree);
    }

    public void CursorPosChanged(vector3_t pt, CursorType type)
    {
        ViewModel.CursorPosChanged(pt, type);
    }

    public void ChangeMouseCursor(UITypes.MouseCursorType cursorType)
    {
        ViewModel.ChangeMouseCursor(cursorType);
    }

    public void CursorLocked(bool locked)
    {
        ViewModel.CursorLocked(locked);
    }

    public List<string> HelpOfKey(string keyword)
    {
        return ViewModel.HelpOfKey(keyword);
    }
}
