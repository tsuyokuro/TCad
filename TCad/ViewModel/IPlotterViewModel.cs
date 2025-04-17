using Plotter;
using Plotter.Controller;
using System.Collections.Generic;
using System.Windows.Input;
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
    void Startup();

    void Shutdown();


    string CurrentFileName
    {
        get;
        set;
    }

    IPlotterController Controller
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

    ICadObjectTree ObjectTree
    {
        get;
        set;
    }

    IAutoCompleteTextBox CommandTextBox
    {
        get;
    }

    LayerListViewModel LayerListVM { get; }

    CursorPosViewModel CursorPosVM { get; }
    DrawContext DC { get; }

    void Redraw();


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

    void AttachCommandView(IAutoCompleteTextBox textBox);
    bool OnKeyDown(object sender, KeyEventArgs e);
    bool OnKeyUp(object sender, KeyEventArgs e);
}

public class DummyPlotterViewModel : IPlotterViewModel
{
    public string CurrentFileName { get; set; }

    public IPlotterController Controller { get; }

    public ICadMainWindow MainWindow { get; }

    public SettingsVeiwModel Settings { get; }

    public ViewManager ViewManager { get; }

    public ICadObjectTree ObjectTree { get; set; }

    public IAutoCompleteTextBox CommandTextBox { get; }

    public LayerListViewModel LayerListVM { get; }

    public CursorPosViewModel CursorPosVM { get; }

    public DrawContext DC { get; }

    public void AttachCommandView(IAutoCompleteTextBox textBox)
    {
    }

    public void ChangeMouseCursor(UITypes.MouseCursorType cursorType)
    {
    }

    public void Shutdown()
    {
    }

    public void ClosePopupMessage()
    {
    }

    public void CursorLocked(bool locked)
    {
    }

    public void CursorPosChanged(vector3_t pt, Plotter.Controller.CursorType type)
    {
    }

    public void ExecCommand(string cmd)
    {
    }

    public int FindTreeViewItemIndex(uint id)
    {
        return 0;
    }

    public List<string> HelpOfKey(string keyword)
    {
        return new();
    }

    public void LayerListChanged(LayerListInfo layerListInfo)
    {
    }

    public bool OnKeyDown(object sender, KeyEventArgs e)
    {
        return false;
    }

    public bool OnKeyUp(object sender, KeyEventArgs e)
    {
        return false;
    }

    public void Startup()
    {
    }

    public void OpenPopupMessage(string text, UITypes.MessageType messageType)
    {
    }

    public void Redraw()
    {
    }

    public void SetTreeViewPos(int index)
    {
    }

    public void SetWorldScale(float scale)
    {
    }

    public void ShowContextMenu(MenuInfo menuInfo, int x, int y)
    {
    }

    public void StateChanged(StateChangedParam si)
    {
    }

    public void UpdateTreeView(bool remakeTree)
    {
    }
}

