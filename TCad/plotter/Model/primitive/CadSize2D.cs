//#define DEFAULT_DATA_TYPE_DOUBLE


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

public struct CadSize2D
{
    public vcompo_t Width;
    public vcompo_t Height;

    public CadSize2D(vcompo_t w, vcompo_t h)
    {
        Width = w;
        Height = h;
    }

    public static CadSize2D operator * (CadSize2D me,vcompo_t f)
    {
        return new CadSize2D(me.Width * f, me.Height * f);
    } 
}
