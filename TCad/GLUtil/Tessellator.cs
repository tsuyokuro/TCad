using CadDataTypes;
using OpenGL.GLU;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using Plotter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using static IronPython.Modules.PythonIterTools;
using static OpenTK.Graphics.OpenGL.GL;

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

    public class Contour
    {
        public List<int> IndexList = new();
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

    public CadMesh Tessellate(List<Contour> contourList, List<Vector3d> vertexList)
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

            Contour contour = contourList[i];

            for (int j = 0; j < contour.IndexList.Count; j++)
            {
                int idx = contour.IndexList[j];

                Vector3d v = vertexList[idx];
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
        v.X = coords[0]; v.Y = coords[1]; v.Z = coords[2];
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
