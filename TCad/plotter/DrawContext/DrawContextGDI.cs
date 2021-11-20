using OpenTK;
using System.Drawing;
using System.Windows.Forms;

namespace Plotter
{
    public class DrawContextGDI : DrawContext
    {
        protected Control ViewCtrl;

        BufferedGraphics Buffer;

        protected Graphics mGdiGraphics = null;
        public Graphics GdiGraphics
        {
            protected set => mGdiGraphics = value;
            get => mGdiGraphics;
        }

        public override double UnitPerMilli
        {
            set
            {
                mUnitPerMilli = value;
                CalcProjectionMatrix();
            }

            get => mUnitPerMilli;
        }

        public DrawContextGDI()
        {
        }

        public DrawContextGDI(Control formsControl)
        {
            Init(formsControl);
        }

        private void Init(Control formsControl)
        {
            ViewCtrl = formsControl;

            SetViewSize(1, 1);  // Create dummy Graphics

            mUnitPerMilli = 4; // 4 pix = 1mm
            mViewOrg.X = 0;
            mViewOrg.Y = 0;

            CalcProjectionMatrix();
            CalcProjectionZW();

            SetupDrawing();
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            if (w == 0 || h == 0)
            {
                return;
            }

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            CalcProjectionMatrix();
            CalcProjectionZW();

            DisposeGraphics();
            CreateGraphics();
        }

        protected virtual void DisposeGraphics()
        {
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }

        protected virtual void CreateGraphics()
        {
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;

            Buffer = currentContext.Allocate(
                ViewCtrl.CreateGraphics(),
                ViewCtrl.DisplayRectangle
                );

            mGdiGraphics = Buffer.Graphics;
        }

        public override void Dispose()
        {
            DisposeGraphics();

            if (Tools != null)
            {
                Tools.Dispose();
            }

            if (mDrawing != null)
            {
                mDrawing.Dispose();
            }
        }

        public override void CalcProjectionMatrix()
        {
            mProjectionMatrix = Matrix4d.CreateOrthographic(
                                            ViewWidth / mUnitPerMilli,
                                            ViewHeight / mUnitPerMilli,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }

        public Pen Pen(int id)
        {
            DrawPen pen = Tools.Pen(id);
            return pen.GdiPen;
        }

        public Color PenColor(int id)
        {
            return Tools.PenColorTbl[id];
        }

        public Font Font(int id)
        {
            return Tools.font(id);
        }

        public Brush Brush(int id)
        {
            DrawBrush brush = Tools.Brush(id);
            return brush.GdiBrush;
        }

        public Color BrushColor(int id)
        {
            return Tools.BrushColorTbl[id];
        }

        public void Render()
        {
            if (Buffer != null)
            {
                // Bufferの内容を既定のデバイスに書き込む
                // Push buffered image to device
                Buffer.Render();
            }
        }

        public override DrawPen GetPen(int idx)
        {
            return Tools.Pen(idx);
        }

        public override DrawBrush GetBrush(int idx)
        {
            return Tools.Brush(idx);
        }

        public override DrawContext Clone()
        {
            DrawContextGDI dc = new DrawContextGDI();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(ViewWidth, ViewHeight);

            dc.SetViewOrg(ViewOrg);

            return dc;
        }

        virtual protected void SetupDrawing()
        {
            mDrawing = new DrawingGDI(this);
        }
    }
}
