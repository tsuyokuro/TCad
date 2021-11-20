using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;

namespace Plotter
{
    /**
     * GDI向け描画クラス
     * Drawing class for GDI Bitmap
     */
    public class DrawingGDIBmp : DrawingGDI
    {
        public DrawContextGDIBmp BmpDC
        {
            get => (DrawContextGDIBmp)DC;
        }

        public DrawingGDIBmp(DrawContextGDIBmp dc)
        {
            DC = dc;
        }

        public override void DrawGrid(Gridding grid)
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


            Color c = DC.PenColor(DrawTools.PEN_GRID);

            int argb = c.ToArgb();

            double n = grid.Decimate(DC, grid, 8);

            double sx, sy, sz;
            double szx = grid.GridSize.X * n;
            double szy = grid.GridSize.Y * n;
            double szz = grid.GridSize.Z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

            DrawDots(sx, sy, sz, szx, szy, szz, maxx, maxy, maxz, argb);
        }

        private void DrawDots(
            double sx,
            double sy,
            double sz,
            double szx,
            double szy,
            double szz,
            double maxx,
            double maxy,
            double maxz,
            int argb
            )
        {
            double x;
            double y;
            double z;

            Vector3d p = default;
            Vector3d up = default;


            Bitmap tgt = BmpDC.Image;

            BitmapData bitmapData = BmpDC.LockBits();

            unsafe
            {
                int* srcPixels = (int*)bitmapData.Scan0;

                x = sx;
                while (x < maxx)
                {
                    p.X = x;
                    p.Z = 0;

                    y = sy;

                    while (y < maxy)
                    {
                        p.Y = y;
                        up = DC.WorldPointToDevPoint(p);

                        if (up.X >= 0 && up.X < tgt.Width && up.Y >= 0 && up.Y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.Y * tgt.Width) + (int)up.X) = argb;
                        }

                        y += szy;
                    }

                    x += szx;
                }

                z = sz;
                while (z < maxz)
                {
                    p.Z = z;
                    p.X = 0;

                    y = sy;

                    while (y < maxy)
                    {
                        p.Y = y;

                        up = DC.WorldPointToDevPoint(p);

                        if (up.X >= 0 && up.X < tgt.Width && up.Y >= 0 && up.Y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.Y * tgt.Width) + (int)up.X) = argb;
                        }

                        y += szy;
                    }

                    z += szz;
                }

                x = sx;
                while (x < maxx)
                {
                    p.X = x;
                    p.Y = 0;

                    z = sz;

                    while (z < maxz)
                    {
                        p.Z = z;

                        up = DC.WorldPointToDevPoint(p);

                        if (up.X >= 0 && up.X < tgt.Width && up.Y >= 0 && up.Y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.Y * tgt.Width) + (int)up.X) = argb;
                        }

                        z += szz;
                    }

                    x += szx;
                }
            }

            BmpDC.UnlockBits();
        }

        public override void DrawDot(DrawPen pen, Vector3d p)
        {
            Vector3d p0 = DC.WorldPointToDevPoint(p);

            if (p0.X >= 0 && p0.Y >= 0 && p0.X < DC.ViewWidth && p0.Y < DC.ViewHeight)
            {
                BmpDC.Image.SetPixel((int)p0.X, (int)p0.Y, pen.GdiPen.Color);
            }
        }
    }
}
