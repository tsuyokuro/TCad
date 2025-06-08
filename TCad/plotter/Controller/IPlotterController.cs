using System.Collections.Generic;
using System.Drawing;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Scripting;
using TCad.Plotter.undo;
using TCad.ViewModel;

namespace TCad.Plotter.Controller;

public interface IPlotterController
{
    IPlotterViewModel ViewModel { get; }
    PlotterCommandProcessor CommandProcessor { get; }
    ContextMenuManager ContextMenuMan { get; }
    CadFigure.Types CreatingFigType { get; set; }
    string CurrentFileName { get; set; }
    CadLayer CurrentLayer { get; set; }
    ControllerState CurrentState { get; }

    CadObjectDB DB { get; }
    PaperPageSize PageSize { get; }
    vcompo_t WorldScale { get; }
   
    DrawContext DC { get; set; }
    PlotterDrawer Drawer { get; }
    PlotterEditManager EditManager { get; }
    PlotterEditor Editor { get; }
    FigCreator FigureCreator { get; set; }
    HistoryManager HistoryMan { get; }
    PlotterInput Input { get; }
    FigCreator MeasureFigureCreator { get; set; }
    MeasureModes MeasureMode { get; set; }
    PlotterTaskRunner PlotterTaskRunner { get; set; }
    ScriptEnvironment ScriptEnv { get; }
    SelectModes SelectMode { get; set; }
    ControllerStateID StateID { get; }
    ControllerStateMachine StateMachine { get; }
    List<CadFigure> TempFigureList { get; }


    void ConnectViewModel(IPlotterViewModel viewModel);

    void StartUp();
    void ShutDown();

    void ClearAll();

    void CloseFigure();

    void EvalTextCommand(string s);

    List<CadFigure> GetSelectedFigureList();
    List<CadFigure> GetSelectedRootFigureList();

    bool HasSelect();

    void NextState();

    void NotifyStateChange(StateChangedParam param);

    void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize);

    void SetCurrentLayer(uint id);


    void SetDB(CadObjectDB db, bool clearHistory = true);

    void SetWorldScale(vcompo_t scale);

    void SetPaperPageSize(PaperPageSize size);


    void SetObjectTreePos(int index);

    void StartCreatingFigure(CadFigure.Types type);
    void EndCreatingFigure();

    void StartMeasure(MeasureModes mode);
    void EndMeasure();

    void Undo();
    void Redo();

    void UpdateLayerList();
    void UpdateObjectTree(bool remakeTree);
    void UpdateTreeView(bool remakeTree);
    int FindObjectTreeItem(uint id);

    void Redraw();

    void OpenPopupMessage(string text, UITypes.MessageType type);
    void ClosePopupMessage();

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);

    void CursorPosChanged(vector3_t pt, CursorType type);

    void ChangeMouseCursor(UITypes.MouseCursorType cursorType);
    void CursorLocked(bool locked);

    List<string> HelpOfKey(string keyword);
}
