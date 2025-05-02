using CadDataTypes;
using TCad.Plotter.Controller;
using System;
using System.Collections.Generic;
using TCad.Plotter.DrawToolSet;
using TCad.Plotter.Model.HalfEdgeModel;

namespace TCad.Plotter.Drawing;

public interface IDrawing : IDisposable
{
    void Clear(DrawBrush brush);

    void DrawAxis();

    void DrawAxisLabel();

    void DrawCompass();

    void DrawPageFrame(vcompo_t w, vcompo_t h, vector3_t center);

    void DrawGrid(Gridding grid);

    void DrawHighlightPoint(vector3_t pt, DrawPen pen);

    void DrawHighlightPoints(List<HighlightPointListItem> list);

    void DrawSelectedPoint(vector3_t pt, DrawPen pen);

    void DrawLastSelectedPoint(vector3_t pt, DrawPen pen);

    void DrawSelectedPoints(VertexList pointList, DrawPen pen);

    void DrawExtSnapPoints(Vector3List pointList, DrawPen pen);

    void DrawMarkCursor(DrawPen pen, vector3_t p, vcompo_t pix_size);

    void DrawRect(DrawPen pen, vector3_t p0, vector3_t p1);

    void DrawCross(DrawPen pen, vector3_t p, vcompo_t size);

    void DrawLine(DrawPen pen, vector3_t a, vector3_t b);

    void DrawDot(DrawPen pen, vector3_t p);

    void DrawHarfEdgeModel(
        DrawBrush brush, DrawPen pen, DrawPen edgePen, vcompo_t edgeThreshold, HeModel model);

    void DrawText(int font, DrawBrush brush, vector3_t a, vector3_t xdir, vector3_t ydir, DrawTextOption opt, vcompo_t scale, string s);

    void DrawArrow(DrawPen pen, vector3_t pt0, vector3_t pt1, ArrowTypes type, ArrowPos pos, vcompo_t len, vcompo_t width);

    void DrawCrossCursorScrn(CadCursor pp, DrawPen pen);

    void DrawCrossCursorScrn(CadCursor pp, DrawPen pen, vcompo_t xsize, vcompo_t ysize);

    void DrawRectScrn(DrawPen pen, vector3_t p0, vector3_t p1);

    void DrawCrossScrn(DrawPen pen, vector3_t p, vcompo_t size);

    void DrawBouncingBox(DrawPen pen, MinMax3D mm);
}
