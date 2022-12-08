using OpenTK.Mathematics;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter;

public struct CrossInfo
{
    public bool IsCross;
    public vector3_t CrossPoint;
    public vcompo_t Distance;
}

