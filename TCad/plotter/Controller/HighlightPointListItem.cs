using OpenTK.Mathematics;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

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
