//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


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
