using OpenTK.Graphics.OpenGL;
using System;
using TCad.Plotter.DrawContexts;

namespace GLUtil;


public class WireFrameShader
{
    /**
        // 座標変換
        public static string VertexShaderSrc =
            """
            # version 120
            void main(void)
            {
              gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }
            """;

        // 赤だけ返す
        public static string FragmentShaderSrc =
            """
            # version 120
            void main (void)
            {
                gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
            }
            """;
    **/

    /*
    public static string VertexShaderSrc =
        """
        # version 460 core

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
        # version 460 core

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
    */

    public static string VertexShaderSrc =
        """
        # version 460 core

        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 normal;

        uniform mat4 modelViewMatrix;
        uniform mat4 projectionMatrix;

        void main()
        {
          gl_Position = projectionMatrix * modelViewMatrix * vec4(aPos, 1.0);
        }
        """;

    public static string FragmentShaderSrc =
        """
        # version 460 core

        out vec4 FragColor;
        
        void main()
        {
          FragColor = vec4(1.0, 0.0, 0.0, 1.0);
        }
        """;

    private int ShaderProgram = -1;

    public int modelViewMatrixLocation = -1;

    public int projectionMatrixLocation = -1;


    public WireFrameShader()
    {
        SetupShader();
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

        // unifrom変数の位置取得
        modelViewMatrixLocation = GL.GetUniformLocation(ShaderProgram, "modelViewMatrix");
        projectionMatrixLocation = GL.GetUniformLocation(ShaderProgram, "projectionMatrix");
    }

    public void Dispose()
    {
        if (ShaderProgram != -1)
        {
            GL.DeleteProgram(ShaderProgram);
            ShaderProgram = -1;
        }
    }

    public void Start()
    {
        GL.UseProgram(ShaderProgram);
    }

    public void SetModelViewMatrix(matrix4_t v)
    {
        GL.UniformMatrix4(modelViewMatrixLocation, 1, false, ref v.Row0.X);
    }

    public void SetProjectionMatrix(matrix4_t v)
    {
        GL.UniformMatrix4(projectionMatrixLocation, 1, false, ref v.Row0.X);
    }

    public void End()
    {
        GL.UseProgram(0);
    }
}
