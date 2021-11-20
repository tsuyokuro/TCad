using System;
using System.Windows.Forms;
using CadDataTypes;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGLPers : DrawContextGL
    {
        public DrawContextGLPers()
        {
            Init(null);
            mUnitPerMilli = 1;
        }

        public DrawContextGLPers(Control control)
        {
            Init(control);
            mUnitPerMilli = 1;
        }

        public override void StartDraw()
        {
            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();


            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.Matrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mProjectionMatrix.Matrix);

            SetupLight();
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            mViewOrg.X = w / 2.0;
            mViewOrg.Y = h / 2.0;

            mViewCenter.X = w / 2.0;
            mViewCenter.Y = h / 2.0;

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            CalcProjectionMatrix();
            CalcProjectionZW();

            Matrix2D = Matrix4d.CreateOrthographicOffCenter(
                                        0, mViewWidth,
                                        mViewHeight, 0,
                                        0, mProjectionFar);
        }

        public override void CalcProjectionMatrix()
        {
            double aspect = mViewWidth / mViewHeight;
            mProjectionMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            mFovY,
                                            aspect,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }

        public override DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
        {
            DrawContextGL dc = new DrawContextGLPers();

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

        public void RotateEyePoint(Vector2 prev, Vector2 current)
        {
            Vector2 d = current - prev;

            double ry = (d.X / 10.0) * (Math.PI / 20);
            double rx = (d.Y / 10.0) * (Math.PI / 20);

            CadQuaternion q;
            CadQuaternion r;
            CadQuaternion qp;

            q = CadQuaternion.RotateQuaternion(Vector3d.UnitY, ry);

            r = q.Conjugate();

            qp = CadQuaternion.FromVector(mEye);
            qp = r * qp;
            qp = qp * q;
            mEye = qp.ToVector3d();

            qp = CadQuaternion.FromVector(mUpVector);
            qp = r * qp;
            qp = qp * q;
            mUpVector = qp.ToVector3d();

            Vector3d ev = mLookAt - mEye;

            Vector3d a = new Vector3d(ev);
            Vector3d b = new Vector3d(mUpVector);

            Vector3d axis = CadMath.Normal(a, b);

            if (!axis.IsZero())
            {

                q = CadQuaternion.RotateQuaternion(axis, rx);

                r = q.Conjugate();

                qp = CadQuaternion.FromVector(mEye);
                qp = r * qp;
                qp = qp * q;

                mEye = qp.ToVector3d();

                qp = CadQuaternion.FromVector(mUpVector);
                qp = r * qp;
                qp = qp * q;
                mUpVector = qp.ToVector3d();
            }

            CalcViewMatrix();
            CalcViewDir();
            CalcProjectionZW();
        }

        public void MoveForwardEyePoint(double d, bool withLookAt = false)
        {
            Vector3d dv = ViewDir * d;

            if (withLookAt)
            {
                mEye += dv;
                mLookAt += dv;
            }
            else
            {
                Vector3d eye = mEye + dv;

                Vector3d viewDir = mLookAt - eye;

                viewDir.Normalize();

                if ((ViewDir - viewDir).Length > 1.0)
                {
                    return;
                }

                mEye += dv;
            }

            CalcViewMatrix();
            CalcProjectionMatrix();
            CalcProjectionZW();
            CalcViewDir();
        }

        public override DrawContext Clone()
        {
            DrawContextGLPers dc = new DrawContextGLPers();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(ViewWidth, ViewHeight);

            dc.SetViewOrg(ViewOrg);

            return dc;
        }


        #region Point converter
        public override CadVertex WorldPointToDevPoint(CadVertex pt)
        {
            pt.vector = WorldVectorToDevVector(pt.vector);
            pt.vector += mViewCenter;
            return pt;
        }

        public override CadVertex DevPointToWorldPoint(CadVertex pt)
        {
            pt.vector -= mViewCenter;
            pt.vector = DevVectorToWorldVector(pt.vector);
            return pt;
        }


        public override CadVertex WorldVectorToDevVector(CadVertex pt)
        {
            pt.vector = WorldVectorToDevVector(pt.vector);
            return pt;
        }

        public override CadVertex DevVectorToWorldVector(CadVertex pt)
        {
            pt.vector = DevVectorToWorldVector(pt.vector);
            return pt;
        }


        public override Vector3d WorldPointToDevPoint(Vector3d pt)
        {
            Vector3d p = WorldVectorToDevVector(pt);
            p = p + mViewCenter;
            return p;
        }

        public override Vector3d DevPointToWorldPoint(Vector3d pt)
        {
            pt = pt - mViewCenter;
            return DevVectorToWorldVector(pt);
        }


        public override Vector3d WorldVectorToDevVector(Vector3d pt)
        {
            pt *= WorldScale;

            Vector4d wv = pt.ToVector4d(1.0);

            Vector4d sv = wv * mViewMatrix;
            Vector4d pv = sv * mProjectionMatrix;

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            dv.X = dv.X * DeviceScaleX;
            dv.Y = dv.Y * DeviceScaleY;
            dv.Z = 0;

            return dv.ToVector3d();
        }

        public override Vector3d DevVectorToWorldVector(Vector3d pt)
        {
            pt.X = pt.X / DeviceScaleX;
            pt.Y = pt.Y / DeviceScaleY;

            Vector4d wv;

            wv.W = mProjectionW;
            wv.Z = mProjectionZ;

            wv.X = pt.X * wv.W;
            wv.Y = pt.Y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WorldScale;

            return wv.ToVector3d();
        }

        public override double DevSizeToWoldSize(double s)
        {
            Vector3d vd = DevVectorToWorldVector(Vector3d.UnitX * s);
            Vector3d v0 = DevVectorToWorldVector(Vector3d.Zero);
            Vector3d v = vd - v0;
            return v.Norm();
        }
        #endregion
    }
}
