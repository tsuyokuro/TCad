using MyCollections;
using OpenTK;

namespace CadDataTypes
{
    public class Vector3dList : FlexArray<Vector3d>
    {
        public Vector3dList() : base(8) { }
        public Vector3dList(int capa) : base(capa) { }
        public Vector3dList(Vector3dList src) : base(src) { }
    }
}
