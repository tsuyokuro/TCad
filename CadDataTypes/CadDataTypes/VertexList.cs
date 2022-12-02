using MyCollections;

using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace CadDataTypes;

public class VertexList : FlexArray<CadVertex>
{
    public VertexList() : base(8) { }
    public VertexList(int capa) : base(capa) { }
    public VertexList(VertexList src) : base(src) { }
}
