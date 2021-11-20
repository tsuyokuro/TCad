using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public class CadMouse
    {
        public delegate void ButtonHandler(CadMouse pointer, DrawContext dc, double x, double y);
        public delegate void MoveHandler(CadMouse pointer, DrawContext dc, double x, double y);
        public delegate void WheelHandler(CadMouse pointer, DrawContext dc, double x, double y, int delta);


        public ButtonHandler LButtonDown;
        public ButtonHandler LButtonUp;
        public ButtonHandler RButtonDown;
        public ButtonHandler RButtonUp;
        public ButtonHandler MButtonDown;
        public ButtonHandler MButtonUp;

        public WheelHandler Wheel;
        public MoveHandler PointerMoved;

        public Vector3d LDownPoint = default;
        public Vector3d RDownPoint = default;
        public Vector3d MDownPoint = default;

        public void MouseMove(DrawContext dc, double x, double y)
        {
            if (PointerMoved != null)
            {
                PointerMoved?.Invoke(this, dc, x, y);
            }
        }

        public void MouseDown(DrawContext dc, MouseButtons btn, double x, double y)
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

        public void MouseUp(DrawContext dc, MouseButtons btn, double x, double y)
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

        public void MouseWheel(DrawContext dc, double x, double y, int delta)
        {
            if (Wheel != null)
            {
                Wheel(this, dc, x, y, delta);
            }
        }
    }
}
