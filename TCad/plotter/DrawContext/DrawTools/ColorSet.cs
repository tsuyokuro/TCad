//#define DEFAULT_DATA_TYPE_DOUBLE
using System.Drawing;



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

public abstract class ColorSet
{
    public readonly Color[] PenColorTbl = new Color[DrawTools.PEN_TBL_SIZE];
    public readonly Color[] BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];
}
