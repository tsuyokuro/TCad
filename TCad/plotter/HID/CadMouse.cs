using System.Windows.Forms;
using TCad.Logger;
using TCad.Plotter.DrawContexts;
using Windows.AI.MachineLearning;

namespace TCad.Plotter;

public class CadMouse
{
    public delegate void ButtonHandler(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y);
    public delegate void MoveHandler(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y);
    public delegate void WheelHandler(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y, int delta);


    public ButtonHandler LButtonDown_ = DummyLButtonDown;
    public ButtonHandler LButtonDown
    {
        get => LButtonDown_;
        set => LButtonDown_ = value ?? DummyLButtonDown;
    }

    public ButtonHandler LButtonUp_ = DummyLButtonUp;
    public ButtonHandler LButtonUp
    {
        get => LButtonUp_;
        set => LButtonUp_ = value ?? DummyLButtonUp;
    }

    public ButtonHandler RButtonDown_ = DummyRButtonDown;
    public ButtonHandler RButtonDown {
        get => RButtonDown_;
        set => RButtonDown_ = value ?? DummyRButtonDown;
    }   

    public ButtonHandler RButtonUp_ = DummyRButtonUp;
    public ButtonHandler RButtonUp { 
        get => RButtonUp_;
        set => RButtonUp_ = value ?? DummyRButtonUp;
    }

    public ButtonHandler MButtonDown_ = DummyMButtonDown;
    public ButtonHandler MButtonDown {
        get => MButtonDown_;
        set => MButtonDown_ = value ?? DummyMButtonDown;
    }

    public ButtonHandler MButtonUp_ = DummyMButtonUp;
    public ButtonHandler MButtonUp {
        get => MButtonUp_;
        set => MButtonUp_ = value ?? DummyMButtonUp;
    }

    public WheelHandler Wheel_ = DummyWheel;
    public WheelHandler Wheel {
        get => Wheel_;
        set => Wheel_ = value ?? DummyWheel;
    }

    public MoveHandler PointerMove_ = DummyPointerMove;
    public MoveHandler PointerMove
    {
        get => PointerMove_;
        set => PointerMove_ = value ?? DummyPointerMove;
    }

    public void MouseMove(DrawContext dc, vcompo_t x, vcompo_t y)
    {
        PointerMove(this, dc, x, y);
    }

    public void MouseDown(DrawContext dc, MouseButtons btn, vcompo_t x, vcompo_t y)
    {
        //Log.pl($"MouseDown btn={btn:X}");

        if ((btn & MouseButtons.Left) != 0)
        {
            LButtonDown(this, dc, x, y);
        }

        if ((btn & MouseButtons.Right) != 0)
        {
            RButtonDown(this, dc, x, y);
        }

        if ((btn & MouseButtons.Middle) != 0)
        {
            MButtonDown(this, dc, x, y);
        }
    }

    public void MouseUp(DrawContext dc, MouseButtons btn, vcompo_t x, vcompo_t y)
    {
        //Log.pl($"MouseUp btn={btn:X}");

        if ((btn & MouseButtons.Left) != 0)
        {
            LButtonUp(this, dc, x, y);
        }

        if ((btn & MouseButtons.Right) != 0)
        {
            RButtonUp(this, dc, x, y);
        }

        if ((btn & MouseButtons.Middle) != 0)
        {
            MButtonUp(this, dc, x, y);
        }
    }

    public void MouseWheel(DrawContext dc, vcompo_t x, vcompo_t y, int delta)
    {
        Wheel(this, dc, x, y, delta);
    }

    private static void DummyLButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyLButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyRButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyRButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyMButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyMButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyPointerMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y) { }
    private static void DummyWheel(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y, int delta) { }
}
