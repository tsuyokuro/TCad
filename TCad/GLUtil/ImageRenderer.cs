using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace GLUtil;

public class ImageRenderer
{
    private int TextureID = -1;

    private ImageShader Shader;

    private TextureProvider TextureProvider;

    public ImageRenderer(ImageShader shader, TextureProvider textureProvider)
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


    public void Render(Bitmap bitmap, vector3_t p, vector3_t xv, vector3_t yv)
    {
        int texUnitNumber = 1;

        // Not use my shader
        //GL.Enable(EnableCap.Texture2D);
        //GL.Disable(EnableCap.Lighting);

        // Use my shader
        GL.ActiveTexture(TextureUnit.Texture0 + texUnitNumber);


        GL.BindTexture(TextureTarget.Texture2D, TextureID);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        int bitmapW = bitmap.Width;
        int bitmapH = bitmap.Height;

        Rectangle r = new Rectangle(0, 0, bitmapW, bitmapH);

        BitmapData data = bitmap.LockBits(
                r,
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            bitmapW, bitmapH,
            0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0);


        bitmap.UnlockBits(data);


        // Use my shader
        Shader.Start(texUnitNumber);


        vector3_t x = xv;
        vector3_t y = yv;

        GL.TexCoord2((vcompo_t)1.0, (vcompo_t)1.0);

        GL.Normal3(new vector3_t(0, 0, 1));

        GL.Begin(PrimitiveType.Quads);

        GL.TexCoord2((vcompo_t)1.0, (vcompo_t)1.0);
        GL.Vertex3(p + x + y);

        GL.TexCoord2((vcompo_t)0.0, (vcompo_t)1.0);
        GL.Vertex3(p + y);

        GL.TexCoord2((vcompo_t)0.0, (vcompo_t)0.0);
        GL.Vertex3(p);

        GL.TexCoord2((vcompo_t)1.0, (vcompo_t)0.0);
        GL.Vertex3(p + x);

        GL.End();


        // Use my shader
        Shader.End();


        // Not use my shader
        //GL.Disable(EnableCap.Texture2D);
    }
}
