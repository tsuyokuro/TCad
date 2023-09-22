//#define DEFAULT_DATA_TYPE_DOUBLE
using System.Windows.Forms;



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

class CadKeyboard
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    public static bool IsKeyPressed(Keys keyCode)
    {
        return GetKeyState((int)keyCode) < 0;
    }

    public static bool IsCtrlKeyDown()
    {
        bool ret = IsKeyPressed(Keys.ControlKey);
        return ret;
    }

    public static bool IsShiftKeyDown()
    {
        bool ret = IsKeyPressed(Keys.ShiftKey);
        return ret;
    }
}
