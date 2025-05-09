using TCad.Plotter;
using System.Drawing;
using System.Windows.Forms;
using TCad.Plotter.Drawing;
using TCad.Plotter.DrawToolSet;

namespace TCad.Plotter.DrawContexts;

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

    public override vcompo_t UnitPerMilli
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

    public override void SetViewSize(vcompo_t w, vcompo_t h)
    {
        mViewWidth = w;
        mViewHeight = h;

        if (w == 0 || h == 0)
        {
            return;
        }

        DeviceScaleX = w / (vcompo_t)(2.0);
        DeviceScaleY = -h / (vcompo_t)(2.0);

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
        mProjectionMatrix = matrix4_t.CreateOrthographic(
                                        ViewWidth / mUnitPerMilli,
                                        ViewHeight / mUnitPerMilli,
                                        mProjectionNear,
                                        mProjectionFar
                                        );

        mProjectionMatrixInv = mProjectionMatrix.Inv();
    }

    public Pen Pen(int id)
    {
        DrawPen pen = Tools.Pen(id);
        return pen.GdiPen;
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
        dc.CopyCamera(this);
        dc.SetViewSize(ViewWidth, ViewHeight);

        dc.SetViewOrg(ViewOrg);

        return dc;
    }

    virtual protected void SetupDrawing()
    {
        mDrawing = new DrawingGDI(this);
    }

    public override void EnableLight()
    {
        // NOP
    }

    public override void DisableLight()
    {
        // NOP
    }
}
