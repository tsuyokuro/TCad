using CadDataTypes;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Plotter;
using System;
using System.Windows.Forms;
using TCad.MathFunctions;

namespace TCad.Plotter.DrawContexts;

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
        GL.LoadMatrix(ref mModelViewMatrix);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref mProjectionMatrix);

        SetupLight();
    }

    public override void SetViewSize(vcompo_t w, vcompo_t h)
    {
        mViewWidth = w;
        mViewHeight = h;

        mViewOrg.X = w / (vcompo_t)(2.0);
        mViewOrg.Y = h / (vcompo_t)(2.0);

        mViewCenter.X = w / (vcompo_t)(2.0);
        mViewCenter.Y = h / (vcompo_t)(2.0);

        DeviceScaleX = w / (vcompo_t)(2.0);
        DeviceScaleY = -h / (vcompo_t)(2.0);

        GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

        CalcProjectionMatrix();
        CalcProjectionZW();

        Matrix2D = matrix4_t.CreateOrthographicOffCenter(
                                    0, mViewWidth,
                                    mViewHeight, 0,
                                    0, mProjectionFar);
    }

    public override void CalcProjectionMatrix()
    {
        vcompo_t aspect = mViewWidth / mViewHeight;
        mProjectionMatrix = matrix4_t.CreatePerspectiveFieldOfView(
                                        mFovY,
                                        aspect,
                                        mProjectionNear,
                                        mProjectionFar
                                        );

        mProjectionMatrixInv = mProjectionMatrix.Inv();
    }

    public override DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
    {
        DrawContextGL dc = new DrawContextGLPers();

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

    public void RotateEyePoint(Vector2 prev, Vector2 current)
    {
        Vector2 d = current - prev;

        vcompo_t ry = (d.X / ((vcompo_t)10.0)) * ((vcompo_t)Math.PI / 20);
        vcompo_t rx = (d.Y / ((vcompo_t)10.0)) * ((vcompo_t)Math.PI / 20);

        CadQuaternion q;
        CadQuaternion r;
        CadQuaternion qp;

        q = CadQuaternion.RotateQuaternion(vector3_t.UnitY, ry);

        r = q.Conjugate();

        qp = CadQuaternion.FromVector(mEye);
        qp = r * qp;
        qp = qp * q;
        mEye = qp.ToVector3();

        qp = CadQuaternion.FromVector(mUpVector);
        qp = r * qp;
        qp = qp * q;
        mUpVector = qp.ToVector3();

        vector3_t ev = mLookAt - mEye;

        vector3_t a = new vector3_t(ev);
        vector3_t b = new vector3_t(mUpVector);

        vector3_t axis = CadMath.Normal(a, b);

        if (!axis.IsZero())
        {

            q = CadQuaternion.RotateQuaternion(axis, rx);

            r = q.Conjugate();

            qp = CadQuaternion.FromVector(mEye);
            qp = r * qp;
            qp = qp * q;

            mEye = qp.ToVector3();

            qp = CadQuaternion.FromVector(mUpVector);
            qp = r * qp;
            qp = qp * q;
            mUpVector = qp.ToVector3();
        }

        CalcViewMatrix();
        CalcViewDir();
        CalcProjectionZW();
    }

    public void MoveForwardEyePoint(vcompo_t d, bool withLookAt = false)
    {
        vector3_t dv = ViewDir * d;

        if (withLookAt)
        {
            mEye += dv;
            mLookAt += dv;
        }
        else
        {
            vector3_t eye = mEye + dv;

            vector3_t viewDir = mLookAt - eye;

            viewDir.Normalize();

            if ((ViewDir - viewDir).Length > (vcompo_t)(1.0))
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


    public override vector3_t WorldPointToDevPoint(vector3_t pt)
    {
        vector3_t p = WorldVectorToDevVector(pt);
        p = p + mViewCenter;
        return p;
    }

    public override vector3_t DevPointToWorldPoint(vector3_t pt)
    {
        pt = pt - mViewCenter;
        return DevVectorToWorldVector(pt);
    }


    public override vector3_t WorldVectorToDevVector(vector3_t pt)
    {
        vector4_t wv = pt.ToVector4((vcompo_t)(1.0));

        vector4_t sv = wv * mModelViewMatrix;
        vector4_t pv = sv * mProjectionMatrix;

        vector4_t dv;

        dv.X = pv.X / pv.W;
        dv.Y = pv.Y / pv.W;
        dv.Z = pv.Z / pv.W;
        dv.W = pv.W;

        dv.X = dv.X * DeviceScaleX;
        dv.Y = dv.Y * DeviceScaleY;
        dv.Z = 0;

        return dv.ToVector3();
    }

    public override vector3_t DevVectorToWorldVector(vector3_t pt)
    {
        pt.X = pt.X / DeviceScaleX;
        pt.Y = pt.Y / DeviceScaleY;

        vector4_t wv;

        wv.W = mProjectionW;
        wv.Z = mProjectionZ;

        wv.X = pt.X * wv.W;
        wv.Y = pt.Y * wv.W;

        wv = wv * mProjectionMatrixInv;
        wv = wv * mViewMatrixInv;

        return wv.ToVector3();
    }

    public override vcompo_t DevSizeToWoldSize(vcompo_t s)
    {
        vector3_t vd = DevVectorToWorldVector(vector3_t.UnitX * s);
        vector3_t v0 = DevVectorToWorldVector(vector3_t.Zero);
        vector3_t v = vd - v0;
        return v.Norm();
    }
    #endregion
}
