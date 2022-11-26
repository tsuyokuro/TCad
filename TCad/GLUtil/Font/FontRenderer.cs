using GLUtil;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace GLFont;

public class FontRenderer
{
    public int Texture = -1;
    private bool mInitialized = false;

    FontShader mShader;

    public bool Initialized
    {
        get => mInitialized;
    }

    private static FontRenderer sInstance = null;
    public static FontRenderer Instance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            if (sInstance == null)
            {
                sInstance = new FontRenderer();
                sInstance.Init();
            }

            return sInstance;
        }
    }

    private FontRenderer()
    {

    }

    private void Init()
    {
        Dispose();

        Texture = TextureProvider.Instance.GetNew();

        mShader = FontShader.GetInstance();

        mInitialized = true;
    }

    public void Dispose()
    {
        if (mInitialized)
        {
            TextureProvider.Instance.Remove(Texture);
        }

        mInitialized = false;
    }

    public void Render(FontTex tex)
    {
        Vector3d p = Vector3d.Zero;
        Vector3d xv = Vector3d.UnitX * tex.ImgW;
        Vector3d yv = Vector3d.UnitY * tex.ImgH;

        Render(tex, p, xv, yv);
    }

    public void Render(FontTex tex, Vector3d p, Vector3d xv, Vector3d yv)
    {
        if (!mInitialized)
        {
            throw new ObjectDisposedException(nameof(FontRenderer));
        }

        int texUnitNumber = 0;

        GL.ActiveTexture(TextureUnit.Texture0 + texUnitNumber);

        GL.BindTexture(TextureTarget.Texture2D, Texture);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexImage2D(
            TextureTarget.Texture2D, 0,
            PixelInternalFormat.Alpha8,
            tex.W, tex.H, 0,
            PixelFormat.Alpha,
            PixelType.UnsignedByte, tex.Data);


        mShader.Start(texUnitNumber);

        GL.TexCoord2(1.0, 1.0);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Normal3(new Vector3d(0, 0, 1));

        GL.Begin(PrimitiveType.Quads);

        GL.TexCoord2(1.0, 1.0);
        GL.Vertex3(p + xv + yv);

        GL.TexCoord2(0.0, 1.0);
        GL.Vertex3(p + yv);

        GL.TexCoord2(0.0, 0.0);
        GL.Vertex3(p);

        GL.TexCoord2(1.0, 0.0);
        GL.Vertex3(p + xv);

        GL.End();

        GL.UseProgram(0);
    }
}
