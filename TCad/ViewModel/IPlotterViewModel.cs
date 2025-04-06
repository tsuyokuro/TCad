using OpenTK.Mathematics;
using System.Collections.Generic;
using Plotter.Controller;
using TCad.Controls;
using Plotter;
using System.Windows.Input;

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
    void Open();

    void Close();


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

    void AttachCommandView(IAutoCompleteTextBox textBox);
    bool OnKeyDown(object sender, KeyEventArgs e);
    bool OnKeyUp(object sender, KeyEventArgs e);
}
