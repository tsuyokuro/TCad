using TCad.Plotter.Controller;
using TCad.Plotter.DrawContexts;
using TCad.ViewModel;

namespace TCad.MainView;

public interface IPlotterView
{
    DrawContext DrawContext
    {
        get;
    }

    System.Windows.Forms.Control FormsControl
    {
        get;
    }

    void EnablePerse(bool enable);

    void CursorLocked(bool locked);

    void ChangeMouseCursor(UITypes.MouseCursorType cursorType);

    void SetWorldScale(vcompo_t scale);

    void DrawModeChanged(DrawModes mode);

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);

    void SwapBuffers();

    void MakeCurrent();
}
