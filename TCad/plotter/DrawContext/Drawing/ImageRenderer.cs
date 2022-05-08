using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class ImageRenderer
    {
        private int Texture = -1;

        private bool mInitialized = false;

        private ImageShader mShader;

        public bool Initialized
        {
            get => mInitialized;
        }

        public void Init()
        {
            Dispose();

            Texture = GL.GenTexture();

            // Use my shader
            mShader = ImageShader.GetInstance();

            mInitialized = true;
        }

        public void Dispose()
        {
            if (mInitialized)
            {
                GL.DeleteTexture(Texture);
            }

            mInitialized = false;
        }


        public void Render(Bitmap bitmap, Vector3d p, Vector3d xv, Vector3d yv)
        {
            int texUnitNumber = 1;

            // Not use my shader
            //GL.Enable(EnableCap.Texture2D);
            //GL.Disable(EnableCap.Lighting);

            // Use my shader
            GL.ActiveTexture(TextureUnit.Texture0 + texUnitNumber);

                        
            GL.BindTexture(TextureTarget.Texture2D, Texture);

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


            Vector3d x = xv;
            Vector3d y = yv;

            GL.TexCoord2(1.0, 1.0);

            GL.Normal3(new Vector3d(0, 0, 1));

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(1.0, 1.0);
            GL.Vertex3(p + x + y);

            GL.TexCoord2(0.0, 1.0);
            GL.Vertex3(p + y);

            GL.TexCoord2(0.0, 0.0);
            GL.Vertex3(p);

            GL.TexCoord2(1.0, 0.0);
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

            private static int RefCnt = 0;

            public static ImageRenderer Get()
            {
                RefCnt++;

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
                if (RefCnt > 1)
                {
                    RefCnt--;
                    return;
                }

                sImageRenderer.Dispose();
                RefCnt = 0;
            }
        }
    }
}
