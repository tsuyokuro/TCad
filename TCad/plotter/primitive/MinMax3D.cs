using System;
using OpenTK;

namespace Plotter
{
    public struct MinMax3D
    {
        public Vector3d Min;
        public Vector3d Max;

        public static MinMax3D Create(
            )
        {
            MinMax3D mm = default;

            mm.Min.X = Double.MaxValue;
            mm.Min.Y = Double.MaxValue;
            mm.Min.Z = Double.MaxValue;

            mm.Max.X = Double.MinValue;
            mm.Max.Y = Double.MinValue;
            mm.Max.Z = Double.MinValue;

            return mm;
        }

        public void CheckMin(Vector3d p)
        {
            Min.X = Math.Min(Min.X, p.X);
            Min.Y = Math.Min(Min.Y, p.Y);
            Min.Z = Math.Min(Min.Z, p.Z);
        }

        public void CheckMax(Vector3d p)
        {
            Max.X = Math.Max(Max.X, p.X);
            Max.Y = Math.Max(Max.Y, p.Y);
            Max.Z = Math.Max(Max.Z, p.Z);
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
            Min.Z = Math.Min(Min.Z, mm.Min.Z);
        }

        public void CheckMax(MinMax3D mm)
        {
            Max.X = Math.Max(Max.X, mm.Max.X);
            Max.Y = Math.Max(Max.Y, mm.Max.Y);
            Max.Z = Math.Max(Max.Z, mm.Max.Z);
        }

        public void Check(MinMax3D mm)
        {
            CheckMin(mm);
            CheckMax(mm);
        }

        public Vector3d GetMinAsVector()
        {
            return Min;
        }

        public Vector3d GetMaxAsVector()
        {
            return Max;
        }
    }
}