using TCad.Plotter.DrawToolSet;

namespace TCad.Plotter.Controller;

public struct HighlightPointListItem
{
    public vector3_t Point;
    public DrawPen Pen;

    public HighlightPointListItem(vector3_t p, DrawPen pen)
    {
        Point = p;
        Pen = pen;
    }
}
