//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Settings;
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

public class CadFigurePolyLines : CadFigure
{
    protected bool RestrictionByNormal = false;

    public CadFigurePolyLines()
    {
        Type = Types.POLY_LINES;
    }

    public override void StartCreate(DrawContext dc)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
    }


    #region Point Move
    public override void MoveSelectedPointsFromStored(DrawContext dc, MoveInfo moveInfo)
    {
        //base.MoveSelectedPoints(DC, delta);

        if (Locked) return;

        vector3_t d;

        vector3_t delta = moveInfo.Delta;

        if (!IsSelectedAll() && mPointList.Count > 2 && RestrictionByNormal)
        {
            vector3_t vdir = dc.ViewDir;

            vector3_t a = delta;
            vector3_t b = delta + vdir;

            d = CadMath.CrossPlane(a, b, StoreList[0].vector, Normal);

            if (!d.IsValid())
            {
                vector3_t nvNormal = CadMath.Normal(Normal, vdir);

                vcompo_t ip = CadMath.InnerProduct(nvNormal, delta);

                d = nvNormal * ip;
            }
        }
        else
        {
            d = delta;
        }

        FigUtil.MoveSelectedPointsFromStored(this, dc, moveInfo);

        mChildList.ForEach(c =>
        {
            c.MoveSelectedPointsFromStored(dc, moveInfo);
        });
    }

    public override void MoveAllPoints(vector3_t delta)
    {
        if (Locked) return;

        FigUtil.MoveAllPoints(this, delta);
    }
    #endregion


    public override int PointCount
    {
        get
        {
            return mPointList.Count;
        }
    }

    public override void RemoveSelected()
    {
        mPointList.RemoveAll(a => a.Selected);

        if (PointCount < 2)
        {
            mPointList.Clear();
        }
    }

    public override void AddPoint(CadVertex p)
    {
        mPointList.Add(p);
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
        DrawPolyLines(dc, dp);
    }

    public void DrawPolyLines(DrawContext dc, DrawOption opt)
    {
        if (mStoreList != null)
        {
            DrawLines(dc, dc.OptionSet.Before, mStoreList);
        }

        DrawLines(dc, opt, mPointList);

        if (SettingsHolder.Settings.DrawNormal && !Normal.IsZero())
        {
            vcompo_t len = dc.DevSizeToWoldSize(DrawingConst.NormalLen);
            vcompo_t arrowLen = dc.DevSizeToWoldSize(DrawingConst.NormalArrowLen);
            vcompo_t arrowW = dc.DevSizeToWoldSize(DrawingConst.NormalArrowWidth);

            vector3_t np0 = PointList[0].vector;
            vector3_t np1 = np0 + (Normal * len);
            dc.Drawing.DrawArrow(dc.GetPen(DrawTools.PEN_NORMAL), np0, np1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW);
        }
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
        DrawSelectedLines(dc, dp);
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
        CadVertex a = PointList[idxA];
        CadVertex b = PointList[idxB];

        dc.Drawing.DrawLine(pen, a.vector, b.vector);
    }

    public override void InvertDir()
    {
        mPointList.Reverse();
        Normal = -Normal;
    }

    protected void DrawLines(DrawContext dc, DrawOption opt, VertexList pl)
    {
        int start = 0;
        int cnt = pl.Count;

        if (cnt <= 0)
        {
            return;
        }

        if (Normal.IsZero())
        {
            Normal = CadUtil.TypicalNormal(pl);
        }

        CadVertex a;

        a = pl[start];

        if (cnt == 1)
        {
            dc.Drawing.DrawCross(opt.LinePen, a.vector, 2);
            return;
        }

        DrawPen saveLinePen = opt.LinePen;

        if (!opt.ForcePen && (!LinePen.IsInvalid))
        {
            opt.LinePen = LinePen;
        }

        PolyLineExpander.Draw(pl, IsLoop, 8, dc, opt);

        opt.LinePen = saveLinePen;
    }

    public override VertexList GetPoints(int curveSplitNum)
    {
        return PolyLineExpander.GetExpandList(mPointList, curveSplitNum);
    }

    private void DrawSelectedLines(DrawContext dc, DrawOption dp)
    {
        int i;
        int num = PointList.Count;

        for (i = 0; i < num; i++)
        {
            CadVertex p = PointList[i];

            if (!p.Selected) continue;

            dc.Drawing.DrawSelectedPoint(p.vector, dp.SelectedPointPen);


            if (p.IsHandle)
            {
                int idx = i + 1;

                if (idx >= num) idx = 0;

                CadVertex next = GetPointAt(idx);
                if (!next.IsHandle)
                {
                    // Draw handle
                    dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_HANDLE_LINE), p.vector, next.vector);
                    dc.Drawing.DrawSelectedPoint(next.vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
                }

                idx = i - 1;

                if (idx >= 0)
                {
                    CadVertex prev = GetPointAt(idx);
                    if (!prev.IsHandle)
                    {
                        // Draw handle
                        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_HANDLE_LINE), p.vector, prev.vector);
                        dc.Drawing.DrawSelectedPoint(prev.vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
                    }
                }
            }
            else
            {
                int idx = i + 1;

                if (idx < PointCount)
                {
                    CadVertex np = GetPointAt(idx);
                    if (np.IsHandle)
                    {
                        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p.vector, np.vector);
                        dc.Drawing.DrawSelectedPoint(np.vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
                    }
                }

                idx = i - 1;

                if (idx >= 0)
                {
                    CadVertex np = GetPointAt(idx);
                    if (np.IsHandle)
                    {
                        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p.vector, np.vector);
                        dc.Drawing.DrawSelectedPoint(np.vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
                    }
                }
            }
        }
    }

    public override void SetPointAt(int index, CadVertex pt)
    {
        mPointList[index] = pt;
    }

   public override void EndEdit()
    {
        base.EndEdit();
        RecalcNormal();
        //例外ハンドリングテスト用
        //CadVector v = mPointList[100];
    }

    public override Centroid GetCentroid()
    {
        if (PointList.Count == 0)
        {
            return default;
        }

        if (PointList.Count == 1)
        {
            return GetPointCentroid();
        }

        if (PointList.Count < 3)
        {
            return GetSegCentroid();
        }

        return GetPointListCentroid();
    }

    public override void RecalcNormal()
    {
        if (PointList.Count == 0)
        {
            return;
        }

        vector3_t prevNormal = Normal;

        vector3_t normal = CadUtil.TypicalNormal(PointList);

        if (CadMath.InnerProduct(prevNormal, normal) < 0)
        {
            normal *= -1;
        }

        Normal = normal;
    }

    private Centroid GetPointListCentroid()
    {
        Centroid ret = default;

        List<CadFigure> triangles = TriangleSplitter.Split(this);

        ret = CadUtil.TriangleListCentroid(triangles);

        return ret;
    }

    private Centroid GetPointCentroid()
    {
        Centroid ret = default;

        ret.Point = PointList[0].vector;
        ret.Area = 0;

        return ret;
    }

    private Centroid GetSegCentroid()
    {
        Centroid ret = default;

        vector3_t d = PointList[1].vector - PointList[0].vector;

        d /= (vcompo_t)(2.0);

        ret.Point = PointList[0].vector + d;
        ret.Area = 0;

        return ret;
    }
}
