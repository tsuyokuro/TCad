using OpenTK.Graphics.OpenGL;
using System;

namespace Plotter;

public class WireFrameShader
{
    public static string VertexShaderSrc =
        """
        #version 460 core

        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 incolor;
        layout(location = 2) in vec3 barycentric;

        out vec4 vertexColor;
        out vec3 baryxyz;

        uniform mat4 modelViewMatrix;
        uniform mat4 projectionMatrix;

        void main()
        {
          gl_Position = projectionMatrix * modelViewMatrix * vec4(aPos, 1.0);
          vertexColor = vec4(incolor, 1.0);
          baryxyz = barycentric;
        }
        """;

    public static string FragmentShaderSrc =
        """
        #version 460 core

        out vec4 FragColor;
        in vec4 vertexColor;

        in vec3 baryxyz;

        const float lineWidth = 1.0;

        const vec3 lineColor = vec3(1.0, 1.0, 1.0);

        float edgeFactor() {
          vec3 d = fwidth( baryxyz );
          vec3 f = step( d * lineWidth, baryxyz );
          return min( min( f.x, f.y ), f.z );
        }

        void main()
        {
          FragColor.rgb = mix(
            lineColor,
            vertexColor.xyz,
            edgeFactor()
          );
        }
        """;

    private int ShaderProgram = -1;

    private static WireFrameShader sInstance;

    public static WireFrameShader GetInstance()
    {
        if (sInstance == null)
        {
            sInstance = new WireFrameShader();
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
            ShaderProgram = -1;
        }
    }

    public void Start(int texUnitNumber)
    {
        GL.UseProgram(ShaderProgram);
    }

    public void End()
    {
        GL.UseProgram(0);
    }
}
