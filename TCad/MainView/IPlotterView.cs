using Plotter.Controller;
using TCad.Plotter.DrawContexts;
using TCad.ViewModel;

namespace Plotter;

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
}

public interface IPlotterViewForDC
{
    void GLMakeCurrent();
    void SwapBuffers(DrawContext dc);
}
