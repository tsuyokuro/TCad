using Plotter.Controller.TaskRunner;
using System.Collections.Generic;
using System.Drawing;
using TCad.plotter.Scripting;
using TCad.ViewModel;

namespace Plotter.Controller;

public interface IPlotterController
{
    PlotterCommandProcessor CommandProc { get; }
    ContextMenuManager ContextMenuMan { get; }
    CadFigure.Types CreatingFigType { get; set; }
    string CurrentFileName { get; set; }
    CadLayer CurrentLayer { get; set; }
    ControllerState CurrentState { get; }
    CadObjectDB DB { get; }
    DrawContext DC { get; set; }
    PlotterDrawer Drawer { get; }
    PlotterEditManager EditManager { get; }
    PlotterEditor Editor { get; }
    FigCreator FigureCreator { get; set; }
    HistoryManager HistoryMan { get; }
    PlotterInput Input { get; }
    FigCreator MeasureFigureCreator { get; set; }
    MeasureModes MeasureMode { get; set; }
    PaperPageSize PageSize { get; set; }
    PlotterTaskRunner PlotterTaskRunner { get; set; }
    ScriptEnvironment ScriptEnv { get; }
    SelectModes SelectMode { get; set; }
    ControllerStates StateID { get; }
    ControllerStateMachine StateMachine { get; }
    List<CadFigure> TempFigureList { get; }


    void ConnectViewModel(IPlotterViewModel viewModel);
    void Startup();
    void Shutdown();

    void ChangeState(ControllerStates state);
    void ClearAll();
    void CloseFigure();
    void EndCreateFigure();
    void EndMeasure();
    void EvalTextCommand(string s);
    int FindObjectTreeItem(uint id);
    List<CadFigure> GetSelectedFigureList();
    List<CadFigure> GetSelectedRootFigureList();
    bool HasSelect();
    void NextState();
    void NotifyStateChange(StateChangedParam param);
    void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize);
    void Redo();
    void SetCurrentLayer(uint id);
    void SetDB(CadObjectDB db);
    void SetDB(CadObjectDB db, bool clearHistory);
    void SetObjectTreePos(int index);
    void StartCreateFigure(CadFigure.Types type);
    void StartMeasure(MeasureModes mode);
    void Undo();
    void UpdateLayerList();
    void UpdateObjectTree(bool remakeTree);
    void Redraw();

    void OpenPopupMessage(string text, UITypes.MessageType type);
    void ClosePopupMessage();

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);
    void UpdateTreeView(bool remakeTree);

    void CursorPosChanged(vector3_t pt, Plotter.Controller.CursorType type);
    void ChangeMouseCursor(UITypes.MouseCursorType cursorType);
    void CursorLocked(bool locked);

    List<string> HelpOfKey(string keyword);
}
