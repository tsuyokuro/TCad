//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using System.Reflection.Metadata;


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


namespace CadDataTypes;

public struct CadVertexAttr
{
    public Color4 Color;

    public vector3_t Normal;

    public CadVertexAttr()
    {
        Color.A = -1;
        Color.R = -1;
        Color.G = -1;
        Color.B = -1;

        Normal = vector3_t.Zero;
    }
}
