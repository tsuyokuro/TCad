using CadDataTypes;
using CarveWapper;
using HalfEdgeNS;
using MeshMakerNS;
using MyCollections;
using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;

namespace MeshUtilNS
{
    public class MeshUtil
    {
        public static CadMesh MoveMesh(CadMesh cm, Vector3d mv)
        {
            for (int i = 0; i < cm.VertexStore.Count; i++)
            {
                cm.VertexStore.Ref(i) += mv;
            }

            return cm;
        }

        public static CadMesh ScaleMesh(CadMesh cm, double scale)
        {
            for (int i = 0; i < cm.VertexStore.Count; i++)
            {
                cm.VertexStore.Ref(i) *= scale;
            }

            return cm;
        }

        // 全てのFaceを3角形に分割する
        public static void SplitAllFace(CadMesh mesh)
        {
            FlexArray<CadFace> faceStore = new FlexArray<CadFace>();

            for (int i=0; i<mesh.FaceStore.Count; i++)
            {
                CadFace face = mesh.FaceStore[i];
                if (face.VList.Count < 3)
                {
                    continue;
                }

                if (face.VList.Count == 3)
                {
                    faceStore.Add(face);
                    continue;
                }

                List<CadFace> flist = Split(face, mesh);

                faceStore.AddRange(flist);
            }

            mesh.FaceStore = faceStore;
        }

        // Faceを三角形に分割する
        public static List<CadFace> Split(CadFace face, CadMesh mesh)
        {
            CadVertex p0 = default(CadVertex);

            // Deep copy
            CadFace src = new CadFace(face);

            var triangles = new List<CadFace>();

            int i1 = -1;

            int state = 0;

            CadFace triangle;

            i1 = FindMaxDistantPointIndex(p0, src, mesh);

            if (i1 == -1)
            {
                return triangles;
            }

            triangle = GetTriangleWithCenterPoint(src, i1);

            Vector3d tp0 = mesh.VertexStore[ triangle.VList[0] ].vector;
            Vector3d tp1 = mesh.VertexStore[ triangle.VList[1] ].vector;
            Vector3d tp2 = mesh.VertexStore[ triangle.VList[2] ].vector;
            
            Vector3d dir = CadMath.Normal(tp1, tp0, tp2);
            Vector3d currentDir = Vector3d.Zero;

            while (src.VList.Count > 3)
            {
                if (state == 0)
                {
                    i1 = FindMaxDistantPointIndex(p0, src, mesh);
                    if (i1 == -1)
                    {
                        return triangles;
                    }
                }

                triangle = GetTriangleWithCenterPoint(src, i1);

                tp0 = mesh.VertexStore[ triangle.VList[0] ].vector;
                tp1 = mesh.VertexStore[ triangle.VList[1] ].vector;
                tp2 = mesh.VertexStore[ triangle.VList[2] ].vector;

                currentDir = CadMath.Normal(tp1, tp0, tp2);

                bool hasIn = ListContainsPointInTriangle(triangle, src, mesh);

                double scala = CadMath.InnerProduct(dir, currentDir);

                if (!hasIn && (scala > 0))
                {
                    triangles.Add(triangle);
                    src.VList.RemoveAt(i1);
                    state = 0;
                    continue;
                }

                if (state == 0)
                {
                    state = 1;
                    i1 = 0;
                }
                else if (state == 1)
                {
                    i1++;
                    if (i1 >= src.VList.Count)
                    {
                        break;
                    }
                }
            }

            if (src.VList.Count == 3)
            {
                triangle = new CadFace();
                triangle.VList.Add(src.VList[0]);
                triangle.VList.Add(src.VList[1]);
                triangle.VList.Add(src.VList[2]);

                triangles.Add(triangle);
            }

            return triangles;
        }

        private static bool ListContainsPointInTriangle(CadFace triangle, CadFace face, CadMesh mesh)
        {
            FlexArray<int> tps = triangle.VList;

            for (int i=0; i< face.VList.Count; i++)
            {
                Vector3d fv = mesh.VertexStore[ face.VList[i] ].vector;

                if (
                    fv.Equals(mesh.VertexStore[ tps[0] ].vector) ||
                    fv.Equals(mesh.VertexStore[ tps[1] ].vector) ||
                    fv.Equals(mesh.VertexStore[ tps[2] ].vector)
                    )
                {
                    continue;
                }

                bool ret = IsPointInTriangle(fv, triangle, mesh);
                if (ret)
                {
                    return true;
                }
            }

            return false;
        }

        public static int FindMaxDistantPointIndex(CadVertex p0, CadFace f, CadMesh mesh)
        {
            int ret = -1;
            int i;

            CadVertex t;

            double maxd = 0;

            for (i = 0; i < f.VList.Count; i++)
            {
                CadVertex fp = mesh.VertexStore[ f.VList[i] ];

                t = fp - p0;
                double d = t.Norm();

                if (d > maxd)
                {
                    maxd = d;
                    ret = i;
                }
            }

            return ret;
        }

        private static CadFace GetTriangleWithCenterPoint(CadFace face, int cpIndex)
        {
            int i1 = cpIndex;
            int endi = face.VList.Count - 1;

            int i0 = i1 - 1;
            int i2 = i1 + 1;

            if (i0 < 0) { i0 = endi; }
            if (i2 > endi) { i2 = 0; }

            var triangle = new CadFace();

            triangle.VList.Add(face.VList[i0]);
            triangle.VList.Add(face.VList[i1]);
            triangle.VList.Add(face.VList[i2]);

            return triangle;
        }

        public static bool IsPointInTriangle(Vector3d p, CadFace triangle, CadMesh mesh)
        {
            if (triangle.VList.Count < 3)
            {
                return false;
            }

            Vector3d p0 = mesh.VertexStore[ triangle.VList[0] ].vector;
            Vector3d p1 = mesh.VertexStore[ triangle.VList[1] ].vector;
            Vector3d p2 = mesh.VertexStore[ triangle.VList[2] ].vector;

            Vector3d c1 = CadMath.CrossProduct(p, p0, p1);
            Vector3d c2 = CadMath.CrossProduct(p, p1, p2);
            Vector3d c3 = CadMath.CrossProduct(p, p2, p0);

            double ip12 = CadMath.InnerProduct(c1, c2);
            double ip13 = CadMath.InnerProduct(c1, c3);


            // When all corossProduct result's sign are same, Point is in triangle
            if (ip12 > 0 && ip13 > 0)
            {
                return true;
            }

            return false;
        }

        public static CadMesh CreateFrom(VertexList vl)
        {
            if (vl.Count < 2)
            {
                return null;
            }

            CadMesh m = new CadMesh(vl.Count, 1);

            m.VertexStore.AddRange(vl);

            int i;

            FlexArray<int> nl = new FlexArray<int>(vl.Count);

            for (i=0; i<vl.Count; i++)
            {
                nl.Add(i);
            }

            CadFace face = new CadFace(nl);

            m.FaceStore.Add(face);

            return m;
        }

        public static (CadMesh m1, CadMesh m2) CutMeshWithVector(
            CadMesh src, Vector3d p0, Vector3d p1, Vector3d normal)
        {
            Vector3d wv = (p1 - p0).UnitVector();
            Vector3d hv = normal;

            CadMesh cubeA = MeshMaker.CreateUnitCube(wv, hv, MeshMaker.FaceType.QUADRANGLE);
            MoveMesh(cubeA, -hv / 2);
            ScaleMesh(cubeA, 10000);
            MoveMesh(cubeA, (p1 - p0) / 2 + p0);

            CadMesh cubeB = MeshMaker.CreateUnitCube(wv, hv, MeshMaker.FaceType.QUADRANGLE);
            MoveMesh(cubeB, hv / 2);
            ScaleMesh(cubeB, 10000);
            MoveMesh(cubeB, (p1 - p0) / 2 + p0);


            CadMesh m1;
            try
            {
                m1 = CarveW.AMinusB(src, cubeA);
            }
            catch (Exception e)
            {
                return (null, null);
            }

            MeshUtil.SplitAllFace(m1);

            CadMesh m2;
            try
            {
                m2 = CarveW.AMinusB(src, cubeB);
            }
            catch (Exception e)
            {
                return (null, null);
            }

            MeshUtil.SplitAllFace(m2);

            return (m1, m2);
        }
    }
}
