using CadDataTypes;
using OpenTK.Mathematics;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace SplineCurve;

public class SplineUtil
{
    /*
            +  +  +  +
            |
         v  +  +  +  +
     vcnt:3 |
            +--+--+--+
                u
             ucnt:4
    */
    public static VertexList CreateFlatControlPoints(int ucnt, int vcnt, vector3_t uunit, vector3_t vunit)
    {
        VertexList vl = new VertexList(ucnt * vcnt);

        vector3_t ud = ((vcompo_t)(ucnt-1) / (vcompo_t)(2.0)) * uunit;
        vector3_t vd = ((vcompo_t)(vcnt-1) / (vcompo_t)(2.0)) * vunit;

        vector3_t p = vector3_t.Zero;

        p -= ud;
        p -= vd;

        vector3_t lp = p;

        for (int v = 0; v < vcnt; v++)
        {
            p = lp;

            for (int u = 0; u < ucnt; u++)
            {
                vl.Add(new CadVertex(p));
                p += uunit;
            }

            lp += vunit;
        }

        return vl;
    }

    public static VertexList CreateBoxControlPoints(
        int ucnt, int vcnt,
        vector3_t uunit, vector3_t vunit, vector3_t tunit
        )
    {
        VertexList vl = new VertexList(ucnt * vcnt);

        vector3_t ud = ((vcompo_t)(ucnt - 1) / (vcompo_t)(2.0)) * uunit;
        vector3_t vd = ((vcompo_t)(vcnt - 1) / (vcompo_t)(2.0)) * vunit;

        vector3_t p = vector3_t.Zero;

        p -= ud;
        p -= vd;

        vector3_t lp = p;

        for (int v = 0; v < vcnt; v++)
        {
            p = lp;

            for (int u = 0; u < ucnt; u++)
            {
                vl.Add(new CadVertex(p));
                p += uunit;
            }

            p -= uunit;

            p += tunit;

            for (int u = 0; u < ucnt; u++)
            {
                vl.Add(new CadVertex(p));
                p -= uunit;
            }

            lp += vunit;
        }

        return vl;
    }
}

public class BSpline
{
    public static vcompo_t Epsilon = 0.000001f;   // とても小さい値

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i">Knot番号</param>
    /// <param name="degree">次数</param>
    /// <param name="t">媒介変数</param>
    /// <param name="knots">Knot配列</param>
    /// <returns></returns>
    public static vcompo_t BasisFunc(int i, int degree, vcompo_t t, vcompo_t[] knots)
    {
        if (degree == 0)
        {
            if (t >= knots[i] && t < knots[i + 1])
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }

        vcompo_t w1 = 0d;
        vcompo_t w2 = 0d;
        vcompo_t d1 = knots[i + degree] - knots[i];
        vcompo_t d2 = knots[i + degree + 1] - knots[i + 1];

        if (d1 != 0d)
        {
            w1 = (t - knots[i]) / d1;
        }

        if (d2 != 0d)
        {
            w2 = (knots[i + degree + 1] - t) / d2;
        }

        vcompo_t term1 = 0d;
        vcompo_t term2 = 0d;

        if (w1 != 0d)
        {
            term1 = w1 * BasisFunc(i, degree - 1, t, knots);
        }

        if (w2 != 0d)
        {
            term2 = w2 * BasisFunc(i + 1, degree - 1, t, knots);
        }

        return term1 + term2;
    }

    // 2次での一様なBスプライン基底関数。
    public static CadVertex CalcurateUniformBSplinePointWithDegree2(VertexList vl, int i, vcompo_t t)
    {
        return 0.5f * (t * t - 2f * t + 1f) * vl[i] +
            0.5f * (-2f * t * t + 2f * t + 1f) * vl[i + 1] +
            0.5f * t * t * vl[i + 2];
    }

    // 3次での一様なBスプライン基底関数。
    public static CadVertex CalcurateUniformBSplinePointWithDegree3(VertexList vl, int i, vcompo_t t)
    {
        return 1f / 6f * (-vl[i] + 3f * vl[i + 1] - 3f * vl[i + 2] + vl[i + 3]) * t * t * t +
            0.5f * (vl[i] - 2f * vl[i + 1] + vl[i + 2]) * t * t +
            0.5f * (-vl[i] + vl[i + 2]) * t +
            1f / 6f * (vl[i] + 4f * vl[i + 1] + vl[i + 2]);
    }
}
