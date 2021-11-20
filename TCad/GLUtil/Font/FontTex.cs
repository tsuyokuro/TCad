using System;
using SharpFont;

namespace GLFont
{
    public class FontTex
    {
        public byte[] Data = null;
        public int W = 0;
        public int H = 0;

        public int ImgW = 0;
        public int ImgH = 0;

        public int PosX = 0;
        public int PosY = 0;

        public int FontW = 0;
        public int FontH = 0;

        public int Top = 0;

        public bool IsSpace
        {
            get => Data == null;
        }

        public FontTex() { }

        public FontTex(int imgW, int imgH)
        {
            ImgW = imgW;
            ImgH = imgH;

            W = ((imgW + 3) / 4) * 4;
            H = imgH;

            Data = new byte[W * H];
        }

        public void Set(int x, int y, byte v)
        {
            int i = ((H - 1 - y) * W) + x;
            Data[i] = v;
        }

        public static FontTex CreateSpace(int w, int h)
        {
            FontTex ft = new FontTex();
            ft.ImgW = w;
            ft.ImgH = h;
            return ft;
        }

        public static FontTex Merge(FontTex[] ta)
        {
            int fw = 0;
            int fh = 0;

            foreach (FontTex f in ta)
            {
                fw += f.ImgW;
                if (f.ImgH > fh)
                {
                    fh = f.ImgH;
                }
            }

            FontTex ft = new FontTex(fw, fh);

            int fx = 0;
            int fy = 0;

            foreach (FontTex f in ta)
            {
                ft.Paste(fx, fy, f);
                fx += f.ImgW;
            }

            return ft;
        }

        public void Paste(int x, int y, FontTex src)
        {
            if (src.IsSpace)
            {
                return;
            }

            int sx = 0;
            int sy = 0;
            int sw = src.ImgW;
            int sh = src.ImgH;

            if (x < 0)
            {
                sx += -x;
                x = 0;
            }

            if (x + sw > W)
            {
                sw = W - x;
            }

            if (y < 0)
            {
                sy += -y;
                y = 0;
            }

            if (y + sh > H)
            {
                sh = H - y;
            }

            int dx = x;
            int dy = y;

            int cx = sx;

            for (; sy < sh; sy++)
            {
                dx = x;
                sx = cx;
                for (; sx < sw; sx++)
                {
                    int si = ((src.H - 1 - sy) * src.W) + sx;
                    int di = ((H - 1 - dy) * W) + dx;

                    Data[di] = src.Data[si];

                    dx++;
                }
                dy++;
            }
        }

        public static unsafe FontTex Create(FTBitmap ftb)
        {
            byte* buffer = (byte*)ftb.Buffer;
            int sw = ftb.Width;
            int sh = ftb.Rows;

            FontTex ft = new FontTex(ftb.Width, ftb.Rows);
            int dw = ft.W;
            int dh = ft.H;

            byte[] data = ft.Data;

            int si;
            int di;

            int dy = 0;

            if (ftb.GrayLevels == 2)
            {
                int sbw = ((sw + 7) / 8) * 8;

                for (int sy = sh - 1; sy >= 0;)
                {
                    si = sy * sbw;
                    di = dy * dw;

                    for (int x = 0; x < ftb.Width; x++)
                    {
                        data[di + x] = (byte)(BitUtil.GetAt(buffer, si + x) * 255);
                    }

                    sy--;
                    dy++;
                }
            }
            else
            {
                for (int sy = sh - 1; sy >= 0;)
                {
                    si = sy * sw;
                    di = dy * dw;

                    for (int x = 0; x < sw; x++)
                    {
                        data[di + x] = buffer[si + x];
                    }

                    sy--;
                    dy++;
                }
            }

            return ft;
        }

        public void dump()
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    byte v = Data[i];
                    Console.Write(v.ToString("x2") + " ");
                }
                Console.WriteLine();
            }
        }

        public void dump_b()
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    byte v = Data[i];
                    if (v != 0)
                    {
                        Console.Write("@");
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
        }

        public unsafe struct BitUtil
        {
            public static int GetAt(byte* p, int idx)
            {
                int di = idx / 8;
                int bp = idx - (di * 8);

                return (p[di] >> (7 - bp)) & 0x01;
            }

            public static int GetAt(byte[] p, int idx)
            {
                int di = idx / 8;
                int bp = idx - (di * 8);

                return (p[di] >> (7 - bp)) & 0x01;
            }
        }
    }
}
