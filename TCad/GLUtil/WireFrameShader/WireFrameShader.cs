using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using TCad.Logger;
using TCad.Util;

namespace GLUtil;


public class WireFrameShader
{
    private int ShaderProgram = -1;

    // Uniform location
    public int modelViewMatrixLocation = -1;

    public int projectionMatrixLocation = -1;

    public int objColorLocation = -1;

    public int lightDirLocation = -1;

    // Attribute location
    public int posLocation = -1;

    public int normalLocation = -1;

    public int barycentriclLocation = -1;

    public WireFrameShader()
    {
        SetupShader();
    }

    private void SetupShader()
    {
        Log.plx("in");

        string vertexSrc = ResourceLoader.LoadString("/GLUtil/WireFrameShader/shader1/VertexShader.vert");
        string fragmentSrc = ResourceLoader.LoadString("/GLUtil/WireFrameShader/shader1/FragmentShader.frag");

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
            String s = GL.GetShaderInfoLog(fragmentShader);

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
        objColorLocation = GL.GetUniformLocation(ShaderProgram, "uObjColor");
        lightDirLocation = GL.GetUniformLocation(ShaderProgram, "uLightDir");

        posLocation = GL.GetAttribLocation(ShaderProgram, "aPos");
        normalLocation = GL.GetAttribLocation(ShaderProgram, "aNormal");
        barycentriclLocation = GL.GetAttribLocation(ShaderProgram, "aBarycentric");

        Log.plx("out");
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
        if (modelViewMatrixLocation < 0) return;

        GL.UniformMatrix4(modelViewMatrixLocation, 1, false, ref v.Row0.X);
    }

    public void SetProjectionMatrix(matrix4_t v)
    {
        if (projectionMatrixLocation < 0) return;
        GL.UniformMatrix4(projectionMatrixLocation, 1, false, ref v.Row0.X);
    }

    public void SetObjColor(Color4 color)
    {
        if (objColorLocation < 0) return;

        GL.Uniform4(objColorLocation, (float)color.R, (float)color.G, (float)color.B, (float)color.A);
    }

    public void SetLightDir(vector3_t v)
    {
        if (lightDirLocation < 0) return;

        GL.Uniform3(lightDirLocation, (float)v.X, (float)v.Y, (float)v.Z);
    }

    public void End()
    {
        GL.UseProgram(0);
    }
}
