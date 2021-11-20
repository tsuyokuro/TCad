using HalfEdgeNS;
using System.Collections.Generic;
using OpenTK;
using System;
using CadDataTypes;

namespace Plotter
{
    public struct DrawTextOption
    {
        public const uint H_CENTER = 1;

        public uint Option;

        public DrawTextOption(uint option)
        {
            Option = option;
        }
    }

    public class DrawingConst
    {
        public const float HighlightPointLineWidth = 2;
        public const float HighlightPointLineLength = 6;

        public const double AxisLength = 100;
    }

    public interface IDrawing : IDisposable
    {
        void Clear(DrawBrush brush);

        void DrawSelected(List<CadFigure> list);

        void DrawAxis();

        void DrawAxisLabel();

        void DrawCompass();

        void DrawPageFrame(double w, double h, Vector3d center);

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(Vector3d pt, DrawPen pen);

        void DrawHighlightPoints(List<HighlightPointListItem> list);

        void DrawSelectedPoint(Vector3d pt, DrawPen pen);

        void DrawSelectedPoints(VertexList pointList, DrawPen pen);

        void DrawExtSnapPoints(Vector3dList pointList, DrawPen pen);

        void DrawMarkCursor(DrawPen pen, Vector3d p, double pix_size);

        void DrawRect(DrawPen pen, Vector3d p0, Vector3d p1);

        void DrawCross(DrawPen pen, Vector3d p, double size);

        void DrawLine(DrawPen pen, Vector3d a, Vector3d b);

        void DrawDot(DrawPen pen, Vector3d p);

        void DrawHarfEdgeModel(
            DrawBrush brush, DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model);

        void DrawText(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, string s);

        void DrawArrow(DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawCrossCursorScrn(CadCursor pp, DrawPen pen);

        void DrawRectScrn(DrawPen pen, Vector3d p0, Vector3d p1);

        void DrawCrossScrn(DrawPen pen, Vector3d p, double size);

        void DrawBouncingBox(DrawPen pen, MinMax3D mm);
    }
}
