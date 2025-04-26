using System;

namespace TCad.MathFunctions;

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
