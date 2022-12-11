//#define DEFAULT_DATA_TYPE_DOUBLE
using MyCollections;
using OpenTK;
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


namespace CadDataTypes;

public class Vector3List : FlexArray<vector3_t>
{
    public Vector3List() : base(8) { }
    public Vector3List(int capa) : base(capa) { }
    public Vector3List(Vector3List src) : base(src) { }
}
