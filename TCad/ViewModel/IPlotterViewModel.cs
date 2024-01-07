//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using System.Collections.Generic;
using Plotter.Controller;
using TCad.Controls;



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

    void DrawModeChanged(DrawModes mode);


    void StateChanged(StateChangedParam si);

    void LayerListChanged(LayerListInfo layerListInfo);

    void CursorPosChanged(vector3_t pt, Plotter.Controller.CursorType type);

    void UpdateTreeView(bool remakeTree);

    void SetTreeViewPos(int index);

    int FindTreeViewItemIndex(uint id);

    void OpenPopupMessage(string text, UITypes.MessageType messageType);

    void ClosePopupMessage();

    void CursorLocked(bool locked);

    void ChangeMouseCursor(UITypes.MouseCursorType cursorType);

    List<string> HelpOfKey(string keyword);

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);

    void SetWorldScale(vcompo_t scale);

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
    public void CursorPosChanged(vector3_t pt, Plotter.Controller.CursorType type) { }
    public void DrawModeChanged(DrawModes mode) { }

    public void ExecCommand(string cmd) { }
    public int FindTreeViewItemIndex(uint id) { return -1; }
    public List<string> HelpOfKey(string keyword) { return EmptyList; }
    public void LayerListChanged(LayerListInfo layerListInfo) { }
    public void OpenPopupMessage(string text, UITypes.MessageType messageType) { }
    public void Redraw() { }
    public void SetTreeViewPos(int index) { }
    public void SetWorldScale(vcompo_t scale) { }
    public void ShowContextMenu(MenuInfo menuInfo, int x, int y) { }
    public void StateChanged(StateChangedParam si) { }
    public void UpdateTreeView(bool remakeTree) { }
}
