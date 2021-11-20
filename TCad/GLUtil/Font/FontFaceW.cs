using System;
using System.Collections.Generic;
using SharpFont;
using System.Windows.Resources;
using System.Windows;
using System.IO;

namespace GLFont
{
    public class FontFaceW
    {
        private Library mLib;

        private Face FontFace;

        private float Size;

        private Dictionary<char, FontTex> Cache = new Dictionary<char, FontTex>();

        public FontFaceW()
        {
            mLib = new Library();
            Size = 8.25f;
        }

        public void SetFont(string filename, int face_index = 0)
        {
            FontFace = new Face(mLib, filename, face_index);
            SetSize(this.Size);

            Cache.Clear();
        }

        public void SetFont(byte[] data, int face_index = 0)
        {
            FontFace = new Face(mLib, data, face_index);
            SetSize(this.Size);

            Cache.Clear();
        }

        // url e.g. "/Fonts/mplus-1m-thin.ttf"
        public void SetResourceFont(string url, int face_index = 0)
        {
            Uri fileUri = new Uri(url, UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(fileUri);
            Stream stream = info.Stream;

            long len = stream.Length;

            byte[] data = new byte[len];

            int read = stream.Read(data, 0, (int)len);

            SetFont(data, face_index);
        }

        public void SetSize(float size)
        {
            Size = size;
            if (FontFace != null)
            {
                FontFace.SetCharSize(0, size, 0, 96);
            }

            Cache.Clear();
        }

        public FontTex CreateTexture(char c)
        {
            FontTex ft;

            if (Cache.TryGetValue(c, out ft))
            {
                return ft;
            }

            uint glyphIndex = FontFace.GetCharIndex(c);
            FontFace.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
            FontFace.Glyph.RenderGlyph(RenderMode.Light);
            FTBitmap ftbmp = FontFace.Glyph.Bitmap;

            int fontW = (int)((float)FontFace.Glyph.Metrics.HorizontalAdvance);
            int fontH = (int)((float)FontFace.Glyph.Metrics.VerticalAdvance);

            if (ftbmp.Width > 0 && ftbmp.Rows > 0)
            {
                ft = FontTex.Create(ftbmp);

                ft.PosX = (int)((float)FontFace.Glyph.Metrics.HorizontalBearingX);
                if (ft.PosX < 0)
                {
                    ft.PosX = 0;
                };

                float top = (float)FontFace.Size.Metrics.Ascender;
                float bottom = (float)(FontFace.Glyph.Metrics.Height - FontFace.Glyph.Metrics.HorizontalBearingY);

                int y = (int)(top - (float)FontFace.Glyph.Metrics.HorizontalBearingY);

                ft.PosY = y;

                ft.FontW = Math.Max(fontW, ft.ImgW);
                ft.FontH = (int)(top + bottom);
            }
            else
            {
                ft = FontTex.CreateSpace((int)FontFace.Glyph.Advance.X, (int)FontFace.Glyph.Advance.Y);
                ft.FontW = fontW;
                ft.FontH = fontH;
            }

            Cache.Add(c, ft);

            //ft.dump_b();
            //Console.WriteLine();

            return ft;
        }

        public FontTex CreateTexture(string s)
        {
            List<FontTex> ta = new List<FontTex>();

            int fw = 0;
            int fh = 0;

            foreach (char c in s)
            {
                FontTex ft = CreateTexture(c);

                fw += ft.FontW;
                if (ft.FontH > fh)
                {
                    fh = ft.FontH;
                }

                ta.Add(ft);
            }

            FontTex mft = new FontTex(fw, fh);

            int x = 0;
            int y = 0;

            foreach (FontTex ft in ta)
            {
                mft.Paste(x + ft.PosX, y + ft.PosY, ft);
                x += ft.FontW;
            }

            //mft.dump_b();
            //Console.WriteLine("");

            return mft;
        }
    }
}
