using Plotter.Controller;
using System;
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

    void CursorLocked(bool locked);

    void ChangeMouseCursor(UITypes.MouseCursorType cursorType);

    void SetWorldScale(double scale);

    void DrawModeUpdated(DrawModes mode);

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);
}

public interface IPlotterViewForDC
{
    void GLMakeCurrent();
    void PushToFront(DrawContext dc);
}
