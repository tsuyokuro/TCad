using OpenTK.Mathematics;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

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
