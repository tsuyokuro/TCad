using HalfEdgeNS;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;
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

        public const double AxisLength = 100; // DrawingGDIでしか使っていない

        public const double NormalLen = 20;
        public const double NormalArrowLen = 10;
        public const double NormalArrowWidth = 5;

        public const double AxisArrowLen = 16;
        public const double AxisArrowWidth = 8;
    }

    public interface IDrawing : IDisposable
    {
        void Clear(in DrawBrush brush);

        void DrawSelected(List<CadFigure> list);

        void DrawAxis();

        void DrawAxisLabel();

        void DrawCompass();

        void DrawPageFrame(double w, double h, Vector3d center);

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(Vector3d pt, in DrawPen pen);

        void DrawHighlightPoints(List<HighlightPointListItem> list);

        void DrawSelectedPoint(Vector3d pt, in DrawPen pen);

        void DrawSelectedPoints(VertexList pointList, in DrawPen pen);

        void DrawExtSnapPoints(Vector3dList pointList, in DrawPen pen);

        void DrawMarkCursor(in DrawPen pen, Vector3d p, double pix_size);

        void DrawRect(in DrawPen pen, Vector3d p0, Vector3d p1);

        void DrawCross(in DrawPen pen, Vector3d p, double size);

        void DrawLine(in DrawPen pen, Vector3d a, Vector3d b);

        void DrawDot(in DrawPen pen, Vector3d p);

        void DrawHarfEdgeModel(
            in DrawBrush brush, in DrawPen pen, in DrawPen edgePen, double edgeThreshold, HeModel model);

        void DrawText(int font, in DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, double scale, string s);

        void DrawArrow(in DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawCrossCursorScrn(CadCursor pp, in DrawPen pen);

        void DrawRectScrn(in DrawPen pen, Vector3d p0, Vector3d p1);

        void DrawCrossScrn(in DrawPen pen, Vector3d p, double size);

        void DrawBouncingBox(in DrawPen pen, MinMax3D mm);
    }
}
