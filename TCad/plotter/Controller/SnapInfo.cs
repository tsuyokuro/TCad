namespace Plotter.Controller;

public struct SnapInfo
{
    public CadCursor Cursor;
    public vector3_t SnapPoint;

    public bool IsPointMatch { get; set; }

    public PointSearcher PointSearcher;

    public SegSearcher SegSearcher;

    public SnapInfo(
        CadCursor cursor,
        vector3_t snapPoint,
        PointSearcher pointSearcher,
        SegSearcher segSearcher
        )
    {
        Cursor = cursor;
        SnapPoint = snapPoint;
        IsPointMatch = false;
        PointSearcher = pointSearcher;
        SegSearcher = segSearcher;
    }
}
