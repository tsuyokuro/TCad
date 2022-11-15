using MyCollections;
using Plotter.Controller;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Resources;

namespace Plotter;

public class DrawTools : IDisposable
{
    public const int PEN_DEFAULT = 1;
    public const int PEN_DEFAULT_FIGURE = 2;
    public const int PEN_SELECTED_POINT = 3;
    public const int PEN_CROSS_CURSOR = 4;
    public const int PEN_TEMP_FIGURE = 5;
    public const int PEN_POINT_HIGHLIGHT = 6;
    public const int PEN_MATCH_SEG = 7;
    public const int PEN_LAST_POINT_MARKER = 8;
    public const int PEN_LAST_POINT_MARKER2 = 9;
    public const int PEN_AXIS = 10;
    public const int PEN_PAGE_FRAME = 11;
    public const int PEN_TEST_FIGURE = 12;
    public const int PEN_GRID = 13;
    public const int PEN_POINT_HIGHLIGHT2 = 14;
    public const int PEN_FIGURE_HIGHLIGHT = 15;
    public const int PEN_PALE_FIGURE = 16;
    public const int PEN_MEASURE_FIGURE = 17;
    public const int PEN_DIMENTION = 18;
    public const int PEN_MESH_LINE = 19;
    public const int PEN_TEST = 20;
    public const int PEN_NURBS_CTRL_LINE = 21;
    public const int PEN_DRAG_LINE = 22;
    public const int PEN_NORMAL = 23;
    public const int PEN_EXT_SNAP = 24;
    public const int PEN_HANDLE_LINE = 25;
    public const int PEN_AXIS_X = 26;
    public const int PEN_AXIS_Y = 27;
    public const int PEN_AXIS_Z = 28;
    public const int PEN_OLD_FIGURE = 29;
    public const int PEN_COMPASS_X = 30;
    public const int PEN_COMPASS_Y = 31;
    public const int PEN_COMPASS_Z = 32;
    public const int PEN_MESH_EDGE_LINE = 33;
    public const int PEN_CURRENT_FIG_SELECTED_POINT = 34;

    public const int PEN_TBL_SIZE = 35;


    public const int BRUSH_DEFAULT = 1;
    public const int BRUSH_BACKGROUND = 2;
    public const int BRUSH_TEXT = 3;
    public const int BRUSH_TRANSPARENT = 4;
    public const int BRUSH_DEFAULT_MESH_FILL = 5;
    public const int BRUSH_PALE_TEXT = 6;
    public const int BRUSH_AXIS_LABEL_X = 6;
    public const int BRUSH_AXIS_LABEL_Y = 7;
    public const int BRUSH_AXIS_LABEL_Z = 8;
    public const int BRUSH_COMPASS_LABEL_X = 9;
    public const int BRUSH_COMPASS_LABEL_Y = 10;
    public const int BRUSH_COMPASS_LABEL_Z = 11;
    public const int BRUSH_SELECTED_POINT = 12;
    public const int BRUSH_CURRENT_FIG_SELECTED_POINT = 13;

    public const int BRUSH_TBL_SIZE = 14;

    public const int FONT_DEFAULT = 1;
    public const int FONT_SMALL = 2;
    public const int FONT_TBL_SIZE = 3;

    public const int FONT_SIZE_DEFAULT = 11;
    public const int FONT_SIZE_SMALL = 11;

    public Color[] PenColorTbl;
    public Color[] BrushColorTbl;

    FlexArray<DrawPen> PenTbl = null;
    FlexArray<DrawBrush> BrushTbl = null;
    FlexArray<Font> GDIFontTbl = null; // GDI Modeでしか使わない

    private void AllocTbl()
    {
        PenTbl = new FlexArray<DrawPen>(new DrawPen[PEN_TBL_SIZE]);
        BrushTbl = new FlexArray<DrawBrush>(new DrawBrush[BRUSH_TBL_SIZE]);
        GDIFontTbl = new FlexArray<Font>(new Font[FONT_TBL_SIZE]);
    }

    public void Setup(DrawModes t, int penW = 0)
    {
        Dispose();

        if (t == DrawModes.DARK)
        {
            SetupScreenSet(DarkColors.Instance, penW);
        }
        else if (t == DrawModes.LIGHT)
        {
            SetupScreenSet(LightColors.Instance, penW);
        }
        else if (t == DrawModes.PRINTER)
        {
            SetupPrinterSet(penW);
        }
    }

    private void SetupScreenSet(ColorSet colorSet, int penW)
    {
        AllocTbl();

        PenColorTbl = colorSet.PenColorTbl;
        BrushColorTbl = colorSet.BrushColorTbl;

        for (int i = 0; i < PEN_TBL_SIZE; i++)
        {
            PenTbl[i] = new DrawPen(PenColorTbl[i].ToArgb(), penW);
        }

        for (int i = 0; i < BRUSH_TBL_SIZE; i++)
        {
            BrushTbl[i] = new DrawBrush(BrushColorTbl[i].ToArgb());
        }

        //FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
        FontFamily fontFamily = new FontFamily("MS UI Gothic");
        //FontFamily fontFamily = new FontFamily("ＭＳ ゴシック");

        GDIFontTbl[FONT_DEFAULT] = new Font(fontFamily, FONT_SIZE_DEFAULT);
        GDIFontTbl[FONT_SMALL] = new Font(fontFamily, FONT_SIZE_SMALL);
    }

    private void SetupPrinterSet(int penW)
    {
        AllocTbl();

        ColorSet colorSet = PrintColors.Instance;

        PenColorTbl = colorSet.PenColorTbl;
        BrushColorTbl = colorSet.BrushColorTbl;

        for (int i = 0; i < PEN_TBL_SIZE; i++)
        {
            PenTbl[i] = new DrawPen(PenColorTbl[i].ToArgb(), penW);
        }

        for (int i = 0; i < BRUSH_TBL_SIZE; i++)
        {
            BrushTbl[i] = new DrawBrush(BrushColorTbl[i].ToArgb());
        }

        //FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
        //FontFamily fontFamily = new FontFamily("MS UI Gothic");
        FontFamily fontFamily = new FontFamily("MS Gothic");

        GDIFontTbl[FONT_DEFAULT] = new Font(fontFamily, FONT_SIZE_DEFAULT);
        GDIFontTbl[FONT_SMALL] = new Font(fontFamily, FONT_SIZE_SMALL);
    }

    public void Dispose()
    {
        if (PenTbl != null)
        {
            PenTbl = null;
        }

        if (BrushTbl != null)
        {
            BrushTbl = null;
        }

        if (GDIFontTbl != null)
        {
            foreach (Font font in GDIFontTbl)
            {
                if (font != null)
                {
                    font.Dispose();
                }
            }

            GDIFontTbl = null;
        }
    }

    public DrawTools()
    {
    }

    ~DrawTools()
    {
        Dispose();
    }

    #region Utilities
    public static FontFamily LoadFontFamilyFromResource(string fname)
    {
        StreamResourceInfo si = System.Windows.Application.GetResourceStream(
            new Uri(fname, UriKind.Relative));

        return LoadFontFamily(si.Stream);
    }

    // Load font family from stream
    public static FontFamily LoadFontFamily(Stream stream)
    {
        var buffer = new byte[stream.Length];

        stream.Read(buffer, 0, buffer.Length);

        return LoadFontFamily(buffer);
    }


    static PrivateFontCollection PrivateFonts = new PrivateFontCollection();

    // load font family from byte array
    public static FontFamily LoadFontFamily(byte[] buffer)
    {
        IntPtr data = Marshal.AllocCoTaskMem(buffer.Length);

        Marshal.Copy(buffer, 0, data, buffer.Length);

        PrivateFonts.AddMemoryFont(data, buffer.Length);

        Marshal.FreeCoTaskMem(data);

        return PrivateFonts.Families[0];
    }

    #endregion

    public DrawPen Pen(int id)
    {
        return PenTbl[id];
    }

    public DrawBrush Brush(int id)
    {
        return BrushTbl[id];
    }

    public Font font(int id)
    {
        return GDIFontTbl[id];
    }
}
