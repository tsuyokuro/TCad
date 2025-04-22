using Plotter.Controller.TaskRunner;
using Plotter.Scripting;
using Plotter.Settings;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using TCad.ViewModel;

namespace Plotter.Controller;

public class PlotterController : IPlotterController
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


    private IPlotterViewModel ViewModel
    {
        get;
        set;
    } = null;

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

    public ControllerStates StateID
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

    public PlotterCommandProcessor CommandProc
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

    public PlotterController()
    {
        Log.plx("in");

        Drawer = new PlotterDrawer(this);

        Input = new PlotterInput(this);

        CommandProc = new PlotterCommandProcessor(this);

        EditManager = new PlotterEditManager(this);

        Editor = new PlotterEditor(this);

        StateMachine = new ControllerStateMachine(this);
        ChangeState(ControllerStates.SELECT);

        HistoryMan = new HistoryManager(this);

        ScriptEnv = new ScriptEnvironment(this);

        ContextMenuMan = new ContextMenuManager(this);

        PlotterTaskRunner = new PlotterTaskRunner(this);

        var layer = DB.NewLayer();
        DB.LayerList.Add(layer);
        DB.CurrentLayer = layer;

        Log.plx("out");
    }

    public void ConnectViewModel(IPlotterViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public void Startup()
    {
        Log.plx("in");

        UpdateLayerList();
        UpdateObjectTree(true);

        Log.plx("out");
    }

    public void Shutdown()
    {
        Log.plx("in");
        DC.Dispose();
        Log.plx("out");
    }

    public void ChangeState(ControllerStates state)
    {
        StateMachine.ChangeState(state);
    }

    #region ObjectTree handling
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
    #endregion ObjectTree handling


    public void UpdateLayerList()
    {
        ViewModel.LayerListChanged(GetLayerListInfo());
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
        ViewModel.StateChanged(param);
    }

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

    public void SetDB(CadObjectDB db, bool clearHistory)
    {
        DB = db;

        if (clearHistory)
        {
            HistoryMan.Clear();
        }

        UpdateLayerList();

        UpdateObjectTree(true);
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

    public void ClearAll()
    {
        PageSize = new PaperPageSize();

        DB.ClearAll();
        HistoryMan.Clear();

        UpdateLayerList();
        UpdateObjectTree(true);
    }

    public void Redraw()
    {
        Drawer.Redraw(DC);
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

    public void CursorPosChanged(vector3_t pt, Plotter.Controller.CursorType type)
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
