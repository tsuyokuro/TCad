using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLFont;

public class FontShader
{
    public static string VertexShaderSrc =
        """
        void main(void)
        {
            gl_FrontColor = gl_Color;
            gl_TexCoord[0] = gl_MultiTexCoord0;
            gl_Position = ftransform();
        }
        """;

    public static string FragmentShaderSrc =
        """
        uniform sampler2D tex;

        void main()
        {
            vec4 a = texture2D(tex, gl_TexCoord[0].st);
            vec4 color = gl_Color;
            color[3] = color[3] * a[3];
            gl_FragColor = color;
        }
        """;

    private int ShaderProgram = -1;

    private static FontShader sInstance;

    public static FontShader GetInstance()
    {
        if (sInstance == null)
        {
            sInstance = new FontShader();
            sInstance.SetupShader();
        }

        return sInstance;
    }

    private void SetupShader()
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

        ShaderProgram = shaderProgram;
    }

    public void Dispose()
    {
        if (ShaderProgram != -1)
        {
            GL.DeleteProgram(ShaderProgram);
        }
    }

    public void Start(int texUnitNumber)
    {
        GL.UseProgram(ShaderProgram);
        int texLoc = GL.GetUniformLocation(ShaderProgram, "tex");
        GL.Uniform1(texLoc, texUnitNumber);
    }

    public void End()
    {
        GL.UseProgram(0);
    }
}
