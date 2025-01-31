using CadDataTypes;
using OpenTK.Mathematics;
using static Plotter.Controller.PlotterController;
using System;

namespace Plotter;

public class SegSearcher
{
    public enum Priority
    {
        NONE,
        PRIORITY_X,
        PRIORITY_Y,
    }

    private MarkSegment mMatchSeg;
    public MarkSegment MatchSegment { get => mMatchSeg; }


    private CadCursor Target;

    public vcompo_t Range;

    public vcompo_t MinDist = 0;

    public bool IsMatch
    {
        get
        {
            return mMatchSeg.FigureID != 0;
        }
    }

    public bool CheckStorePoint = false;

    //public Priority CheckPriority = Priority.NONE;

    public void SetRangePixel(DrawContext dc, vcompo_t pixel)
    {
        Range = pixel;
    }

    public void Clean()
    {
        mMatchSeg = default(MarkSegment);
        mMatchSeg.Clean();
        //CheckPriority = Priority.NONE;
    }

    public void SetTargetPoint(CadCursor cursor)
    {
        Target = cursor;
    }

    public MarkSegment GetMatch()
    {
        return mMatchSeg;
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

    private void CheckSeg(DrawContext dc, CadLayer layer, FigureSegment fseg)
    {
        CadFigure fig = fseg.Figure;
        vector3_t a = fseg.Point0.vector;
        vector3_t b = fseg.Point1.vector;

        if (fig.StoreList != null && fig.StoreList.Count > 1)
        {
            if (!CheckStorePoint)
            {
                return;
            }

            a = fseg.StoredPoint0.vector;
            b = fseg.StoredPoint1.vector;
        }

        vector3_t cwp = dc.DevPointToWorldPoint(Target.Pos);

        vector3_t xfaceNormal = dc.DevVectorToWorldVector(Target.DirX);
        vector3_t yfaceNormal = dc.DevVectorToWorldVector(Target.DirY);

        vector3_t cx = CadMath.CrossSegPlane(a, b, cwp, xfaceNormal);
        vector3_t cy = CadMath.CrossSegPlane(a, b, cwp, yfaceNormal);

        if (!cx.IsValid() && !cy.IsValid())
        {
            return;
        }

        vector3_t p = VectorExt.InvalidVector3;
        vcompo_t mind = vcompo_t.MaxValue;

        vector3_t centerP = CadMath.CenterPoint(a, b);
        vector3_t dcenter = dc.WorldPointToDevPoint(centerP);
        vcompo_t centerDist = CadMath.SegNormNZ(dcenter, Target.Pos);

        if (centerDist >= Range)
        {
            StackArray<vector3_t> vtbl = default;

            vtbl[0] = cx;
            vtbl[1] = cy;
            vtbl.Length = 2;

            for (int i = 0; i < vtbl.Length; i++)
            {
                vector3_t v = vtbl[i];

                if (!v.IsValid())
                {
                    continue;
                }

                vector3_t devv = dc.WorldPointToDevPoint(v);

                vcompo_t td = CadMath.SegNormNZ(devv, Target.Pos);

                if (td < mind)
                {
                    mind = td;
                    p = v;
                }
            }
        }
        else
        {
            p = centerP;
            mind = centerDist;
        }

        if (!p.IsValid())
        {
            return;
        }

        if (mind > Range)
        {
            return;
        }

        bool replace = false;

        if (Math.Abs(mind - MinDist) < (vcompo_t)0.1)
        {
            vcompo_t newZ = dc.WorldPointToDevPoint(p).Z;
            vcompo_t currentZ = mMatchSeg.CrossPointScrn.Z;

            if (newZ < currentZ)
            {
                replace = true;
            }
        }
        else if (mind < MinDist)
        {
            replace = true;
        }

        if (replace)
        {
            mMatchSeg.Layer = layer;
            mMatchSeg.FigSeg = fseg;
            mMatchSeg.CrossPoint = p;
            mMatchSeg.CrossPointScrn = dc.WorldPointToDevPoint(p);
            mMatchSeg.Distance = mind;

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

        vector3_t c = vl[0].vector;
        vector3_t a = vl[1].vector;
        vector3_t b = vl[2].vector;
        vector3_t normal = CadMath.Normal(a - c, b - c);

        vector3_t tw = Target.Pos;
        tw.Z = 0;
        tw = dc.DevPointToWorldPoint(tw);

        vector3_t crossP = CadMath.CrossPlane(tw, tw + dc.ViewDir, c, normal);

        if (crossP.IsInvalid())
        {
            // 真横から見ている場合
            // viewed edge-on
            //DOut.tpl("crossP is invalid");
            return;
        }

        vcompo_t r = (a - c).Norm();
        vcompo_t tr = (crossP - c).Norm();

        vector3_t cirP = c + (crossP - c) * (r / tr);

        vector3_t dcirP = dc.WorldPointToDevPoint(cirP);
        vector3_t dcrossP = dc.WorldPointToDevPoint(crossP);

        dcirP.Z = 0;
        dcrossP.Z = 0;

        vcompo_t dist = (dcirP - Target.Pos).Norm();

        if (dist > Range)
        {
            //DOut.tpl($"dist:{dist} Range:{Range}");
            return;
        }

        if (dist < MinDist)
        {
            FigureSegment fseg = new FigureSegment(fig, 0, 0, 0);

            mMatchSeg.Layer = layer;
            mMatchSeg.FigSeg = fseg;
            mMatchSeg.CrossPoint = cirP;
            mMatchSeg.CrossPointScrn = dc.WorldPointToDevPoint(cirP);
            mMatchSeg.Distance = dist;

            MinDist = dist;
        }
    }

    private void CheckSegs(DrawContext dc, CadLayer layer, CadFigure fig)
    {
        for (int i=0; i < fig.SegmentCount; i++)
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
            case CadFigure.Types.PICTURE:
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
