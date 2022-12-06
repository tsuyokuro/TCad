using OpenTK.Mathematics;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public struct CrossInfo
{
    public bool IsCross;
    public vector3_t CrossPoint;
    public vcompo_t Distance;
}

