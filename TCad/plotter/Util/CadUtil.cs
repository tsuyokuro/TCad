using CadDataTypes;
using System;
using System.Collections.Generic;
using TCad.MathFunctions;
using TCad.Plotter.Model.Figure;

namespace Plotter;

public delegate bool ForEachDelegate<T>(T obj);

public class CadUtil
{
    public static void RotateFigure(CadFigure fig, vector3_t org, vector3_t axis, vcompo_t t)
    {
        CadQuaternion q = CadQuaternion.RotateQuaternion(axis, t);
        CadQuaternion r = q.Conjugate(); ;

        fig.Rotate(org, q, r);
    }

    public static void ScaleFigure(CadFigure fig, vector3_t org, vcompo_t scale)
    {
        int n = fig.PointList.Count;

        for (int i = 0; i < n; i++)
        {
            CadVertex p = fig.PointList[i];
            p -= org;
            p *= scale;
            p += org;

            fig.SetPointAt(i, p);
        }
    }

    // 三角形の面積 3D対応
    public static vcompo_t TriangleArea(Vector3List triangle)
    {
        return CadMath.TriangleArea(
            triangle[0],
            triangle[1],
            triangle[2]
            );
    }

    // 三角形の重心を求める
    public static vector3_t TriangleCentroid(Vector3List triangle)
    {
        return CadMath.TriangleCentroid(
            triangle[0],
            triangle[1],
            triangle[2]
            );
    }

    // 三角形群の重心を求める
    public static Centroid TriangleListCentroid(List<Vector3List> triangles)
    {
        Centroid c0 = default;
        Centroid c1 = default;
        Centroid ct = default;

        int i = 1;

        c0.Area = TriangleArea(triangles[0]);
        c0.Point = TriangleCentroid(triangles[0]);

        for (; i < triangles.Count; i++)
        {
            c1.Area = TriangleArea(triangles[i]);
            c1.Point = TriangleCentroid(triangles[i]);

            ct = c0.Merge(c1);

            c0 = ct;
        }

        return c0;
    }

    public static vcompo_t AroundLength(CadFigure fig)
    {
        if (fig == null)
        {
            return 0;
        }

        int cnt = fig.PointCount;

        if (cnt < 2)
        {
            return 0;
        }

        CadVertex p0;
        CadVertex p1;

        CadVertex pd;

        vcompo_t d = 0;

        for (int i = 0; i < cnt - 1; i++)
        {
            p0 = fig.GetPointAt(i);
            p1 = fig.GetPointAt(i + 1);

            pd = p1 - p0;

            d += pd.Norm();
        }

        return d;
    }

    public static int InsertBezierHandle(CadFigure fig, int idx1, int idx2)
    {
        CadVertex a = fig.GetPointAt(idx1);
        CadVertex b = fig.GetPointAt(idx2);

        CadVertex hp1 = b - a;
        hp1 = hp1 / 3;
        hp1 = hp1 + a;

        CadVertex hp2 = a - b;
        hp2 = hp2 / 3;
        hp2 = hp2 + b;

        hp1.IsHandle = true;
        hp2.IsHandle = true;

        fig.InsertPointAt(idx1 + 1, hp1);
        fig.InsertPointAt(idx1 + 2, hp2);

        return 2;
    }

    // 指定された座標から最も遠いPointのIndexを求める
    public static int FindMaxDistantPointIndex(CadVertex p0, VertexList points)
    {
        int ret = -1;
        int i;

        CadVertex t;

        vcompo_t maxd = 0;

        for (i = 0; i < points.Count; i++)
        {
            CadVertex fp = points[i];

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

    // 法線の代表値を求める
    //public static CadVertex RepresentativeNormal(Vector3List points)
    //{
    //    if (points.Count < 3)
    //    {
    //        return CadVertex.Zero;
    //    }

    //    int idx = FindMaxDistantPointIndex(points[0], points);

    //    int idxA = idx - 1;
    //    int idxB = idx + 1;

    //    if (idxA < 0)
    //    {
    //        idxA = points.Count - 1;
    //    }

    //    if (idxB >= points.Count)
    //    {
    //        idxB = idxB - points.Count;
    //    }

    //    CadVertex normal = CadMath.Normal(points[idx], points[idxA], points[idxB]);

    //    return normal;
    //}

    public static vector3_t TypicalNormal(VertexList points)
    {
        if (points.Count < 3)
        {
            return vector3_t.Zero;
        }

        int idx = FindMaxDistantPointIndex(points[0], points);

        int idxA = idx - 1;
        int idxB = idx + 1;

        if (idxA < 0)
        {
            idxA = points.Count - 1;
        }

        if (idxB >= points.Count)
        {
            idxB = idxB - points.Count;
        }

        vector3_t normal = CadMath.Normal(points[idx].vector, points[idxA].vector, points[idxB].vector);

        return normal;
    }


    // 図形は凸である
    public static bool IsConvex(VertexList points)
    {
        int cnt = points.Count;

        if (cnt < 3)
        {
            return false;
        }

        int i = 0;
        vector3_t n = default;
        vector3_t cn = default;
        vcompo_t scala = 0;

        for (; i < cnt - 2;)
        {
            n = CadMath.Normal(points[i].vector, points[i + 1].vector, points[i + 2].vector);

            i++;

            if (!n.IsZero())
            {
                break;
            }
        }

        if (n.IsZero())
        {
            return false;
        }

        for (; i < cnt - 2;)
        {
            cn = CadMath.Normal(points[i].vector, points[i + 1].vector, points[i + 2].vector);

            i++;


            scala = CadMath.InnerProduct(cn, n);

            if (Math.Abs(scala) < CadMath.Epsilon)
            {
                continue;
            }

            if (scala < CadMath.RP1Min)
            {
                return false;
            }
        }


        cn = CadMath.Normal(points[i].vector, points[i + 1].vector, points[0].vector);

        scala = CadMath.InnerProduct(cn, n);

        if (Math.Abs(scala) < (vcompo_t)(0.000001))
        {
            return true;
        }

        if (scala < (vcompo_t)(0.999999))
        {
            return false;
        }

        return true;
    }



    //
    // 点pを通り、a - b に平行で、a-bに垂直な線分を求める
    //
    //   +----------p------------+
    //   |                       |
    //   |                       |
    //   a                       b
    //
    public static CadSegment PerpSeg(CadVertex a, CadVertex b, CadVertex p)
    {
        CadSegment seg = default(CadSegment);

        seg.P0 = a;
        seg.P1 = b;

        CrossInfo ci = CadMath.PerpCrossLine(a.vector, b.vector, p.vector);

        if (ci.IsCross)
        {
            CadVertex nv = p - ci.CrossPoint;

            seg.P0 += nv;
            seg.P1 += nv;
        }

        return seg;
    }

    public static void MovePoints(VertexList list, vector3_t delta)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CadVertex op = list[i];
            list[i] = op + delta;
        }
    }

    public static CadRect GetContainsRect(VertexList list)
    {
        CadRect rect = default(CadRect);

        vcompo_t minx = CadConst.MaxValue;
        vcompo_t miny = CadConst.MaxValue;
        vcompo_t minz = CadConst.MaxValue;

        vcompo_t maxx = CadConst.MinValue;
        vcompo_t maxy = CadConst.MinValue;
        vcompo_t maxz = CadConst.MinValue;

        foreach (CadVertex p in list)
        {
            minx = (vcompo_t)Math.Min(minx, p.X);
            miny = (vcompo_t)Math.Min(miny, p.Y);
            minz = (vcompo_t)Math.Min(minz, p.Z);

            maxx = (vcompo_t)Math.Max(maxx, p.X);
            maxy = (vcompo_t)Math.Max(maxy, p.Y);
            maxz = (vcompo_t)Math.Max(maxz, p.Z);
        }

        rect.p0 = default;
        rect.p1 = default;

        rect.p0.X = minx;
        rect.p0.Y = miny;
        rect.p0.Z = minz;

        rect.p1.X = maxx;
        rect.p1.Y = maxy;
        rect.p1.Z = maxz;

        return rect;
    }

    public static MinMax3D GetFigureMinMax(CadFigure fig)
    {
        MinMax3D mm = MinMax3D.Create();

        int i = 0;
        for (; i < fig.PointCount; i++)
        {
            mm.Check(fig.PointList[i].vector);
        }

        return mm;
    }

    public static MinMax3D GetFigureMinMaxIncludeChild(CadFigure fig)
    {
        MinMax3D mm = MinMax3D.Create();

        fig.ForEachFig(item =>
        {
            mm.Check(GetFigureMinMax(item));
        });

        return mm;
    }

    public static MinMax3D GetFigureMinMaxIncludeChild(List<CadFigure> figList)
    {
        MinMax3D mm = MinMax3D.Create();

        foreach (CadFigure fig in figList)
        {
            MinMax3D tmm = GetFigureMinMaxIncludeChild(fig);
            mm.Check(tmm);
        }

        return mm;
    }

    public static CadRect GetContainsRectScrn(DrawContext dc, List<CadFigure> list)
    {
        CadRect rect = default(CadRect);
        CadRect fr;

        vcompo_t minx = CadConst.MaxValue;
        vcompo_t miny = CadConst.MaxValue;

        vcompo_t maxx = CadConst.MinValue;
        vcompo_t maxy = CadConst.MinValue;

        foreach (CadFigure fig in list)
        {
            fr = fig.GetContainsRectScrn(dc);

            fr.Normalize();

            minx = (vcompo_t)Math.Min(minx, fr.p0.X);
            miny = (vcompo_t)Math.Min(miny, fr.p0.Y);
            maxx = (vcompo_t)Math.Max(maxx, fr.p1.X);
            maxy = (vcompo_t)Math.Max(maxy, fr.p1.Y);
        }

        rect.p0 = default;
        rect.p1 = default;

        rect.p0.X = minx;
        rect.p0.Y = miny;
        rect.p0.Z = 0;

        rect.p1.X = maxx;
        rect.p1.Y = maxy;
        rect.p1.Z = 0;

        return rect;
    }

    public static CadRect GetContainsRectScrn(DrawContext dc, VertexList list)
    {
        CadRect rect = default(CadRect);

        vcompo_t minx = CadConst.MaxValue;
        vcompo_t miny = CadConst.MaxValue;

        vcompo_t maxx = CadConst.MinValue;
        vcompo_t maxy = CadConst.MinValue;

        list.ForEach(p =>
        {
            CadVertex v = dc.WorldPointToDevPoint(p);

            minx = (vcompo_t)Math.Min(minx, v.X);
            miny = (vcompo_t)Math.Min(miny, v.Y);

            maxx = (vcompo_t)Math.Max(maxx, v.X);
            maxy = (vcompo_t)Math.Max(maxy, v.Y);
        });

        rect.p0 = default;
        rect.p1 = default;

        rect.p0.X = minx;
        rect.p0.Y = miny;
        rect.p0.Z = 0;

        rect.p1.X = maxx;
        rect.p1.Y = maxy;
        rect.p1.Z = 0;

        return rect;
    }

    /// <summary>
    /// 点が矩形内あるかチェック
    /// </summary>
    /// <param name="minp">矩形の最小頂点</param>
    /// <param name="maxp">矩形の最大頂点</param>
    /// <param name="p">検査対象点</param>
    /// <returns>
    /// true:  点は矩形内
    /// false: 点は矩形外
    /// </returns>
    /// 
    public static bool IsInRect2D(CadVertex minp, CadVertex maxp, CadVertex p)
    {
        if (p.X < minp.X) return false;
        if (p.X > maxp.X) return false;

        if (p.Y < minp.Y) return false;
        if (p.Y > maxp.Y) return false;

        return true;
    }

    /// <summary>
    /// 点が矩形内あるかチェック
    /// </summary>
    /// <param name="minp">矩形の最小頂点</param>
    /// <param name="maxp">矩形の最大頂点</param>
    /// <param name="p">検査対象点</param>
    /// <returns>
    /// true:  点は矩形内
    /// false: 点は矩形外
    /// </returns>
    /// 
    public static bool IsInRect2D(vector3_t minp, vector3_t maxp, vector3_t p)
    {
        if (p.X < minp.X) return false;
        if (p.X > maxp.X) return false;

        if (p.Y < minp.Y) return false;
        if (p.Y > maxp.Y) return false;

        return true;
    }

    /// <summary>
    /// 浮動小数点数を文字列に変換
    /// </summary>
    /// <param name="v">値</param>
    /// <returns>当プログラムでの標準的な変換方法で変換された文字列</returns>
    public static string ValToString(vcompo_t v)
    {
        return v.ToString("f2");
    }

    // 1inchは何ミリ?
    public const vcompo_t MILLI_PER_INCH = (vcompo_t)(25.4);

    public static vcompo_t MilliToInch(vcompo_t mm)
    {
        return mm / MILLI_PER_INCH;
    }

    public static vcompo_t InchToMilli(vcompo_t inchi)
    {
        return inchi * MILLI_PER_INCH;
    }

    //public static PointPair LeftTopRightBottom2D(vector3_t p0, vector3_t p1)
    //{
    //    vcompo_t lx = p0.X;
    //    vcompo_t rx = p1.X;

    //    vcompo_t ty = p0.Y;
    //    vcompo_t by = p1.Y;

    //    if (p0.X > p1.X)
    //    {
    //        lx = p1.X;
    //        rx = p0.X;
    //    }

    //    if (p0.Y > p1.Y)
    //    {
    //        ty = p1.Y;
    //        by = p0.Y;
    //    }

    //    return new PointPair(CadVertex.Create(lx, ty, 0), CadVertex.Create(rx, by, 0));
    //}

    public static List<vector3_t> Getvector3_tListFrom(CadFigure fig)
    {
        List<vector3_t> list = new();
        for (int i = 0; i < fig.PointList.Count; i++)
        {
            list.Add((vector3_t)fig.PointList[i]);
        }

        return list;
    }

    public static void SetVertexListTo(CadFigure fig, List<vector3_t> vl)
    {
        List<vector3_t> list = new();
        for (int i = 0; i < vl.Count; i++)
        {
            fig.AddPoint(new CadVertex(vl[i]));
        }
    }
}
