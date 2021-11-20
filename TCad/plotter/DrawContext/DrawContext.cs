using OpenTK;
using System;
using CadDataTypes;

namespace Plotter
{
    public abstract class DrawContext : IDisposable
    {
        public enum ProjectionType
        {
            Orthographic,
            Perspective,
        }

        protected Action<DrawContext> mPushToViewAction;
        public Action<DrawContext> PushToViewAction
        {
            set => mPushToViewAction = value;
            get => mPushToViewAction;
        }

        // 画素/Milli
        // 1ミリあたりの画素数
        protected double mUnitPerMilli = 1;
        public virtual double UnitPerMilli
        {
            set => mUnitPerMilli = value;
            get => mUnitPerMilli;
        }

        // 視点
        public const double STD_EYE_DIST = 250.0;
        protected Vector3d mEye = Vector3d.UnitZ * STD_EYE_DIST;
        public Vector3d Eye => mEye;

        // 注視点
        protected Vector3d mLookAt = Vector3d.Zero;
        public Vector3d LookAt => mLookAt;

        // 投影面までの距離
        protected double mProjectionNear = 0.1;
        protected double ProjectionNear => mProjectionNear;

        // 視野空間の遠方側クリップ面までの距離
        protected double mProjectionFar = 2000.0;
        protected double ProjectionFar => mProjectionFar;

        // 画角 大きければ広角レンズ、小さければ望遠レンズ
        protected double mFovY = Math.PI / 4;
        protected double FovY => mFovY;

        // 上を示す Vector
        protected Vector3d mUpVector = Vector3d.UnitY;
        public Vector3d UpVector => mUpVector;

        // 投影スクリーンの向き
        protected Vector3d mViewDir = default(Vector3d);
        public virtual Vector3d ViewDir => mViewDir;

        // ワールド座標系から視点座標系への変換(ビュー変換)行列
        protected UMatrix4 mViewMatrix = new UMatrix4();
        protected UMatrix4 ViewMatrix => mViewMatrix;
        protected ref Matrix4d ViewMatrixRef => ref mViewMatrix.Matrix;

        // 視点座標系からワールド座標系への変換行列
        protected UMatrix4 mViewMatrixInv = new UMatrix4();
        protected UMatrix4 ViewMatrixInv => mViewMatrixInv;

        // 視点座標系から投影座標系への変換行列
        protected UMatrix4 mProjectionMatrix = new UMatrix4();
        protected UMatrix4 ProjectionMatrix => mProjectionMatrix;
        protected ref Matrix4d ProjectionMatrixRef => ref mProjectionMatrix.Matrix;

        // 投影座標系から視点座標系への変換行列
        protected UMatrix4 mProjectionMatrixInv = new UMatrix4();
        protected UMatrix4 ProjectionMatrixInv => mProjectionMatrixInv;

        protected double mProjectionW = 1.0;
        protected double ProjectionW => mProjectionW;

        protected double mProjectionZ = 0;
        protected double ProjectionZ => mProjectionZ;

        // Screen 座標系の原点 
        // 座標系の原点がView座標上で何処にあるかを示す
        protected Vector3d mViewOrg;
        public virtual Vector3d ViewOrg
        {
            get => mViewOrg;
        }

        public double mViewWidth = 32;
        public double mViewHeight = 32;

        // Screenのサイズ
        public double ViewWidth => mViewWidth;
        public double ViewHeight => mViewHeight;

        // Viewの中心
        protected Vector3d mViewCenter;
        public virtual Vector3d ViewCenter
        {
            get => mViewCenter;
        }

        // 縮尺
        public double WorldScale = 1.0;

        // 画面に描画する際の係数
        public double DeviceScaleX = 1.0;
        public double DeviceScaleY = -1.0;

        public DrawTools Tools = new DrawTools();

        protected IDrawing mDrawing;
        public IDrawing Drawing => mDrawing;

        public virtual void Active()
        {

        }

        public virtual void Deactive()
        {

        }

        public virtual void SetViewOrg(Vector3d org)
        {
            mViewOrg = org;
        }

        public void SetupTools(DrawTools.DrawMode type, int penW=0)
        {
            Tools.Setup(type, penW);
        }

        public virtual void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            mViewCenter.X = w / 2.0;
            mViewCenter.Y = h / 2.0;
        }

        public virtual void StartDraw()
        {
        }

        public virtual void EndDraw()
        {
        }

        public void PushToView()
        {
            mPushToViewAction?.Invoke(this);
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

        public virtual Vector3d WorldPointToDevPoint(Vector3d pt)
        {
            Vector3d p = WorldVectorToDevVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public virtual Vector3d DevPointToWorldPoint(Vector3d pt)
        {
            pt = pt - mViewOrg;
            return DevVectorToWorldVector(pt);
        }

        public virtual Vector3d WorldVectorToDevVector(Vector3d pt)
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

        public virtual Vector3d DevVectorToWorldVector(Vector3d pt)
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

        public virtual double DevSizeToWoldSize(double s)
        {
            CadVertex vd = DevVectorToWorldVector(CadVertex.UnitX * s);
            CadVertex v0 = DevVectorToWorldVector(CadVertex.Zero);
            CadVertex v = vd - v0;
            return v.Norm();
        }
        #endregion

        protected void CalcViewDir()
        {
            Vector3d ret = mLookAt - mEye;
            ret.Normalize();
            mViewDir = ret;
        }

        protected void CalcProjectionZW()
        {
            Vector4d wv = Vector4d.Zero;
            wv.W = 1.0f;
            wv.Z = -((mEye - mLookAt).Length);

            Vector4d pv = wv * mProjectionMatrix;

            mProjectionW = pv.W;
            mProjectionZ = pv.Z;
        }

        protected void CalcViewMatrix()
        {
            mViewMatrix = Matrix4d.LookAt(mEye, mLookAt, mUpVector);
            mViewMatrixInv = mViewMatrix.Invert();
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
            SetCamera(dc.mEye, dc.mLookAt, dc.mUpVector);
        }

        public void CopyProjectionMatrix(DrawContext dc)
        {
            mProjectionMatrix = dc.mProjectionMatrix;
            mProjectionMatrixInv = dc.mProjectionMatrixInv;
        }

        public void CopyViewMatrix(DrawContext dc)
        {
            mViewMatrix = dc.mViewMatrix;
            mViewMatrixInv = dc.mViewMatrixInv;
        }

        public void SetCamera(Vector3d eye, Vector3d lookAt, Vector3d upVector)
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

        public virtual void dump()
        {
            ViewOrg.dump("ViewOrg");

            DOut.pl("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

            CadVertex t = CadVertex.Create(mViewDir);
            t.dump("ViewDir");

            DOut.pl("ViewMatrix");
            mViewMatrix.dump();

            DOut.pl("ProjectionMatrix");
            mProjectionMatrix.dump();

            DOut.pl($"ProjectionW={mProjectionW}");
            DOut.pl($"ProjectionZ={mProjectionZ}");
        }
    }
}
