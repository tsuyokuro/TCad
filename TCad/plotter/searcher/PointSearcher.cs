#define LOG_DEBUG

using CadDataTypes;
using Plotter;
using Plotter.Controller;
using System;
using System.Collections.Generic;
using TCad.MathFunctions;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.searcher;

namespace TCad.Plotter.searcher;

public class PointSearcher
{
    private MarkPoint mXMatch = default;
    public MarkPoint XMatch { get => mXMatch; }


    private MarkPoint mYMatch = default;
    public MarkPoint YMatch { get => mYMatch; }


    private MarkPoint mXYMatch = default;
    public MarkPoint XYMatch { get => mXYMatch; }


    public CadCursor Target; // Cursor(スクリーン座標系)

    public vcompo_t Range; // matchする範囲(スクリーン座標系)

    public bool CheckStorePoint = false;

    public uint CurrentLayerID
    {
        set; get;
    } = 0;

    public bool IsXMatch
    {
        get
        {
            return mXMatch.IsValid;
        }
    }

    public bool IsYMatch
    {
        get
        {
            return mYMatch.IsValid;
        }
    }

    public bool IsXYMatch
    {
        get
        {
            return mXYMatch.IsValid;
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
        mXMatch.reset();
        mYMatch.reset();
        mXYMatch.reset();

        IgnoreFigureIDSet.Clear();
    }

    public void SetTargetPoint(CadCursor cursor)
    {
        Target = cursor;
    }

    public vcompo_t Distance
    {
        get
        {
            vcompo_t ret = vcompo_t.MaxValue;
            vcompo_t t;

            if (IsXMatch)
            {
                ret = (mXMatch.PointScrn - Target.Pos).Norm();
            }

            if (IsYMatch)
            {
                t = (mYMatch.PointScrn - Target.Pos).Norm();
                ret = (vcompo_t)Math.Min(t, ret);
            }

            if (IsXYMatch)
            {
                t = (mXYMatch.PointScrn - Target.Pos).Norm();
                ret = (vcompo_t)Math.Min(t, ret);
            }

            return ret;
        }
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

        vcompo_t nx = CadMath.SegNormNZ(ppt, ciy.CrossPoint); // Cursor Y軸からの距離
        vcompo_t ny = CadMath.SegNormNZ(ppt, cix.CrossPoint); // Cursor X軸からの距離

        if (nx <= Range)
        {
            if (nx < mXMatch.DistanceX || (nx == mXMatch.DistanceX && ny < mXMatch.DistanceY))
            {
                mXMatch = CreateMarkPoint(pt, ppt, nx, ny, layer, fig, ptIdx);
            }
        }

        if (ny <= Range)
        {
            if (ny < mYMatch.DistanceY || (ny == mYMatch.DistanceY && nx < mYMatch.DistanceX))
            {
                mYMatch = CreateMarkPoint(pt, ppt, nx, ny, layer, fig, ptIdx);
            }
        }

        if (dx <= Range && dy <= Range)
        {
            vcompo_t minDist = (mXYMatch.DistanceX * mXYMatch.DistanceX) + (mXYMatch.DistanceY * mXYMatch.DistanceY);
            vcompo_t curDist = (dx * dx) + (dy * dy);

            MarkPoint t = CreateMarkPoint(pt, ppt, dx, dy, layer, fig, ptIdx);

            if (curDist < minDist)
            {
                mXYMatch = t;
            }
            else if (curDist == minDist)
            {
                if (
                       (!mXYMatch.IsSelected() && !t.IsSelected()) ||
                       (mXYMatch.IsSelected() && t.IsSelected())
                   )
                {
                    // 視点に近い方を採用する
                    if (t.PointScrn.Z < mXYMatch.PointScrn.Z)
                    {
                        mXYMatch = t;
                    }
                }
                else if (!mXYMatch.IsSelected() && t.IsSelected())
                {
                    mXYMatch = t;
                }
                else if (mXYMatch.IsSelected() && !t.IsSelected())
                {
                    // 更新しない
                }

            }
        }
    }

    private MarkPoint CreateMarkPoint(
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
