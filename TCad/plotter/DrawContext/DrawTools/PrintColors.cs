using System.Drawing;

namespace Plotter;

public class PrintColors : ColorSet
{
    private static PrintColors mInstance = new PrintColors();
    public static PrintColors Instance
    {
        get { return mInstance; }
    }

    private PrintColors()
    {
        PenColorTbl[DrawTools.PEN_DEFAULT] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_SELECTED_POINT] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_CROSS_CURSOR] = Color.FromArgb(128, 128, 128);
        PenColorTbl[DrawTools.PEN_CROSS_CURSOR2] = Color.FromArgb(128, 128, 128);
        PenColorTbl[DrawTools.PEN_DEFAULT_FIGURE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_TEMP_FIGURE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_MATCH_SEG] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER2] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_AXIS] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_PAGE_FRAME] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_TEST_FIGURE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_GRID] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT2] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_FIGURE_HIGHLIGHT] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_PALE_FIGURE] = Color.FromArgb(128, 128, 128);
        PenColorTbl[DrawTools.PEN_MEASURE_FIGURE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_DIMENTION] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_MESH_LINE] = Color.FromArgb(128, 128, 128);
        PenColorTbl[DrawTools.PEN_MESH_EDGE_LINE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_TEST] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_NURBS_CTRL_LINE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_DRAG_LINE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_NORMAL] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_EXT_SNAP] = Color.FromArgb(0, 0, 0, 0);
        PenColorTbl[DrawTools.PEN_HANDLE_LINE] = Color.FromArgb(0, 0, 0);
        PenColorTbl[DrawTools.PEN_AXIS_X] = Color.FromArgb(192, 60, 60);
        PenColorTbl[DrawTools.PEN_AXIS_Y] = Color.FromArgb(60, 128, 60);
        PenColorTbl[DrawTools.PEN_AXIS_Z] = Color.FromArgb(60, 60, 192);
        PenColorTbl[DrawTools.PEN_OLD_FIGURE] = Color.FromArgb(92, 92, 92);
        PenColorTbl[DrawTools.PEN_COMPASS_X] = Color.FromArgb(192, 60, 60);
        PenColorTbl[DrawTools.PEN_COMPASS_Y] = Color.FromArgb(60, 128, 60);
        PenColorTbl[DrawTools.PEN_COMPASS_Z] = Color.FromArgb(60, 60, 192);
        PenColorTbl[DrawTools.PEN_CURRENT_FIG_SELECTED_POINT] = Color.FromArgb(0,0,0,0);
        PenColorTbl[DrawTools.PEN_LAST_SEL_SEG] = Color.FromArgb(0,0,0);

        BrushColorTbl[DrawTools.BRUSH_DEFAULT] = Color.FromArgb(128, 128, 128);
        BrushColorTbl[DrawTools.BRUSH_BACKGROUND] = Color.FromArgb(0, 0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_TEXT] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_DEFAULT_MESH_FILL] = Color.FromArgb(211, 211, 211);
        BrushColorTbl[DrawTools.BRUSH_TRANSPARENT] = Color.FromArgb(0, 0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_PALE_TEXT] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_AXIS_LABEL_X] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_AXIS_LABEL_Y] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_AXIS_LABEL_Z] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_COMPASS_LABEL_X] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_COMPASS_LABEL_Y] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_COMPASS_LABEL_Z] = Color.FromArgb(0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_SELECTED_POINT] = Color.FromArgb(0, 0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_CURRENT_FIG_SELECTED_POINT] = Color.FromArgb(0, 0, 0, 0);
    }
}
