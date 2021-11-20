using System.Drawing;
using System.Drawing.Imaging;

namespace Plotter
{
    public class DrawContextGDIBmp : DrawContextGDI
    {
        private BitmapData LockedBitmapData = null;

        private Bitmap mImage = null;
        public Bitmap Image
        {
            get => mImage;
        }

        public DrawContextGDIBmp()
        {
            Init();
        }

        private void Init()
        {
            SetViewSize(8, 1);  // Create dummy Image and Graphics

            mUnitPerMilli = 4; // 4 pix = 1mm
            mViewOrg.X = 0;
            mViewOrg.Y = 0;

            mProjectionMatrix = UMatrix4.Unit;
            mProjectionMatrixInv = UMatrix4.Unit;

            CalcProjectionMatrix();
            CalcProjectionZW();

            SetupDrawing();
        }

        protected override void DisposeGraphics()
        {
            if (mGdiGraphics != null)
            {
                mGdiGraphics.Dispose();
                mGdiGraphics = null;
            }

            if (mImage != null)
            {
                mImage.Dispose();
                mImage = null;
            }
        }

        protected override void CreateGraphics()
        {
            mImage = new Bitmap((int)mViewWidth, (int)mViewHeight);
            mGdiGraphics = Graphics.FromImage(mImage);
        }

        public BitmapData GetLockedBits()
        {
            return LockedBitmapData;
        }

        public BitmapData LockBits()
        {
            if (mImage == null)
            {
                return null;
            }

            if (LockedBitmapData != null)
            {
                return LockedBitmapData;
            }

            Rectangle r = new Rectangle(0, 0, mImage.Width, mImage.Height);

            LockedBitmapData = mImage.LockBits(
                    r,
                    ImageLockMode.ReadWrite, mImage.PixelFormat);

            return LockedBitmapData;
        }

        public void UnlockBits()
        {
            if (mImage == null)
            {
                return;
            }

            if (LockedBitmapData == null)
            {
                return;
            }

            mImage.UnlockBits(LockedBitmapData);
            LockedBitmapData = null;
        }

        public override void Dispose()
        {
            DisposeGraphics();
            Tools.Dispose();
        }

        public override DrawContext Clone()
        {
            DrawContextGDIBmp dc = new DrawContextGDIBmp();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(ViewWidth, ViewHeight);

            dc.SetViewOrg(ViewOrg);

            return dc;
        }

        protected override void SetupDrawing()
        {
            mDrawing = new DrawingGDIBmp(this);
        }
    }
}
