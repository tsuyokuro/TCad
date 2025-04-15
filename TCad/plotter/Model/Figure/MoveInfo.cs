using OpenTK.Mathematics;

namespace Plotter;

public struct MoveRestriction
{
    public const uint POLY_LINES_WITH_NORMAL = 0b_0000_0000_0000_0000_0000_0000_0000_0001;

    public uint Flags = 0;


    public bool IsOn(uint flag)
    {
        return (Flags & flag) != 0;
    }

    public void On(uint flag)
    {
        Flags |= flag;
    }

    public void Off(uint flag)
    {
        Flags = Flags & (~flag);
    }

    public MoveRestriction()
    {
        Flags = 0;
        On(POLY_LINES_WITH_NORMAL);
    }

    public MoveRestriction(uint flags)
    {
        Flags = 0;
        On(flags);
    }
}


public struct MoveInfo
{
    public MoveRestriction Restrict = new();

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

        Restrict = new(MoveRestriction.POLY_LINES_WITH_NORMAL);
    }

    public MoveInfo(vector3_t start, vector3_t moved, vector3_t cursorPos, MoveRestriction restrict)
    {
        Start = start;
        Moved = moved;
        CursorScrnPoint = cursorPos;
        Delta = Moved - start;

        Restrict = restrict;
    }
}
