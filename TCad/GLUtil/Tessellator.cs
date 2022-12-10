//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenGL.GLU;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using Plotter;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;



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


namespace GLUtil;

public class Tessellator
{
    private IntPtr pTess = 0;

    private Glu.TessBeginCallback MeshBeginCallback;
    private Glu.TessEndCallback MeshEndCallback;
    private Glu.TessVertexCallback MeshVertexCallback;
    private Glu.TessCombineCallback MeshCombineCallback;
    private Glu.TessErrorCallback MeshErrorCallback;

    private List<GCHandle> TempGCHs = new();

    private BeginMode CurrentMode;

    public class VertexContour
    {
        public List<vector3_t> VList = new();
    }

    public Tessellator()
    {
        pTess = Glu.NewTess();

        MeshBeginCallback = MeshBegin;
        MeshEndCallback = MeshEnd;
        MeshVertexCallback = MeshVertex;
        MeshCombineCallback = MeshCombine;
        MeshErrorCallback = MeshError;
    }

    public void Dispose()
    {
        if (pTess != 0)
        {
            Glu.DeleteTess(pTess);
            pTess = 0;
        }
    }


    private CadMesh CurMesh;
    private CadFace CurFace;

    private int VCnt;

    private int FanIdx;

    private int StripIdx1;
    private int StripIdx2;

    public CadMesh Tessellate(List<List<int>> contourList, List<vector3_t> vertexList)
    {
        Glu.TessCallback(pTess, GluTessCallback.Begin, MeshBeginCallback);
        Glu.TessCallback(pTess, GluTessCallback.End, MeshEndCallback);
        Glu.TessCallback(pTess, GluTessCallback.Vertex, MeshVertexCallback);
        Glu.TessCallback(pTess, GluTessCallback.TessCombine, MeshCombineCallback);
        Glu.TessCallback(pTess, GluTessCallback.TessError, MeshErrorCallback);

        CurMesh = new CadMesh(vertexList.Count, 10);

        CadVertex cv = new();

        double[] tv = new double[3];

        Glu.TessNormal(pTess, new Vector3(0f, 0f, 1f));
        Glu.TessBeginPolygon(pTess, 0);

        for (int i = 0; i < contourList.Count; i++)
        {
            Glu.TessBeginContour(pTess);

            List<int> contour = contourList[i];

            for (int j = 0; j < contour.Count; j++)
            {
                int idx = contour[j];

                vector3_t v = vertexList[idx];
                tv[0] = v.X;
                tv[1] = v.Y;
                tv[2] = 0;

                cv.vector = v;

                CurMesh.VertexStore.Add(cv);

                Glu.TessVertex(pTess, tv, idx);
            }

            Glu.TessEndContour(pTess);
        }

        Glu.TessEndPolygon(pTess);

        FreeTempGCH();

        return CurMesh;
    }

    public CadMesh Tessellate(List<Vector3List> contourList)
    {
        List<vector3_t> vertexList = new();
        List<List<int>> indexContourList = new();

        int idx = 0;

        for (int i = 0; i < contourList.Count; i++)
        {
            Vector3List vcont = contourList[i];
            List<int> icont = new();

            for (int j = 0; j < vcont.Count; j++)
            {
                vector3_t v = vcont[j];
                vertexList.Add(v);
                icont.Add(idx);

                idx++;
            }

            indexContourList.Add(icont);
        }

        return Tessellate(indexContourList, vertexList);
    }


    private void FreeTempGCH()
    {
        for (int i = 0; i < TempGCHs.Count; i++)
        {
            TempGCHs[i].Free();
        }

        TempGCHs.Clear();
    }

    private void MeshBegin(int mode)
    {
        CurrentMode = (BeginMode)mode;
        VCnt = 0;
        DOut.pl("MeshBegin PrimitiveType:" + CurrentMode.ToString());
    }

    private void MeshEnd()
    {
        DOut.pl("MeshEnd");
    }

    private void MeshVertex(IntPtr data)
    {
        //int vIndex = (int)GCHandle.FromIntPtr(data).Target;

        // data is not address, it is value of vertex index
        int vIndex = (int)data;
        DOut.pl("MeshVertex vIndex:" + vIndex);

        switch (CurrentMode)
        {
            case BeginMode.TriangleFan:
                if (VCnt == 0)
                {
                    FanIdx = vIndex;

                    CurFace = new();
                    CurFace.VList.Add(vIndex);
                }
                else if (VCnt == 1)
                {
                    CurFace.VList.Add(vIndex);
                }
                else if (VCnt == 2)
                {
                    CurFace.VList.Add(vIndex);
                    CurMesh.FaceStore.Add(CurFace);
                }
                else if (VCnt >= 3)
                {
                    CurFace = new();
                    CadFace prevFace = CurMesh.FaceStore.End();
                    CurFace.VList.Add(FanIdx);
                    CurFace.VList.Add(prevFace.VList[2]);
                    CurFace.VList.Add(vIndex);
                    CurMesh.FaceStore.Add(CurFace);
                }

                VCnt++;
                break;
            case BeginMode.TriangleStrip:
                if (VCnt == 0)
                {
                    CurFace = new();
                    CurFace.VList.Add(vIndex);
                }
                else if (VCnt == 1)
                {
                    CurFace.VList.Add(vIndex);
                }
                else if (VCnt == 2)
                {
                    CurFace.VList.Add(vIndex);
                    CurMesh.FaceStore.Add(CurFace);
                }
                else if (VCnt >= 3)
                {
                    CurFace = new();

                    int rem = VCnt % 2;
                    if (rem == 0)
                    {
                        CurFace.VList.Add(StripIdx1);
                        CurFace.VList.Add(StripIdx2);
                        CurFace.VList.Add(vIndex);
                        CurMesh.FaceStore.Add(CurFace);
                    }
                    else
                    {
                        CurFace.VList.Add(StripIdx2);
                        CurFace.VList.Add(StripIdx1);
                        CurFace.VList.Add(vIndex);
                        CurMesh.FaceStore.Add(CurFace);
                    }
                }

                StripIdx1 = StripIdx2;
                StripIdx2 = vIndex;
                VCnt++;
                break;
            case BeginMode.Triangles:
                if (VCnt == 0)
                {
                    CurFace = new();
                    CurFace.VList.Add(vIndex);
                }
                else if (VCnt == 1)
                {
                    CurFace.VList.Add(vIndex);
                }
                else if (VCnt == 2)
                {
                    CurFace.VList.Add(vIndex);
                    CurMesh.FaceStore.Add(CurFace);
                }

                VCnt++;
                if (VCnt == 3)
                {
                    VCnt = 0;
                }

                break;

            default:
                break;
        }
    }

    private void MeshCombine(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] double[] coords,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] double[] data,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] weight,
        ref IntPtr dataOut)
    {
        DOut.pl("MeshCombine");
        CadVertex v = new();
        v.X = (vcompo_t)coords[0]; v.Y = (vcompo_t)coords[1]; v.Z = (vcompo_t)coords[2];
        CurMesh.VertexStore.Add(v);

        int vi = CurMesh.VertexStore.Count - 1;

        //GCHandle gch = GCHandle.Alloc(vi, GCHandleType.Pinned);
        //TempGCHs.Add(gch);
        //IntPtr ptr = GCHandle.ToIntPtr(gch);
        //dataOut = ptr;

        dataOut = vi;
    }

    void MeshError(int err)
    {
        DOut.pl("MeshError err:" + err);
    }
}
