//#define LOG_DEBUG

using CadDataTypes;

namespace Plotter
{
    public class CadFigureGroup : CadFigure
    {
        public CadFigureGroup()
        {
            Type = Types.GROUP;
        }

        public override void Draw(DrawContext dc)
        {
        }

        public override void Draw(DrawContext dc, DrawParams dp)
        {
        }

        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
        }

        public override void DrawSelected(DrawContext dc)
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
}