//#define DEFAULT_DATA_TYPE_DOUBLE
//#define LOG_DEBUG

using CadDataTypes;



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

public class CadFigureGroup : CadFigure
{
    public CadFigureGroup()
    {
        Type = Types.GROUP;
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
    }

    public override void StartCreate(DrawContext dc)
    {
    }
}
