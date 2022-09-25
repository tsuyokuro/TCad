
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter
{
    public struct DrawBrush
    {
        public int Argb;

        public Color4 mColor4;

        public SolidBrush mGdiBrush;
        public SolidBrush GdiBrush
        {
            get => mGdiBrush;
        }

        public static DrawBrush NullBrush = new DrawBrush(Color.FromArgb(0, 0, 0, 0));

        public bool IsNullBrush
        {
            get => ((uint)Argb & 0xff000000) == 0;
        }

        public void Dispose()
        {
            if (mGdiBrush != null)
            {
                mGdiBrush.Dispose();
                mGdiBrush = null;
            }
        }

        public Color4 Color4()
        {
            return mColor4;
        }

        public Color GdiColor()
        {
            return Color.FromArgb(Argb);
        }

        public DrawBrush(SolidBrush brush)
        {
            mGdiBrush = brush;
            Argb = brush.Color.ToArgb();
            mColor4 = Color4Util.FromArgb(Argb);
        }

        public DrawBrush(Color color)
        {
            mGdiBrush = null;
            Argb = color.ToArgb();
            mColor4 = Color4Util.FromArgb(Argb);
        }

        public DrawBrush(Color4 color)
        {
            mGdiBrush = null;
            Argb = color.ToArgb();
            mColor4 = color;
        }
    }
}
