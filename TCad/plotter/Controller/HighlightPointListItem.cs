using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public class HighlightPointListItem
    {
        public Vector3d Point;
        public DrawPen Pen;

        public HighlightPointListItem(Vector3d p, DrawPen pen)
        {
            Point = p;
            Pen = pen;
        }
    }
}
