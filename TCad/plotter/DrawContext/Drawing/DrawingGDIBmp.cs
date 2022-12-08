using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Drawing.Imaging;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter;

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


        Color c = DC.PenColor(DrawTools.PEN_GRID);

        int argb = c.ToArgb();

        vcompo_t n = grid.Decimate(DC, grid, 8);

        vcompo_t sx, sy, sz;
        vcompo_t szx = grid.GridSize.X * n;
        vcompo_t szy = grid.GridSize.Y * n;
        vcompo_t szz = grid.GridSize.Z * n;

        sx = (vcompo_t)Math.Round(minx / szx) * szx;
        sy = (vcompo_t)Math.Round(miny / szy) * szy;
        sz = (vcompo_t)Math.Round(minz / szz) * szz;

        DrawDots(sx, sy, sz, szx, szy, szz, maxx, maxy, maxz, argb);
    }

    private void DrawDots(
        vcompo_t sx,
        vcompo_t sy,
        vcompo_t sz,
        vcompo_t szx,
        vcompo_t szy,
        vcompo_t szz,
        vcompo_t maxx,
        vcompo_t maxy,
        vcompo_t maxz,
        int argb
        )
    {
        vcompo_t x;
        vcompo_t y;
        vcompo_t z;

        vector3_t p = default;
        vector3_t up = default;


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

    public override void DrawDot(DrawPen pen, vector3_t p)
    {
        vector3_t p0 = DC.WorldPointToDevPoint(p);

        if (p0.X >= 0 && p0.Y >= 0 && p0.X < DC.ViewWidth && p0.Y < DC.ViewHeight)
        {
            BmpDC.Image.SetPixel((int)p0.X, (int)p0.Y, pen.GdiPen.Color);
        }
    }
}
