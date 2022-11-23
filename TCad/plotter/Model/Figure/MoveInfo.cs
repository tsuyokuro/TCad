using OpenTK.Mathematics;

namespace Plotter;

public struct MoveInfo
{
    public Vector3d Start;

    public Vector3d Moved;

    public Vector3d CursorScrnPoint;

    public Vector3d Delta;

    public MoveInfo(Vector3d start, Vector3d moved, Vector3d cursorPos)
    {
        Start = start;
        Moved = moved;
        CursorScrnPoint = cursorPos;
        Delta = Moved - start;
    }
}
