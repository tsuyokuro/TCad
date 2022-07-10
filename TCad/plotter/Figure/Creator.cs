using CadDataTypes;
using OpenTK.Mathematics;

namespace Plotter
{
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
                //Vector3d normal = CadUtil.RepresentativeNormal(fig.PointList);
                //double t = Vector3d.Dot(normal, dc.ViewDir);

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
                Vector3d p0 = Figure_.PointList[0].vector;
                Vector3d p2 = p.vector;

                Vector3d hv = CadMath.CrossProduct(dc.UpVector, dc.ViewDir).Normalized();
                Vector3d uv = dc.UpVector;

                Vector3d crossV = p2 - p0;

                Vector3d v1 = CadMath.InnerProduct(crossV, hv) * hv;
                Vector3d p1 = v1 + p0;

                Vector3d v3 = CadMath.InnerProduct(crossV, uv) * uv;
                Vector3d p3 = v3 + p0;

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
}