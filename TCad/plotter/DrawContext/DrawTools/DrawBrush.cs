
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter
{
    public struct DrawBrush
    {
        public Color4 mColor4;

        private SolidBrush mGdiBrush;
        public SolidBrush GdiBrush
        {
            get => mGdiBrush;
        }

        public static DrawBrush NullBrush = new DrawBrush(Color.FromArgb(0, 0, 0, 0));

        public bool IsNullBrush
        {
            get => mColor4.A == 0;
        }

        public void Dispose()
        {
            if (mGdiBrush != null)
            {
                mGdiBrush.Dispose();
            }
        }

        public Color4 Color4()
        {
            return mColor4;
        }

        public DrawBrush(SolidBrush brush)
        {
            mGdiBrush = brush;
            mColor4 = Color4Util.FromArgb(brush.Color.ToArgb());
        }

        public DrawBrush(Color color)
        {
            mGdiBrush = null;
            mColor4 = Color4Util.FromArgb(color.ToArgb());
        }

        public DrawBrush(Color4 color)
        {
            mGdiBrush = null;
            mColor4 = color;
        }
    }
}
