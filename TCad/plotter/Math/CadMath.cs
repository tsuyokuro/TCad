using System;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public partial class CadMath
    {
        public const double Epsilon = 0.0000005;

        public const double RP1Min = 1.0 - Epsilon;
        public const double RP1Max = 1.0 + Epsilon;

        public const double RM1Min = -1.0 - Epsilon;
        public const double RM1Max = -1.0 + Epsilon;

        public const double R0Min = -Epsilon;
        public const double R0Max = Epsilon;

        public static bool Near_P1(double v)
        {
            return (v > RP1Min && v < RP1Max);
        }

        public static bool Near_M1(double v)
        {
            return (v > RM1Min && v < RM1Max);
        }

        public static bool Near_0(double v)
        {
            return (v > R0Min && v < R0Max);
        }

        /**
         * ラジアンを角度に変換
         * 
         */
        public static double Rad2Deg(double rad)
        {
            return 180.0 * rad / Math.PI;
        }

        /**
         * 角度をラジアンに変換
         * 
         */
        public static double Deg2Rad(double deg)
        {
            return Math.PI * deg / 180.0;
        }

        // 内積
        #region inner product
        public static double InnrProduct2D(Vector3d v1, Vector3d v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }

        public static double InnrProduct2D(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return InnrProduct2D(v1 - v0, v2 - v0);
        }

        public static double InnerProduct(Vector3d v1, Vector3d v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
        }

        public static double InnerProduct(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return InnerProduct(v1 - v0, v2 - v0);
        }
        #endregion


        // 外積
        #region Cross product
        public static double CrossProduct2D(Vector3d v1, Vector3d v2)
        {
            return (v1.X * v2.Y) - (v1.Y * v2.X);
        }

        public static double CrossProduct2D(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return CrossProduct2D(v1 - v0, v2 - v0);
        }

        public static Vector3d CrossProduct(Vector3d v1, Vector3d v2)
        {
            Vector3d res = default;

            res.X = v1.Y * v2.Z - v1.Z * v2.Y;
            res.Y = v1.Z * v2.X - v1.X * v2.Z;
            res.Z = v1.X * v2.Y - v1.Y * v2.X;

            return res;
        }

        public static Vector3d CrossProduct(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return CrossProduct(v1 - v0, v2 - v0);
        }
        #endregion

        /**
         * 法線を求める
         * 
         *      v2
         *     / 
         *    /
         * v0/_________v1
         *
         */
        public static Vector3d Normal(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            Vector3d va = v1 - v0;
            Vector3d vb = v2 - v0;

            Vector3d normal = CrossProduct(va, vb);

            if (normal.IsZero())
            {
                return normal;
            }

            normal.Normalize();

            return normal;
        }

        /**
         * 法線を求める
         * 
         *       vb
         *      / 
         *     /
         * 0 /_________va
         * 
         */
        public static Vector3d Normal(Vector3d va, Vector3d vb)
        {
            Vector3d normal = CrossProduct(va, vb);

            if (normal.IsZero())
            {
                return normal;
            }

            normal.Normalize();

            return normal;
        }

        public static bool IsParallel(Vector3d v1, Vector3d v2)
        {
            v1.Normalize();
            v2.Normalize();

            double a = InnerProduct(v1, v2);
            return Near_P1(a) || Near_M1(a);
        }

        /// <summary>
        /// 2つのVectorのなす角を求める 
        ///
        /// 内積の定義を使う
        /// cosθ = ( AとBの内積 ) / (Aの長さ * Bの長さ)
        ///
        /// </summary>
        /// <param name="v1">Vector1</param>
        /// <param name="v2">Vector2</param>
        /// <returns>なす角</returns>
        /// 
        public static double AngleOfVector(Vector3d v1, Vector3d v2)
        {
            double v1n = v1.Norm();
            double v2n = v2.Norm();

            double cost = InnerProduct(v1, v2) / (v1n * v2n);

            double t = Math.Acos(cost);

            return t;
        }

        // 三角形の面積
        public static double TriangleArea(Vector3d p0, Vector3d p1, Vector3d p2)
        {
            Vector3d v1 = p0 - p1;
            Vector3d v2 = p2 - p1;

            Vector3d cp = CrossProduct(v1, v2);

            double area = cp.Norm() / 2.0;

            return area;
        }

        // 三角形の重心を求める
        public static Vector3d TriangleCentroid(Vector3d p0, Vector3d p1, Vector3d p2)
        {
            Vector3d gp = default;

            gp.X = (p0.X + p1.X + p2.X) / 3.0;
            gp.Y = (p0.Y + p1.Y + p2.Y) / 3.0;
            gp.Z = (p0.Z + p1.Z + p2.Z) / 3.0;

            return gp;
        }

        // 線分apと点pの距離
        // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
        // 2D
        public static double DistancePointToSeg2D(Vector3d a, Vector3d b, Vector3d p)
        {
            double t;

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            t = InnrProduct2D(ab, ap);

            if (t < 0)
            {
                return ap.Norm2D();
            }

            Vector3d ba = a - b;
            Vector3d bp = p - b;

            t = InnrProduct2D(ba, bp);

            if (t < 0)
            {
                return bp.Norm2D();
            }

            // 外積結果が a->p a->b を辺とする平行四辺形の面積になる
            double d = Math.Abs(CrossProduct2D(ab, ap));

            double abl = ab.Norm2D();

            // 高さ = 面積 / 底辺の長さ
            return d / abl;
        }

        // 線分apと点pの距離
        // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
        // 3D対応
        public static double DistancePointToSeg(Vector3d a, Vector3d b, Vector3d p)
        {
            double t;

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            t = InnerProduct(ab, ap);

            if (t < 0)
            {
                return ap.Norm();
            }

            Vector3d ba = a - b;
            Vector3d bp = p - b;

            t = InnerProduct(ba, bp);

            if (t < 0)
            {
                return bp.Norm();
            }

            Vector3d cp = CrossProduct(ab, ap);

            // 外積結果の長さが a->p a->b を辺とする平行四辺形の面積になる
            double s = cp.Norm();

            // 高さ = 面積 / 底辺の長さ
            return s / ab.Norm();
        }

        // 点が三角形内にあるか 2D版
        public static bool IsPointInTriangle2D(
            Vector3d p,
            Vector3d p0,
            Vector3d p1,
            Vector3d p2
            )
        {
            double c1 = CrossProduct2D(p, p0, p1);
            double c2 = CrossProduct2D(p, p1, p2);
            double c3 = CrossProduct2D(p, p2, p0);

            // 外積の結果の符号が全て同じなら点は三角形の中
            // When all corossProduct result's sign are same, Point is in triangle
            if ((c1 > 0 && c2 > 0 && c3 > 0) || (c1 < 0 && c2 < 0 && c3 < 0))
            {
                return true;
            }

            return false;
        }

        // 点が三角形内にあるか
        public static bool IsPointInTriangle(
            Vector3d p,
            Vector3d p0,
            Vector3d p1,
            Vector3d p2
            )
        {
            Vector3d c1 = GetC1(p, p0, p1);
            Vector3d c2 = CrossProduct(p, p1, p2);
            Vector3d c3 = CrossProduct(p, p2, p0);

            double ip12 = InnerProduct(c1, c2);
            double ip13 = InnerProduct(c1, c3);

            // 外積の結果の符号が全て同じなら点は三角形の中
            // When all corossProduct result's sign are same, Point is in triangle
            if (ip12 > 0 && ip13 > 0)
            {
                return true;
            }

            return false;
        }

        private static Vector3d GetC1(Vector3d p, Vector3d p0, Vector3d p1)
        {
            return CrossProduct(p, p0, p1);
        }


        // 点pから線分abに向かう垂線との交点を求める
        public static CrossInfo PerpendicularCrossSeg(Vector3d a, Vector3d b, Vector3d p)
        {
            CrossInfo ret = default;

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            Vector3d ba = a - b;
            Vector3d bp = p - b;

            // A-B 単位ベクトル
            Vector3d unit_ab = ab.UnitVector();

            // B-A 単位ベクトル　(A-B単位ベクトルを反転) B側の中外判定に使用
            Vector3d unit_ba = unit_ab * -1.0;

            // Aから交点までの距離 
            // A->交点->B or A->B->交点なら +
            // 交点<-A->B なら -
            double dist_ax = InnerProduct(unit_ab, ap);

            // Bから交点までの距離 B側の中外判定に使用
            double dist_bx = InnerProduct(unit_ba, bp);

            //Console.WriteLine("getNormCross dist_ax={0} dist_bx={1}" , dist_ax.ToString(), dist_bx.ToString());

            if (dist_ax > 0 && dist_bx > 0)
            {
                ret.IsCross = true;
            }

            ret.CrossPoint.X = a.X + (unit_ab.X * dist_ax);
            ret.CrossPoint.Y = a.Y + (unit_ab.Y * dist_ax);
            ret.CrossPoint.Z = a.Z + (unit_ab.Z * dist_ax);

            return ret;
        }

        // 点pから線分abに向かう垂線との交点を求める2D
        public static CrossInfo PerpendicularCrossSeg2D(Vector3d a, Vector3d b, Vector3d p)
        {
            CrossInfo ret = default;

            double t1;

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            t1 = InnrProduct2D(ab, ap);

            if (t1 < 0)
            {
                return ret;
            }

            double t2;

            Vector3d ba = a - b;
            Vector3d bp = p - b;

            t2 = InnrProduct2D(ba, bp);

            if (t2 < 0)
            {
                return ret;
            }

            double abl = ab.Norm2D();
            double abl2 = abl * abl;

            ret.IsCross = true;
            ret.CrossPoint.X = ab.X * t1 / abl2 + a.X;
            ret.CrossPoint.Y = ab.Y * t1 / abl2 + a.Y;

            return ret;
        }

        // 点pから線分abに向かう垂線との交点が線分ab内にあるか
        public static bool IsPointInSeg2D(Vector3d a, Vector3d b, Vector3d p)
        {
            double t1;

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            t1 = InnrProduct2D(ab, ap);

            if (t1 < 0)
            {
                return false;
            }

            double t2;

            Vector3d ba = a - b;
            Vector3d bp = p - b;

            t2 = InnrProduct2D(ba, bp);

            if (t2 < 0)
            {
                return false;
            }

            return true;
        }


        // 点pから直線abに向かう垂線との交点を求める
        public static CrossInfo PerpCrossLine(Vector3d a, Vector3d b, Vector3d p)
        {
            CrossInfo ret = default;

            if (a.Equals(b))
            {
                return ret;
            }

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            // A-B 単位ベクトル
            Vector3d unit_ab = ab.UnitVector();

            // Aから交点までの距離 
            double dist_ax = InnerProduct(unit_ab, ap);

            ret.CrossPoint.X = a.X + (unit_ab.X * dist_ax);
            ret.CrossPoint.Y = a.Y + (unit_ab.Y * dist_ax);
            ret.CrossPoint.Z = a.Z + (unit_ab.Z * dist_ax);

            ret.IsCross = true;

            return ret;
        }

        // 点pから直線abに向かう垂線との交点を求める2D
        public static CrossInfo PerpendicularCrossLine2D(Vector3d a, Vector3d b, Vector3d p)
        {
            CrossInfo ret = default;

            double t1;

            Vector3d ab = b - a;
            Vector3d ap = p - a;

            t1 = InnrProduct2D(ab, ap);

            double norm = ab.Norm2D();
            double norm2 = norm * norm;

            ret.IsCross = true;
            ret.CrossPoint.X = ab.X * t1 / norm2 + a.X;
            ret.CrossPoint.Y = ab.Y * t1 / norm2 + a.Y;

            return ret;
        }

        /// <summary>
        /// a b の中点を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3d CenterPoint(Vector3d a, Vector3d b)
        {
            Vector3d c = b - a;
            c /= 2;
            c += a;

            return c;
        }

        /// <summary>
        /// a b を通る直線上で a からの距離がlenの座標を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static Vector3d LinePoint(Vector3d a, Vector3d b, double len)
        {
            Vector3d v = b - a;

            v = v.UnitVector();

            v *= len;

            v += a;

            return v;
        }

        public static double SegNorm2D(Vector3d a, Vector3d b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public static double SegNorm(Vector3d a, Vector3d b)
        {
            Vector3d v = b - a;
            return v.Norm();
        }

        /// <summary>
        /// 点aに最も近い平面上の点を求める
        /// </summary>
        /// <param name="a">点</param>
        /// <param name="p">平面上の点</param>
        /// <param name="normal">平面の法線</param>
        /// <returns>
        /// 点aに最も近い平面上の点
        /// </returns>
        public static Vector3d CrossPlane(Vector3d a, Vector3d p, Vector3d normal)
        {
            Vector3d pa = a - p;

            // 法線とpaの内積をとる
            // 法線の順方向に点Aがあれば d>0 逆方向だと d<0
            double d = InnerProduct(normal, pa);

            //内積値から平面上の最近点を求める
            Vector3d cp = default;
            cp.X = a.X - (normal.X * d);
            cp.Y = a.Y - (normal.Y * d);
            cp.Z = a.Z - (normal.Z * d);

            return cp;
        }

        /// <summary>
        /// [直線ab] と [点pと法線normalが示す平面] との交点を求める
        /// </summary>
        /// <param name="a">直線上の点</param>
        /// <param name="b">直線上の点</param>
        /// <param name="p">平面上の点</param>
        /// <param name="normal">平面の法線</param>
        /// <returns>交点</returns>
        /// 
        public static Vector3d CrossPlane(Vector3d a, Vector3d b, Vector3d p, Vector3d normal)
        {
            Vector3d cp = default;

            Vector3d e = b - a;

            double de = InnerProduct(normal, e);

            if (de > GetR0Min() && de < GetR0Max())
            {
                //DebugOut.Std.println("CrossPlane is parallel");

                // 平面と直線は平行
                return VectorExt.InvalidVector3d;
            }

            double d = InnerProduct(normal, p);
            double t = (d - NewMethod1(a, normal)) / de;

            cp = a + (e * t);

            return cp;
        }

        private static double NewMethod1(Vector3d a, Vector3d normal)
        {
            return InnerProduct(normal, a);
        }

        private static double GetR0Max()
        {
            return GetR0Max1();
        }

        private static double GetR0Max1()
        {
            return GetR0Max2();
        }

        private static double GetR0Max2()
        {
            return GetR0Max3();
        }

        private static double GetR0Max3()
        {
            return R0Max;
        }

        private static double GetR0Min()
        {
            return R0Min;
        }

        /// <summary>
        /// 直線 a b と p と normalが示す平面との交点を求める
        /// </summary>
        /// <param name="a">直線上の点</param>
        /// <param name="b">直線上の点</param>
        /// <param name="p">平面上の点</param>
        /// <param name="normal">平面の法線</param>
        /// <returns>交点</returns>
        /// 
        public static Vector3d CrossSegPlane(Vector3d a, Vector3d b, Vector3d p, Vector3d normal)
        {
            Vector3d cp = CrossPlane(a, b, p, normal);

            if (!cp.IsValid())
            {
                return VectorExt.InvalidVector3d;
            }

            if (InnerProduct((b - a), (cp - a)) < 0)
            {
                return VectorExt.InvalidVector3d;
            }

            if (NewMethod(a, b, cp) < 0)
            {
                return VectorExt.InvalidVector3d;
            }

            return cp;
        }

        private static double NewMethod(Vector3d a, Vector3d b, Vector3d cp)
        {
            return InnerProduct((a - b), (cp - b));
        }

        /// <summary>
        /// 線分同士の交点が存在するかチェックする
        /// </summary>
        /// <param name="p1">線分A</param>
        /// <param name="p2">線分A</param>
        /// <param name="p3">線分B</param>
        /// <param name="p4">線分B</param>
        /// <returns>
        /// 交点が存在す場合は、true
        /// 存在しない場合は、false
        /// また、同一線上にある場合は、false (交点が無限に存在する)
        /// </returns>
        /// 
        public static bool CheckCrossSegSeg2D(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4)
        {
            if (p1.X >= p2.X)
            {
                if ((p1.X < p3.X && p1.X < p4.X) || (p2.X > p3.X && p2.X > p4.X))
                {
                    return false;
                }
            }
            else
            {
                if ((p2.X < p3.X && p2.X < p4.X) || (p1.X > p3.X && p1.X > p4.X))
                {
                    return false;
                }
            }

            if (p1.Y >= p2.Y)
            {
                if ((p1.Y < p3.Y && p1.Y < p4.Y) || (p2.Y > p3.Y && p2.Y > p4.Y))
                {
                    return false;
                }
            }
            else
            {
                if ((p2.Y < p3.Y && p2.Y < p4.Y) || (p1.Y > p3.Y && p1.Y > p4.Y))
                {
                    return false;
                }
            }

            if (((p1.X - p2.X) * (p3.Y - p1.Y) + (p1.Y - p2.Y) * (p1.X - p3.X)) *
                ((p1.X - p2.X) * (p4.Y - p1.Y) + (p1.Y - p2.Y) * (p1.X - p4.X)) > 0)
            {
                return false;
            }

            if (((p3.X - p4.X) * (p1.Y - p3.Y) + (p3.Y - p4.Y) * (p3.X - p1.X)) *
                ((p3.X - p4.X) * (p2.Y - p3.Y) + (p3.Y - p4.Y) * (p3.X - p2.X)) > 0)
            {
                return false;
            }

            return true;
        }

        public static double Angle2D(Vector3d v)
        {
            return Math.Atan2(v.Y, v.X);
        }

        public static Vector3d CrossLine2D(Vector3d a1, Vector3d a2, Vector3d b1, Vector3d b2)
        {
            Vector3d a = (a2 - a1);
            Vector3d b = (b2 - b1);

            if (a.IsZero() || b.IsZero())
            {
                return VectorExt.InvalidVector3d;
            }

            double cpBA = CrossProduct2D(b, a);

            if (cpBA == 0)
            {
                return VectorExt.InvalidVector3d;
            }

            return a1 + a * CrossProduct2D(b, b1 - a1) / cpBA;
        }
    }
}
