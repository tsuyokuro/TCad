using CadDataTypes;
using TCad.Plotter;
using SplineCurve;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;

namespace TCad.Plotter.Model.Figure;

public partial class CadFigureNurbsLine : CadFigure
{
    public NurbsLine Nurbs;

    private VertexList NurbsPointList;

    public CadFigureNurbsLine()
    {
        Type = Types.NURBS_LINE;
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
        base.MoveSelectedPointsFromStored(dc, moveInfo);
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

    public void Setup(int deg, int divCnt, bool edge = true, bool close = false)
    {
        Nurbs = new NurbsLine(deg, mPointList.Count, divCnt, edge, close);
        Nurbs.CtrlPoints = mPointList;
        Nurbs.SetupDefaultCtrlOrder();

        NurbsPointList = new VertexList(Nurbs.OutCnt);
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
        DrawNurbs(dc, dp.LinePen);
    }

    private void DrawNurbs(DrawContext dc, DrawPen pen)
    {
        if (PointList.Count < 2)
        {
            return;
        }

        CadVertex c;
        CadVertex n;

        c = PointList[0];

        for (int i = 1; i < PointList.Count; i++)
        {
            n = PointList[i];
            dc.Drawing.DrawLine(
                dc.GetPen(DrawTools.PEN_NURBS_CTRL_LINE), c.vector, n.vector);

            c = n;
        }

        NurbsPointList.Clear();

        Nurbs.Eval(NurbsPointList);

        if (NurbsPointList.Count < 2)
        {
            return;
        }

        c = NurbsPointList[0];

        for (int i = 1; i < NurbsPointList.Count; i++)
        {
            n = NurbsPointList[i];
            dc.Drawing.DrawLine(pen, c.vector, n.vector);

            c = n;
        }
    }

    public override void InvertDir()
    {
        mPointList.Reverse();
    }

    public override void SetPointAt(int index, CadVertex pt)
    {
        mPointList[index] = pt;
    }

    public override void EndEdit()
    {
        base.EndEdit();
    }


    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
    }
}
