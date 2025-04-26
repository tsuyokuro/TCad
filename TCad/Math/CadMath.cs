using Plotter;
using System;

namespace TCad.MathFunctions;

public partial class CadMath
{
    public const vcompo_t Epsilon = (vcompo_t)(0.0000005);

    public const vcompo_t RP1Min = (vcompo_t)(1.0) - Epsilon;
    public const vcompo_t RP1Max = (vcompo_t)(1.0) + Epsilon;

    public const vcompo_t RM1Min = (vcompo_t)(-1.0) - Epsilon;
    public const vcompo_t RM1Max = (vcompo_t)(-1.0) + Epsilon;

    public const vcompo_t R0Min = -Epsilon;
    public const vcompo_t R0Max = Epsilon;

    public static bool Near_P1(vcompo_t v)
    {
        return (v > RP1Min && v < RP1Max);
    }

    public static bool Near_M1(vcompo_t v)
    {
        return (v > RM1Min && v < RM1Max);
    }

    public static bool Near_0(vcompo_t v)
    {
        return (v > R0Min && v < R0Max);
    }

    /**
     * ラジアンを角度に変換
     * 
     */
    public static vcompo_t Rad2Deg(vcompo_t rad)
    {
        return (vcompo_t)(180.0) * rad / (vcompo_t)Math.PI;
    }

    /**
     * 角度をラジアンに変換
     * 
     */
    public static vcompo_t Deg2Rad(vcompo_t deg)
    {
        return (vcompo_t)Math.PI * deg / (vcompo_t)(180.0);
    }

    // 内積
    #region inner product
    public static vcompo_t InnrProduct2D(vector3_t v1, vector3_t v2)
    {
        return (v1.X * v2.X) + (v1.Y * v2.Y);
    }

    public static vcompo_t InnrProduct2D(vector3_t v0, vector3_t v1, vector3_t v2)
    {
        return InnrProduct2D(v1 - v0, v2 - v0);
    }

    public static vcompo_t InnerProduct(vector3_t v1, vector3_t v2)
    {
        return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
    }

    public static vcompo_t InnerProduct(vector3_t v0, vector3_t v1, vector3_t v2)
    {
        return InnerProduct(v1 - v0, v2 - v0);
    }
    #endregion


    // 外積
    #region Cross product
    public static vcompo_t OuterProduct2D(vector3_t v1, vector3_t v2)
    {
        return (v1.X * v2.Y) - (v1.Y * v2.X);
    }

    public static vcompo_t OuterProduct2D(vector3_t v0, vector3_t v1, vector3_t v2)
    {
        return OuterProduct2D(v1 - v0, v2 - v0);
    }

    public static vector3_t OuterProduct(vector3_t v1, vector3_t v2)
    {
        vector3_t res = default;

        res.X = v1.Y * v2.Z - v1.Z * v2.Y;
        res.Y = v1.Z * v2.X - v1.X * v2.Z;
        res.Z = v1.X * v2.Y - v1.Y * v2.X;

        return res;
    }

    public static vector3_t OuterProduct(vector3_t v0, vector3_t v1, vector3_t v2)
    {
        return OuterProduct(v1 - v0, v2 - v0);
    }
    #endregion

    /**
     * 法線を求める
     * 
     * Normal 
     *   |   v2
     *   |  / 
     *   | /
     * v0|/_________v1
     *
     */
    public static vector3_t Normal(vector3_t v0, vector3_t v1, vector3_t v2)
    {
        vector3_t va = v1 - v0;
        vector3_t vb = v2 - v0;

        vector3_t normal = OuterProduct(va, vb);

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
     * Normal 
     *   |   vb
     *   |  / 
     *   | /
     * 0 |/_________va
     * 
     */
    public static vector3_t Normal(vector3_t va, vector3_t vb)
    {
        vector3_t normal = OuterProduct(va, vb);

        if (normal.IsZero())
        {
            return normal;
        }

        normal.Normalize();

        return normal;
    }

    public static bool IsParallel(vector3_t v1, vector3_t v2)
    {
        v1.Normalize();
        v2.Normalize();

        vcompo_t a = InnerProduct(v1, v2);
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
    public static vcompo_t AngleOfVector(vector3_t v1, vector3_t v2)
    {
        vcompo_t v1n = v1.Norm();
        vcompo_t v2n = v2.Norm();

        vcompo_t cost = InnerProduct(v1, v2) / (v1n * v2n);

        vcompo_t t = (vcompo_t)Math.Acos(cost);

        return t;
    }

    // 三角形の面積
    public static vcompo_t TriangleArea(vector3_t p0, vector3_t p1, vector3_t p2)
    {
        vector3_t v1 = p0 - p1;
        vector3_t v2 = p2 - p1;

        vector3_t cp = OuterProduct(v1, v2);

        vcompo_t area = cp.Norm() / (vcompo_t)(2.0);

        return area;
    }

    // 三角形の重心を求める
    public static vector3_t TriangleCentroid(vector3_t p0, vector3_t p1, vector3_t p2)
    {
        vector3_t gp = default;

        gp.X = (p0.X + p1.X + p2.X) / (vcompo_t)(3.0);
        gp.Y = (p0.Y + p1.Y + p2.Y) / (vcompo_t)(3.0);
        gp.Z = (p0.Z + p1.Z + p2.Z) / (vcompo_t)(3.0);

        return gp;
    }

    // 線分apと点pの距離
    // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
    // 2D
    public static vcompo_t DistancePointToSeg2D(vector3_t a, vector3_t b, vector3_t p)
    {
        vcompo_t t;

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        t = InnrProduct2D(ab, ap);

        if (t < 0)
        {
            return ap.Norm2D();
        }

        vector3_t ba = a - b;
        vector3_t bp = p - b;

        t = InnrProduct2D(ba, bp);

        if (t < 0)
        {
            return bp.Norm2D();
        }

        // 外積結果が a->p a->b を辺とする平行四辺形の面積になる
        vcompo_t d = (vcompo_t)Math.Abs(OuterProduct2D(ab, ap));

        vcompo_t abl = ab.Norm2D();

        // 高さ = 面積 / 底辺の長さ
        return d / abl;
    }

    // 線分apと点pの距離
    // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
    // 3D対応
    public static vcompo_t DistancePointToSeg(vector3_t a, vector3_t b, vector3_t p)
    {
        vcompo_t t;

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        t = InnerProduct(ab, ap);

        if (t < 0)
        {
            return ap.Norm();
        }

        vector3_t ba = a - b;
        vector3_t bp = p - b;

        t = InnerProduct(ba, bp);

        if (t < 0)
        {
            return bp.Norm();
        }

        vector3_t cp = OuterProduct(ab, ap);

        // 外積結果の長さが a->p a->b を辺とする平行四辺形の面積になる
        vcompo_t s = cp.Norm();

        // 高さ = 面積 / 底辺の長さ
        return s / ab.Norm();
    }

    // 点が三角形内にあるか 2D版
    public static bool IsPointInTriangle2D(
        vector3_t p,
        vector3_t p0,
        vector3_t p1,
        vector3_t p2
        )
    {
        vcompo_t c1 = OuterProduct2D(p, p0, p1);
        vcompo_t c2 = OuterProduct2D(p, p1, p2);
        vcompo_t c3 = OuterProduct2D(p, p2, p0);

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
        vector3_t p,
        vector3_t p0,
        vector3_t p1,
        vector3_t p2
        )
    {
        vector3_t c1 = GetC1(p, p0, p1);
        vector3_t c2 = OuterProduct(p, p1, p2);
        vector3_t c3 = OuterProduct(p, p2, p0);

        vcompo_t ip12 = InnerProduct(c1, c2);
        vcompo_t ip13 = InnerProduct(c1, c3);

        // 外積の結果の符号が全て同じなら点は三角形の中
        // When all corossProduct result's sign are same, Point is in triangle
        if (ip12 > 0 && ip13 > 0)
        {
            return true;
        }

        return false;
    }

    private static vector3_t GetC1(vector3_t p, vector3_t p0, vector3_t p1)
    {
        return OuterProduct(p, p0, p1);
    }


    // 点pから線分abに向かう垂線との交点を求める
    public static CrossInfo PerpendicularCrossSeg(vector3_t a, vector3_t b, vector3_t p)
    {
        CrossInfo ret = default;

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        vector3_t ba = a - b;
        vector3_t bp = p - b;

        // A-B 単位ベクトル
        vector3_t unit_ab = ab.UnitVector();

        // B-A 単位ベクトル　(A-B単位ベクトルを反転) B側の中外判定に使用
        vector3_t unit_ba = unit_ab * (vcompo_t)(-1.0);

        // Aから交点までの距離 
        // A->交点->B or A->B->交点なら +
        // 交点<-A->B なら -
        vcompo_t dist_ax = InnerProduct(unit_ab, ap);

        // Bから交点までの距離 B側の中外判定に使用
        vcompo_t dist_bx = InnerProduct(unit_ba, bp);

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
    public static CrossInfo PerpendicularCrossSeg2D(vector3_t a, vector3_t b, vector3_t p)
    {
        CrossInfo ret = default;

        vcompo_t t1;

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        t1 = InnrProduct2D(ab, ap);

        if (t1 < 0)
        {
            return ret;
        }

        vcompo_t t2;

        vector3_t ba = a - b;
        vector3_t bp = p - b;

        t2 = InnrProduct2D(ba, bp);

        if (t2 < 0)
        {
            return ret;
        }

        vcompo_t abl = ab.Norm2D();
        vcompo_t abl2 = abl * abl;

        ret.IsCross = true;
        ret.CrossPoint.X = ab.X * t1 / abl2 + a.X;
        ret.CrossPoint.Y = ab.Y * t1 / abl2 + a.Y;

        return ret;
    }

    // 点pから線分abに向かう垂線との交点が線分ab内にあるか
    public static bool IsPointInSeg2D(vector3_t a, vector3_t b, vector3_t p)
    {
        vcompo_t t1;

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        t1 = InnrProduct2D(ab, ap);

        if (t1 < 0)
        {
            return false;
        }

        vcompo_t t2;

        vector3_t ba = a - b;
        vector3_t bp = p - b;

        t2 = InnrProduct2D(ba, bp);

        if (t2 < 0)
        {
            return false;
        }

        return true;
    }


    // 点pから直線abに向かう垂線との交点を求める
    public static CrossInfo PerpCrossLine(vector3_t a, vector3_t b, vector3_t p)
    {
        CrossInfo ret = default;

        if (a.Equals(b))
        {
            return ret;
        }

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        // A-B 単位ベクトル
        vector3_t unit_ab = ab.UnitVector();

        // Aから交点までの距離 
        vcompo_t dist_ax = InnerProduct(unit_ab, ap);

        ret.CrossPoint.X = a.X + (unit_ab.X * dist_ax);
        ret.CrossPoint.Y = a.Y + (unit_ab.Y * dist_ax);
        ret.CrossPoint.Z = a.Z + (unit_ab.Z * dist_ax);

        ret.IsCross = true;

        return ret;
    }

    // 点pから直線abに向かう垂線との交点を求める2D
    public static CrossInfo PerpendicularCrossLine2D(vector3_t a, vector3_t b, vector3_t p)
    {
        CrossInfo ret = default;

        vcompo_t t1;

        vector3_t ab = b - a;
        vector3_t ap = p - a;

        t1 = InnrProduct2D(ab, ap);

        vcompo_t norm = ab.Norm2D();
        vcompo_t norm2 = norm * norm;

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
    public static vector3_t CenterPoint(vector3_t a, vector3_t b)
    {
        vector3_t c = b - a;
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
    public static vector3_t LinePoint(vector3_t a, vector3_t b, vcompo_t len)
    {
        vector3_t v = b - a;

        v = v.UnitVector();

        v *= len;

        v += a;

        return v;
    }

    public static vcompo_t SegNorm2D(vector3_t a, vector3_t b)
    {
        vcompo_t dx = b.X - a.X;
        vcompo_t dy = b.Y - a.Y;

        return (vcompo_t)Math.Sqrt((dx * dx) + (dy * dy));
    }

    public static vcompo_t SegNorm(vector3_t a, vector3_t b)
    {
        vector3_t v = b - a;
        return v.Norm();
    }

    public static vcompo_t SegNormNZ(vector3_t a, vector3_t b)
    {
        a.Z = 0;
        b.Z = 0;

        vector3_t v = b - a;
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
    public static vector3_t CrossPlane(vector3_t a, vector3_t p, vector3_t normal)
    {
        vector3_t pa = a - p;

        // 法線とpaの内積をとる
        // 法線の順方向に点Aがあれば d>0 逆方向だと d<0
        vcompo_t d = InnerProduct(normal, pa);

        //内積値から平面上の最近点を求める
        vector3_t cp = default;
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
    public static vector3_t CrossPlane(vector3_t a, vector3_t b, vector3_t p, vector3_t normal)
    {
        vector3_t cp = default;

        vector3_t e = b - a;

        vcompo_t de = InnerProduct(normal, e);

        if (de > GetR0Min() && de < GetR0Max())
        {
            //DebugOut.Std.PrintLn("CrossPlane is parallel");

            // 平面と直線は平行
            return VectorExt.InvalidVector3;
        }

        vcompo_t d = InnerProduct(normal, p);
        vcompo_t t = (d - InnerProduct(normal, a)) / de;

        cp = a + (e * t);

        return cp;
    }


    private static vcompo_t GetR0Max()
    {
        return GetR0Max1();
    }

    private static vcompo_t GetR0Max1()
    {
        return GetR0Max2();
    }

    private static vcompo_t GetR0Max2()
    {
        return GetR0Max3();
    }

    private static vcompo_t GetR0Max3()
    {
        return R0Max;
    }

    private static vcompo_t GetR0Min()
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
    public static vector3_t CrossSegPlane(vector3_t a, vector3_t b, vector3_t p, vector3_t normal)
    {
        vector3_t cp = CrossPlane(a, b, p, normal);

        if (!cp.IsValid())
        {
            return VectorExt.InvalidVector3;
        }

        if (InnerProduct((b - a), (cp - a)) < 0)
        {
            return VectorExt.InvalidVector3;
        }

        if (NewMethod(a, b, cp) < 0)
        {
            return VectorExt.InvalidVector3;
        }

        return cp;
    }

    private static vcompo_t NewMethod(vector3_t a, vector3_t b, vector3_t cp)
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
    public static bool CheckCrossSegSeg2D(vector3_t p1, vector3_t p2, vector3_t p3, vector3_t p4)
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

    public static vcompo_t Angle2D(vector3_t v)
    {
        return (vcompo_t)Math.Atan2(v.Y, v.X);
    }

    public static vector3_t CrossLine2D(vector3_t a1, vector3_t a2, vector3_t b1, vector3_t b2)
    {
        vector3_t a = (a2 - a1);
        vector3_t b = (b2 - b1);

        if (a.IsZero() || b.IsZero())
        {
            return VectorExt.InvalidVector3;
        }

        vcompo_t cpBA = OuterProduct2D(b, a);

        if (cpBA == 0)
        {
            return VectorExt.InvalidVector3;
        }

        return a1 + a * OuterProduct2D(b, b1 - a1) / cpBA;
    }
}
