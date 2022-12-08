using System.Drawing;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter;

public abstract class ColorSet
{
    public readonly Color[] PenColorTbl = new Color[DrawTools.PEN_TBL_SIZE];
    public readonly Color[] BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];
}
