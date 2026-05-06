using CadDataTypes;
using System;
using TCad.MathFunctions;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;


namespace TCad.Plotter.Model.Figure;

public partial class CadFigureCircle : CadFigure
{
    public override int SegmentCount
    {
        get => 0;
    }


    public CadFigureCircle()
    {
        Type = Types.CIRCLE;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        mPointList.Add(p);
    }

    public override void AddPoint(CadVertex p)
    {
        mPointList.Add(p);
    }

    public override void SetPointAt(int index, CadVertex pt)
    {
        mPointList[index] = pt;
    }

    public override void RemoveSelected()
    {
        mPointList.Clear();
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
        DrawCircle(dc, dp.LinePen);
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
        //drawCircle(DC, pen);
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
        DrawSelectedCircle(dc);
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        if (PointList.Count <= 0)
        {
            return;
        }

        CadVertex cp = PointList[0];

        CadVertex a = tp;
        CadVertex b = new(GetRightAngleP(dc, cp, tp));

        CadVertex c = -(a - cp) + cp;
        CadVertex d = -(b - cp) + cp;

        CircleExpander.Draw(cp, a, b, 32, dc, pen);

        dc.Drawing.DrawLine(pen, cp.vector, a.vector);
        dc.Drawing.DrawLine(pen, cp.vector, b.vector);
        dc.Drawing.DrawLine(pen, cp.vector, c.vector);
        dc.Drawing.DrawLine(pen, cp.vector, d.vector);
    }

    private void DrawCircle(DrawContext dc, DrawPen pen)
    {
        if (PointList.Count == 0)
        {
            return;
        }

        if (PointList.Count == 1)
        {
            dc.Drawing.DrawCross(pen, PointList[0].vector, 2);
            if (PointList[0].Selected) dc.Drawing.DrawSelectedPoint(PointList[0].vector, dc.Pen(DrawTools.PEN_SELECTED_POINT));
            return;
        }

        CircleExpander.Draw(PointList[0], PointList[1], PointList[2], 32, dc, pen);

        vcompo_t size = dc.DevSizeToWoldSize(4);
        dc.Drawing.DrawCross(pen, PointList[0].vector, size);
    }

    private void DrawSelectedCircle(DrawContext dc)
    {
        for (int i = 0; i < PointList.Count; i++)
        {
            if (PointList[i].Selected)
            {
                dc.Drawing.DrawSelectedPoint(
                    PointList[i].vector, dc.Pen(DrawTools.PEN_SELECTED_POINT));
            }

        }
    }

    public override void StartCreate(DrawContext dc)
    {
        // NOP
    }

    public override void EndCreate(DrawContext dc)
    {
        if (PointCount < 2)
        {
            return;
        }

        CadVertex cp = mPointList[0];

        CadVertex a = mPointList[1];

        CadVertex b = new(GetRightAngleP(dc, cp, a));

        AddPoint(b);

        CadVertex c = -(a - cp) + cp;
        CadVertex d = -(b - cp) + cp;

        AddPoint(c);

        AddPoint(d);

        return;
    }

    public override void MoveSelectedPointsFromStored(DrawContext dc, MoveInfo moveInfo)
    {
        CadVertex cp = StoreList[0];

        vector3_t delta = moveInfo.Delta;

        if (cp.Selected)
        {
            mPointList[0] = cp + delta;
            mPointList[1] = StoreList[1] + delta;
            mPointList[2] = StoreList[2] + delta;
            mPointList[3] = StoreList[3] + delta;
            mPointList[4] = StoreList[4] + delta;
            return;
        }

        StackArray<CadVertex> vt = default;

        vt[0] = StoreList[1] - cp;
        vt[1] = StoreList[2] - cp;
        vt[2] = StoreList[3] - cp;
        vt[3] = StoreList[4] - cp;
        vt.Length = 4;

        if (vt[0].Norm() < (vcompo_t)(0.01))
        {
            return;
        }

        int ai = -1;

        for (int i = 0; i < 4; i++)
        {
            if (StoreList[i + 1].Selected)
            {
                ai = i;
                break;
            }
        }

        if (ai < 0)
        {
            return;
        }

        int bi = (ai + 1) % 4;
        int ci = (ai + 2) % 4;
        int di = (ai + 3) % 4;

        vector3_t normal = CadMath.OuterProduct(vt[ai].vector, vt[bi].vector);
        normal = normal.UnitVector();

        vt[ai] += delta;

        CadVertex uva = vt[ai].UnitVector();
        CadVertex uvb = vt[bi].UnitVector();

        if (!uva.EqualsThreshold(uvb))
        {
            normal = CadMath.OuterProduct(vt[ai].vector, vt[bi].vector);

            if (normal.IsZero())
            {
                return;
            }

            normal = normal.UnitVector();

        }

        CadQuaternion q = CadQuaternion.RotateQuaternion(normal, (vcompo_t)Math.PI / (vcompo_t)(2.0));
        CadQuaternion r = q.Conjugate();

        CadQuaternion qp = CadQuaternion.FromPoint(vt[ai].vector);
        qp = r * qp;
        qp = qp * q;

        vt[bi] = (CadVertex)qp.ToPoint();

        vt[ci] = -vt[ai];
        vt[di] = -vt[bi];

        CadVertex tmp;

        for (int i = 0; i < vt.Length; i++)
        {
            tmp = vt[i];
            tmp.Selected = false;
            vt[i] = tmp;
        }

        tmp = vt[ai];
        tmp.Selected = true;
        vt[ai] = tmp;

        mPointList[1] = vt[0] + cp;
        mPointList[2] = vt[1] + cp;
        mPointList[3] = vt[2] + cp;
        mPointList[4] = vt[3] + cp;
    }

    public override Centroid GetCentroid()
    {
        Centroid ret = default;

        vector3_t cp = StoreList[0].vector;
        vector3_t rp = StoreList[1].vector;

        vector3_t d = rp - cp;

        vcompo_t r = d.Norm();

        ret.Point = cp;
        ret.Area = r * r * (vcompo_t)Math.PI;

        return ret;
    }

    public override CadSegment GetSegmentAt(int n)
    {
        return new CadSegment(CadVertex.InvalidValue, CadVertex.InvalidValue);
    }

    public override FigureSegment GetFigSegmentAt(int n)
    {
        return new FigureSegment(null, -1, -1, -1);
    }

    //
    // 視線ベクトルとcpによる平面上で cp->pと垂直な点を求める
    //    rp
    //     | 
    //     |
    //    cp-----p
    //
    private static vector3_t GetRightAngleP(DrawContext dc, CadVertex cp, CadVertex p)
    {
        if (p.Equals(cp))
        {
            return cp.vector;
        }


        vector3_t r = CadMath.OuterProduct(p.vector - cp.vector, dc.ViewDir);

        r = r.UnitVector();

        r = r * (p.vector - cp.vector).Norm() + cp.vector;

        return r;
    }
}
