using MyCollections;

using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace CadDataTypes;

public class VertexList : FlexArray<CadVertex>
{
    public VertexList() : base(8) { }
    public VertexList(int capa) : base(capa) { }
    public VertexList(VertexList src) : base(src) { }
}
