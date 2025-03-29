using System.Drawing;

namespace Plotter;

public class DarkColors : ColorSet
{
    private static DarkColors mInstance = new DarkColors();
    public static DarkColors Instance
    {
        get { return mInstance; }
    }

    private DarkColors()
    {
        PenColorTbl[DrawTools.PEN_DEFAULT] = Color.FromArgb(255, 255, 255);
        PenColorTbl[DrawTools.PEN_SELECTED_POINT] = Color.FromArgb(128, 255, 0);
        PenColorTbl[DrawTools.PEN_CROSS_CURSOR] = Color.FromArgb(32, 64, 64);
        PenColorTbl[DrawTools.PEN_CROSS_CURSOR2] = Color.FromArgb(48, 128, 96);
        PenColorTbl[DrawTools.PEN_DEFAULT_FIGURE] = Color.FromArgb(255, 255, 255);
        PenColorTbl[DrawTools.PEN_TEMP_FIGURE] = Color.FromArgb(95, 158, 160);
        PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT] = Color.FromArgb(255, 165, 0);
        PenColorTbl[DrawTools.PEN_MATCH_SEG] = Color.FromArgb(154, 205, 50);
        PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER] = Color.FromArgb(100, 149, 237);
        PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER2] = Color.FromArgb(154, 205, 50);
        PenColorTbl[DrawTools.PEN_AXIS] = Color.FromArgb(60, 60, 92);
        PenColorTbl[DrawTools.PEN_PAGE_FRAME] = Color.FromArgb(92, 92, 92);
        PenColorTbl[DrawTools.PEN_TEST_FIGURE] = Color.FromArgb(255, 255, 0);
        PenColorTbl[DrawTools.PEN_GRID] = Color.FromArgb(92, 92, 92);
        PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT2] = Color.FromArgb(64, 255, 255);
        PenColorTbl[DrawTools.PEN_FIGURE_HIGHLIGHT] = Color.FromArgb(255, 92, 192);
        PenColorTbl[DrawTools.PEN_PALE_FIGURE] = Color.FromArgb(126, 126, 126);
        PenColorTbl[DrawTools.PEN_MEASURE_FIGURE] = Color.FromArgb(255, 69, 0);
        PenColorTbl[DrawTools.PEN_DIMENTION] = Color.FromArgb(255, 128, 192, 255);

        PenColorTbl[DrawTools.PEN_MESH_LINE] = Color.FromArgb(112, 112, 112);
        PenColorTbl[DrawTools.PEN_MESH_EDGE_LINE] = Color.FromArgb(67, 184, 255);

        PenColorTbl[DrawTools.PEN_TEST] = Color.FromArgb(187, 204, 221);
        PenColorTbl[DrawTools.PEN_NURBS_CTRL_LINE] = Color.FromArgb(255, 96, 192, 96);
        PenColorTbl[DrawTools.PEN_DRAG_LINE] = Color.FromArgb(255, 96, 96, 128);
        PenColorTbl[DrawTools.PEN_NORMAL] = Color.FromArgb(255, 0, 255, 127);
        PenColorTbl[DrawTools.PEN_EXT_SNAP] = Color.FromArgb(255, 255, 0, 0);
        PenColorTbl[DrawTools.PEN_HANDLE_LINE] = Color.FromArgb(154, 205, 50);
        PenColorTbl[DrawTools.PEN_AXIS_X] = Color.FromArgb(112, 50, 50);
        PenColorTbl[DrawTools.PEN_AXIS_Y] = Color.FromArgb(50, 92, 50);
        PenColorTbl[DrawTools.PEN_AXIS_Z] = Color.FromArgb(50, 50, 128);
        PenColorTbl[DrawTools.PEN_OLD_FIGURE] = Color.FromArgb(92, 92, 92);
        PenColorTbl[DrawTools.PEN_COMPASS_X] = Color.FromArgb(192, 92, 92);
        PenColorTbl[DrawTools.PEN_COMPASS_Y] = Color.FromArgb(92, 192, 92);
        PenColorTbl[DrawTools.PEN_COMPASS_Z] = Color.FromArgb(92, 92, 255);
        PenColorTbl[DrawTools.PEN_CURRENT_FIG_SELECTED_POINT] = Color.FromArgb(255, 92, 192);
        PenColorTbl[DrawTools.PEN_LAST_SEL_SEG] = Color.FromArgb(255, 192, 255);


        BrushColorTbl[DrawTools.BRUSH_DEFAULT] = Color.FromArgb(255, 255, 255);
        BrushColorTbl[DrawTools.BRUSH_BACKGROUND] = Color.FromArgb(8, 8, 8);
        BrushColorTbl[DrawTools.BRUSH_TEXT] = Color.FromArgb(255, 255, 255);
        BrushColorTbl[DrawTools.BRUSH_DEFAULT_MESH_FILL] = Color.FromArgb(128, 128, 128);
        BrushColorTbl[DrawTools.BRUSH_TRANSPARENT] = Color.FromArgb(0, 0, 0, 0);
        BrushColorTbl[DrawTools.BRUSH_PALE_TEXT] = Color.FromArgb(126, 126, 126);

        Color AxisLabel_X = Color.FromArgb(144, 144, 144);
        Color AxisLabel_Y = Color.FromArgb(144, 144, 144);
        Color AxisLabel_Z = Color.FromArgb(144, 144, 144);

        BrushColorTbl[DrawTools.BRUSH_AXIS_LABEL_X] = AxisLabel_X;
        BrushColorTbl[DrawTools.BRUSH_AXIS_LABEL_Y] = AxisLabel_Y;
        BrushColorTbl[DrawTools.BRUSH_AXIS_LABEL_Z] = AxisLabel_Z;
        BrushColorTbl[DrawTools.BRUSH_COMPASS_LABEL_X] = AxisLabel_X;
        BrushColorTbl[DrawTools.BRUSH_COMPASS_LABEL_Y] = AxisLabel_Y;
        BrushColorTbl[DrawTools.BRUSH_COMPASS_LABEL_Z] = AxisLabel_Z;

        BrushColorTbl[DrawTools.BRUSH_SELECTED_POINT] = Color.FromArgb(128, 255, 0);
        BrushColorTbl[DrawTools.BRUSH_CURRENT_FIG_SELECTED_POINT] = Color.FromArgb(255, 92, 192);
    }
}
