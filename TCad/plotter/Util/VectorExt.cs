using System;

namespace TCad.Plotter;

public static class VectorExt
{
    public static readonly vector3_t InvalidVector3 = new vector3_t(vcompo_t.NaN, vcompo_t.NaN, vcompo_t.NaN);

    public static vector4_t ToVector4(this vector3_t v, vcompo_t w)
    {
        return new vector4_t(v.X, v.Y, v.Z, w);
    }

    public static vector3_t ToVector3(this vector4_t v)
    {
        return new vector3_t(v.X, v.Y, v.Z);
    }

    public static bool IsZero(this vector3_t v)
    {
        return (v.X + v.Y + v.Z) == 0;
    }

    public static bool IsInvalid(this vector3_t v)
    {
        return vcompo_t.IsNaN(v.X);
    }

    public static bool IsValid(this vector3_t v)
    {
        return !vcompo_t.IsNaN(v.X);
    }

    public static vector3_t Min(vector3_t v1, vector3_t v2)
    {
        vector3_t v = default(vector3_t);

        v.X = Math.Min(v1.X, v2.X);
        v.Y = Math.Min(v1.Y, v2.Y);
        v.Z = Math.Min(v1.Z, v2.Z);

        return v;
    }

    public static vector3_t Max(vector3_t v1, vector3_t v2)
    {
        vector3_t v = default;

        v.X = Math.Max(v1.X, v2.X);
        v.Y = Math.Max(v1.Y, v2.Y);
        v.Z = Math.Max(v1.Z, v2.Z);

        return v;
    }

    public static void Set(out vector3_t v, vcompo_t x, vcompo_t y, vcompo_t z)
    {
        v.X = x;
        v.Y = y;
        v.Z = z;
    }

    public static void Set(out vector4_t v, vcompo_t x, vcompo_t y, vcompo_t z, vcompo_t w)
    {
        v.X = x;
        v.Y = y;
        v.Z = z;
        v.W = w;
    }

    public static vector3_t UnitVector(this vector3_t v)
    {
        vcompo_t norm = v.Length;

        vcompo_t f = (vcompo_t)1.0 / norm;

        v.X *= f;
        v.Y *= f;
        v.Z *= f;

        return v;
    }

    public static vcompo_t Norm(this vector3_t v)
    {
        return v.Length;
    }

    public static vcompo_t Norm2D(this vector3_t v)
    {
        return (vcompo_t)Math.Sqrt((v.X * v.X) + (v.Y * v.Y));
    }

    public static bool EqualsThreshold(this vector3_t v, vector3_t p, vcompo_t m = (vcompo_t)0.000001)
    {
        return (
            v.X > p.X - m && v.X < p.X + m &&
            v.Y > p.Y - m && v.Y < p.Y + m &&
            v.Z > p.Z - m && v.Z < p.Z + m
            );
    }

    public static vector3_t Add(this vector3_t p1, vcompo_t d)
    {
        p1.X += d;
        p1.Y += d;
        p1.Z += d;

        return p1;
    }

    public static string CoordString(this vector3_t v)
    {
        return v.X.ToString() + ", " + v.Y.ToString() + ", " + v.Z.ToString();
    }

    public static void dump(this vector3_t v, string prefix = nameof(vector3_t))
    {
        Log.pl(prefix + "{");
        Log.Indent++;
        Log.pl("x:" + v.X.ToString());
        Log.pl("y:" + v.Y.ToString());
        Log.pl("z:" + v.Z.ToString());
        Log.Indent--;
        Log.pl("}");
    }
}



