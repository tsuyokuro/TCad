//#define DEFAULT_DATA_TYPE_DOUBLE
using MyCollections;


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

public class VertexList : FlexArray<CadVertex>
{
    public VertexList() : base(8) { }
    public VertexList(int capa) : base(capa) { }
    public VertexList(VertexList src) : base(src) { }
}
