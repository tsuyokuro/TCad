using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CadDataTypes;

namespace Plotter
{
    public class BitmapUtil
    {
        public static void BitmapToClipboardAsPNG(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();

            bmp.Save(ms, ImageFormat.Png);

            IDataObject dataObject = new DataObject();

            dataObject.SetData("PNG", false, ms);

            Clipboard.SetDataObject(dataObject);
        }


        public static Bitmap CreateAABitmap2x2(Bitmap src, Color color)
        {
            int dw = (int)src.Width / 2;
            int dh = (int)src.Height / 2;

            Bitmap dest = new Bitmap(dw, dh);

            BitmapData dstBits = dest.LockBits(
                    new System.Drawing.Rectangle(0, 0, dest.Width, dest.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, dest.PixelFormat);

            BitmapData srcBits = src.LockBits(
                    new System.Drawing.Rectangle(0, 0, src.Width, src.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, src.PixelFormat);

            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            unsafe
            {
                byte* s0;
                byte* s1;
                byte* s2;
                byte* s3;
                int spcnt = src.Width;
                int spcnt2 = spcnt * 2;

                uint* srcPixels = (uint*)srcBits.Scan0;
                uint* dstPixels = (uint*)dstBits.Scan0;


                int x;
                int y = 0;

                uint* psrcLine = srcPixels;
                uint* pdstLine = dstPixels;

                byte* dst;
                int dpcnt = dest.Width;

                for (; y < dh; y++)
                {
                    x = 0;
                    int x2 = 0;
                    for (; x < dw; x++)
                    {
                        s0 = (byte*)(psrcLine + x2 + 0);
                        s1 = (byte*)(psrcLine + x2 + 1);
                        s2 = (byte*)(psrcLine + x2 + spcnt + 0);
                        s3 = (byte*)(psrcLine + x2 + spcnt + 1);

                        int a = (int)(s0[3] + s1[3] + s2[3] + s3[3]) / 4;

                        if (a != 0)
                        {
                            dst = (byte*)(pdstLine + x);
                            dst[0] = b;
                            dst[1] = g;
                            dst[2] = r;
                            dst[3] = (byte)a;
                        }

                        x2 += 2;
                    }

                    psrcLine += spcnt2;
                    pdstLine += dpcnt;
                }
            }

            return dest;
        }

        public static void BresenhamLine(BitmapData bitmapData, CadVertex p0, CadVertex p1, uint color)
        {
            int x0 = (int)p0.X;
            int y0 = (int)p0.Y;

            int x1 = (int)p1.X;
            int y1 = (int)p1.Y;

            int stepx;
            int stepy;

            int dx;
            int dy;

            int a;
            int a1;
            int e;

            if (x0 < x1)
            {
                dx = x1 - x0;
                stepx = 1;
            }
            else
            {
                dx = x0 - x1;
                stepx = -1;
            }

            if (y0 < y1)
            {
                dy = y1 - y0;
                stepy = 1;
            }
            else
            {
                dy = y0 - y1;
                stepy = -1;
            }

            int x = x0;
            int y = y0;

            int wc = bitmapData.Width;

            unsafe
            {
                uint* p = (uint*)bitmapData.Scan0;

                if (dx > dy)
                {
                    a = 2 * dy;
                    a1 = a - (2 * dx);
                    e = a - dx;

                    while (true)
                    {
                        *(p + (y * wc) + x) = color;

                        if (x == x1) break;

                        if (e >= 0)
                        {
                            y += stepy;
                            e += a1;
                        }
                        else
                        {
                            e += a;
                        }

                        x += stepx;
                    }
                }
                else
                {
                    a = 2 * dx;
                    a1 = a - (2 * dy);
                    e = a - dy;

                    while (true)
                    {
                        *(p + (y * wc) + x) = color;

                        if (y == y1) break;

                        if (e >= 0)
                        {
                            x += stepx;
                            e += a1;
                        }
                        else
                        {
                            e += a;
                        }

                        y += stepy;
                    }
                }
            }
        }
    }
}
