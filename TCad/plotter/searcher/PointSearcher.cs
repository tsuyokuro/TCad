//#define DEFAULT_DATA_TYPE_DOUBLE
#define LOG_DEBUG

using CadDataTypes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;



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

public class PointSearcher
{
    private MarkPoint XMatch = default;
    private MarkPoint YMatch = default;
    private MarkPoint XYMatch = default;

    private List<MarkPoint> XYMatchList = new List<MarkPoint>();
    private HashSet<MarkPoint> XYMatchSet = new HashSet<MarkPoint>();

    public CadCursor Target;    // Cursor(スクリーン座標系)

    public vcompo_t Range;        // matchする範囲(スクリーン座標系)

    public bool CheckStorePoint = false;

    public uint CurrentLayerID
    {
        set; get;
    } = 0;

    public bool IsXMatch
    {
        get
        {
            return XMatch.IsValid;
        }
    }

    public bool IsYMatch
    {
        get
        {
            return YMatch.IsValid;
        }
    }

    public bool IsXYMatch
    {
        get
        {
            return XYMatch.IsValid;
        }
    }

    HashSet<uint> IgnoreFigureIDSet = new HashSet<uint>();

    public PointSearcher()
    {
        Clean();
    }

    public void SetRangePixel(DrawContext dc, vcompo_t pixel)
    {
        Range = pixel;
    }

    public void AddIgnoreFigureID(uint id)
    {
        IgnoreFigureIDSet.Add(id);
    }

    public void Clean()
    {
        XMatch.reset();
        YMatch.reset();
        XYMatch.reset();

        XYMatchList.Clear();
        XYMatchSet.Clear();

        IgnoreFigureIDSet.Clear();
    }

    public void SetTargetPoint(CadCursor cursor)
    {
        Target = cursor;
    }

    public MarkPoint GetXMatch()
    {
        return XMatch;
    }

    public MarkPoint GetYMatch()
    {
        return YMatch;
    }

    public MarkPoint GetXYMatch(int n = -1)
    {
        if (n == -1)
        {
            return XYMatch;
        }

        if (XYMatchList.Count == 0)
        {
            return XYMatch;
        }

        return XYMatchList[n];
    }

    public List<MarkPoint> GetXYMatches()
    {
        return XYMatchList;
    }

    public vcompo_t Distance()
    {
        vcompo_t ret = vcompo_t.MaxValue;
        vcompo_t t;

        if (IsXMatch)
        {
            ret = (XMatch.PointScrn - Target.Pos).Norm();
        }

        if (IsYMatch)
        {
            t = (YMatch.PointScrn - Target.Pos).Norm();
            ret = (vcompo_t)Math.Min(t, ret);
        }

        if (IsXYMatch)
        {
            t = (XYMatch.PointScrn - Target.Pos).Norm();
            ret = (vcompo_t)Math.Min(t, ret);
        }

        return ret;
    }

    public void SearchAllLayer(DrawContext dc, CadObjectDB db)
    {
        if (db.CurrentLayer.Visible)
        {
            Search(dc, db, db.CurrentLayer);
        }

        for (int i = 0; i < db.LayerList.Count; i++)
        {
            CadLayer layer = db.LayerList[i];

            if (layer.ID == db.CurrentLayerID)
            {
                continue;
            }

            if (!layer.Visible)
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

        for (int i = 0; i < layer.FigureList.Count; i++)
        {
            CadFigure fig = layer.FigureList[i];
            CheckFigure(dc, layer, fig);
        }
    }

    //public void Check(DrawContext DC, CadVertex pt)
    //{
    //    CheckFigPoint(DC, pt, null, null, 0);
    //}

    public void Check(DrawContext dc, vector3_t pt)
    {
        CheckFigPoint(dc, pt, null, null, 0);
    }

    public void Check(DrawContext dc, VertexList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Check(dc, list[i].vector);
        }
    }

    public void CheckFigure(DrawContext dc, CadLayer layer, CadFigure fig)
    {
        if (IgnoreFigureIDSet.Contains(fig.ID))
        {
            return;
        }

        VertexList list = fig.PointList;

        if (fig.StoreList != null)
        {
            if (CheckStorePoint)
            {
                list = fig.StoreList;
            }
            else
            {
                list = null;
            }
        }

        if (list != null)
        {
            int cnt = list.Count;

            for (int i = 0; i < cnt; i++)
            {
                CheckFigPoint(dc, list[i].vector, layer, fig, i);
            }
        }

        if (fig.ChildList != null)
        {
            for (int i = 0; i < fig.ChildList.Count; i++)
            {
                CadFigure c = fig.ChildList[i];
                CheckFigure(dc, layer, c);
            }
        }
    }

    private void CheckFigPoint(DrawContext dc, vector3_t pt, CadLayer layer, CadFigure fig, int ptIdx)
    {
        vector3_t ppt = dc.WorldPointToDevPoint(pt);

        vcompo_t dx = (vcompo_t)Math.Abs(ppt.X - Target.Pos.X);
        vcompo_t dy = (vcompo_t)Math.Abs(ppt.Y - Target.Pos.Y);

        CrossInfo cix = CadMath.PerpCrossLine(Target.Pos, Target.Pos + Target.DirX, ppt);
        CrossInfo ciy = CadMath.PerpCrossLine(Target.Pos, Target.Pos + Target.DirY, ppt);

        vcompo_t nx = (ppt - ciy.CrossPoint).Norm(); // Cursor Y軸からの距離
        vcompo_t ny = (ppt - cix.CrossPoint).Norm(); // Cursor X軸からの距離

        if (nx <= Range)
        {
            if (nx < XMatch.DistanceX || (nx == XMatch.DistanceX && ny < XMatch.DistanceY))
            {
                XMatch = GetMarkPoint(pt, ppt, nx, ny, layer, fig, ptIdx);
            }
        }

        if (ny <= Range)
        {
            if (ny < YMatch.DistanceY || (ny == YMatch.DistanceY && nx < YMatch.DistanceX))
            {
                YMatch = GetMarkPoint(pt, ppt, nx, ny, layer, fig, ptIdx);
            }
        }

        if (dx <= Range && dy <= Range)
        {
            vcompo_t minDist = (XYMatch.DistanceX * XYMatch.DistanceX) + (XYMatch.DistanceY * XYMatch.DistanceY);
            vcompo_t curDist = (dx * dx) + (dy * dy);

            if (curDist <= minDist)
            {
                MarkPoint t = GetMarkPoint(pt, ppt, dx, dy, layer, fig, ptIdx);

                //t.dump();

                XYMatch = t;

                if (!XYMatchSet.Contains(t))
                {
                    XYMatchList.Add(XYMatch);
                    XYMatchSet.Add(XYMatch);
                    //DOut.pl($"PointSearcher XYMatchList cnt:{XYMatchList.Count}");
                }
            }
        }
    }

    private MarkPoint GetMarkPoint(
        vector3_t pt, vector3_t ppt, vcompo_t distx, vcompo_t disty, CadLayer layer, CadFigure fig, int ptIdx)
    {
        MarkPoint mp = default;

        mp.IsValid = true;
        mp.Layer = layer;
        mp.Figure = fig;
        mp.PointIndex = ptIdx;
        mp.Point = pt;
        mp.PointScrn = ppt;
        mp.DistanceX = distx;
        mp.DistanceY = disty;

        return mp;
    }
}
