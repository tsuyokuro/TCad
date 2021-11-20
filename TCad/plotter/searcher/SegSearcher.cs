using System;
using System.Collections.Generic;
using System.Linq;
using CadDataTypes;
using OpenTK;
using static Plotter.Controller.PlotterController;

namespace Plotter
{
    public class SegSearcher
    {
        public enum Priority
        {
            NONE,
            PRIORITY_X,
            PRIORITY_Y,
        }

        private MarkSegment MarkSeg;

        private CadCursor Target;

        public double Range;

        public double MinDist = 0;

        public bool IsMatch
        {
            get
            {
                return MarkSeg.FigureID != 0;
            }
        }

        public bool CheckStorePoint = false;

        public Priority CheckPriority = Priority.NONE;

        public void SetRangePixel(DrawContext dc, double pixel)
        {
            Range = pixel;
        }

        public void Clean()
        {
            MarkSeg = default(MarkSegment);
            MarkSeg.Clean();
            CheckPriority = Priority.NONE;
        }

        public void SetTargetPoint(CadCursor cursor)
        {
            Target = cursor;
        }

        public MarkSegment GetMatch()
        {
            return MarkSeg;
        }

        public void SearchAllLayer(DrawContext dc, CadObjectDB db)
        {
            Search(dc, db, db.CurrentLayer);

            for (int i=0; i<db.LayerList.Count; i++)
            {
                CadLayer layer = db.LayerList[i];

                if (layer.ID == db.CurrentLayerID)
                {
                    continue;
                }

                Search(dc, db, layer);
            }
        }

        public void Search(DrawContext dc, CadObjectDB db, CadLayer layer)
        {
            if (layer == null)
            {
                return;
            }

            if (!layer.Visible)
            {
                return;
            }

            MinDist = CadConst.MaxValue;

            for (int i=layer.FigureList.Count-1; i>=0; i--)
            {
                CadFigure fig = layer.FigureList[i];
                CheckFig(dc, layer, fig);
            }
        }

        public void SetCheckPriorityWithSnapInfo(SnapInfo si)
        {
            if (si.PriorityMatch == SnapInfo.MatchType.X_MATCH)
            {
                CheckPriority = Priority.PRIORITY_X;
            }
            else if (si.PriorityMatch == SnapInfo.MatchType.Y_MATCH)
            {
                CheckPriority = Priority.PRIORITY_Y;
            }
            else
            {
                CheckPriority = Priority.NONE;
            }
        }

        private void CheckSeg(DrawContext dc, CadLayer layer, FigureSegment fseg)
        {
            CadFigure fig = fseg.Figure;
            int idxA = fseg.Index0;
            int idxB = fseg.Index1;
            Vector3d a = fseg.Point0.vector;
            Vector3d b = fseg.Point1.vector;

            if (fig.StoreList != null && fig.StoreList.Count > 1)
            {
                if (!CheckStorePoint)
                {
                    return;
                }

                a = fseg.StoredPoint0.vector;
                b = fseg.StoredPoint1.vector;
            }

            Vector3d cwp = dc.DevPointToWorldPoint(Target.Pos);

            Vector3d xfaceNormal = dc.DevVectorToWorldVector(Target.DirX);
            Vector3d yfaceNormal = dc.DevVectorToWorldVector(Target.DirY);

            Vector3d cx = CadMath.CrossSegPlane(a, b, cwp, xfaceNormal);
            Vector3d cy = CadMath.CrossSegPlane(a, b, cwp, yfaceNormal);

            if (!cx.IsValid() && !cy.IsValid())
            {
                return;
            }

            Vector3d p = VectorExt.InvalidVector3d;
            double mind = double.MaxValue;

            Vector3d dcenter = dc.WorldPointToDevPoint(CadMath.CenterPoint(a, b));
            double centerDist = (dcenter - Target.Pos).Norm();

            if (CheckPriority == Priority.NONE || centerDist < Range)
            {
                StackArray<Vector3d> vtbl = default;

                vtbl[0] = cx;
                vtbl[1] = cy;
                vtbl.Length = 2;

                for (int i = 0; i < vtbl.Length; i++)
                {
                    Vector3d v = vtbl[i];

                    if (!v.IsValid())
                    {
                        continue;
                    }

                    Vector3d devv = dc.WorldPointToDevPoint(v);
                    double td = (devv - Target.Pos).Norm();

                    if (td < mind)
                    {
                        mind = td;
                        p = v;
                    }
                }
            }
            else
            {
                if (CheckPriority == Priority.PRIORITY_X)
                {
                    p = cx;
                }
                else if (CheckPriority == Priority.PRIORITY_Y)
                {
                    p = cy;
                }

                if (p.IsInvalid())
                {
                    return;
                }

                Vector3d devv = dc.WorldPointToDevPoint(p);
                mind = (devv - Target.Pos).Norm();
            }

            if (!p.IsValid())
            {
                return;
            }

            if (mind > Range)
            {
                return;
            }

            if (mind < MinDist)
            {
                MarkSeg.Layer = layer;
                MarkSeg.FigSeg = fseg;
                MarkSeg.CrossPoint = p;
                MarkSeg.CrossPointScrn = dc.WorldPointToDevPoint(p);
                MarkSeg.Distance = mind;

                MinDist = mind;
            }
        }

        private void CheckCircle(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            if (fig.PointCount < 3)
            {
                return;
            }

            VertexList vl = fig.PointList;

            if (fig.StoreList != null)
            {
                vl = fig.StoreList;
            }

            Vector3d c = vl[0].vector;
            Vector3d a = vl[1].vector;
            Vector3d b = vl[2].vector;
            Vector3d normal = CadMath.Normal(a - c, b - c);

            Vector3d tw = Target.Pos;
            tw.Z = 0;
            tw = dc.DevPointToWorldPoint(tw);

            Vector3d crossP = CadMath.CrossPlane(tw, tw + dc.ViewDir, c, normal);

            if (crossP.IsInvalid())
            {
                // 真横から見ている場合
                // viewed edge-on
                //DOut.tpl("crossP is invalid");
                return;
            }

            double r = (a - c).Norm();
            double tr = (crossP - c).Norm();

            Vector3d cirP = c + (crossP - c) * (r / tr);

            Vector3d dcirP = dc.WorldPointToDevPoint(cirP);
            Vector3d dcrossP = dc.WorldPointToDevPoint(crossP);

            dcirP.Z = 0;
            dcrossP.Z = 0;

            double dist = (dcirP - Target.Pos).Norm();

            if (dist > Range)
            {
                //DOut.tpl($"dist:{dist} Range:{Range}");
                return;
            }

            if (dist < MinDist)
            {
                FigureSegment fseg = new FigureSegment(fig, 0, 0, 0);

                MarkSeg.Layer = layer;
                MarkSeg.FigSeg = fseg;
                MarkSeg.CrossPoint = cirP;
                MarkSeg.CrossPointScrn = dc.WorldPointToDevPoint(cirP);
                MarkSeg.Distance = dist;

                MinDist = dist;
            }
        }

        private void CheckSegs(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            for (int i=0;i < fig.SegmentCount; i++)
            {
                FigureSegment seg = fig.GetFigSegmentAt(i);
                CheckSeg(dc, layer, seg);
            }
        }

        private void CheckFig(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            switch (fig.Type)
            {
                case CadFigure.Types.LINE:
                case CadFigure.Types.POLY_LINES:
                case CadFigure.Types.RECT:
                case CadFigure.Types.DIMENTION_LINE:
                case CadFigure.Types.MESH:
                    CheckSegs(dc, layer, fig);
                    break;
                case CadFigure.Types.CIRCLE:
                    CheckCircle(dc, layer, fig);
                    break;
                default:
                    break;
            }
        }
    }
}
