//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Imaging;



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

        mProjectionMatrix = matrix4_t.Identity;
        mProjectionMatrixInv = matrix4_t.Identity;

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
