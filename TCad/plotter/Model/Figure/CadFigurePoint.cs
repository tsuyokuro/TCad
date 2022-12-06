using CadDataTypes;
using OpenTK.Mathematics;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public class CadFigurePoint : CadFigure
{
    public CadFigurePoint()
    {
        Type = Types.POINT;
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        mPointList.Add(p);
    }

    public override void AddPoint(CadVertex p)
    {
        if (mPointList.Count > 0)
        {
            return;
        }

        mPointList.Add(p);
    }

    public override void SetPointAt(int index, CadVertex pt)
    {
        if (index > 0)
        {
            return;
        }

        mPointList[index] = pt;
    }

    public override void RemoveSelected()
    {
        mPointList.RemoveAll(a => a.Selected);

        if (PointCount < 1)
        {
            mPointList.Clear();
        }
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
        drawPoint(dc, dp.LinePen);
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
        // NOP
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
        drawSelected_Point(dc);
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        // NOP
    }

    public override void InvertDir()
    {
        // NOP
    }

    private void drawPoint(DrawContext dc, DrawPen pen)
    {
        if (PointList.Count == 0)
        {
            return;
        }

        vcompo_t size = dc.DevSizeToWoldSize(4);

        dc.Drawing.DrawCross(pen, PointList[0].vector, size);
    }

    private void drawSelected_Point(DrawContext dc)
    {
        if (PointList.Count > 0)
        {
            if (PointList[0].Selected)
            {
                dc.Drawing.DrawSelectedPoint(PointList[0].vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
            }
        }
    }

    public override void StartCreate(DrawContext dc)
    {
        // NOP
    }

    public override void EndCreate(DrawContext dc)
    {
    }

    public override void MoveSelectedPointsFromStored(DrawContext dc, MoveInfo moveInfo)
    {
        CadVertex p = StoreList[0];

        vector3_t delta = moveInfo.Delta;

        if (p.Selected)
        {
            mPointList[0] = p + delta;
            return;
        }
    }

    public override Centroid GetCentroid()
    {
        Centroid ret = default;

        ret.Point = mPointList[0].vector;

        return ret;
    }
}
