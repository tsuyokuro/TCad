using MyCollections;
using Plotter.Controller;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
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
    public const int PEN_CROSS_CURSOR2 = 35;
    public const int PEN_LAST_SEL_SEG = 36;

    public const int PEN_TBL_SIZE = 37;


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

    FlexArray<DrawPen> PenTbl = null;
    FlexArray<DrawBrush> BrushTbl = null;
    FlexArray<Font> GDIFontTbl = null; // GDI Modeでしか使わない

    private void AllocTbl()
    {
        PenTbl = new FlexArray<DrawPen>(new DrawPen[PEN_TBL_SIZE]);
        BrushTbl = new FlexArray<DrawBrush>(new DrawBrush[BRUSH_TBL_SIZE]);
        GDIFontTbl = new FlexArray<Font>(new Font[FONT_TBL_SIZE]);
    }

    public void Setup(DrawModes t)
    {
        Dispose();

        if (t == DrawModes.DARK)
        {
            SetupScreenSet("dark.json", new ColorPack(255, 192, 192, 192));
        }
        else if (t == DrawModes.LIGHT)
        {
            SetupScreenSet("light.json", new ColorPack(255, 92, 92, 92));
        }
        else if (t == DrawModes.PRINTER)
        {
            SetupPrinterSet();
        }
    }

    private void SetupScreenSet(String fname, ColorPack defColor)
    {
        AllocTbl();

        var pathName = PathName(fname);

        LoadTheme(pathName, defColor);


        //FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
        FontFamily fontFamily = new("MS UI Gothic");
        //FontFamily fontFamily = new FontFamily("ＭＳ ゴシック");

        GDIFontTbl[FONT_DEFAULT] = new Font(fontFamily, FONT_SIZE_DEFAULT);
        GDIFontTbl[FONT_SMALL] = new Font(fontFamily, FONT_SIZE_SMALL);
    }

    private void SetupPrinterSet()
    {
        AllocTbl();


        LoadTheme(PathName("printer.json"), new ColorPack(255, 0, 0, 0));


        //FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
        //FontFamily fontFamily = new FontFamily("MS UI Gothic");
        FontFamily fontFamily = new("MS Gothic");

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
                font?.Dispose();
            }

            GDIFontTbl = null;
        }

        GC.SuppressFinalize(this);
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

        stream.ReadExactly(buffer, 0, buffer.Length);

        return LoadFontFamily(buffer);
    }


    static readonly PrivateFontCollection PrivateFonts = new();

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


    private void LoadTheme(string fname, ColorPack defColor)
    {
        for (int i = 0; i < PEN_TBL_SIZE; i++)
        {
            PenTbl[i] = new DrawPen(defColor.Argb, 1);
        }

        for (int i = 0; i < BRUSH_TBL_SIZE; i++)
        {
            BrushTbl[i] = new DrawBrush(defColor.Argb);
        }

        string json = File.ReadAllText(fname);
        JsonDocument jdoc = JsonDocument.Parse(json);

        // Pens
        jdoc.RootElement.TryGetProperty("pens", out JsonElement jPens);
        var jpenList = jPens.EnumerateArray().ToList();

        foreach (var jpen in jpenList)
        {
            string? name = jpen.GetProperty("name").GetString();

            if (name == null) continue;

            var c = jpen.GetProperty("color");

            ColorPack color = GetColorFromJson(c, defColor);

            int width = jpen.GetProperty("width").GetInt32();
            if (width == 0) width = 1;

            DrawPen pen = new DrawPen(color.Argb, width);

            SetPenTbl(name, pen);
        }

        // Brushes
        jdoc.RootElement.TryGetProperty("brushes", out JsonElement jBrushes);
        var jbrushList = jBrushes.EnumerateArray().ToList();

        foreach (var jbrush in jbrushList)
        {
            string? name = jbrush.GetProperty("name").GetString();

            if (name == null) continue;

            var c = jbrush.GetProperty("color");
            ColorPack color = GetColorFromJson(c, defColor);

            DrawBrush brush = new DrawBrush(color.Argb);

            SetBrushTbl(name, brush);
        }
    }

    private ColorPack GetColorFromJson(JsonElement jColor, ColorPack defaultColor)
    {
        var c = jColor.EnumerateArray().ToList();
        if (c == null) return defaultColor;

        if (c.Count < 3) return defaultColor;

        if (c.Count == 3)
        {
            return new ColorPack(
                255,
                c[0].GetByte(),
                c[1].GetByte(),
                c[2].GetByte()
                );
        }

        return new ColorPack(
            c[0].GetByte(),
            c[1].GetByte(),
            c[2].GetByte(),
            c[3].GetByte()
            );
    }

    private bool SetPenTbl(string name, DrawPen pen)
    {
        if (name == "") return false;

        FieldInfo? fi = typeof(DrawTools).GetField(name);
        if (fi == null) return false;
        if (fi.FieldType != typeof(int)) return false;

        int? penId = (int?)fi.GetValue(this);

        if (penId == null) return false;

        PenTbl[penId.Value] = pen;

        return true;
    }

    private bool SetBrushTbl(string name, DrawBrush brush)
    {
        FieldInfo? fi = typeof(DrawTools).GetField(name);
        if (fi == null) return false;

        int? brushId = (int?)fi.GetValue(this);

        if (brushId == null) return false;

        BrushTbl[brushId.Value] = brush;

        return true;
    }


    private string PathName(string fname)
    {
        Assembly? asm = Assembly.GetEntryAssembly();

        string exePath = asm!.Location;

        string? dir = Path.GetDirectoryName(exePath);

        string fileName = dir + @"\Resources\DrawTheme\" + fname;

        return fileName;
    }
}
