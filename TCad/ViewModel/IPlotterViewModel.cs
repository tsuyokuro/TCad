using OpenTK.Mathematics;
using System.Collections.Generic;
using Plotter.Controller;
using Plotter;
using TCad.Controls;

namespace TCad.ViewModel;

public interface UITypes
{
    public enum MessageType
    {
        INFO,
        INPUT,
        ERROR,
    }

    public enum MouseCursorType
    {
        NORMAL_ARROW,
        CROSS,
        HAND,
    }
}

public interface IPlotterViewModel
{
    string CurrentFileName
    {
        get;
        set;
    }

    PlotterController Controller
    {
        get;
    }

    ICadMainWindow MainWindow
    {
        get;
    }

    SettingsVeiwModel Settings
    {
        get;
    }

    ViewManager ViewManager
    {
        get;
    }

    AutoCompleteTextBox CommandTextBox
    {
        get;
    }

    void Redraw();

    void DrawModeUpdated(DrawModes mode);


    void StateChanged(StateChangedParam si);

    void LayerListChanged(LayerListInfo layerListInfo);

    void CursorPosChanged(Vector3d pt, Plotter.Controller.CursorType type);

    void UpdateTreeView(bool remakeTree);

    void SetTreeViewPos(int index);

    int FindTreeViewItemIndex(uint id);

    void OpenPopupMessage(string text, UITypes.MessageType messageType);

    void ClosePopupMessage();

    void CursorLocked(bool locked);

    void ChangeMouseCursor(UITypes.MouseCursorType cursorType);

    List<string> HelpOfKey(string keyword);

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);

    void SetWorldScale(double scale);

    void ExecCommand(string cmd);

    public static readonly IPlotterViewModel Dummy = new DummyPlotterViewModel();
}


public class DummyPlotterViewModel : IPlotterViewModel
{
    private static List<string> EmptyList = new List<string>();

    public string CurrentFileName { get => null; set { } }

    public PlotterController Controller
    {
        get => null;
    }

    public ICadMainWindow MainWindow => throw new System.NotImplementedException();

    public ViewManager ViewManager => throw new System.NotImplementedException();

    public SettingsVeiwModel Settings => throw new System.NotImplementedException();

    public AutoCompleteTextBox CommandTextBox => throw new System.NotImplementedException();

    public void ChangeMouseCursor(UITypes.MouseCursorType cursorType) { }
    public void ClosePopupMessage() { }
    public void CursorLocked(bool locked) { }
    public void CursorPosChanged(Vector3d pt, Plotter.Controller.CursorType type) { }
    public void DrawModeUpdated(DrawModes mode) { }

    public void ExecCommand(string cmd) { }
    public int FindTreeViewItemIndex(uint id) { return -1; }
    public List<string> HelpOfKey(string keyword) { return EmptyList; }
    public void LayerListChanged(LayerListInfo layerListInfo) { }
    public void OpenPopupMessage(string text, UITypes.MessageType messageType) { }
    public void Redraw() { }
    public void SetTreeViewPos(int index) { }
    public void SetWorldScale(double scale) { }
    public void ShowContextMenu(MenuInfo menuInfo, int x, int y) { }
    public void StateChanged(StateChangedParam si) { }
    public void UpdateTreeView(bool remakeTree) { }
}
