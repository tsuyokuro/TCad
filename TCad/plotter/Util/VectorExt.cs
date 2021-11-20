using OpenTK;
using System;

namespace Plotter
{
    public static class VectorExt
    {
        public static readonly Vector3d InvalidVector3d = new Vector3d(Double.NaN, Double.NaN, Double.NaN);

        public static Vector4d ToVector4d(this Vector3d v, double w)
        {
            return new Vector4d(v.X, v.Y, v.Z, w);
        }

        public static Vector3d ToVector3d(this Vector4d v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static bool IsZero(this Vector3d v)
        {
            return (v.X + v.Y + v.Z) == 0;
        }

        public static bool IsInvalid(this Vector3d v)
        {
            return double.IsNaN(v.X);
        }

        public static bool IsValid(this Vector3d v)
        {
            return !double.IsNaN(v.X);
        }

        public static Vector3d Min(Vector3d v1, Vector3d v2)
        {
            Vector3d v = default(Vector3d);

            v.X = Math.Min(v1.X, v2.X);
            v.Y = Math.Min(v1.Y, v2.Y);
            v.Z = Math.Min(v1.Z, v2.Z);

            return v;
        }

        public static Vector3d Max(Vector3d v1, Vector3d v2)
        {
            Vector3d v = default;

            v.X = Math.Max(v1.X, v2.X);
            v.Y = Math.Max(v1.Y, v2.Y);
            v.Z = Math.Max(v1.Z, v2.Z);

            return v;
        }

        public static void Set(out Vector3d v, double x, double y, double z)
        {
            v.X = x;
            v.Y = y;
            v.Z = z;
        }

        public static void Set(out Vector4d v, double x, double y, double z, double w)
        {
            v.X = x;
            v.Y = y;
            v.Z = z;
            v.W = w;
        }

        public static Vector3d UnitVector(this Vector3d v)
        {
            double norm = v.Length;

            double f = 1.0 / norm;

            v.X *= f;
            v.Y *= f;
            v.Z *= f;

            return v;
        }

        public static double Norm(this Vector3d v)
        {
            return v.Length;
        }

        public static double Norm2D(this Vector3d v)
        {
            return Math.Sqrt((v.X * v.X) + (v.Y * v.Y));
        }

        public static bool EqualsThreshold(this Vector3d v, Vector3d p, double m = 0.000001)
        {
            return (
                v.X > p.X - m && v.X < p.X + m &&
                v.Y > p.Y - m && v.Y < p.Y + m &&
                v.Z > p.Z - m && v.Z < p.Z + m
                );
        }

        public static Vector3d Add(this Vector3d p1, double d)
        {
            p1.X += d;
            p1.Y += d;
            p1.Z += d;

            return p1;
        }

        public static string CoordString(this Vector3d v)
        {
            return v.X.ToString() + ", " + v.Y.ToString() + ", " + v.Z.ToString();
        }

        public static void dump(this Vector3d v, string prefix = nameof(Vector3d))
        {
            DOut.pl(prefix + "{");
            DOut.Indent++;
            DOut.pl("x:" + v.X.ToString());
            DOut.pl("y:" + v.Y.ToString());
            DOut.pl("z:" + v.Z.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
