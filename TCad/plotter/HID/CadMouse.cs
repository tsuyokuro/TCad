//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
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

public class CadMouse
{
    public delegate void ButtonHandler(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y);
    public delegate void MoveHandler(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y);
    public delegate void WheelHandler(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y, int delta);


    public ButtonHandler LButtonDown;
    public ButtonHandler LButtonUp;
    public ButtonHandler RButtonDown;
    public ButtonHandler RButtonUp;
    public ButtonHandler MButtonDown;
    public ButtonHandler MButtonUp;

    public WheelHandler Wheel;
    public MoveHandler PointerMoved;

    public vector3_t LDownPoint = default;
    public vector3_t RDownPoint = default;
    public vector3_t MDownPoint = default;

    public void MouseMove(DrawContext dc, vcompo_t x, vcompo_t y)
    {
        if (PointerMoved != null)
        {
            PointerMoved?.Invoke(this, dc, x, y);
        }
    }

    public void MouseDown(DrawContext dc, MouseButtons btn, vcompo_t x, vcompo_t y)
    {
        if (btn == MouseButtons.Left)
        {
            LDownPoint.X = x;
            LDownPoint.Y = y;

            if (LButtonDown != null) LButtonDown(this, dc, x, y);
        }
        else if (btn == MouseButtons.Right)
        {
            RDownPoint.X = x;
            RDownPoint.Y = y;

            if (LButtonDown != null) RButtonDown(this, dc, x, y);
        }
        else if (btn == MouseButtons.Middle)
        {
            MDownPoint.X = x;
            MDownPoint.Y = y;

            if (MButtonDown != null) MButtonDown(this, dc, x, y);
        }
    }

    public void MouseUp(DrawContext dc, MouseButtons btn, vcompo_t x, vcompo_t y)
    {
        if (btn == MouseButtons.Left)
        {
            if (LButtonUp != null) LButtonUp(this, dc, x, y);
        }
        else if (btn == MouseButtons.Right)
        {
            if (LButtonUp != null) RButtonUp(this, dc, x, y);
        }
        else if (btn == MouseButtons.Middle)
        {
            if (MButtonUp != null) MButtonUp(this, dc, x, y);
        }
    }

    public void MouseWheel(DrawContext dc, vcompo_t x, vcompo_t y, int delta)
    {
        if (Wheel != null)
        {
            Wheel(this, dc, x, y, delta);
        }
    }
}
