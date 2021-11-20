using MyCollections;

namespace CadDataTypes
{
    public class VertexList : FlexArray<CadVertex>
    {
        public VertexList() : base(8) { }
        public VertexList(int capa) : base(capa) { }
        public VertexList(VertexList src) : base(src) { }
    }
}
