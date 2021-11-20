using MyCollections;

namespace CadDataTypes
{
    public class CadMesh
    {
        public VertexList VertexStore;
        public FlexArray<CadFace> FaceStore;

        public CadMesh()
        {
        }

        public CadMesh(int vertexCount, int faceCount)
        {
            VertexStore = new VertexList(vertexCount);
            FaceStore = new FlexArray<CadFace>(faceCount);
        }
    }
}
