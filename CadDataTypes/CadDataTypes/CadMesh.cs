using MyCollections;

using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

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
