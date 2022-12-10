//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
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

    public void CheckMin(vector3_t p)
    {
        Min.X = (vcompo_t)Math.Min(Min.X, p.X);
        Min.Y = (vcompo_t)Math.Min(Min.Y, p.Y);
    }

    public void CheckMax(vector3_t p)
    {
        Max.X = (vcompo_t)Math.Max(Max.X, p.X);
        Max.Y = (vcompo_t)Math.Max(Max.Y, p.Y);
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
    }

    public void CheckMax(MinMax3D mm)
    {
        Max.X = (vcompo_t)Math.Max(Max.X, mm.Max.X);
        Max.Y = (vcompo_t)Math.Max(Max.Y, mm.Max.Y);
    }

    public void Check(MinMax3D mm)
    {
        CheckMin(mm);
        CheckMax(mm);
    }

    public vector3_t GetMinAsVector()
    {
        return new vector3_t((vcompo_t)Min.X, (vcompo_t)Min.Y, 0);
    }

    public vector3_t GetMaxAsVector()
    {
        return new vector3_t((vcompo_t)Max.X, (vcompo_t)Max.Y, 0);
    }
}
