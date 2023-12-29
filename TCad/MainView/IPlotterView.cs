//#define DEFAULT_DATA_TYPE_DOUBLE
using Plotter.Controller;
using TCad.ViewModel;



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

    void DrawModeUpdated(DrawModes mode);

    void ShowContextMenu(MenuInfo menuInfo, int x, int y);
}

public interface IPlotterViewForDC
{
    void GLMakeCurrent();
    void SwapBuffers(DrawContext dc);
}
