using System.Drawing;

namespace Plotter
{
    public abstract class ColorSet
    {
        public readonly Color[] PenColorTbl = new Color[DrawTools.PEN_TBL_SIZE];
        public readonly Color[] BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];
    }
}
