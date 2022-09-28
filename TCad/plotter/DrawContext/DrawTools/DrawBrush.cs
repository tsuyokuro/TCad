
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter
{
    public struct DrawBrush
    {
        public Color4 mColor4;

        public SolidBrush GdiBrush
        {
            get
            {
                return new SolidBrush(Color4Util.ToGDIColor(mColor4));
            }
        }

        public static DrawBrush NullBrush = new DrawBrush(Color.FromArgb(0, 0, 0, 0));

        public bool IsNullBrush
        {
            get => mColor4.A == 0;
        }

        public void Dispose()
        {
        }

        public Color4 Color4()
        {
            return mColor4;
        }

        public DrawBrush(SolidBrush brush)
        {
            mColor4 = Color4Util.FromArgb(brush.Color.ToArgb());
        }

        public DrawBrush(int argb)
        {
            mColor4 = Color4Util.FromArgb(argb);
        }

        public DrawBrush(Color4 color)
        {
            mColor4 = color;
        }
    }
}
