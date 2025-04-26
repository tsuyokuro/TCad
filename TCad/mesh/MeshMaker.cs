using CadDataTypes;
using MeshUtilNS;
using TCad.Plotter;
using System;
using TCad.MathFunctions;

namespace MeshMakerNS;

public class MeshMaker
{
    public enum FaceType
    {
        TRIANGLE,
        QUADRANGLE,
    }

    public static CadMesh CreateBox(vector3_t pos, vector3_t sizeV, FaceType faceType = FaceType.TRIANGLE)
    {
        CadMesh cm = CreateUnitCube(faceType);

        for (int i = 0; i < cm.VertexStore.Count; i++)
        {
            cm.VertexStore[i] *= sizeV;
            cm.VertexStore[i] += pos;
        }

        return cm;
    }

    // 単位立方体作成
    public static CadMesh CreateUnitCube(FaceType faceType = FaceType.TRIANGLE)
    {
        CadMesh cm = new CadMesh(8, 12);

        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(+0.5), (vcompo_t)(+0.5), (vcompo_t)(+0.5)));
        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(-0.5), (vcompo_t)(+0.5), (vcompo_t)(+0.5)));
        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(-0.5), (vcompo_t)(-0.5), (vcompo_t)(+0.5)));
        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(+0.5), (vcompo_t)(-0.5), (vcompo_t)(+0.5)));

        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(+0.5), (vcompo_t)(+0.5), (vcompo_t)(-0.5)));
        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(-0.5), (vcompo_t)(+0.5), (vcompo_t)(-0.5)));
        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(-0.5), (vcompo_t)(-0.5), (vcompo_t)(-0.5)));
        cm.VertexStore.Add(CadVertex.Create((vcompo_t)(+0.5), (vcompo_t)(-0.5), (vcompo_t)(-0.5)));

        if (faceType == FaceType.QUADRANGLE)
        {
            cm.FaceStore.Add(new CadFace(0, 1, 2, 3));

            cm.FaceStore.Add(new CadFace(7, 6, 5, 4));

            cm.FaceStore.Add(new CadFace(0, 4, 5, 1));

            cm.FaceStore.Add(new CadFace(1, 5, 6, 2));

            cm.FaceStore.Add(new CadFace(2, 6, 7, 3));

            cm.FaceStore.Add(new CadFace(3, 7, 4, 0));
        }
        else
        {
            cm.FaceStore.Add(new CadFace(0, 1, 2));
            cm.FaceStore.Add(new CadFace(2, 3, 0));

            cm.FaceStore.Add(new CadFace(7, 6, 5));
            cm.FaceStore.Add(new CadFace(5, 4, 7));

            cm.FaceStore.Add(new CadFace(0, 4, 5));
            cm.FaceStore.Add(new CadFace(5, 1, 0));

            cm.FaceStore.Add(new CadFace(1, 5, 6));
            cm.FaceStore.Add(new CadFace(6, 2, 1));

            cm.FaceStore.Add(new CadFace(2, 6, 7));
            cm.FaceStore.Add(new CadFace(7, 3, 2));

            cm.FaceStore.Add(new CadFace(3, 7, 4));
            cm.FaceStore.Add(new CadFace(4, 0, 3));
        }

        return cm;
    }

    public static CadMesh CreateUnitCube(vector3_t wv, vector3_t hv, FaceType faceType = FaceType.TRIANGLE)
    {
        vector3_t tv = CadMath.Normal(wv, hv);

        vector3_t wv2 = wv / 2;
        vector3_t hv2 = hv / 2;
        vector3_t tv2 = tv / 2;

        CadMesh cm = new CadMesh(8, 12);

        cm.VertexStore.Add(CadVertex.Create(wv2 + hv2 + tv2));
        cm.VertexStore.Add(CadVertex.Create(-wv2 + hv2 + tv2));
        cm.VertexStore.Add(CadVertex.Create(-wv2 - hv2 + tv2));
        cm.VertexStore.Add(CadVertex.Create(wv2 - hv2 + tv2));

        cm.VertexStore.Add(CadVertex.Create(wv2 + hv2 - tv2));
        cm.VertexStore.Add(CadVertex.Create(-wv2 + hv2 - tv2));
        cm.VertexStore.Add(CadVertex.Create(-wv2 - hv2 - tv2));
        cm.VertexStore.Add(CadVertex.Create(wv2 - hv2 - tv2));

        if (faceType == FaceType.QUADRANGLE)
        {
            cm.FaceStore.Add(new CadFace(0, 1, 2, 3));

            cm.FaceStore.Add(new CadFace(7, 6, 5, 4));

            cm.FaceStore.Add(new CadFace(0, 4, 5, 1));

            cm.FaceStore.Add(new CadFace(1, 5, 6, 2));

            cm.FaceStore.Add(new CadFace(2, 6, 7, 3));

            cm.FaceStore.Add(new CadFace(3, 7, 4, 0));
        }
        else
        {
            cm.FaceStore.Add(new CadFace(0, 1, 2));
            cm.FaceStore.Add(new CadFace(2, 3, 0));

            cm.FaceStore.Add(new CadFace(7, 6, 5));
            cm.FaceStore.Add(new CadFace(5, 4, 7));

            cm.FaceStore.Add(new CadFace(0, 4, 5));
            cm.FaceStore.Add(new CadFace(5, 1, 0));

            cm.FaceStore.Add(new CadFace(1, 5, 6));
            cm.FaceStore.Add(new CadFace(6, 2, 1));

            cm.FaceStore.Add(new CadFace(2, 6, 7));
            cm.FaceStore.Add(new CadFace(7, 3, 2));

            cm.FaceStore.Add(new CadFace(3, 7, 4));
            cm.FaceStore.Add(new CadFace(4, 0, 3));
        }

        return cm;
    }

    public static CadMesh CreateTetrahedron(vector3_t pos, vector3_t sizeV)
    {
        CadMesh cm = CreateUnitTetrahedron();

        for (int i = 0; i < cm.VertexStore.Count; i++)
        {
            cm.VertexStore[i] *= sizeV;
            cm.VertexStore[i] += pos;
        }

        return cm;
    }

    // 単位正四面体作成
    public static CadMesh CreateUnitTetrahedron()
    {
        CadMesh cm = new CadMesh(8, 12);

        var v0 = new CadVertex((vcompo_t)(-0.81649658), (vcompo_t)(-0.47140452), (vcompo_t)(-0.33333333));
        var v1 = new CadVertex((vcompo_t)(0.81649658), (vcompo_t)(-0.47140452), (vcompo_t)(-0.33333333));
        var v2 = new CadVertex((vcompo_t)(0.00000000), (vcompo_t)(0.00000000), (vcompo_t)(1.00000000));
        var v3 = new CadVertex((vcompo_t)(0.00000000), (vcompo_t)(0.94280904), (vcompo_t)(-0.33333333));

        cm.VertexStore.Add(v0);
        cm.VertexStore.Add(v1);
        cm.VertexStore.Add(v2);
        cm.VertexStore.Add(v3);

        cm.FaceStore.Add(new CadFace(3, 1, 0));
        cm.FaceStore.Add(new CadFace(2, 3, 0));
        cm.FaceStore.Add(new CadFace(1, 2, 0));
        cm.FaceStore.Add(new CadFace(3, 2, 1));

        return cm;
    }

    public static CadMesh CreateOctahedron(vector3_t pos, vector3_t sizeV)
    {
        CadMesh cm = CreateUnitOctahedron();

        for (int i = 0; i < cm.VertexStore.Count; i++)
        {
            cm.VertexStore[i] *= sizeV;
            cm.VertexStore[i] += pos;
        }

        return cm;
    }

    // 単位正八面体作成
    public static CadMesh CreateUnitOctahedron()
    {
        CadMesh cm = new CadMesh(8, 12);

        var v0 = new CadVertex((vcompo_t)(-0.70710678), (vcompo_t)(-0.70710678), (vcompo_t)(0.00000000));
        var v1 = new CadVertex((vcompo_t)(-0.70710678), (vcompo_t)(0.70710678), (vcompo_t)(0.00000000));
        var v2 = new CadVertex((vcompo_t)(0.70710678), (vcompo_t)(0.70710678), (vcompo_t)(0.00000000));
        var v3 = new CadVertex((vcompo_t)(0.70710678), (vcompo_t)(-0.70710678), (vcompo_t)(0.00000000));
        var v4 = new CadVertex((vcompo_t)(0.00000000), (vcompo_t)(0.00000000), (vcompo_t)(-1.00000000));
        var v5 = new CadVertex((vcompo_t)(0.00000000), (vcompo_t)(0.00000000), (vcompo_t)(1.00000000));

        cm.VertexStore.Add(v0);
        cm.VertexStore.Add(v1);
        cm.VertexStore.Add(v2);
        cm.VertexStore.Add(v3);
        cm.VertexStore.Add(v4);
        cm.VertexStore.Add(v5);

        cm.FaceStore.Add(new CadFace(0, 1, 4));
        cm.FaceStore.Add(new CadFace(0, 4, 3));
        cm.FaceStore.Add(new CadFace(0, 3, 5));
        cm.FaceStore.Add(new CadFace(0, 5, 1));

        cm.FaceStore.Add(new CadFace(1, 2, 4));
        cm.FaceStore.Add(new CadFace(1, 5, 2));
        cm.FaceStore.Add(new CadFace(2, 3, 4));
        cm.FaceStore.Add(new CadFace(2, 5, 3));

        return cm;
    }

    public static CadMesh CreateCylinder(vector3_t pos, int circleDiv, int slices, vcompo_t r, vcompo_t len)
    {
        CadMesh mesh = CreateCylinder(circleDiv, slices, r, len);

        for (int i = 0; i < mesh.VertexStore.Count; i++)
        {
            mesh.VertexStore[i] += pos;
        }

        return mesh;
    }

    public static CadMesh CreateCylinder(int circleDiv, int slices, vcompo_t r, vcompo_t len)
    {
        VertexList vl = new VertexList();

        vcompo_t sl = slices;

        vcompo_t dl = len / sl;
        vcompo_t y = len / 2;

        for (vcompo_t i = 0; i < sl; i++)
        {
            vl.Add(new CadVertex(r, y - (i * dl), 0));
        }

        vl.Add(new CadVertex(r, -len / 2, 0));

        return CreateRotatingBody(
            circleDiv, vector3_t.Zero, vector3_t.UnitY, vl, true, true, FaceType.TRIANGLE);
    }

    public static CadMesh CreateSphere(vector3_t pos, vcompo_t r, int slices1, int slices2)
    {
        VertexList vl = new VertexList(slices2);

        vcompo_t d = (vcompo_t)Math.PI / slices2;


        for (int i = 0; i < slices2; i++)
        {
            vcompo_t a = i * d;

            vcompo_t x = (vcompo_t)Math.Sin(a) * r;
            vcompo_t y = (vcompo_t)Math.Cos(a) * r;

            vl.Add(CadVertex.Create(x, y, 0));
        }

        vl.Add(CadVertex.Create(0, -r, 0));


        CadMesh mesh = CreateRotatingBody(
            slices1, vector3_t.Zero, vector3_t.UnitY, vl, false, false, FaceType.TRIANGLE);

        for (int i = 0; i < mesh.VertexStore.Count; i++)
        {
            mesh.VertexStore[i].vector += pos;
        }

        return mesh;
    }

    // 回転体の作成
    // 削除予定
    public static CadMesh CreateRotatingBody(int circleDiv, VertexList vl, FaceType facetype = FaceType.TRIANGLE)
    {
        if (vl.Count < 2)
        {
            return null;
        }

        CadMesh mesh = new CadMesh(vl.Count * circleDiv, vl.Count * circleDiv);

        // 上下端が中心軸にあるなら共有
        int s = 0;
        int e = vl.Count;

        int vc = vl.Count;

        bool topCap = false;
        bool bottomCap = false;

        int ps = 0;

        if (vl[0].X == 0)
        {
            mesh.VertexStore.Add(vl[0]);
            s += 1;
            topCap = true;
            vc--;
            ps++;
        }

        if (vl[vl.Count - 1].X == 0)
        {
            mesh.VertexStore.Add(vl[vl.Count - 1]);
            e -= 1;
            bottomCap = true;
            vc--;
            ps++;
        }

        vcompo_t d = (vcompo_t)Math.PI * (vcompo_t)(2.0) / circleDiv;

        for (int i = 0; i < circleDiv; i++)
        {
            vcompo_t a = i * d;

            for (int vi = s; vi < e; vi++)
            {
                CadVertex v = vl[vi];
                CadVertex vv = default(CadVertex);

                vv.X = v.X * (vcompo_t)Math.Cos(a);
                vv.Y = v.Y;
                vv.Z = v.X * (vcompo_t)Math.Sin(a);

                mesh.VertexStore.Add(vv);
            }
        }

        CadFace f;

        if (topCap)
        {
            for (int i = 0; i < circleDiv; i++)
            {
                f = new CadFace(0, ((i + 1) % circleDiv) * vc + ps, i * vc + ps);
                mesh.FaceStore.Add(f);
            }
        }

        if (bottomCap)
        {
            for (int i = 0; i < circleDiv; i++)
            {
                int bi = (vc - 1);

                f = new CadFace(1, (i * vc) + bi + ps, ((i + 1) % circleDiv) * vc + bi + ps);
                mesh.FaceStore.Add(f);
            }
        }

        // 四角形で作成
        if (facetype == FaceType.QUADRANGLE)
        {
            for (int i = 0; i < circleDiv; i++)
            {
                int nextSlice = ((i + 1) % circleDiv) * vc + ps;

                for (int vi = 0; vi < vc - 1; vi++)
                {
                    f = new CadFace(
                        (i * vc) + ps + vi,
                        nextSlice + vi,
                        nextSlice + vi + 1,
                        (i * vc) + ps + vi + 1
                        );

                    mesh.FaceStore.Add(f);
                }
            }
        }
        else
        {
            // 三角形で作成
            for (int i = 0; i < circleDiv; i++)
            {
                int nextSlice = ((i + 1) % circleDiv) * vc + ps;

                for (int vi = 0; vi < vc - 1; vi++)
                {
                    f = new CadFace(
                        (i * vc) + ps + vi,
                        nextSlice + vi,
                        (i * vc) + ps + vi + 1
                        );

                    mesh.FaceStore.Add(f);

                    f = new CadFace(
                       nextSlice + vi,
                       nextSlice + vi + 1,
                       (i * vc) + ps + vi + 1
                       );

                    mesh.FaceStore.Add(f);
                }
            }

        }

        return mesh;
    }

    // 回転体の作成
    public static CadMesh CreateRotatingBody(int circleDiv, vector3_t org, vector3_t axis, VertexList vl, bool topCap, bool btmCap, FaceType facetype = FaceType.TRIANGLE)
    {
        if (vl.Count < 2)
        {
            return null;
        }

        #region 面の向きが外から中心に向かってみた場合に左回りになるように回転軸の向きを調整
        // vlの全体的な向きを求めるためvl[0]から一番遠い点を求める
        int fi = CadUtil.FindMaxDistantPointIndex(vl[0], vl);
        vector3_t vldir = (vector3_t)(vl[fi] - vl[0]);

        // vlの全体的な向きと回転軸の向きが同じ場合、回転軸の向きを反転
        vcompo_t dot = CadMath.InnerProduct(axis, vldir);
        if (dot > 0)
        {
            axis *= -1;
        }
        #endregion

        CadMesh mesh = new CadMesh(vl.Count * circleDiv, vl.Count * circleDiv);

        CrossInfo crossS = CadMath.PerpCrossLine(org, org + axis, (vector3_t)vl[0]);
        CrossInfo crossE = CadMath.PerpCrossLine(org, org + axis, (vector3_t)vl.End());

        crossS.Distance = (crossS.CrossPoint - (vector3_t)vl[0]).Length;
        crossE.Distance = (crossE.CrossPoint - (vector3_t)vl.End()).Length;

        int s = 0;
        int e = vl.Count;

        int vc = vl.Count;
        int ps = 0;

        // VertexStoreの並びは以下の様になる。vlの個数をnとする。
        // topCap中心点, btmCap中心点, 側面点0, 側面点1 ..... 側面点n-1
        // 変数ps: 側面の点0の位置
        // 変数s:  vlから取り出す先頭位置
        //        最初の点をtopCapの中心とする場合、側面点0ではなくなるので、+1する
        // 変数e:  vlから取り出す終端
        //        最後の点をbtmCapの中心とする場合、側面点n-1ではなくなるので、-1する
        // 変数vc: 側面1列の点の数

        if (crossS.Distance < CadMath.Epsilon)
        {
            mesh.VertexStore.Add(vl[0]);
            s += 1;
            topCap = true;
            vc--;
            ps++;
        }
        else if (topCap)
        {
            mesh.VertexStore.Add((CadVertex)crossS.CrossPoint);
            ps++;
        }

        if (crossE.Distance < CadMath.Epsilon)
        {
            mesh.VertexStore.Add(vl.End());
            e -= 1;
            btmCap = true;
            vc--;
            ps++;
        }
        else if (btmCap)
        {
            mesh.VertexStore.Add((CadVertex)crossE.CrossPoint);
            ps++;
        }

        vcompo_t d = (vcompo_t)Math.PI * (vcompo_t)(2.0) / circleDiv;


        CadQuaternion qp;

        for (int i = 0; i < circleDiv; i++)
        {
            vcompo_t a = i * d;
            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, a);
            CadQuaternion con = q.Conjugate();

            for (int vi = s; vi < e; vi++)
            {
                CadVertex p = vl[vi];

                p.vector -= org;

                qp = CadQuaternion.FromPoint(p.vector);

                qp = con * qp;
                qp = qp * q;

                p.vector = qp.ToPoint();

                p += org;

                mesh.VertexStore.Add(p);
            }
        }

        CadFace f;

        if (topCap)
        {
            for (int i = 0; i < circleDiv; i++)
            {
                f = new CadFace(0, ((i + 1) % circleDiv) * vc + ps, i * vc + ps);
                mesh.FaceStore.Add(f);
            }
        }

        if (btmCap)
        {
            for (int i = 0; i < circleDiv; i++)
            {
                int bi = (vc - 1);

                f = new CadFace(1, (i * vc) + bi + ps, ((i + 1) % circleDiv) * vc + bi + ps);
                mesh.FaceStore.Add(f);
            }
        }

        // 四角形で作成
        if (facetype == FaceType.QUADRANGLE)
        {
            for (int i = 0; i < circleDiv; i++)
            {
                int nextSlice = ((i + 1) % circleDiv) * vc + ps;

                for (int vi = 0; vi < vc - 1; vi++)
                {
                    f = new CadFace(
                        (i * vc) + ps + vi,
                        nextSlice + vi,
                        nextSlice + vi + 1,
                        (i * vc) + ps + vi + 1
                        );

                    mesh.FaceStore.Add(f);
                }
            }
        }
        else
        {
            // 三角形で作成
            for (int i = 0; i < circleDiv; i++)
            {
                int nextSlice = ((i + 1) % circleDiv) * vc + ps;

                for (int vi = 0; vi < vc - 1; vi++)
                {
                    f = new CadFace(
                        (i * vc) + ps + vi,
                        nextSlice + vi,
                        (i * vc) + ps + vi + 1
                        );

                    mesh.FaceStore.Add(f);

                    f = new CadFace(
                       nextSlice + vi,
                       nextSlice + vi + 1,
                       (i * vc) + ps + vi + 1
                       );

                    mesh.FaceStore.Add(f);
                }
            }

        }

        return mesh;
    }

    public static CadMesh CreateExtruded(VertexList src, vector3_t dv, int div = 0)
    {
        if (src.Count < 3)
        {
            return null;
        }

        div += 1;

        VertexList vl;

        vector3_t n = CadUtil.TypicalNormal(src);


        if (CadMath.InnerProduct(n, dv) <= 0)
        {
            vl = new VertexList(src);
            vl.Reverse();
        }
        else
        {
            vl = src;
        }

        int vlCnt = vl.Count;

        CadMesh mesh = new CadMesh(vl.Count * 2, vl.Count);

        CadFace f;

        vector3_t dt = dv / div;

        vector3_t sv = vector3_t.Zero;

        // 頂点リスト作成
        for (int i = 0; i < div + 1; i++)
        {
            for (int j = 0; j < vlCnt; j++)
            {
                mesh.VertexStore.Add(vl[j] + sv);
            }

            sv += dt;
        }


        // 表面
        f = new CadFace();

        for (int i = 0; i < vlCnt; i++)
        {
            f.VList.Add(i);
        }

        mesh.FaceStore.Add(f);


        // 裏面
        f = new CadFace();

        int si = (div + 1) * vlCnt - 1;
        int ei = si - (vlCnt - 1);

        for (int i = si; i >= ei; i--)
        {
            f.VList.Add(i);
        }

        mesh.FaceStore.Add(f);

        // 側面
        for (int k = 0; k < div; k++)
        {
            int ti = vlCnt * k;

            for (int i = 0; i < vlCnt; i++)
            {
                int j = (i + 1) % vlCnt;

                f = new CadFace();

                f.VList.Add(i + ti);
                f.VList.Add(i + ti + vlCnt);
                f.VList.Add(j + ti + vlCnt);
                f.VList.Add(j + ti);

                mesh.FaceStore.Add(f);
            }
        }

        MeshUtil.SplitAllFaceToTriangle(mesh);

        return mesh;
    }
}
