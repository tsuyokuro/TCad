using System;
using System.IO;
using System.Threading.Tasks;
using CadDataTypes;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter;

class CadDxfLoader
{
    public enum States
    {
        ON_GOING,
        COMPLETE,
        ERROR,
    }

    public delegate void Progress(States state, int percent, CadMesh mesh);

    public async void AsyncLoad(string fname, vcompo_t scale, Progress progress)
    {
        CadMesh mesh = await Task.Run(() => Load(fname, scale));

        progress(States.COMPLETE, 100, mesh);
    }

    private enum DxfState
    {
        STATE_NONE,
        STATE_3DFACE,
    }

    public int TotalPointCount;

    public int TotalFaceCount;

    public CadMesh Load(string fname, vcompo_t scale)
    {
        TotalPointCount = 0;
        TotalFaceCount = 0;

        CadMesh mesh = new CadMesh(10,10);

        StreamReader reader = new StreamReader(fname);

        string L1;
        string L2;

        DxfState state = DxfState.STATE_NONE;
        int valCnt = 0;


        vcompo_t[] val = new vcompo_t[3];

        int code;

        VertexList pointList = new VertexList();

        VertexList tpList = new VertexList(3);

        while (!reader.EndOfStream)
        {
            L1 = reader.ReadLine();
            L2 = reader.ReadLine();

            code = Int32.Parse(L1);
            L2 = L2.Trim();


            if (code == 0)
            {
                if (L2 == "3DFACE")
                {
                    state = DxfState.STATE_3DFACE;
                    valCnt = 0;
                }

                if (pointList.Count > 0)
                {
                    if (pointList.Count == 3)
                    {
                        AddFace(mesh, pointList);
                        TotalFaceCount++;
                    }
                    else if (pointList.Count == 4)
                    {
                        tpList.Clear();
                        tpList.Add(pointList[0]);
                        tpList.Add(pointList[1]);
                        tpList.Add(pointList[2]);
                        AddFace(mesh, tpList);
                        TotalFaceCount++;

                        tpList.Clear();
                        tpList.Add(pointList[2]);
                        tpList.Add(pointList[3]);
                        tpList.Add(pointList[0]);
                        AddFace(mesh, tpList);
                        TotalFaceCount++;
                    }
                    else
                    {
                        DOut.pl("pointList.Count:" + pointList.Count);
                    }

                    pointList.Clear();
                }

                if (valCnt != 0)
                {
                    state = 0;
                }
            }

            if (state == DxfState.STATE_3DFACE)
            {
                if (code < 10)
                {
                    continue;
                }

                val[valCnt] = vcompo_t.Parse(L2) * scale;
                valCnt++;

                if (valCnt >= 3)
                {
                    pointList.Add(CadVertex.Create(val[2], val[1], val[0]));
                    valCnt = 0;

                    TotalPointCount++;
                }

                continue;
            }
        }

        return mesh;
    }

    private void AddFace(CadMesh mesh, VertexList plist)
    {
        if (plist.Count == 0)
        {
            return;
        }

        int pidx;

        CadFace f = new CadFace();

        for (int i=0; i<plist.Count; i++)
        {
            pidx = mesh.VertexStore.Add(plist[i]);
            f.VList.Add(pidx);
        }

        mesh.FaceStore.Add(f);
    }
}
