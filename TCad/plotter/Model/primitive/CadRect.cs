//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;



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

// 直方体の対角線を保持
public struct CadRect
{
    public vector3_t p0;
    public vector3_t p1;

    public void Normalize()
    {
        vector3_t minv = p0;
        vector3_t maxv = p0;

        if (p0.X < p1.X)
        {
            maxv.X = p1.X;
        }
        else
        {
            minv.X = p1.X;
        }

        if (p0.Y < p1.Y)
        {
            maxv.Y = p1.Y;
        }
        else
        {
            minv.Y = p1.Y;
        }

        if (p0.Z < p1.Z)
        {
            maxv.Z = p1.Z;
        }
        else
        {
            minv.Z = p1.Z;
        }

        p0 = minv;
        p1 = maxv;
    }

    public vector3_t Center()
    {
        vector3_t cv = default;

        cv.X = p0.X + ((p1.X - p0.X) / (vcompo_t)(2.0));
        cv.Y = p0.Y + ((p1.Y - p0.Y) / (vcompo_t)(2.0));
        cv.Z = p0.Z + ((p1.Z - p0.Z) / (vcompo_t)(2.0));

        return cv;
    }
}
