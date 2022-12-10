//#define DEFAULT_DATA_TYPE_DOUBLE
using System;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace Plotter;

class BezierFuncs
{
    static vcompo_t[] FactorialTbl =
    {
        (vcompo_t)(1.0), // 0!
        (vcompo_t)(1.0),
        (vcompo_t)(2.0) * (vcompo_t)(1.0),
        (vcompo_t)(3.0) * (vcompo_t)(2.0) * (vcompo_t)(1.0),
        (vcompo_t)(4.0) * (vcompo_t)(3.0) * (vcompo_t)(2.0) * (vcompo_t)(1.0),
        (vcompo_t)(5.0) * (vcompo_t)(4.0) * (vcompo_t)(3.0) * (vcompo_t)(2.0) * (vcompo_t)(1.0),
        (vcompo_t)(6.0) * (vcompo_t)(5.0) * (vcompo_t)(4.0) * (vcompo_t)(3.0) * (vcompo_t)(2.0) * (vcompo_t)(1.0),
    };

    // Bernstein basis polynomials
    public static vcompo_t BernsteinBasisF(int n, int i, vcompo_t t)
    {
        return BinomialCoefficientsF(n, i) * (vcompo_t)Math.Pow(t, i) * (vcompo_t)Math.Pow(1 - t, n - i);
    }

    // Binomial coefficient
    public static vcompo_t BinomialCoefficientsF(int n, int k)
    {
        return FactorialTbl[n] / (FactorialTbl[k] * FactorialTbl[n - k]);
    }



    // Bernstein basis polynomials
    public static vcompo_t BernsteinBasis(int n, int i, vcompo_t t)
    {
        return BinomialCoefficients(n, i) * (vcompo_t)Math.Pow(t, i) * (vcompo_t)Math.Pow(1 - t, n - i);
    }

    // Binomial coefficient
    public static vcompo_t BinomialCoefficients(int n, int k)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

    // e.g 6! = 6*5*4*3*2*1
    public static vcompo_t Factorial(int a)
    {
        vcompo_t r = (vcompo_t)(1.0);
        for (int i = 2; i <= a; i++)
        {
            r *= (vcompo_t)i;
        }

        return r;
    }
}
