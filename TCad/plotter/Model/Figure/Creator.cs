//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Mathematics;



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

public abstract class FigCreator
{
    public enum State : byte
    {
        NONE,
        NOT_ENOUGH,
        ENOUGH,
        WAIT_LAST_POINT,
        WAIT_NEXT_POINT,
        FULL,
    }

    public abstract CadFigure Figure
    {
        get;
    }

    protected FigCreator() { }


    public abstract void AddPointInCreating(DrawContext dc, CadVertex p);

    public abstract void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen);

    public abstract void EndCreate(DrawContext dc);

    public abstract void StartCreate(DrawContext dc);

    public abstract State GetCreateState();

    public static FigCreator Get(CadFigure.Types createType, CadFigure fig)
    {
        CadFigure.Types type = createType;

        FigCreator creator = null;

        switch (type)
        {
            case CadFigure.Types.LINE:
                creator = new LineCreator(fig);
                break;

            case CadFigure.Types.RECT:
                creator = new RectCreator(fig);
                break;

            case CadFigure.Types.POLY_LINES:
                creator = new PolyLinesCreator(fig);
                break;

            case CadFigure.Types.CIRCLE:
                creator = new CircleCreator(fig);
                break;

            case CadFigure.Types.POINT:
                creator = new PointCreator(fig);
                break;

            case CadFigure.Types.DIMENTION_LINE:
                creator = new DimLineCreator(fig);
                break;

            default:
                break;
        }

        return creator;
    }
}

public class PolyLinesCreator : FigCreator
{
    protected CadFigurePolyLines Figure_;

    public override CadFigure Figure
    {
        get => Figure_;
    }


    public PolyLinesCreator(CadFigure fig)
    {
        Figure_ = (CadFigurePolyLines)fig;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        if (Figure_.PointList.Count > 0)
        {
            if (Figure_.PointList.End().vector == p.vector)
            {
                return;
            }
        }

        Figure_.PointList.Add(p);
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        if (Figure_.PointCount == 0)
        {
            return;
        }

        CadVertex lastPt = Figure_.PointList[Figure_.PointCount - 1];

        dc.Drawing.DrawLine(pen, lastPt.vector, tp.vector);
    }

    public override void StartCreate(DrawContext dc)
    {
        Figure_.StartCreate(dc);
    }

    public override void EndCreate(DrawContext dc)
    {
        if (Figure_.PointList.Count > 2)
        {
            //vector3_t normal = CadUtil.RepresentativeNormal(fig.PointList);
            //vcompo_t t = vector3_t.Dot(normal, DC.ViewDir);

            Figure_.Normal = dc.ViewDir;
            Figure_.Normal *= -1;
        }

        Figure_.EndCreate(dc);
    }

    public override State GetCreateState()
    {
        if (Figure_.PointList.Count < 2)
        {
            return State.NOT_ENOUGH;
        }
        else if (Figure_.PointList.Count == 2)
        {
            return State.ENOUGH;
        }
        else if (Figure_.PointList.Count > 2)
        {
            return State.WAIT_NEXT_POINT;
        }

        return State.NONE;
    }
}

public class LineCreator : PolyLinesCreator
{
    public LineCreator(CadFigure fig) : base(fig)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
        Figure_.Type = CadFigure.Types.POLY_LINES;
        Figure_.EndCreate(dc);
    }

    public override State GetCreateState()
    {
        if (Figure_.PointList.Count < 1)
        {
            return State.NOT_ENOUGH;
        }
        else if (Figure_.PointList.Count < 2)
        {
            return State.WAIT_LAST_POINT;
        }

        return State.FULL;
    }
}

public class RectCreator : FigCreator
{
    CadFigurePolyLines Figure_;
    public override CadFigure Figure
    {
        get => Figure_;
    }


    public RectCreator(CadFigure fig)
    {
        Figure_ = (CadFigurePolyLines)fig;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        if (Figure_.PointList.Count == 0)
        {
            Figure_.PointList.Add(p);
        }
        else
        {
            vector3_t p0 = Figure_.PointList[0].vector;
            vector3_t p2 = p.vector;

            if (p0 == p2)
            {
                return;
            }

            vector3_t hv = CadMath.OuterProduct(dc.UpVector, dc.ViewDir).Normalized();
            vector3_t uv = dc.UpVector;

            vector3_t crossV = p2 - p0;

            vector3_t v1 = CadMath.InnerProduct(crossV, hv) * hv;
            vector3_t p1 = v1 + p0;

            vector3_t v3 = CadMath.InnerProduct(crossV, uv) * uv;
            vector3_t p3 = v3 + p0;

            Figure_.PointList.Add(new CadVertex(p3));
            Figure_.PointList.Add(new CadVertex(p2));
            Figure_.PointList.Add(new CadVertex(p1));

            Figure_.IsLoop = true;
        }
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        if (Figure_.PointList.Count <= 0)
        {
            return;
        }

        dc.Drawing.DrawRect(pen, Figure_.PointList[0].vector, tp.vector);
    }
    public override void StartCreate(DrawContext dc)
    {
        Figure_.StartCreate(dc);
    }

    public override void EndCreate(DrawContext dc)
    {
        Figure_.Normal = dc.ViewDir;
        Figure_.Normal *= -1;
        Figure_.Type = CadFigure.Types.POLY_LINES;
        Figure_.EndCreate(dc);
    }

    public override State GetCreateState()
    {
        if (Figure_.PointList.Count < 1)
        {
            return State.NOT_ENOUGH;
        }
        else if (Figure_.PointList.Count < 4)
        {
            return State.WAIT_LAST_POINT;
        }

        return State.FULL;
    }
}

public class CircleCreator : FigCreator
{
    CadFigureCircle Figure_;
    public override CadFigure Figure
    {
        get => Figure_;
    }

    public CircleCreator(CadFigure fig)
    {
        Figure_ = (CadFigureCircle)fig;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        Figure_.PointList.Add(p);
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        Figure_.DrawTemp(dc, tp, pen);
    }

    public override void EndCreate(DrawContext dc)
    {
        Figure_.EndCreate(dc);
    }

    public override State GetCreateState()
    {
        if (Figure_.PointList.Count < 1)
        {
            return State.NOT_ENOUGH;
        }
        else if (Figure_.PointList.Count < 2)
        {
            return State.WAIT_LAST_POINT;
        }

        return State.FULL;
    }

    public override void StartCreate(DrawContext dc)
    {
        Figure_.StartCreate(dc);
    }
}

public class DimLineCreator : FigCreator
{
    CadFigureDimLine Figure_;
    public override CadFigure Figure
    {
        get => Figure_;
    }

    public DimLineCreator(CadFigure fig)
    {
        Figure_ = (CadFigureDimLine)fig;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        Figure_.PointList.Add(p);
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        Figure_.DrawTemp(dc, tp, pen);
    }

    public override void StartCreate(DrawContext dc)
    {
        Figure_.StartCreate(dc);
    }

    public override void EndCreate(DrawContext dc)
    {
        Figure_.EndCreate(dc);
    }

    public override State GetCreateState()
    {
        if (Figure_.PointList.Count < 2)
        {
            return State.NOT_ENOUGH;
        }
        else if (Figure_.PointList.Count < 3)
        {
            return State.WAIT_LAST_POINT;
        }

        return State.FULL;
    }
}

public class PointCreator : FigCreator
{
    CadFigurePoint Figure_;
    public override CadFigure Figure
    {
        get => Figure_;
    }

    public PointCreator(CadFigure fig)
    {
        Figure_ = (CadFigurePoint)fig;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        Figure_.PointList.Add(p);
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        Figure_.DrawTemp(dc, tp, pen);
    }

    public override void EndCreate(DrawContext dc)
    {
        Figure_.EndCreate(dc);
    }

    public override void StartCreate(DrawContext dc)
    {
        Figure_.StartCreate(dc);
    }

    public override State GetCreateState()
    {
        if (Figure_.PointList.Count < 1)
        {
            return State.NOT_ENOUGH;
        }

        return State.FULL;
    }
}
