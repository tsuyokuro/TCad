using Plotter.Settings;
using SharpFont.Fnt;
using static System.Windows.Forms.DataFormats;

namespace Plotter;

public class DrawOption
{
    public static byte FORCE_PEN = 0x01;
    public static byte FORCE_MESH_PEN = 0x02;
    public static byte FORCE_MESH_BREASH = 0x04;

    byte Flag = 0;

    public DrawPen LinePen = default;
    public DrawPen MeshLinePen = default;
    public DrawPen MeshEdgePen = default;
    public DrawBrush MeshBrush = default;
    public DrawBrush TextBrush = default;

    public bool ForcePen
    {
        set => Flag = value ? (byte)(Flag | FORCE_PEN) : (byte)(Flag & ~FORCE_PEN);
        get => (Flag & FORCE_PEN) != 0;
    }

    public bool ForceMeshPen
    {
        set => Flag = value ? (byte)(Flag | FORCE_MESH_PEN) : (byte)(Flag & ~FORCE_MESH_PEN);
        get => (Flag & FORCE_PEN) != 0;
    }

    public bool ForceMeshBrush
    {
        set => Flag = value ? (byte)(Flag | FORCE_MESH_BREASH) : (byte)(Flag & ~FORCE_MESH_BREASH);
        get => (Flag & FORCE_MESH_BREASH) != 0;
    }

    public DrawOption()
    {
    }

    public void ForceAllOn()
    {
        Flag = 0xFF;
    }

    public void ForceAllOff()
    {
        Flag = 0x00;
    }
}

public class DrawOptionSet
{
    private DrawContext DC;

    public DrawOption Normal = new DrawOption();
    public DrawOption Pale = new DrawOption();
    public DrawOption Temp = new DrawOption();
    public DrawOption Current = new DrawOption();
    public DrawOption Measure = new DrawOption();
    public DrawOption Before = new DrawOption();

    public DrawOptionSet(DrawContext dc)
    {
        DC = dc;
    }

    public void Update()
    {
        // Pale
        Pale.LinePen = DC.GetPen(DrawTools.PEN_PALE_FIGURE);
        Pale.MeshLinePen = DC.GetPen(DrawTools.PEN_PALE_FIGURE);
        Pale.MeshEdgePen = DC.GetPen(DrawTools.PEN_PALE_FIGURE);
        Pale.MeshBrush = DrawBrush.InvalidBrush;
        Pale.TextBrush = DC.GetBrush(DrawTools.BRUSH_PALE_TEXT);
        Pale.ForceAllOn();

        // Before
        Before.LinePen = DC.GetPen(DrawTools.PEN_OLD_FIGURE);
        Before.MeshLinePen = DC.GetPen(DrawTools.PEN_OLD_FIGURE);
        Before.MeshEdgePen = DC.GetPen(DrawTools.PEN_OLD_FIGURE);
        Before.MeshBrush = DrawBrush.InvalidBrush;
        Before.TextBrush = DC.GetBrush(DrawTools.BRUSH_PALE_TEXT);
        Before.ForceAllOn();

        // Temp
        Temp.LinePen = DC.GetPen(DrawTools.PEN_TEST_FIGURE);
        Temp.MeshLinePen = DC.GetPen(DrawTools.PEN_TEST_FIGURE);
        Temp.MeshEdgePen = DC.GetPen(DrawTools.PEN_TEST_FIGURE);
        Temp.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL); ;
        Temp.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);
        Temp.ForceAllOn();

        // Current
        Current.ForceAllOn();
        Current.LinePen = DC.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);

        Current.MeshLinePen = DC.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);
        Current.MeshEdgePen = DC.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);

        if (SettingsHolder.Settings.FillMesh)
        {
            Current.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL);
            Current.ForceMeshBrush = false;
        }
        else
        {
            Current.MeshBrush = DrawBrush.InvalidBrush;
        }

        Current.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);

        // Measure
        Measure.LinePen = DC.GetPen(DrawTools.PEN_MEASURE_FIGURE);
        Measure.MeshLinePen = DC.GetPen(DrawTools.PEN_MEASURE_FIGURE);
        Measure.MeshEdgePen = DC.GetPen(DrawTools.PEN_MEASURE_FIGURE);
        Measure.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL); ;
        Measure.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);
        Measure.ForceAllOn();

        // Noraml
        Normal.ForceAllOff();

        Normal.LinePen = DC.GetPen(DrawTools.PEN_DEFAULT_FIGURE);

        if (SettingsHolder.Settings.DrawMeshEdge)
        {
            Normal.MeshLinePen = DC.GetPen(DrawTools.PEN_MESH_LINE);
            Normal.MeshEdgePen = DC.GetPen(DrawTools.PEN_DEFAULT_FIGURE);
        }
        else
        {
            Normal.MeshLinePen = DrawPen.InvalidPen;
            Normal.MeshEdgePen = DrawPen.InvalidPen;
            Normal.ForceMeshPen = true;
        }

        if (SettingsHolder.Settings.FillMesh)
        {
            Normal.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL);
        }
        else
        {
            Normal.MeshBrush = DrawBrush.InvalidBrush;
            Normal.ForceMeshBrush = true;
        }

        Normal.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);
    }
}
