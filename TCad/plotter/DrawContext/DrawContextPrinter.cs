using System.Drawing;
using System.Drawing.Drawing2D;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    class DrawContextPrinter : DrawContextGDI
    {
        public DrawContextPrinter(DrawContext currentDC, Graphics g, CadSize2D pageSize, CadSize2D deviceSize)
        {
            GdiGraphics = g;

            GdiGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            GdiGraphics.SmoothingMode = SmoothingMode.HighQuality;

            if (currentDC.GetType() == typeof(DrawContextGLPers))
            {
                WorldScale = currentDC.WorldScale;
                mUnitPerMilli = deviceSize.Width / pageSize.Width;
                CopyCamera(currentDC);
                CopyProjectionMatrix(currentDC);

                DeviceScaleX = currentDC.ViewWidth / 4;
                DeviceScaleY = -(currentDC.ViewHeight / 4);
            }
            else
            {
                CopyProjectionMetrics(currentDC);
                CopyCamera(currentDC);
                WorldScale = currentDC.WorldScale;
                UnitPerMilli = deviceSize.Width / pageSize.Width;
                SetViewSize(deviceSize.Width, deviceSize.Height);
            }

            Vector3d org = default;

            org.X = deviceSize.Width / 2.0;
            org.Y = deviceSize.Height / 2.0;
            
            SetViewOrg(org);

            SetupDrawing();
        }

        public DrawContextPrinter()
        {
        }

        protected override void DisposeGraphics()
        {
            // NOP
        }

        protected override void CreateGraphics()
        {
            // NOP
        }

        public override DrawContext Clone()
        {
            DrawContextPrinter dc = new DrawContextPrinter();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(ViewWidth, ViewHeight);

            dc.SetViewOrg(ViewOrg);

            return dc;
        }
    }
}
