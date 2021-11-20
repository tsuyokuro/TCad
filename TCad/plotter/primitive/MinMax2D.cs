using System;
using OpenTK;

namespace Plotter
{
    public struct MinMax2D
    {
        public Vector2d Min;
        public Vector2d Max;

        public static MinMax2D Create()
        {
            MinMax2D mm = default(MinMax2D);

            mm.Min.X = Double.MaxValue;
            mm.Min.Y = Double.MaxValue;

            mm.Max.X = Double.MinValue;
            mm.Max.Y = Double.MinValue;
            return mm;
        }

        public void CheckMin(Vector3d p)
        {
            Min.X = Math.Min(Min.X, p.X);
            Min.Y = Math.Min(Min.Y, p.Y);
        }

        public void CheckMax(Vector3d p)
        {
            Max.X = Math.Max(Max.X, p.X);
            Max.Y = Math.Max(Max.Y, p.Y);
        }

        public void Check(Vector3d p)
        {
            CheckMin(p);
            CheckMax(p);
        }

        public void CheckMin(MinMax3D mm)
        {
            Min.X = Math.Min(Min.X, mm.Min.X);
            Min.Y = Math.Min(Min.Y, mm.Min.Y);
        }

        public void CheckMax(MinMax3D mm)
        {
            Max.X = Math.Max(Max.X, mm.Max.X);
            Max.Y = Math.Max(Max.Y, mm.Max.Y);
        }

        public void Check(MinMax3D mm)
        {
            CheckMin(mm);
            CheckMax(mm);
        }

        public Vector3d GetMinAsVector()
        {
            return new Vector3d(Min.X, Min.Y, 0);
        }

        public Vector3d GetMaxAsVector()
        {
            return new Vector3d(Max.X, Max.Y, 0);
        }
    }
}