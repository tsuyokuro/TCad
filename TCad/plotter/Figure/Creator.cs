using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public partial class CadFigure
    {
        public abstract class Creator
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

            public virtual CadFigure Figure
            {
                get;
                set;
            } = null;


            public virtual void AddPointInCreating(DrawContext dc, CadVertex p)
            {
                Figure.AddPointInCreating(dc, p);
            }

            public virtual void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
            {
                Figure.DrawTemp(dc, tp, pen);
            }

            public virtual void EndCreate(DrawContext dc)
            {
                Figure.EndCreate(dc);
            }

            public virtual void StartCreate(DrawContext dc)
            {
                Figure.StartCreate(dc);
            }

            public abstract State GetCreateState();

            public static Creator Get(CadFigure.Types createType, CadFigure fig)
            {
                CadFigure.Types type = createType;

                Creator creator = null;

                switch (type)
                {
                    case Types.LINE:
                        creator = new LineCreator(fig);
                        break;

                    case Types.RECT:
                        creator = new RectCreator(fig);
                        break;

                    case Types.POLY_LINES:
                        creator = new PolyLinesCreator(fig);
                        break;

                    case Types.CIRCLE:
                        creator = new CircleCreator(fig);
                        break;

                    case Types.POINT:
                        creator = new PointCreator(fig);
                        break;

                    case Types.DIMENTION_LINE:
                        creator = new DimLineCreator(fig);
                        break;

                    default:
                        break;
                }

                return creator;
            }
        }

        public class PolyLinesCreator : Creator
        {
            public PolyLinesCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override void AddPointInCreating(DrawContext dc, CadVertex p)
            {
                Figure.mPointList.Add(p);
            }

            public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
            {
                if (Figure.PointCount == 0)
                {
                    return;
                }

                CadVertex lastPt = Figure.PointList[Figure.PointCount - 1];

                dc.Drawing.DrawLine(pen, lastPt.vector, tp.vector);
            }

            public override void EndCreate(DrawContext dc)
            {
                if (Figure.PointList.Count > 2)
                {
                    //Vector3d normal = CadUtil.RepresentativeNormal(fig.PointList);
                    //double t = Vector3d.Dot(normal, dc.ViewDir);

                    Figure.Normal = dc.ViewDir;
                    Figure.Normal *= -1;
                }
            }

            public override State GetCreateState()
            {
                if (Figure.PointList.Count < 2)
                {
                    return State.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count == 2)
                {
                    return State.ENOUGH;
                }
                else if (Figure.PointList.Count > 2)
                {
                    return State.WAIT_NEXT_POINT;
                }

                return State.NONE;
            }

            public override void StartCreate(DrawContext dc)
            {
                // NOP
            }
        }

        public class RectCreator : Creator
        {
            public RectCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override void AddPointInCreating(DrawContext dc, CadVertex p)
            {
                if (Figure.mPointList.Count == 0)
                {
                    Figure.mPointList.Add(p);
                }
                else
                {
                    Vector3d p0 = Figure.PointList[0].vector;
                    Vector3d p2 = p.vector;

                    Vector3d hv = CadMath.CrossProduct(dc.UpVector, dc.ViewDir).Normalized();
                    Vector3d uv = dc.UpVector;

                    Vector3d crossV = p2 - p0;

                    Vector3d v1 = CadMath.InnerProduct(crossV, hv) * hv;
                    Vector3d p1 = v1 + p0;

                    Vector3d v3 = CadMath.InnerProduct(crossV, uv) * uv;
                    Vector3d p3 = v3 + p0;

                    Figure.mPointList.Add(new CadVertex(p3));
                    Figure.mPointList.Add(new CadVertex(p2));
                    Figure.mPointList.Add(new CadVertex(p1));

                    Figure.IsLoop = true;
                }
            }

            public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
            {
                if (Figure.PointList.Count <= 0)
                {
                    return;
                }

                dc.Drawing.DrawRect(pen, Figure.PointList[0].vector, tp.vector);
            }

            public override void EndCreate(DrawContext dc)
            {
                Figure.Normal = dc.ViewDir;
                Figure.Normal *= -1;
                Figure.Type = Types.POLY_LINES;
            }

            public override void StartCreate(DrawContext dc)
            {
                // NOP
            }

            public override State GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return State.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 4)
                {
                    return State.WAIT_LAST_POINT;
                }

                return State.FULL;
            }
        }

        public class LineCreator : PolyLinesCreator
        {
            public LineCreator(CadFigure fig) : base(fig)
            {
            }

            public override void EndCreate(DrawContext dc)
            {
                Figure.Type = Types.POLY_LINES;
            }

            public override State GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return State.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 2)
                {
                    return State.WAIT_LAST_POINT;
                }

                return State.FULL;
            }
        }

        public class CircleCreator : Creator
        {
            public CircleCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override State GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return State.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 2)
                {
                    return State.WAIT_LAST_POINT;
                }

                return State.FULL;
            }
        }

        public class DimLineCreator : Creator
        {
            public DimLineCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override State GetCreateState()
            {
                if (Figure.PointList.Count < 2)
                {
                    return State.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 3)
                {
                    return State.WAIT_LAST_POINT;
                }

                return State.FULL;
            }
        }

        public class PointCreator : Creator
        {
            public PointCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override State GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return State.NOT_ENOUGH;
                }

                return State.FULL;
            }
        }
    }
}