using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Windows.Forms;

namespace Plotter;

public abstract class DrawContextGL : DrawContext
{
    public const vcompo_t DEFAULT_EYE_Z = (vcompo_t)250.0;
    public const vcompo_t DEFAULT_NEAR = (vcompo_t)0.1;
    public const vcompo_t DEFAULT_FAR = (vcompo_t)2000.0;

    protected Control ViewCtrl;

    Vector4 LightPosition;

    Vector4 SpotLightDirection;

    bool IsSpotLight = true;

    Color4 LightAmbient;    // 環境光
    Color4 LightDiffuse;    // 拡散光
    Color4 LightSpecular;   // 鏡面反射光

    Color4 MaterialAmbient;
    Color4 MaterialDiffuse;
    Color4 MaterialSpecular;
    Color4 MaterialShininess;

    public bool LightingEnable = true;

    public matrix4_t Matrix2D = matrix4_t.Identity;

    public ProjectionType mProjectionType = ProjectionType.Perspective;

    public enum ViewingAngleType
    {
        TELESCOPE,
        STANDERD,
        WIDE_ANGLE,
    }

    public DrawContextGL()
    {
        mUnitPerMilli = 1;
    }

    public DrawContextGL(Control control)
    {
        mUnitPerMilli = 1;
    }

    protected void Init(Control control)
    {
        ViewCtrl = control;

        mUnitPerMilli = (vcompo_t)(1.0);
        WorldScale = 1.0f;

        InitCamera(ViewingAngleType.STANDERD);

        CalcViewMatrix();
        CalcViewDir();

        /*
        LightPosition = new Vector4(150f, 150f, 150f, 0.0f);
        LightAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
        LightDiffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
        LightSpecular = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
        */

        //LightPosition = new Vector4(100.0f, 500f, 150.0f, 0.0f);

        LightPosition = new Vector4(-0.5f, 1f, 1.0f, 0.0f);

        SpotLightDirection = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);

        float v;

        if (IsSpotLight)
        {
            // 環境光
            v = 0.2f;
            LightAmbient = new Color4(v, v, v, 1.0f);

            // 拡散光
            v = 0.8f;
            LightDiffuse = new Color4(v, v, v, 1.0f);

            // 鏡面光
            v = 0.1f;
            LightSpecular = new Color4(v, v, v, 1.0f);
        }
        else
        {
            // 環境光
            v = 0.1f;
            LightAmbient = new Color4(v, v, v, 1.0f);

            // 拡散光
            v = 0.8f;
            LightDiffuse = new Color4(v, v, v, 1.0f);

            // 鏡面光
            v = 0.1f;
            LightSpecular = new Color4(v, v, v, 1.0f);
        }


        MaterialAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
        MaterialDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
        MaterialSpecular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
        MaterialShininess = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

        SetupDrawing();
    }

    public void InitCamera(ViewingAngleType type)
    {
        vcompo_t ez = DEFAULT_EYE_Z;
        vcompo_t near = DEFAULT_NEAR;
        vcompo_t far = DEFAULT_FAR;

        mLookAt = vector3_t.Zero;
        mUpVector = vector3_t.UnitY;

        mProjectionNear = near;
        mProjectionFar = far;

        mEye = vector3_t.Zero;

        // FovY 画角を指定
        // 初期カメラ位置を調整
        switch (type)
        {
            case ViewingAngleType.TELESCOPE:
                // 望遠
                mEye.Z = ez;
                mFovY = (vcompo_t)Math.PI / 4;
                break;

            case ViewingAngleType.STANDERD:
                // 標準
                mEye.Z = ez;
                mFovY = (vcompo_t)Math.PI / 3;
                break;

            case ViewingAngleType.WIDE_ANGLE:
                // 広角
                mEye.Z = ez * (vcompo_t)(0.75);
                mFovY = (vcompo_t)Math.PI / 2;
                break;
        }
    }

    protected void SetupLight()
    {
        if (!LightingEnable)
        {
            return;
        }

        // 裏面を描かない
        //GL.Enable(EnableCap.CullFace);
        //GL.CullFace(CullFaceMode.Back);
        //GL.FrontFace(FrontFaceDirection.Ccw);

        //法線の正規化
        //GL.Enable(EnableCap.Normalize);

        if (IsSpotLight)
        {
            GL.Light(LightName.Light0, LightParameter.SpotDirection, SpotLightDirection);
        }
        else
        {
            GL.Light(LightName.Light0, LightParameter.Position, LightPosition);
        }

        GL.Light(LightName.Light0, LightParameter.Ambient, LightAmbient);
        GL.Light(LightName.Light0, LightParameter.Diffuse, LightDiffuse);
        GL.Light(LightName.Light0, LightParameter.Specular, LightSpecular);

        /*
        GL.Material(MaterialFace.Front, MaterialParameter.Ambient, MaterialAmbient);
        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, MaterialDiffuse);
        GL.Material(MaterialFace.Front, MaterialParameter.Specular, MaterialSpecular);
        GL.Material(MaterialFace.Front, MaterialParameter.Shininess, MaterialShininess);
        */
    }

    public override void Dispose()
    {
        if (Tools != null)
        {
            Tools.Dispose();
        }

        if (mDrawing!=null)
        {
            mDrawing.Dispose();
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

    protected void SetupDrawing()
    {
        mDrawing = new DrawingGL(this);
    }

    public override void EnableLight()
    {
        GL.Enable(EnableCap.Lighting);
        GL.Enable(EnableCap.Light0);
    }

    public override void DisableLight()
    {
        GL.Disable(EnableCap.Lighting);
        GL.Disable(EnableCap.Light0);
    }
}
