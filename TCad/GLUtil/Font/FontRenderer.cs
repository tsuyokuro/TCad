using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;

namespace GLFont
{
    public class FontRenderer
    {
        public int Texture = -1;
        private bool mInitialized = false;

        FontShader mShader;

        public bool Initialized
        {
            get => mInitialized;
        }

        private FontRenderer()
        {

        }

        private void Init()
        {
            Dispose();

            Texture = GL.GenTexture();

            mShader = FontShader.GetInstance();

            mInitialized = true;
        }

        private void Dispose()
        {
            if (mInitialized)
            {
                GL.DeleteTexture(Texture);
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

        public class Provider
        {
            private static FontRenderer sFontRenderer;

            private static int RefCnt = 0;

            public static FontRenderer get()
            {
                RefCnt++;

                if (sFontRenderer == null)
                {
                    sFontRenderer = new FontRenderer();
                }

                if (!sFontRenderer.Initialized)
                {
                    sFontRenderer.Init();
                }

                return sFontRenderer;
            }

            public static void Release()
            {
                if (RefCnt > 1)
                {
                    RefCnt--;
                    return;
                }

                sFontRenderer.Dispose();
                RefCnt = 0;
            }
        }
    }
}
