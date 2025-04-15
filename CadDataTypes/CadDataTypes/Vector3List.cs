using MyCollections;
using OpenTK;
using OpenTK.Mathematics;

namespace CadDataTypes;

public class Vector3List : FlexArray<vector3_t>
{
    public Vector3List() : base(8) { }
    public Vector3List(int capa) : base(capa) { }
    public Vector3List(Vector3List src) : base(src) { }
}
