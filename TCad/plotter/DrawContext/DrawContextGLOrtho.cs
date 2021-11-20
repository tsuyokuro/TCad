using OpenTK;
using OpenTK.Graphics.OpenGL;
using CadDataTypes;
using System.Windows.Forms;

namespace Plotter
{
    class DrawContextGLOrtho : DrawContextGL
    {
        CadVertex Center = default;

        public override double UnitPerMilli
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
            GL.LoadMatrix(ref mViewMatrix.Matrix);
            #endregion

            #region Projection            
            GL.MatrixMode(MatrixMode.Projection);

            Matrix4d proj = mProjectionMatrix.Matrix;

            double dx = ViewOrg.X - (ViewWidth / 2.0);
            double dy = ViewOrg.Y - (ViewHeight / 2.0);

            // x,yの平行移動成分を設定
            // Set x and y translational components
            proj.M41 = dx / (ViewWidth / 2.0);
            proj.M42 = -dy / (ViewHeight / 2.0);

            GL.LoadMatrix(ref proj);
            #endregion

            SetupLight();
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            mViewCenter.X = w / 2.0;
            mViewCenter.Y = h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            CalcProjectionMatrix();
            CalcProjectionZW();

            Center.X = w / 2;
            Center.Y = h / 2;

            Matrix2D = Matrix4d.CreateOrthographicOffCenter(
                                        0, mViewWidth,
                                        mViewHeight, 0,
                                        0, mProjectionFar);
        }

        public override void CalcProjectionMatrix()
        {
            mProjectionMatrix = Matrix4d.CreateOrthographic(
                                            ViewWidth / mUnitPerMilli, ViewHeight / mUnitPerMilli,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }

        public override DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
        {
            DrawContextGLOrtho dc = new DrawContextGLOrtho();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(deviceSize.Width, deviceSize.Height);

            Vector3d org = default;
            org.X = deviceSize.Width / 2.0;
            org.Y = deviceSize.Height / 2.0;

            dc.SetViewOrg(org);

            dc.UnitPerMilli = deviceSize.Width / pageSize.Width;

            return dc;
        }

        public override DrawContext Clone()
        {
            DrawContextGLOrtho dc = new DrawContextGLOrtho();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(ViewWidth, ViewHeight);

            dc.SetViewOrg(ViewOrg);

            dc.UnitPerMilli = UnitPerMilli;

            return dc;
        }
    }
}