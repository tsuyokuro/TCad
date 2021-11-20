using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFont
{
    public class FontRenderer
    {
        public static string VertexShaderSrc =
@"
void main(void)
{
	gl_FrontColor = gl_Color;
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
}
";

        public static string FragmentShaderSrc =
@"
uniform sampler2D tex;

void main()
{
	vec4 a = texture2D(tex, gl_TexCoord[0].st);
	vec4 color = gl_Color;
	color[3] = color[3] * a[3];
	gl_FragColor = color;
}
";

        public int Texture = -1;
        public int FontShaderProgram = -1;

        private bool mInitialized = false;

        public bool Initialized
        {
            get => mInitialized;
        }

        public void Init()
        {
            Dispose();

            Texture = GL.GenTexture();

            SetupFontShader();

            mInitialized = true;
        }

        public void Dispose()
        {
            if (mInitialized)
            {
                GL.DeleteTexture(Texture);
                GL.DeleteProgram(FontShaderProgram);
            }

            mInitialized = false;
        }

        private void SetupFontShader()
        {
            string vertexSrc = VertexShaderSrc;
            string fragmentSrc = FragmentShaderSrc;

            int status;

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSrc);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new ApplicationException(GL.GetShaderInfoLog(vertexShader));
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSrc);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new ApplicationException(GL.GetShaderInfoLog(fragmentShader));
            }

            int shaderProgram = GL.CreateProgram();

            //各シェーダオブジェクトをシェーダプログラムへ登録
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            //不要になった各シェーダオブジェクトを削除
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //シェーダプログラムのリンク
            GL.LinkProgram(shaderProgram);

            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);

            //シェーダプログラムのリンクのチェック
            if (status == 0)
            {
                throw new ApplicationException(GL.GetProgramInfoLog(shaderProgram));
            }

            FontShaderProgram = shaderProgram;
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
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(TextureTarget.Texture2D, Texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Alpha8,
                tex.W, tex.H, 0,
                PixelFormat.Alpha,
                PixelType.UnsignedByte, tex.Data);


            GL.UseProgram(FontShaderProgram);

            GL.TexCoord2(1.0, 1.0);

            int texLoc = GL.GetUniformLocation(FontShaderProgram, "tex");

            GL.Uniform1(texLoc, 0);

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
}
