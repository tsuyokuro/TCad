using MyCollections;
using OpenTK;
using OpenTK.Mathematics;

using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace CadDataTypes
{
    public class Vector3List : FlexArray<vector3_t>
    {
        public Vector3List() : base(8) { }
        public Vector3List(int capa) : base(capa) { }
        public Vector3List(Vector3List src) : base(src) { }
    }
}
