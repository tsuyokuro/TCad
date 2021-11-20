using OpenTK;
using System;
using System.Collections.Generic;
using CadDataTypes;

namespace Plotter
{
    public delegate bool ForEachDelegate<T>(T obj);

    public class CadUtil
    {
        public static void RotateFigure(CadFigure fig, Vector3d org, Vector3d axis, double t)
        {
            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, t);
            CadQuaternion r = q.Conjugate(); ;

            fig.Rotate(org, q, r);
        }

        public static void ScaleFigure(CadFigure fig, Vector3d org, double scale)
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
        public static double TriangleArea(CadFigure fig)
        {
            return CadMath.TriangleArea(
                fig.GetPointAt(0).vector,
                fig.GetPointAt(1).vector,
                fig.GetPointAt(2).vector
                );
        }

        // 三角形の重心を求める
        public static Vector3d TriangleCentroid(CadFigure fig)
        {
            return CadMath.TriangleCentroid(
                fig.GetPointAt(0).vector,
                fig.GetPointAt(1).vector,
                fig.GetPointAt(2).vector
                );
        }

        // 三角形群の重心を求める
        public static Centroid TriangleListCentroid(List<CadFigure> triangles)
        {
            Centroid c0 = default;
            Centroid c1 = default;
            Centroid ct = default;

            int i = 1;

            c0.Area= TriangleArea(triangles[0]);
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

        public static double AroundLength(CadFigure fig)
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

            double d = 0;

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

            double maxd = 0;

            for (i = 0; i < points.Count; i++)
            {
                CadVertex fp = points[i];

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

        // 法線の代表値を求める
        //public static CadVertex RepresentativeNormal(VertexList points)
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

        public static Vector3d TypicalNormal(VertexList points)
        {
            if (points.Count < 3)
            {
                return Vector3d.Zero;
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

            Vector3d normal = CadMath.Normal(points[idx].vector, points[idxA].vector, points[idxB].vector);

            return normal;
        }


        // 図形は凸である
        public static bool IsConvex(VertexList points)
        {
            int cnt = points.Count;

            if (cnt<3)
            {
                return false;
            }

            int i = 0;
            Vector3d n = default;
            Vector3d cn = default;
            double scala = 0;

            for (;i < cnt - 2;)
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

            for (;i<cnt-2;)
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

            if (Math.Abs(scala) < 0.000001)
            {
                return true;
            }

            if (scala < 0.999999)
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

        public static void MovePoints(VertexList list, Vector3d delta)
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

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;
            double minz = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;
            double maxz = CadConst.MinValue;

            foreach (CadVertex p in list)
            {
                minx = Math.Min(minx, p.X);
                miny = Math.Min(miny, p.Y);
                minz = Math.Min(minz, p.Z);

                maxx = Math.Max(maxx, p.X);
                maxy = Math.Max(maxy, p.Y);
                maxz = Math.Max(maxz, p.Z);
            }

            rect.p0 = default(CadVertex);
            rect.p1 = default(CadVertex);

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
            for (;i<fig.PointCount; i++)
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

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;

            foreach (CadFigure fig in list)
            {
                fr = fig.GetContainsRectScrn(dc);

                fr.Normalize();

                minx = Math.Min(minx, fr.p0.X);
                miny = Math.Min(miny, fr.p0.Y);
                maxx = Math.Max(maxx, fr.p1.X);
                maxy = Math.Max(maxy, fr.p1.Y);
            }

            rect.p0 = default(CadVertex);
            rect.p1 = default(CadVertex);

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

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;

            list.ForEach(p =>
            {
                CadVertex v = dc.WorldPointToDevPoint(p);

                minx = Math.Min(minx, v.X);
                miny = Math.Min(miny, v.Y);

                maxx = Math.Max(maxx, v.X);
                maxy = Math.Max(maxy, v.Y);
            });

            rect.p0 = default(CadVertex);
            rect.p1 = default(CadVertex);

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
        public static bool IsInRect2D(Vector3d minp, Vector3d maxp, Vector3d p)
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
        public static string ValToString(double v)
        {
            return v.ToString("f2");
        }

        // 1inchは何ミリ?
        public const double MILLI_PER_INCH = 25.4;

        public static double MilliToInch(double mm)
        {
            return mm / MILLI_PER_INCH;
        }

        public static double InchToMilli(double inchi)
        {
            return inchi * MILLI_PER_INCH;
        }

        //public static PointPair LeftTopRightBottom2D(Vector3d p0, Vector3d p1)
        //{
        //    double lx = p0.X;
        //    double rx = p1.X;

        //    double ty = p0.Y;
        //    double by = p1.Y;

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

        public static void Dump(Vector4d v, string prefix)
        {
            DOut.Begin();

            DOut.p(prefix);
            DOut.pl("{");
            DOut.Indent++;
            DOut.pl("x:" + v.X.ToString());
            DOut.pl("y:" + v.Y.ToString());
            DOut.pl("z:" + v.Z.ToString());
            DOut.pl("w:" + v.W.ToString());
            DOut.Indent--;
            DOut.pl("}");

            DOut.End();
        }

        public static void Dump(UMatrix4 m, string prefix)
        {
            DOut.p(prefix);
            DOut.pl("{");
            DOut.Indent++;
            DOut.pl(m.M11.ToString() + "," + m.M12.ToString() + "," + m.M13.ToString() + "," + m.M14.ToString());
            DOut.pl(m.M21.ToString() + "," + m.M22.ToString() + "," + m.M23.ToString() + "," + m.M24.ToString());
            DOut.pl(m.M31.ToString() + "," + m.M32.ToString() + "," + m.M33.ToString() + "," + m.M34.ToString());
            DOut.pl(m.M41.ToString() + "," + m.M42.ToString() + "," + m.M43.ToString() + "," + m.M44.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
