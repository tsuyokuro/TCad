//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Settings;
using System.Collections.Generic;
using Plotter.Serializer.v1002;
using Plotter.Serializer.v1003;
using Plotter.Serializer;






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

public partial class CadFigurePolyLines : CadFigure
{
    public bool IsLoop_ = false;

    public override bool IsLoop
    {
        set => IsLoop_ = value;
        get => IsLoop_;
    }

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

        bool restrictWithNormal = moveInfo.Restrict.IsOn(MoveRestriction.POLY_LINES_WITH_NORMAL);

        if (!IsSelectedAll() && mPointList.Count > 2 && restrictWithNormal)
        {
            // 同じ平面上に制限する

            vector3_t normal = CadMath.Normal(StoreList[0].vector, StoreList[1].vector, StoreList[2].vector);

            vector3_t vdir = dc.ViewDir;


            vector3_t a = vector3_t.Zero;
            vector3_t b = vdir;

            vector3_t d0 = CadMath.CrossPlane(a, b, StoreList[0].vector, normal);

            a = delta;
            b = delta + vdir;

            vector3_t d1 = CadMath.CrossPlane(a, b, StoreList[0].vector, normal);

            if (d0.IsValid() && d1.IsValid())
            {
                d = d1 - d0;
            }
            else
            {
                vector3_t nvNormal = CadMath.Normal(normal, vdir);

                vcompo_t ip = CadMath.InnerProduct(nvNormal, delta);

                d = nvNormal * ip;
            }
        }
        else
        {
            d = delta;
        }

        MoveInfo mvInfo = moveInfo;

        mvInfo.Delta = d;


        FigUtil.MoveSelectedPointsFromStored(this, dc, mvInfo);

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

        if (SettingsHolder.Settings.DrawNormal && mPointList.Count > 2)
        {
            vcompo_t len = dc.DevSizeToWoldSize(DrawingConst.NormalLen);
            vcompo_t arrowLen = dc.DevSizeToWoldSize(DrawingConst.NormalArrowLen);
            vcompo_t arrowW = dc.DevSizeToWoldSize(DrawingConst.NormalArrowWidth);

            vector3_t normal = CadMath.Normal(PointList[0].vector, PointList[1].vector, PointList[2].vector);

            vector3_t np0 = PointList[0].vector;
            vector3_t np1 = np0 + (normal * len);
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
    }

    protected void DrawLines(DrawContext dc, DrawOption opt, VertexList pl)
    {
        int start = 0;
        int cnt = pl.Count;

        if (cnt <= 0)
        {
            return;
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

    private Centroid GetPointListCentroid()
    {
        Centroid ret = default;

        List<Vector3List> triangles = TriangleSplitter.Split(this);

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
