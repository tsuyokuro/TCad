using System;
using System.Runtime.InteropServices;

namespace OpenGL.GLU;

static partial class Glu
{
    public delegate void TessBeginCallback(int type);
    public delegate void TessBeginDataCallback(int type, [In] IntPtr polygonData);
    public delegate void TessCombineCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)][In] double[] coordinates, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)][In] double[] vertexData, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)][In] float[] weight, ref IntPtr outData);
    public delegate void TessCombineDataCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)][In] double[] coordinates, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)][In] double[] vertexData, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)][In] float[] weight, ref IntPtr outData, [In] IntPtr polygonData);
    public delegate void TessEdgeFlagCallback(int flag);
    public delegate void TessEdgeFlagDataCallback(int flag, [In] IntPtr polygonData);
    public delegate void TessEndCallback();
    public delegate void TessEndDataCallback(IntPtr polygonData);
    public delegate void TessErrorCallback(int errorCode);
    public delegate void TessErrorDataCallback(int errorCode, [In] IntPtr polygonData);
    public delegate void TessVertexCallback([In] IntPtr vertexData);
    public delegate void TessVertexCallback1([In] double[] vertexData);
    public delegate void TessVertexDataCallback([In] IntPtr vertexData, [In] IntPtr polygonData);
}
