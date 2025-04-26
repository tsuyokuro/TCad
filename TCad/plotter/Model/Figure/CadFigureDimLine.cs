using CadDataTypes;
using Plotter;

namespace TCad.Plotter.Model.Figure;

//
// 寸法線クラス
// 
//   3<-------------------------->2
//    |                          |
//    |                          |
//   0                            1 
// 
//

public partial class CadFigureDimLine : CadFigure
{
    private const vcompo_t ARROW_LEN = 2;
    private const vcompo_t ARROW_W = 1;

    public int FontID { set; get; } = DrawTools.FONT_SMALL;

    public int TextBrushID = DrawTools.BRUSH_TEXT;

    public CadFigureDimLine()
    {
        Type = Types.DIMENTION_LINE;
    }

    public override void AddPoint(CadVertex p)
    {
        mPointList.Add(p);
    }

    public override Centroid GetCentroid()
    {
        Centroid ret = default;

        ret.IsInvalid = true;

        return ret;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        PointList.Add(p);
    }

    public override void SetPointAt(int index, CadVertex pt)
    {
        mPointList[index] = pt;
    }

    public override void RemoveSelected()
    {
        mPointList.RemoveAll(a => a.Selected);

        if (PointCount < 4)
        {
            mPointList.Clear();
        }
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
        DrawDim(dc, dp.LinePen, dp.TextBrush);
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
        foreach (CadVertex p in PointList)
        {
            if (p.Selected)
            {
                dc.Drawing.DrawSelectedPoint(p.vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
            }
        }
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        int cnt = PointList.Count;

        if (cnt < 1) return;

        if (cnt == 1)
        {
            DrawDim(dc, PointList[0], tp, tp, pen);
            return;
        }

        DrawDim(dc, PointList[0], PointList[1], tp, pen);
    }

    public override void StartCreate(DrawContext dc)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
        if (PointList.Count < 3)
        {
            return;
        }

        CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1], PointList[2]);

        PointList[2] = PointList[2].SetVector(seg.P1.vector);
        PointList.Add(seg.P0);
    }

    public override void MoveSelectedPointsFromStored(DrawContext dc, MoveInfo moveInfo)
    {
        vector3_t delta = moveInfo.Delta;

        if (PointList[0].Selected && PointList[1].Selected &&
            PointList[2].Selected && PointList[3].Selected)
        {
            PointList[0] = StoreList[0] + delta;
            PointList[1] = StoreList[1] + delta;
            PointList[2] = StoreList[2] + delta;
            PointList[3] = StoreList[3] + delta;
            return;
        }

        if (PointList[2].Selected || PointList[3].Selected)
        {
            vector3_t v0 = StoreList[3].vector - StoreList[0].vector;

            if (v0.IsZero())
            {
                // 移動方向が不定の場合
                MoveSelectedPointWithHeight(dc, delta);
                return;
            }

            vector3_t v0u = v0.UnitVector();

            vcompo_t d = CadMath.InnerProduct(v0u, delta);

            vector3_t vd = v0u * d;

            CadVertex nv3 = StoreList[3] + vd;
            CadVertex nv2 = StoreList[2] + vd;

            if (nv3.EqualsThreshold(StoreList[0], (vcompo_t)(0.001)) ||
                nv2.EqualsThreshold(StoreList[1], (vcompo_t)(0.001)))
            {
                return;
            }

            PointList[3] = nv3;
            PointList[2] = nv2;

            return;
        }

        if (PointList[0].Selected || PointList[1].Selected)
        {
            vector3_t v0 = StoreList[0].vector;
            vector3_t v1 = StoreList[1].vector;
            vector3_t v2 = StoreList[2].vector;
            vector3_t v3 = StoreList[3].vector;

            vector3_t lv = v3 - v0;
            vcompo_t h = lv.Norm();

            vector3_t planeNormal = CadMath.Normal(v0, v1, v2);

            vector3_t cp0 = v0;
            vector3_t cp1 = v1;

            if (PointList[0].Selected)
            {
                cp0 = CadMath.CrossPlane(v0 + delta, v0, planeNormal);
            }

            if (PointList[1].Selected)
            {
                cp1 = CadMath.CrossPlane(v1 + delta, v1, planeNormal);
            }

            if (cp0.EqualsThreshold(cp1, (vcompo_t)(0.001)))
            {
                return;
            }

            if (PointList[0].Selected)
            {
                PointList[0] = PointList[0].SetVector(cp0);
            }

            if (PointList[1].Selected)
            {
                PointList[1] = PointList[1].SetVector(cp1);
            }

            vector3_t normal = CadMath.Normal(cp0, cp0 + planeNormal, cp1);
            vector3_t d = normal * h;

            PointList[3] = PointList[3].SetVector(PointList[0] + d);
            PointList[2] = PointList[2].SetVector(PointList[1] + d);
        }
    }


    // 高さが０の場合、移動方向が定まらないので
    // 投影座標系でz=0とした座標から,List[0] - List[1]への垂線を計算して
    // そこへ移動する
    private void MoveSelectedPointWithHeight(DrawContext dc, vector3_t delta)
    {
        CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1],
            StoreList[2] + delta);

        PointList[2] = PointList[2].SetVector(seg.P1.vector);
        PointList[3] = PointList[3].SetVector(seg.P0.vector);
    }

    public override void EndEdit()
    {
        base.EndEdit();

        if (PointList.Count == 0)
        {
            return;
        }

        CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1], PointList[2]);

        PointList[2] = PointList[2].SetVector(seg.P1.vector);
        PointList[3] = PointList[3].SetVector(seg.P0.vector);
    }

    private void DrawDim(
                        DrawContext dc,
                        CadVertex a,
                        CadVertex b,
                        CadVertex p,
                        DrawPen pen)
    {
        CadSegment seg = CadUtil.PerpSeg(a, b, p);

        dc.Drawing.DrawLine(pen, a.vector, seg.P0.vector);
        dc.Drawing.DrawLine(pen, b.vector, seg.P1.vector);

        vector3_t cp = CadMath.CenterPoint(seg.P0.vector, seg.P1.vector);

        vcompo_t arrowW = ARROW_W / dc.WorldScale;
        vcompo_t arrowL = ARROW_LEN / dc.WorldScale;

        dc.Drawing.DrawArrow(pen, cp, seg.P0.vector, ArrowTypes.CROSS, ArrowPos.END, arrowL, arrowW);
        dc.Drawing.DrawArrow(pen, cp, seg.P1.vector, ArrowTypes.CROSS, ArrowPos.END, arrowL, arrowW);
    }

    private void DrawDim(DrawContext dc, DrawPen linePen, DrawBrush textBrush)
    {
        dc.Drawing.DrawLine(linePen, PointList[0].vector, PointList[3].vector);
        dc.Drawing.DrawLine(linePen, PointList[1].vector, PointList[2].vector);

        vector3_t cp = CadMath.CenterPoint(PointList[3].vector, PointList[2].vector);

        vcompo_t arrowW = ARROW_W / dc.WorldScale;
        vcompo_t arrowL = ARROW_LEN / dc.WorldScale;

        vcompo_t ww = (PointList[1] - PointList[0]).Norm() / (vcompo_t)(4.0);

        if (ww > arrowL)
        {
            dc.Drawing.DrawArrow(linePen, cp, PointList[3].vector, ArrowTypes.CROSS, ArrowPos.END, arrowL, arrowW);
            dc.Drawing.DrawArrow(linePen, cp, PointList[2].vector, ArrowTypes.CROSS, ArrowPos.END, arrowL, arrowW);
        }
        else
        {
            vector3_t v0 = cp - PointList[3].vector;
            vector3_t v1 = cp - PointList[2].vector;

            v0 = -(v0.Normalized() * (arrowL * (vcompo_t)(1.5))) / dc.WorldScale + PointList[3].vector;
            v1 = -(v1.Normalized() * (arrowL * (vcompo_t)(1.5))) / dc.WorldScale + PointList[2].vector;

            dc.Drawing.DrawArrow(linePen, v0, PointList[3].vector, ArrowTypes.CROSS, ArrowPos.END, arrowL, arrowW);
            dc.Drawing.DrawArrow(linePen, v1, PointList[2].vector, ArrowTypes.CROSS, ArrowPos.END, arrowL, arrowW);

            dc.Drawing.DrawLine(linePen, PointList[2].vector, PointList[3].vector);
        }

        CadVertex lineV = PointList[2] - PointList[3];

        vcompo_t len = lineV.Norm();

        string lenStr = CadUtil.ValToString(len);

        CadVertex p = PointList[3] + (lineV / 2);

        p += (PointList[3] - PointList[0]).UnitVector() * (arrowW);

        CadVertex up = PointList[3] - PointList[0];

        // 裏返しになる場合は、反転する
        // If it turns over, reverse it
        vector3_t normal = CadMath.Normal(lineV.vector, up.vector);

        vcompo_t scala = CadMath.InnerProduct(normal, dc.ViewDir);

        if (scala > 0)
        {
            lineV = -lineV;
        }

        //             --- lineV ---> 
        //    3<------------ p ----------->2
        // ^  |                            |
        // |  |                            |
        // up 0                            1 
        // 
        dc.Drawing.DrawText(FontID, textBrush, p.vector, lineV.vector, up.vector,
            new DrawTextOption(DrawTextOption.H_CENTER), (vcompo_t)(1.0) / dc.WorldScale,
            lenStr);
    }
}

