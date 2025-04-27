using GLUtil;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;

namespace GLFont;

public class FontRenderer
{
    public int TextureID = -1;


    private FontShader Shader;

    TextureProvider TextureProvider;

    public FontRenderer(FontShader shader, TextureProvider textureProvider)
    {
        Shader = shader;
        TextureProvider = textureProvider;
        TextureID = TextureProvider.GetNew();
    }

    public void Dispose()
    {
        if (TextureID != -1)
        {
            TextureProvider.Remove(TextureID);
            TextureID = -1;
        }
    }

    public void Render(FontTex tex)
    {
        vector3_t p = vector3_t.Zero;
        vector3_t xv = vector3_t.UnitX * tex.ImgW;
        vector3_t yv = vector3_t.UnitY * tex.ImgH;

        Render(tex, p, xv, yv);
    }

    public static int Counter = 0;

    public void Render(FontTex tex, vector3_t p, vector3_t xv, vector3_t yv)
    {
        if (TextureID == -1)
        {
            TextureID = GLUtilContainer.TextureProvider.Get().GetNew();
        }

        Counter++;

        int texUnitNumber = 0;
        GL.ActiveTexture(TextureUnit.Texture0 + texUnitNumber);

        if (tex.TextureID == -1)
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Alpha8,
                tex.W, tex.H, 0,
                PixelFormat.Alpha,
                PixelType.UnsignedByte, tex.Data);
        }
        else
        {
            GL.BindTexture(TextureTarget.Texture2D, tex.TextureID);
        }

        Shader.Start(texUnitNumber);

        GL.TexCoord2((vcompo_t)(1.0), (vcompo_t)(1.0));

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Normal3(new vector3_t(0, 0, 1));

        GL.Begin(PrimitiveType.Quads);

        GL.TexCoord2((vcompo_t)(1.0), (vcompo_t)(1.0));
        GL.Vertex3(p + xv + yv);

        GL.TexCoord2((vcompo_t)(0.0), (vcompo_t)(1.0));
        GL.Vertex3(p + yv);

        GL.TexCoord2((vcompo_t)(0.0), (vcompo_t)(0.0));
        GL.Vertex3(p);

        GL.TexCoord2((vcompo_t)(1.0), (vcompo_t)(0.0));
        GL.Vertex3(p + xv);

        GL.End();


        GL.Disable(EnableCap.Blend);

        // Unbind Texture
        GL.BindTexture(TextureTarget.Texture2D, 0);

        // 
        //GL.UseProgram(0);
        Shader.End();
    }
}
