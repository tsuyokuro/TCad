//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Windows.Forms;



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

class DrawContextGLOrtho : DrawContextGL
{
    CadVertex Center = default;

    public override vcompo_t UnitPerMilli
    {
        set
        {
            mUnitPerMilli = value;
            CalcProjectionMatrix();
        }

        get => mUnitPerMilli;
    }

    public DrawContextGLOrtho()
    {
        Init(null);
        mUnitPerMilli = 4;
    }

    public DrawContextGLOrtho(Control control)
    {
        Init(control);
        mUnitPerMilli = 4;
    }

    public override void Active()
    {
        CalcProjectionMatrix();
    }

    public override void StartDraw()
    {
        GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        #region ModelView
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(ref mViewMatrix);
        #endregion

        #region Projection            
        GL.MatrixMode(MatrixMode.Projection);

        matrix4_t proj = mProjectionMatrix;

        vcompo_t dx = ViewOrg.X - (ViewWidth / (vcompo_t)(2.0));
        vcompo_t dy = ViewOrg.Y - (ViewHeight / (vcompo_t)(2.0));

        // x,yの平行移動成分を設定
        // Set x and y translational components
        proj.M41 = dx / (ViewWidth / (vcompo_t)(2.0));
        proj.M42 = -dy / (ViewHeight / (vcompo_t)(2.0));

        GL.LoadMatrix(ref proj);
        #endregion

        SetupLight();
    }

    public override void SetViewSize(vcompo_t w, vcompo_t h)
    {
        mViewWidth = w;
        mViewHeight = h;

        DeviceScaleX = w / (vcompo_t)(2.0);
        DeviceScaleY = -h / (vcompo_t)(2.0);

        mViewCenter.X = w / (vcompo_t)(2.0);
        mViewCenter.Y = h / (vcompo_t)(2.0);

        GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

        CalcProjectionMatrix();
        CalcProjectionZW();

        Center.X = w / 2;
        Center.Y = h / 2;

        Matrix2D = matrix4_t.CreateOrthographicOffCenter(
                                    0, mViewWidth,
                                    mViewHeight, 0,
                                    0, mProjectionFar);
    }

    public override void CalcProjectionMatrix()
    {
        mProjectionMatrix = matrix4_t.CreateOrthographic(
                                        ViewWidth / mUnitPerMilli, ViewHeight / mUnitPerMilli,
                                        mProjectionNear,
                                        mProjectionFar
                                        );

        mProjectionMatrixInv = mProjectionMatrix.Inv();
    }

    public override DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
    {
        DrawContextGLOrtho dc = new DrawContextGLOrtho();

        dc.CopyProjectionMetrics(this);
        dc.CopyCamera(this);
        dc.SetViewSize(deviceSize.Width, deviceSize.Height);

        vector3_t org = default;
        org.X = deviceSize.Width / (vcompo_t)(2.0);
        org.Y = deviceSize.Height / (vcompo_t)(2.0);

        dc.SetViewOrg(org);

        dc.UnitPerMilli = deviceSize.Width / pageSize.Width;

        return dc;
    }

    public override DrawContext Clone()
    {
        DrawContextGLOrtho dc = new DrawContextGLOrtho();

        dc.CopyProjectionMetrics(this);
        dc.CopyCamera(this);
        dc.SetViewSize(ViewWidth, ViewHeight);

        dc.SetViewOrg(ViewOrg);

        dc.UnitPerMilli = UnitPerMilli;

        return dc;
    }
}
