using HalfEdgeNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using CadDataTypes;

namespace Plotter
{
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

        public void DrawSelected(List<CadFigure> list)
        {
            foreach (CadFigure fig in list)
            {
                fig.DrawSelectedEach(DC);
            }
        }

        #region "Draw base"
        public void DrawAxis()
        {
            Vector3d p0 = default;
            Vector3d p1 = default;

            double len = DrawingConst.AxisLength;

            // X軸
            p0.X = -len;
            p0.Y = 0;
            p0.Z = 0;

            p1.X = len;
            p1.Y = 0;
            p1.Z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DC.GetPen(DrawTools.PEN_AXIS_X), p0, p1);

            // Y軸
            p0.X = 0;
            p0.Y = -len;
            p0.Z = 0;

            p1.X = 0;
            p1.Y = len;
            p1.Z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DC.GetPen(DrawTools.PEN_AXIS_Y), p0, p1);

            // Z軸
            p0.X = 0;
            p0.Y = 0;
            p0.Z = -len;

            p1.X = 0;
            p1.Y = 0;
            p1.Z = len;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

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
            Vector3d lt = Vector3d.Zero;
            Vector3d rb = new Vector3d(DC.ViewWidth, DC.ViewHeight, 0);

            Vector3d ltw = DC.DevPointToWorldPoint(lt);
            Vector3d rbw = DC.DevPointToWorldPoint(rb);

            double minx = Math.Min(ltw.X, rbw.X);
            double maxx = Math.Max(ltw.X, rbw.X);

            double miny = Math.Min(ltw.Y, rbw.Y);
            double maxy = Math.Max(ltw.Y, rbw.Y);

            double minz = Math.Min(ltw.Z, rbw.Z);
            double maxz = Math.Max(ltw.Z, rbw.Z);


            DrawPen pen = DC.GetPen(DrawTools.PEN_GRID);

            Vector3d p = default(Vector3d);


            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.X * n;
            double szy = grid.GridSize.Y * n;
            double szz = grid.GridSize.Z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

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

        public void DrawPageFrame(double w, double h, Vector3d center)
        {
            Vector3d pt = default(Vector3d);

            // p0
            pt.X = -w / 2 + center.X;
            pt.Y = h / 2 + center.Y;
            pt.Z = 0;

            Vector3d p0 = default(Vector3d);
            p0.X = pt.X * DC.UnitPerMilli;
            p0.Y = pt.Y * DC.UnitPerMilli;

            p0 += DC.ViewOrg;

            // p1
            pt.X = w / 2 + center.X;
            pt.Y = -h / 2 + center.Y;
            pt.Z = 0;

            Vector3d p1 = default(Vector3d);
            p1.X = pt.X * DC.UnitPerMilli;
            p1.Y = pt.Y * DC.UnitPerMilli;

            p1 += DC.ViewOrg;

            DrawRectScrn(DC.GetPen(DrawTools.PEN_PAGE_FRAME), p0, p1);
        }
        #endregion

        #region "Draw marker"
        public void DrawHighlightPoint(Vector3d pt, DrawPen pen)
        {
            Vector3d pp = DC.WorldPointToDevPoint(pt);

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

        public void DrawSelectedPoint(Vector3d pt, DrawPen pen)
        {
            Vector3d pp = DC.WorldPointToDevPoint(pt);

            int size = 2;

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

        public void DrawMarkCursor(DrawPen pen, Vector3d p, double pix_size)
        {
            DrawCross(pen, p, pix_size);
        }
        #endregion

        public void DrawHarfEdgeModel(
            DrawBrush brush, DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
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
                        double s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                        if (Math.Abs(s) < edgeThreshold)
                        {
                            edge = true;
                        }
                    }

                    HalfEdge next = c.Next;

                    DrawPen dpen = edge ? edgePen : pen;

                    DrawLine(dpen,
                        model.VertexStore.Ref(c.Vertex).vector,
                        model.VertexStore.Ref(next.Vertex).vector
                        );

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public void DrawRect(DrawPen pen, Vector3d p0, Vector3d p1)
        {
            Vector3d pp0 = DC.WorldPointToDevPoint(p0);
            Vector3d pp1 = DC.WorldPointToDevPoint(p1);

            DrawRectangleScrn(pen, pp0.X, pp0.Y, pp1.X, pp1.Y);
        }

        public void DrawCross(DrawPen pen, Vector3d p, double size)
        {
            double hs = size;

            Vector3d px0 = p;
            px0.X -= hs;
            Vector3d px1 = p;
            px1.X += hs;

            Vector3d py0 = p;
            py0.Y -= hs;
            Vector3d py1 = p;
            py1.Y += hs;

            Vector3d pz0 = p;
            pz0.Z -= hs;
            Vector3d pz1 = p;
            pz1.Z += hs;

            DrawLine(pen, px0, px1);
            DrawLine(pen, py0, py1);
            DrawLine(pen, pz0, pz1);
        }

        public void DrawCrossScrn(DrawPen pen, Vector3d p, double size)
        {
            DrawLineScrn(pen, p.X - size, p.Y + 0, p.X + size, p.Y + 0);
            DrawLineScrn(pen, p.X + 0, p.Y + size, p.X + 0, p.Y - size);
        }

        private void DrawXScrn(DrawPen pen, Vector3d p, double size)
        {
            DrawLineScrn(pen, p.X - size, p.Y + size, p.X + size, p.Y - size);
            DrawLineScrn(pen, p.X - size, p.Y - size, p.X + size, p.Y + size);
        }


        public void DrawLine(DrawPen pen, Vector3d a, Vector3d b)
        {
            if (pen.GdiPen == null) return;

            Vector3d pa = DC.WorldPointToDevPoint(a);
            Vector3d pb = DC.WorldPointToDevPoint(b);

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)pa.X, (int)pa.Y, (int)pb.X, (int)pb.Y);
        }

        public virtual void DrawDot(DrawPen pen, Vector3d p)
        {
            Vector3d p0 = DC.WorldPointToDevPoint(p);
            Vector3d p1 = p0;
            p0.X = (int)p0.X;
            p1.X = p0.X + 0.1;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (float)p0.X, (float)p0.Y, (float)p1.X, (float)p1.Y);
        }

        public void DrawText(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, string s)
        {
            Vector3d pa = DC.WorldPointToDevPoint(a);
            Vector3d d = DC.WorldVectorToDevVector(xdir);

            DrawTextScrn(font, brush, pa, d, opt, s);
        }

        private void DrawTextScrn(int font, DrawBrush brush, Vector3d a, Vector3d dir, DrawTextOption opt, string s)
        {
            if (brush.GdiBrush == null) return;
            if (DC.Font(font) == null) return;

            if (opt.Option != 0)
            {
                Vector3d sz = MeasureText(font, s);

                if ((opt.Option | DrawTextOption.H_CENTER) != 0)
                {
                    double slen = sz.X / 2;

                    Vector3d ud = Vector3d.UnitX;

                    if (!dir.IsZero())
                    {
                        ud = dir.UnitVector();
                    }

                    a = a - (ud * slen);
                }
            }

            double angle = 0;

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

        private Vector3d MeasureText(int font, string s)
        {
            if (DC.Font(font) == null)
            {
                return Vector3d.Zero;
            }

            SizeF size = DC.GdiGraphics.MeasureString(s, DC.Font(font));

            Vector3d v = new Vector3d(size.Width, size.Height, 0);

            return v;
        }

        public void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
        {
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            Vector3d p0 = pp.Pos - (pp.DirX * size);
            Vector3d p1 = pp.Pos + (pp.DirX * size);

            DrawLineScrn(pen, p0.X, p0.Y, p1.X, p1.Y);

            p0 = pp.Pos - (pp.DirY * size);
            p1 = pp.Pos + (pp.DirY * size);

            DrawLineScrn(pen, p0.X, p0.Y, p1.X, p1.Y);
        }

        public void DrawRectScrn(DrawPen pen, Vector3d pp0, Vector3d pp1)
        {
            DrawRectangleScrn(pen, pp0.X, pp0.Y, pp1.X, pp1.Y);
        }

        protected void DrawLineScrn(DrawPen pen, Vector3d a, Vector3d b)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)a.X, (int)a.Y, (int)b.X, (int)b.Y);
        }

        protected void DrawLineScrn(DrawPen pen, double x1, double y1, double x2, double y2)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)x1, (int)y1, (int)x2, (int)y2);
        }

        protected void DrawRectangleScrn(DrawPen pen, double x0, double y0, double x1, double y1)
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

        protected void DrawCircleScrn(DrawPen pen, Vector3d cp, Vector3d p1)
        {
            double r = CadMath.SegNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        protected void DrawCircleScrn(DrawPen pen, Vector3d cp, double r)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawEllipse(
                pen.GdiPen, (int)(cp.X - r), (int)(cp.Y - r), (int)(r * 2), (int)(r * 2));
        }

        protected void FillRectangleScrn(DrawBrush brush, double x0, double y0, double x1, double y1)
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
            Vector3d p0 = new Vector3d(mm.Min.X, mm.Min.Y, mm.Min.Z);
            Vector3d p1 = new Vector3d(mm.Min.X, mm.Min.Y, mm.Max.Z);
            Vector3d p2 = new Vector3d(mm.Max.X, mm.Min.Y, mm.Max.Z);
            Vector3d p3 = new Vector3d(mm.Max.X, mm.Min.Y, mm.Min.Z);

            Vector3d p4 = new Vector3d(mm.Min.X, mm.Max.Y, mm.Min.Z);
            Vector3d p5 = new Vector3d(mm.Min.X, mm.Max.Y, mm.Max.Z);
            Vector3d p6 = new Vector3d(mm.Max.X, mm.Max.Y, mm.Max.Z);
            Vector3d p7 = new Vector3d(mm.Max.X, mm.Max.Y, mm.Min.Z);

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

        public void DrawArrow(DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width)
        {
            DrawUtil.DrawArrow(DrawLine, pen, pt0, pt1, type, pos, len / DC.WorldScale, width / DC.WorldScale);
        }

        public void DrawExtSnapPoints(Vector3dList pointList, DrawPen pen)
        {
            pointList.ForEach(v =>
            {
                DrawHighlightPoint(v, pen);
            });
        }
    }
}
