using OpenTK.Mathematics;
using System;

namespace Plotter;

public struct MinMax3D
{
    public vector3_t Min;
    public vector3_t Max;

    public static MinMax3D Create(
        )
    {
        MinMax3D mm = default;

        mm.Min.X = vcompo_t.MaxValue;
        mm.Min.Y = vcompo_t.MaxValue;
        mm.Min.Z = vcompo_t.MaxValue;

        mm.Max.X = vcompo_t.MinValue;
        mm.Max.Y = vcompo_t.MinValue;
        mm.Max.Z = vcompo_t.MinValue;

        return mm;
    }

    public void CheckMin(vector3_t p)
    {
        Min.X = (vcompo_t)Math.Min(Min.X, p.X);
        Min.Y = (vcompo_t)Math.Min(Min.Y, p.Y);
        Min.Z = (vcompo_t)Math.Min(Min.Z, p.Z);
    }

    public void CheckMax(vector3_t p)
    {
        Max.X = (vcompo_t)Math.Max(Max.X, p.X);
        Max.Y = (vcompo_t)Math.Max(Max.Y, p.Y);
        Max.Z = (vcompo_t)Math.Max(Max.Z, p.Z);
    }

    public void Check(vector3_t p)
    {
        CheckMin(p);
        CheckMax(p);
    }

    public void CheckMin(MinMax3D mm)
    {
        Min.X = (vcompo_t)Math.Min(Min.X, mm.Min.X);
        Min.Y = (vcompo_t)Math.Min(Min.Y, mm.Min.Y);
        Min.Z = (vcompo_t)Math.Min(Min.Z, mm.Min.Z);
    }

    public void CheckMax(MinMax3D mm)
    {
        Max.X = (vcompo_t)Math.Max(Max.X, mm.Max.X);
        Max.Y = (vcompo_t)Math.Max(Max.Y, mm.Max.Y);
        Max.Z = (vcompo_t)Math.Max(Max.Z, mm.Max.Z);
    }

    public void Check(MinMax3D mm)
    {
        CheckMin(mm);
        CheckMax(mm);
    }

    public vector3_t GetMinAsVector()
    {
        return Min;
    }

    public vector3_t GetMaxAsVector()
    {
        return Max;
    }
}
