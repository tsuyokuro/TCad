
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter;

public struct IndexPair
{
    public int Idx0;
    public int Idx1;

    public IndexPair(int i0, int i1)
    {
        Idx0 = i0;
        Idx1 = i1;
    }
}
