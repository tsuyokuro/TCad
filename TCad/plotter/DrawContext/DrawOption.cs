using Plotter.Settings;

namespace Plotter
{
    public class DrawOption
    {
        public bool ForcePen = true;
        public bool ForceBrush = true;

        public DrawPen LinePen = default;

        public DrawPen MeshLinePen = default;
        public DrawPen MeshEdgePen = default;
        public DrawBrush MeshBrush = default;

        public DrawBrush TextBrush = default;

        public DrawOption()
        {
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
            Pale.MeshBrush = DrawBrush.NullBrush;
            Pale.TextBrush = DC.GetBrush(DrawTools.BRUSH_PALE_TEXT);

            // Before
            Before.LinePen = DC.GetPen(DrawTools.PEN_OLD_FIGURE);
            Before.MeshLinePen = DC.GetPen(DrawTools.PEN_OLD_FIGURE);
            Before.MeshEdgePen = DC.GetPen(DrawTools.PEN_OLD_FIGURE);
            Before.MeshBrush = DrawBrush.NullBrush;
            Before.TextBrush = DC.GetBrush(DrawTools.BRUSH_PALE_TEXT);

            // Temp
            Temp.LinePen = DC.GetPen(DrawTools.PEN_TEST_FIGURE);
            Temp.MeshLinePen = DC.GetPen(DrawTools.PEN_TEST_FIGURE);
            Temp.MeshEdgePen = DC.GetPen(DrawTools.PEN_TEST_FIGURE);
            Temp.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL); ;
            Temp.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);

            // Current
            Current.LinePen = DC.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);

            Current.MeshLinePen = DC.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);
            Current.MeshEdgePen = DC.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);

            if (SettingsHolder.Settings.FillMesh)
            {
                Current.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL); ;
            }
            else
            {
                Current.MeshBrush = DrawBrush.NullBrush;
            }

            Current.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);

            // Measure
            Measure.LinePen = DC.GetPen(DrawTools.PEN_MEASURE_FIGURE);
            Measure.MeshLinePen = DC.GetPen(DrawTools.PEN_MEASURE_FIGURE);
            Measure.MeshEdgePen = DC.GetPen(DrawTools.PEN_MEASURE_FIGURE);
            Measure.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL); ;
            Measure.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);

            // Noraml
            Normal.LinePen = DC.GetPen(DrawTools.PEN_DEFAULT_FIGURE);
            if (SettingsHolder.Settings.DrawMeshEdge)
            {
                Normal.MeshLinePen = DC.GetPen(DrawTools.PEN_MESH_LINE);
                Normal.MeshEdgePen = DC.GetPen(DrawTools.PEN_DEFAULT_FIGURE);
            }
            else
            {
                Normal.MeshLinePen = DrawPen.NullPen;
                Normal.MeshEdgePen = DrawPen.NullPen;
            }

            if (SettingsHolder.Settings.FillMesh)
            {
                Normal.MeshBrush = DC.GetBrush(DrawTools.BRUSH_DEFAULT_MESH_FILL);
            }
            else
            {
                Normal.MeshBrush = DrawBrush.NullBrush;
            }

            Normal.TextBrush = DC.GetBrush(DrawTools.BRUSH_TEXT);
            Normal.ForcePen = false;
            Normal.ForceBrush = false;
        }
    }


}
