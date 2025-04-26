//#define LOG_DEBUG

using CadDataTypes;
using TCad.Plotter;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;

namespace TCad.Plotter.Model.Figure;

public partial class CadFigureGroup : CadFigure
{
    public CadFigureGroup()
    {
        Type = Types.GROUP;
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
    }

    public override void StartCreate(DrawContext dc)
    {
    }
}
