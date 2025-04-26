using CadDataTypes;
using Plotter.Controller;
using System;
using TCad.Plotter.Drawing;

namespace Plotter;

public abstract class DrawContext : IDisposable
{
    public enum ProjectionType
    {
        Orthographic,
        Perspective,
    }


    IPlotterViewForDC mPlotterView;
    public IPlotterViewForDC PlotterView
    {
        set => mPlotterView = value;
        get => mPlotterView;
    }

    // 画素/Milli
    // 1ミリあたりの画素数
    protected vcompo_t mUnitPerMilli = 1;
    public virtual vcompo_t UnitPerMilli
    {
        set => mUnitPerMilli = value;
        get => mUnitPerMilli;
    }

    // 視点
    public const vcompo_t STD_EYE_DIST = (vcompo_t)(250.0);
    protected vector3_t mEye = vector3_t.UnitZ * STD_EYE_DIST;
    public vector3_t Eye => mEye;

    // 注視点
    protected vector3_t mLookAt = vector3_t.Zero;
    public vector3_t LookAt => mLookAt;

    // 投影面までの距離
    protected vcompo_t mProjectionNear = (vcompo_t)0.1;
    protected vcompo_t ProjectionNear => mProjectionNear;

    // 視野空間の遠方側クリップ面までの距離
    protected vcompo_t mProjectionFar = (vcompo_t)2000.0;
    protected vcompo_t ProjectionFar => mProjectionFar;

    // 画角 大きければ広角レンズ、小さければ望遠レンズ
    protected vcompo_t mFovY = (vcompo_t)(Math.PI / 4.0);
    protected vcompo_t FovY => mFovY;

    // 上を示す Vector
    protected vector3_t mUpVector = vector3_t.UnitY;
    public vector3_t UpVector => mUpVector;

    // 投影スクリーンの向き
    protected vector3_t mViewDir = default;
    public virtual vector3_t ViewDir => mViewDir;

    // ワールド座標系から視点座標系への変換(ビュー変換)行列
    protected matrix4_t mModelViewMatrix = default;
    protected matrix4_t ModelViewMatrix => mModelViewMatrix;
    protected ref matrix4_t ModelViewMatrixRef => ref mModelViewMatrix;

    // 視点座標系からワールド座標系への変換行列
    protected matrix4_t mViewMatrixInv = default;
    protected matrix4_t ViewMatrixInv => mViewMatrixInv;

    // 視点座標系から投影座標系への変換行列
    protected matrix4_t mProjectionMatrix = default;
    protected matrix4_t ProjectionMatrix => mProjectionMatrix;
    protected ref matrix4_t ProjectionMatrixRef => ref mProjectionMatrix;

    // 投影座標系から視点座標系への変換行列
    protected matrix4_t mProjectionMatrixInv = default;
    protected matrix4_t ProjectionMatrixInv => mProjectionMatrixInv;

    protected vcompo_t mProjectionW = (vcompo_t)(1.0);
    protected vcompo_t ProjectionW => mProjectionW;

    protected vcompo_t mProjectionZ = 0;
    protected vcompo_t ProjectionZ => mProjectionZ;

    // Screen 座標系の原点 
    // 座標系の原点がView座標上で何処にあるかを示す
    protected vector3_t mViewOrg;
    public virtual vector3_t ViewOrg
    {
        get => mViewOrg;
    }

    public vcompo_t mViewWidth = 32;
    public vcompo_t mViewHeight = 32;

    // Screenのサイズ
    public vcompo_t ViewWidth => mViewWidth;
    public vcompo_t ViewHeight => mViewHeight;

    // Viewの中心
    protected vector3_t mViewCenter;
    public virtual vector3_t ViewCenter
    {
        get => mViewCenter;
    }

    // 縮尺
    private vcompo_t WorldScale_ = (vcompo_t)(1.0);

    public vcompo_t WorldScale
    {
        get => WorldScale_;

        set
        {
            WorldScale_ = value;
            CalcViewMatrix();
        }

    }

    // 画面に描画する際の係数
    public vcompo_t DeviceScaleX = (vcompo_t)(1.0);
    public vcompo_t DeviceScaleY = (vcompo_t)(-1.0);

    public DrawTools Tools
    {
        get;
        protected set;
    } = new DrawTools();

    public DrawOptionSet OptionSet
    {
        get;
        protected set;
    }

    protected IDrawing mDrawing;
    public IDrawing Drawing => mDrawing;

    public DrawContext()
    {
        Log.plx("in");

        OptionSet = new DrawOptionSet(this);

        Log.plx("out");
    }

    public virtual void Activate() { }

    public virtual void Deactivate() { }

    public virtual void SetViewOrg(vector3_t org)
    {
        mViewOrg = org;
    }

    public void SetupTools(DrawModes type)
    {
        Tools.Setup(type);
        OptionSet.Initialize();
    }

    public virtual void SetViewSize(vcompo_t w, vcompo_t h)
    {
        mViewWidth = w;
        mViewHeight = h;

        mViewCenter.X = w / (vcompo_t)(2.0);
        mViewCenter.Y = h / (vcompo_t)(2.0);
    }

    public virtual void StartDraw()
    {
    }

    public virtual void EndDraw()
    {
    }

    public void UpdateView()
    {
        mPlotterView?.SwapBuffers(this);
    }

    public void MakeCurrent()
    {
        mPlotterView?.GLMakeCurrent();
    }

    #region Point converter
    public virtual CadVertex WorldPointToDevPoint(CadVertex pt)
    {
        pt.vector = WorldVectorToDevVector(pt.vector);
        pt.vector += mViewOrg;
        return pt;
    }

    public virtual CadVertex DevPointToWorldPoint(CadVertex pt)
    {
        pt.vector -= mViewOrg;
        pt.vector = DevVectorToWorldVector(pt.vector);
        return pt;
    }

    public virtual CadVertex WorldVectorToDevVector(CadVertex pt)
    {
        pt.vector = WorldVectorToDevVector(pt.vector);
        return pt;
    }

    public virtual CadVertex DevVectorToWorldVector(CadVertex pt)
    {
        pt.vector = DevVectorToWorldVector(pt.vector);
        return pt;
    }

    public virtual vector3_t WorldPointToDevPoint(vector3_t pt)
    {
        vector3_t p = WorldVectorToDevVector(pt);
        p = p + mViewOrg;
        return p;
    }

    public virtual vector3_t DevPointToWorldPoint(vector3_t pt)
    {
        pt = pt - mViewOrg;
        return DevVectorToWorldVector(pt);
    }

    public virtual vector3_t WorldVectorToDevVector(vector3_t pt)
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
        //dv.Z = 0;

        return dv.ToVector3();
    }

    public virtual vector3_t DevVectorToWorldVector(vector3_t pt)
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

    public virtual vcompo_t DevSizeToWoldSize(vcompo_t s)
    {
        CadVertex vd = DevVectorToWorldVector(CadVertex.UnitX * s);
        CadVertex v0 = DevVectorToWorldVector(CadVertex.Zero);
        CadVertex v = vd - v0;
        return v.Norm();
    }
    #endregion

    protected void CalcViewDir()
    {
        vector3_t ret = mLookAt - mEye;
        ret.Normalize();
        mViewDir = ret;
    }

    protected void CalcProjectionZW()
    {
        vector4_t wv = vector4_t.Zero;
        wv.W = 1.0f;
        wv.Z = -((mEye - mLookAt).Length);

        vector4_t pv = wv * mProjectionMatrix;

        mProjectionW = pv.W;
        mProjectionZ = pv.Z;
    }

    protected void CalcViewMatrix()
    {
        //mViewMatrix = matrix4_t.Scale(WorldScale_) * matrix4_t.LookAt(mEye, mLookAt, mUpVector);
        mModelViewMatrix = matrix4_t.CreateScale(WorldScale_) * matrix4_t.LookAt(mEye, mLookAt, mUpVector);
        //mViewMatrixInv = mViewMatrix.Invert();
        mViewMatrixInv = mModelViewMatrix.Inv();
    }

    public void CopyProjectionMetrics(DrawContext dc)
    {
        mUnitPerMilli = dc.mUnitPerMilli;
        mProjectionNear = dc.mProjectionNear;
        mProjectionFar = dc.mProjectionFar;
        mFovY = dc.mFovY;
    }

    public void CopyCamera(DrawContext dc)
    {
        WorldScale_ = dc.WorldScale_;
        SetCamera(dc.mEye, dc.mLookAt, dc.mUpVector);
    }

    public void CopyProjectionMatrix(DrawContext dc)
    {
        mProjectionMatrix = dc.mProjectionMatrix;
        mProjectionMatrixInv = dc.mProjectionMatrixInv;
    }

    public void CopyViewMatrix(DrawContext dc)
    {
        mModelViewMatrix = dc.mModelViewMatrix;
        mViewMatrixInv = dc.mViewMatrixInv;
    }

    public void SetCamera(vector3_t eye, vector3_t lookAt, vector3_t upVector)
    {
        mEye = eye;
        mLookAt = lookAt;
        mUpVector = upVector;

        CalcViewMatrix();
        CalcViewDir();
        CalcProjectionZW();
    }

    public virtual DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
    {
        return null;
    }

    public abstract void CalcProjectionMatrix();
    public abstract void Dispose();

    public abstract DrawContext Clone();
    public abstract DrawPen GetPen(int idx);
    public abstract DrawBrush GetBrush(int idx);

    public abstract void EnableLight();
    public abstract void DisableLight();

    public virtual void dump()
    {
        ViewOrg.dump("ViewOrg");

        Log.pl("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

        CadVertex t = CadVertex.Create(mViewDir);
        t.dump("ViewDir");

        Log.pl("ViewMatrix");
        mModelViewMatrix.dump();

        Log.pl("ProjectionMatrix");
        mProjectionMatrix.dump();

        Log.pl($"ProjectionW={mProjectionW}");
        Log.pl($"ProjectionZ={mProjectionZ}");
    }
}
