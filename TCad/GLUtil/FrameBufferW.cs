#define TEX_DEPTH_BUFFER

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace GLUtil
{
    /**
     * OpenGLで描画可能なFrame buffer
     * Frame buffer that can be drawn with OpenGL 
     */
    class FrameBufferW
    {
        private int mWidth = 0;
        public int Width {
            get => mWidth;
        }

        private int mHeight = 0;
        public int Height
        {
            get => mHeight;
        }

        private int FrameBufferDesc = 0;
        private int DepthRenderBufferDesc = 0;
        private int ColorTexDesc = 0;
        private int DepthTexDesc = 0;

        public void Create(int width, int height)
        {
            mWidth = width;
            mHeight = height;

            // Color Texture
            ColorTexDesc = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorTexDesc);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width, height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, 0);

#if TEX_DEPTH_BUFFER
            // Depth Texture
            DepthTexDesc = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTexDesc);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (PixelInternalFormat)All.DepthComponent32,
                width, height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.UnsignedInt,
                IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, 0);
#else
            // Create Depth RenderBuffer
            DepthRenderBufferDesc = GL.GenRenderbuffer();

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRenderBufferDesc);

            GL.RenderbufferStorage(
                RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32, width, height);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
#endif


            // Create FrameBuffer
            FrameBufferDesc = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferDesc);

            // フレームバッファにカラーバッファを割り当てる
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                ColorTexDesc, 0);

#if TEX_DEPTH_BUFFER
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                DepthTexDesc, 0);
#else
            GL.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer,
                DepthRenderBufferDesc);
#endif
            // Since setup is completed, unbind objects.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
            if (ColorTexDesc != 0)
            {
                GL.DeleteTexture(ColorTexDesc);
            }
            
            if (DepthTexDesc != 0)
            {
                GL.DeleteTexture(DepthTexDesc);
            }
            
            if (DepthRenderBufferDesc != 0)
            {
                GL.DeleteRenderbuffer(DepthRenderBufferDesc);
            }
            
            if (FrameBufferDesc != 0)
            {
                GL.DeleteFramebuffer(FrameBufferDesc);
            }
        }

        /**
         * OpenGLの描画対象をこのFrameBufferにする
         * Atach the OpenGL drawing target to this FrameBuffer
         */
        public void Begin()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferDesc);
            GL.Viewport(0, 0, mWidth, mHeight);
            GL.Enable(EnableCap.DepthTest);
        }

        /**
         * OpenGLの描画対象をDefaultに戻す
         * Atach the OpenGL drawing target to default
         */
        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /**
         * Frame bufferをBitmapに変換
         * Convert Frame buffer to Bitmap
         */
        public Bitmap GetBitmap()
        {
            Bitmap bmp = new Bitmap(mWidth, mHeight);
            BitmapData bmpData
                = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.WriteOnly,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadBuffer(ReadBufferMode.Front);

            GL.ReadPixels(
                0, 0,
                mWidth, mHeight,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                bmpData.Scan0);

            // debug
            //byte[] ba = new byte[32];
            //unsafe
            //{
            //    byte* p = (byte*)bmpData.Scan0;

            //    for (int i = 0; i < 32; i++)
            //    {
            //        ba[i] = *(p + i);
            //    }
            //}

            bmp.UnlockBits(bmpData);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }
    }
}
