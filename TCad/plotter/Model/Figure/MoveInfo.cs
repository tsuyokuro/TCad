using OpenTK.Mathematics;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public struct MoveInfo
{
    public vector3_t Start;

    public vector3_t Moved;

    public vector3_t CursorScrnPoint;

    public vector3_t Delta;

    public MoveInfo(vector3_t start, vector3_t moved, vector3_t cursorPos)
    {
        Start = start;
        Moved = moved;
        CursorScrnPoint = cursorPos;
        Delta = Moved - start;
    }
}
