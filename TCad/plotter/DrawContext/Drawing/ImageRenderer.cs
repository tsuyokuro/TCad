//#define DEFAULT_DATA_TYPE_DOUBLE
using GLUtil;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Imaging;



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

public class ImageRenderer
{
    private int TextureID = -1;

    private bool mInitialized = false;

    private ImageShader mShader;

    public bool Initialized
    {
        get => mInitialized;
    }

    public void Init()
    {
        Dispose();

        TextureID = TextureProvider.Instance.GetNew();

        // Use my shader
        mShader = ImageShader.GetInstance();

        mInitialized = true;
    }

    public void Dispose()
    {
        if (mInitialized)
        {
            TextureProvider.Instance.Remove(TextureID);
        }

        mInitialized = false;
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
        mShader.Start(texUnitNumber);


        vector3_t x = xv;
        vector3_t y = yv;

        GL.TexCoord2((vcompo_t)(1.0), (vcompo_t)(1.0));

        GL.Normal3(new vector3_t(0, 0, 1));

        GL.Begin(PrimitiveType.Quads);

        GL.TexCoord2((vcompo_t)(1.0), (vcompo_t)(1.0));
        GL.Vertex3(p + x + y);

        GL.TexCoord2((vcompo_t)(0.0), (vcompo_t)(1.0));
        GL.Vertex3(p + y);

        GL.TexCoord2((vcompo_t)(0.0), (vcompo_t)(0.0));
        GL.Vertex3(p);

        GL.TexCoord2((vcompo_t)(1.0), (vcompo_t)(0.0));
        GL.Vertex3(p + x);

        GL.End();


        // Use my shader
        mShader.End();


        // Not use my shader
        //GL.Disable(EnableCap.Texture2D);
    }


    public class Provider
    {
        private static ImageRenderer sImageRenderer;

        public static ImageRenderer Get()
        {
            if (sImageRenderer == null)
            {
                sImageRenderer = new ImageRenderer();
            }

            if (!sImageRenderer.Initialized)
            {
                sImageRenderer.Init();
            }

            return sImageRenderer;
        }

        public static void Release()
        {
            if (sImageRenderer != null)
            {
                sImageRenderer.Dispose();
            }
        }
    }
}
