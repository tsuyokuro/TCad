using MyCollections;

namespace CadDataTypes;

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

    public CadMesh(CadMesh src)
    {
        VertexStore = new(src.VertexStore);
        FaceStore = new FlexArray<CadFace>(src.FaceStore.Capacity);
        for (int i = 0; i < src.FaceStore.Count; i++)
        {
            FaceStore.Add(new CadFace(src.FaceStore[i]));
        }
    }
}
