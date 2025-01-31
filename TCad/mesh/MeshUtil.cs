using CadDataTypes;
using CarveWapper;
using MeshMakerNS;
using MyCollections;
using OpenTK.Mathematics;
using Plotter;
using System;
using System.Collections.Generic;

namespace MeshUtilNS;

public class MeshUtil
{
    public static CadMesh MoveMesh(CadMesh cm, vector3_t mv)
    {
        for (int i = 0; i < cm.VertexStore.Count; i++)
        {
            cm.VertexStore[i] += mv;
        }

        return cm;
    }

    public static CadMesh ScaleMesh(CadMesh cm, vcompo_t scale)
    {
        for (int i = 0; i < cm.VertexStore.Count; i++)
        {
            cm.VertexStore[i] *= scale;
        }

        return cm;
    }

    // 全てのFaceを3角形に分割する
    public static void SplitAllFaceToTriangle(CadMesh mesh)
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

        vector3_t tp0 = mesh.VertexStore[ triangle.VList[0] ].vector;
        vector3_t tp1 = mesh.VertexStore[ triangle.VList[1] ].vector;
        vector3_t tp2 = mesh.VertexStore[ triangle.VList[2] ].vector;
        
        vector3_t dir = CadMath.Normal(tp1, tp0, tp2);
        vector3_t currentDir = vector3_t.Zero;

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

            vcompo_t scala = CadMath.InnerProduct(dir, currentDir);

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
            vector3_t fv = mesh.VertexStore[ face.VList[i] ].vector;

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

        vcompo_t maxd = 0;

        for (i = 0; i < f.VList.Count; i++)
        {
            CadVertex fp = mesh.VertexStore[ f.VList[i] ];

            t = fp - p0;
            vcompo_t d = t.Norm();

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

    public static bool IsPointInTriangle(vector3_t p, CadFace triangle, CadMesh mesh)
    {
        if (triangle.VList.Count < 3)
        {
            return false;
        }

        vector3_t p0 = mesh.VertexStore[ triangle.VList[0] ].vector;
        vector3_t p1 = mesh.VertexStore[ triangle.VList[1] ].vector;
        vector3_t p2 = mesh.VertexStore[ triangle.VList[2] ].vector;

        vector3_t c1 = CadMath.OuterProduct(p, p0, p1);
        vector3_t c2 = CadMath.OuterProduct(p, p1, p2);
        vector3_t c3 = CadMath.OuterProduct(p, p2, p0);

        vcompo_t ip12 = CadMath.InnerProduct(c1, c2);
        vcompo_t ip13 = CadMath.InnerProduct(c1, c3);


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
        CadMesh src, vector3_t p0, vector3_t p1, vector3_t normal)
    {
        vector3_t wv = (p1 - p0).UnitVector();
        vector3_t hv = normal;

        CadMesh cubeA = MeshMaker.CreateUnitCube(wv, hv, MeshMaker.FaceType.TRIANGLE);
        MoveMesh(cubeA, -hv / 2);
        ScaleMesh(cubeA, 10000);
        MoveMesh(cubeA, (p1 - p0) / 2 + p0);

        CadMesh cubeB = MeshMaker.CreateUnitCube(wv, hv, MeshMaker.FaceType.TRIANGLE);
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
            Log.pl(e.Message);
            return (null, null);
        }

        MeshUtil.SplitAllFaceToTriangle(m1);

        CadMesh m2;
        try
        {
            m2 = CarveW.AMinusB(src, cubeB);
        }
        catch (Exception)
        {
            return (null, null);
        }

        MeshUtil.SplitAllFaceToTriangle(m2);

        return (m1, m2);
    }
}
