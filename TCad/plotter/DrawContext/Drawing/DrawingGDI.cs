using CadDataTypes;
using Plotter;
using Plotter.Controller;
using System;
using System.Collections.Generic;
using System.Drawing;
using TCad.MathFunctions;
using TCad.Plotter.Model.HalfEdgeModel;

namespace TCad.Plotter.Drawing;

/**
 * GDI向け描画クラス
 * Drawing class for GDI 
 */
public class DrawingGDI : IDrawing
{
    public DrawContextGDI DC;

    public DrawingGDI()
    {
    }

    public DrawingGDI(DrawContextGDI dc)
    {
        DC = dc;
    }

    public void Clear(DrawBrush brush)
    {
        FillRectangleScrn(
            brush,
            0, 0, (int)DC.ViewWidth, (int)DC.ViewHeight);
    }

    #region "Draw base"
    public void DrawAxis()
    {
        vector3_t p0 = default;
        vector3_t p1 = default;

        vcompo_t len = DrawSizes.AxisLength;

        // X軸
        p0.X = -len;
        p0.Y = 0;
        p0.Z = 0;

        p1.X = len;
        p1.Y = 0;
        p1.Z = 0;


        DrawLine(DC.GetPen(DrawTools.PEN_AXIS_X), p0, p1);

        // Y軸
        p0.X = 0;
        p0.Y = -len;
        p0.Z = 0;

        p1.X = 0;
        p1.Y = len;
        p1.Z = 0;

        DrawLine(DC.GetPen(DrawTools.PEN_AXIS_Y), p0, p1);

        // Z軸
        p0.X = 0;
        p0.Y = 0;
        p0.Z = -len;

        p1.X = 0;
        p1.Y = 0;
        p1.Z = len;

        DrawLine(DC.GetPen(DrawTools.PEN_AXIS_Z), p0, p1);
    }

    public void DrawAxisLabel()
    {
        // TODO Draw axis label
    }

    public void DrawCompass()
    {

    }

    public virtual void DrawGrid(Gridding grid)
    {
        vector3_t lt = vector3_t.Zero;
        vector3_t rb = new vector3_t(DC.ViewWidth, DC.ViewHeight, 0);

        vector3_t ltw = DC.DevPointToWorldPoint(lt);
        vector3_t rbw = DC.DevPointToWorldPoint(rb);

        vcompo_t minx = (vcompo_t)Math.Min(ltw.X, rbw.X);
        vcompo_t maxx = (vcompo_t)Math.Max(ltw.X, rbw.X);

        vcompo_t miny = (vcompo_t)Math.Min(ltw.Y, rbw.Y);
        vcompo_t maxy = (vcompo_t)Math.Max(ltw.Y, rbw.Y);

        vcompo_t minz = (vcompo_t)Math.Min(ltw.Z, rbw.Z);
        vcompo_t maxz = (vcompo_t)Math.Max(ltw.Z, rbw.Z);


        DrawPen pen = DC.GetPen(DrawTools.PEN_GRID);

        vector3_t p = default(vector3_t);


        vcompo_t n = grid.Decimate(DC, grid, 8);

        vcompo_t x, y, z;
        vcompo_t sx, sy, sz;
        vcompo_t szx = grid.GridSize.X * n;
        vcompo_t szy = grid.GridSize.Y * n;
        vcompo_t szz = grid.GridSize.Z * n;

        sx = (vcompo_t)Math.Round(minx / szx) * szx;
        sy = (vcompo_t)Math.Round(miny / szy) * szy;
        sz = (vcompo_t)Math.Round(minz / szz) * szz;

        x = sx;
        while (x < maxx)
        {
            p.X = x;
            p.Z = 0;

            y = sy;

            while (y < maxy)
            {
                p.Y = y;
                DrawDot(pen, p);
                y += szy;
            }

            x += szx;
        }

        z = sz;
        y = sy;

        while (z < maxz)
        {
            p.Z = z;
            p.X = 0;

            y = sy;

            while (y < maxy)
            {
                p.Y = y;
                DrawDot(pen, p);
                y += szy;
            }

            z += szz;
        }

        z = sz;
        x = sx;

        while (x < maxx)
        {
            p.X = x;
            p.Y = 0;

            z = sz;

            while (z < maxz)
            {
                p.Z = z;
                DrawDot(pen, p);
                z += szz;
            }

            x += szx;
        }
    }

    public void DrawPageFrame(vcompo_t w, vcompo_t h, vector3_t center)
    {
        vector3_t pt = default(vector3_t);

        // p0
        pt.X = -w / 2 + center.X;
        pt.Y = h / 2 + center.Y;
        pt.Z = 0;

        vector3_t p0 = default(vector3_t);
        p0.X = pt.X * DC.UnitPerMilli;
        p0.Y = pt.Y * DC.UnitPerMilli;

        p0 += DC.ViewOrg;

        // p1
        pt.X = w / 2 + center.X;
        pt.Y = -h / 2 + center.Y;
        pt.Z = 0;

        vector3_t p1 = default(vector3_t);
        p1.X = pt.X * DC.UnitPerMilli;
        p1.Y = pt.Y * DC.UnitPerMilli;

        p1 += DC.ViewOrg;

        DrawRectScrn(DC.GetPen(DrawTools.PEN_PAGE_FRAME), p0, p1);
    }
    #endregion

    #region "Draw marker"
    public void DrawHighlightPoint(vector3_t pt, DrawPen pen)
    {
        vector3_t pp = DC.WorldPointToDevPoint(pt);

        //DrawCircleScrn(pen, pp, 3);

        DrawCrossScrn(pen, pp, 4);
    }

    public void DrawHighlightPoints(List<HighlightPointListItem> list)
    {
        list.ForEach(item =>
        {
            DrawHighlightPoint(item.Point, item.Pen);
        });
    }

    public void DrawSelectedPoint(vector3_t pt, DrawPen pen)
    {
        vector3_t pp = DC.WorldPointToDevPoint(pt);

        int size = 2;

        DrawRectangleScrn(
            pen,
            (int)pp.X - size, (int)pp.Y - size,
            (int)pp.X + size, (int)pp.Y + size
            );
    }

    public void DrawLastSelectedPoint(vector3_t pt, DrawPen pen)
    {
        vector3_t pp = DC.WorldPointToDevPoint(pt);

        int size = 3;

        DrawRectangleScrn(
            pen,
            (int)pp.X - size, (int)pp.Y - size,
            (int)pp.X + size, (int)pp.Y + size
            );
    }

    public void DrawSelectedPoints(VertexList pointList, DrawPen pen)
    {
        foreach (CadVertex p in pointList)
        {
            if (p.Selected)
            {
                DrawSelectedPoint(p.vector, pen);
            }
        }
    }

    public void DrawMarkCursor(DrawPen pen, vector3_t p, vcompo_t pix_size)
    {
        DrawCross(pen, p, pix_size);
    }
    #endregion

    public void DrawHarfEdgeModel(
        DrawBrush brush, DrawPen pen, DrawPen edgePen, vcompo_t edgeThreshold, HeModel model)
    {
        for (int i = 0; i < model.FaceStore.Count; i++)
        {
            HeFace f = model.FaceStore[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            HalfEdge pair;

            for (; ; )
            {
                bool edge = false;

                pair = c.Pair;

                if (pair == null)
                {
                    edge = true;
                }
                else
                {
                    vcompo_t s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                    if (Math.Abs(s) < edgeThreshold)
                    {
                        edge = true;
                    }
                }

                HalfEdge next = c.Next;

                DrawPen dpen = edge ? edgePen : pen;

                DrawLine(dpen,
                    model.VertexStore[c.Vertex].vector,
                    model.VertexStore[next.Vertex].vector
                    );

                c = next;

                if (c == head)
                {
                    break;
                }
            }
        }
    }

    public void DrawRect(DrawPen pen, vector3_t p0, vector3_t p1)
    {
        vector3_t pp0 = DC.WorldPointToDevPoint(p0);
        vector3_t pp1 = DC.WorldPointToDevPoint(p1);

        DrawRectangleScrn(pen, pp0.X, pp0.Y, pp1.X, pp1.Y);
    }

    public void DrawCross(DrawPen pen, vector3_t p, vcompo_t size)
    {
        vcompo_t hs = size;

        vector3_t px0 = p;
        px0.X -= hs;
        vector3_t px1 = p;
        px1.X += hs;

        vector3_t py0 = p;
        py0.Y -= hs;
        vector3_t py1 = p;
        py1.Y += hs;

        vector3_t pz0 = p;
        pz0.Z -= hs;
        vector3_t pz1 = p;
        pz1.Z += hs;

        DrawLine(pen, px0, px1);
        DrawLine(pen, py0, py1);
        DrawLine(pen, pz0, pz1);
    }

    public void DrawCrossScrn(DrawPen pen, vector3_t p, vcompo_t size)
    {
        DrawLineScrn(pen, p.X - size, p.Y + 0, p.X + size, p.Y + 0);
        DrawLineScrn(pen, p.X + 0, p.Y + size, p.X + 0, p.Y - size);
    }

    private void DrawXScrn(DrawPen pen, vector3_t p, vcompo_t size)
    {
        DrawLineScrn(pen, p.X - size, p.Y + size, p.X + size, p.Y - size);
        DrawLineScrn(pen, p.X - size, p.Y - size, p.X + size, p.Y + size);
    }


    public void DrawLine(DrawPen pen, vector3_t a, vector3_t b)
    {
        if (pen.GdiPen == null) return;

        vector3_t pa = DC.WorldPointToDevPoint(a);
        vector3_t pb = DC.WorldPointToDevPoint(b);

        DC.GdiGraphics.DrawLine(pen.GdiPen, (int)pa.X, (int)pa.Y, (int)pb.X, (int)pb.Y);
    }

    public virtual void DrawDot(DrawPen pen, vector3_t p)
    {
        vector3_t p0 = DC.WorldPointToDevPoint(p);
        vector3_t p1 = p0;
        p0.X = (int)p0.X;
        p1.X = p0.X + (vcompo_t)(0.1);

        DC.GdiGraphics.DrawLine(pen.GdiPen, (float)p0.X, (float)p0.Y, (float)p1.X, (float)p1.Y);
    }

    public void DrawText(int font, DrawBrush brush, vector3_t a, vector3_t xdir, vector3_t ydir, DrawTextOption opt, vcompo_t scale, string s)
    {
        vector3_t pa = DC.WorldPointToDevPoint(a);
        vector3_t d = DC.WorldVectorToDevVector(xdir);

        DrawTextScrn(font, brush, pa, d, opt, s);
    }

    private void DrawTextScrn(int font, DrawBrush brush, vector3_t a, vector3_t dir, DrawTextOption opt, string s)
    {
        if (brush.GdiBrush == null) return;
        if (DC.Font(font) == null) return;

        if (opt.Option != 0)
        {
            vector3_t sz = MeasureText(font, s);

            if ((opt.Option | DrawTextOption.H_CENTER) != 0)
            {
                vcompo_t slen = sz.X / 2;

                vector3_t ud = vector3_t.UnitX;

                if (!dir.IsZero())
                {
                    ud = dir.UnitVector();
                }

                a = a - (ud * slen);
            }
        }

        vcompo_t angle = 0;

        if (!(dir.X == 0 && dir.Y == 0))
        {
            angle = CadMath.Angle2D(dir);
        }

        angle = CadMath.Rad2Deg(angle);

        DC.GdiGraphics.TranslateTransform((int)a.X, (int)a.Y);

        DC.GdiGraphics.RotateTransform((float)angle);

        Font f = DC.Font(font);
        Brush b = brush.GdiBrush;

        DC.GdiGraphics.DrawString(s, f, b, 0, 0);

        DC.GdiGraphics.ResetTransform();
    }

    private vector3_t MeasureText(int font, string s)
    {
        if (DC.Font(font) == null)
        {
            return vector3_t.Zero;
        }

        SizeF size = DC.GdiGraphics.MeasureString(s, DC.Font(font));

        vector3_t v = new vector3_t(size.Width, size.Height, 0);

        return v;
    }

    public void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
    {
        DrawCrossCursorScrn(pp, pen, -1, -1);
    }

    public void DrawCrossCursorScrn(CadCursor pp, DrawPen pen, vcompo_t xsize, vcompo_t ysize)
    {
        if (xsize == -1)
        {
            xsize = DC.ViewWidth;
        }

        if (ysize == -1)
        {
            ysize = DC.ViewHeight;
        }


        vector3_t p0 = pp.Pos - (pp.DirX * xsize);
        vector3_t p1 = pp.Pos + (pp.DirX * xsize);

        DrawLineScrn(pen, p0.X, p0.Y, p1.X, p1.Y);

        p0 = pp.Pos - (pp.DirY * ysize);
        p1 = pp.Pos + (pp.DirY * ysize);

        DrawLineScrn(pen, p0.X, p0.Y, p1.X, p1.Y);
    }

    public void DrawRectScrn(DrawPen pen, vector3_t pp0, vector3_t pp1)
    {
        DrawRectangleScrn(pen, pp0.X, pp0.Y, pp1.X, pp1.Y);
    }

    protected void DrawLineScrn(DrawPen pen, vector3_t a, vector3_t b)
    {
        if (pen.GdiPen == null) return;

        DC.GdiGraphics.DrawLine(pen.GdiPen, (int)a.X, (int)a.Y, (int)b.X, (int)b.Y);
    }

    protected void DrawLineScrn(DrawPen pen, vcompo_t x1, vcompo_t y1, vcompo_t x2, vcompo_t y2)
    {
        if (pen.GdiPen == null) return;

        DC.GdiGraphics.DrawLine(pen.GdiPen, (int)x1, (int)y1, (int)x2, (int)y2);
    }

    protected void DrawRectangleScrn(DrawPen pen, vcompo_t x0, vcompo_t y0, vcompo_t x1, vcompo_t y1)
    {
        if (pen.GdiPen == null) return;

        int lx = (int)x0;
        int rx = (int)x1;

        int ty = (int)y0;
        int by = (int)y1;

        if (x0 > x1)
        {
            lx = (int)x1;
            rx = (int)x0;
        }

        if (y0 > y1)
        {
            ty = (int)y1;
            by = (int)y0;
        }

        int dx = rx - lx;
        int dy = by - ty;

        DC.GdiGraphics.DrawRectangle(pen.GdiPen, lx, ty, dx, dy);
    }

    protected void DrawCircleScrn(DrawPen pen, vector3_t cp, vector3_t p1)
    {
        vcompo_t r = CadMath.SegNorm(cp, p1);
        DrawCircleScrn(pen, cp, r);
    }

    protected void DrawCircleScrn(DrawPen pen, vector3_t cp, vcompo_t r)
    {
        if (pen.GdiPen == null) return;

        DC.GdiGraphics.DrawEllipse(
            pen.GdiPen, (int)(cp.X - r), (int)(cp.Y - r), (int)(r * 2), (int)(r * 2));
    }

    protected void FillRectangleScrn(DrawBrush brush, vcompo_t x0, vcompo_t y0, vcompo_t x1, vcompo_t y1)
    {
        if (brush.GdiBrush == null) return;

        int lx = (int)x0;
        int rx = (int)x1;

        int ty = (int)y0;
        int by = (int)y1;

        if (x0 > x1)
        {
            lx = (int)x1;
            rx = (int)x0;
        }

        if (y0 > y1)
        {
            ty = (int)y1;
            by = (int)y0;
        }

        int dx = rx - lx;
        int dy = by - ty;

        DC.GdiGraphics.FillRectangle(brush.GdiBrush, lx, ty, dx, dy);
    }

    public void Dispose()
    {
    }

    public void DrawBouncingBox(DrawPen pen, MinMax3D mm)
    {
        vector3_t p0 = new vector3_t(mm.Min.X, mm.Min.Y, mm.Min.Z);
        vector3_t p1 = new vector3_t(mm.Min.X, mm.Min.Y, mm.Max.Z);
        vector3_t p2 = new vector3_t(mm.Max.X, mm.Min.Y, mm.Max.Z);
        vector3_t p3 = new vector3_t(mm.Max.X, mm.Min.Y, mm.Min.Z);

        vector3_t p4 = new vector3_t(mm.Min.X, mm.Max.Y, mm.Min.Z);
        vector3_t p5 = new vector3_t(mm.Min.X, mm.Max.Y, mm.Max.Z);
        vector3_t p6 = new vector3_t(mm.Max.X, mm.Max.Y, mm.Max.Z);
        vector3_t p7 = new vector3_t(mm.Max.X, mm.Max.Y, mm.Min.Z);

        DC.Drawing.DrawLine(pen, p0, p1);
        DC.Drawing.DrawLine(pen, p1, p2);
        DC.Drawing.DrawLine(pen, p2, p3);
        DC.Drawing.DrawLine(pen, p3, p0);

        DC.Drawing.DrawLine(pen, p4, p5);
        DC.Drawing.DrawLine(pen, p5, p6);
        DC.Drawing.DrawLine(pen, p6, p7);
        DC.Drawing.DrawLine(pen, p7, p4);

        DC.Drawing.DrawLine(pen, p0, p4);
        DC.Drawing.DrawLine(pen, p1, p5);
        DC.Drawing.DrawLine(pen, p2, p6);
        DC.Drawing.DrawLine(pen, p3, p7);
    }

    public void DrawArrow(DrawPen pen, vector3_t pt0, vector3_t pt1, ArrowTypes type, ArrowPos pos, vcompo_t len, vcompo_t width)
    {
        DrawUtil.DrawArrow(this, pen, pt0, pt1, type, pos, len, width);
    }

    public void DrawExtSnapPoints(Vector3List pointList, DrawPen pen)
    {
        foreach (var v in pointList)
        {
            DrawHighlightPoint(v, pen);
        }
    }
}
