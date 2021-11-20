using System;
using System.IO;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    class CadDxfLoader
    {
        public enum States
        {
            ON_GOING,
            COMPLETE,
            ERROR,
        }

        public delegate void Progress(States state, int percent, CadMesh mesh);

        public async void AsyncLoad(string fname, double scale, Progress progress)
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

        public CadMesh Load(string fname, double scale)
        {
            TotalPointCount = 0;
            TotalFaceCount = 0;

            CadMesh mesh = new CadMesh(10,10);

            StreamReader reader = new StreamReader(fname);

            string L1;
            string L2;

            DxfState state = DxfState.STATE_NONE;
            int valCnt = 0;


            double[] val = new double[3];

            int code;

            VertexList pointList = new VertexList();

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
                        AddFace(mesh, pointList);
                        TotalFaceCount++;

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

                    val[valCnt] = Double.Parse(L2) * scale;
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
}
