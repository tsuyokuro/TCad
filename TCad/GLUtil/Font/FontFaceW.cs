//#define DEFAULT_DATA_TYPE_DOUBLE
using GLUtil;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Resources;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace GLFont;

public class FontFaceW
{
    private Library mLib;

    private Face FontFace;

    private float Size;

    private Dictionary<char, FontTex> TextureCache = new();

    private Dictionary<char, FontPoly> PolyCache = new();

    private FontFaceW()
    {
        mLib = new Library();
        Size = 8.25f;
    }

    private void SetFont(string filename, int face_index = 0)
    {
        FontFace = new Face(mLib, filename, face_index);
        SetSize(this.Size);

        TextureCache.Clear();
    }

    private void SetFont(byte[] data, int face_index = 0)
    {
        FontFace = new Face(mLib, data, face_index);
        SetSize(this.Size);

        TextureCache.Clear();
    }

    // url e.g. "/Fonts/mplus-1m-thin.ttf"
    private void SetResourceFont(string url, int face_index = 0)
    {
        Uri fileUri = new Uri(url, UriKind.Relative);
        StreamResourceInfo info = Application.GetResourceStream(fileUri);
        Stream stream = info.Stream;

        long len = stream.Length;

        byte[] data = new byte[len];

        int read = stream.Read(data, 0, (int)len);

        SetFont(data, face_index);
    }

    private void SetSize(float size)
    {
        Size = size;
        if (FontFace != null)
        {
            FontFace.SetCharSize(0, size, 0, 96);
        }

        TextureCache.Clear();
    }

    public FontPoly CreatePoly(char c)
    {
        FontPoly fp;

        if (PolyCache.TryGetValue(c, out fp))
        {
            return new(fp);
        }

        uint glyphIndex = FontFace.GetCharIndex(c);
        FontFace.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

        Tessellator tesse = new();

        fp = FontTessellator.Tessellate(FontFace.Glyph, 3, tesse);

        tesse?.Dispose();

        PolyCache.Add(c, fp);

        return new(fp);
    }

    public GlyphSlot GetGlyph(char c)
    {
        uint glyphIndex = FontFace.GetCharIndex(c);
        FontFace.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
        return FontFace.Glyph;
    }

    public FontTex CreateTexture(char c, bool attachTexture = false)
    {
        FontTex ft;

        if (TextureCache.TryGetValue(c, out ft))
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

            if (attachTexture)
            {
                AttachTexture(ft);
            }
        }
        else
        {
            ft = FontTex.CreateSpace((int)FontFace.Glyph.Advance.X, (int)FontFace.Glyph.Advance.Y);
            ft.FontW = fontW;
            ft.FontH = fontH;
        }

        TextureCache.Add(c, ft);

        //ft.dump_b();
        //Console.WriteLine();

        return ft;
    }

    public FontTex CreateTexture(string s)
    {
        if (s.Length == 1)
        {
            return CreateTexture(s[0]);
        }

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

    private void AttachTexture(in FontTex fontTex)
    {
        int texUnitNumber = 0;

        fontTex.TextureID = TextureProvider.Instance.GetNew();

        GL.ActiveTexture(TextureUnit.Texture0 + texUnitNumber);

        GL.BindTexture(TextureTarget.Texture2D, fontTex.TextureID);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexImage2D(
            TextureTarget.Texture2D, 0,
            PixelInternalFormat.Alpha8,
            fontTex.W, fontTex.H, 0,
            PixelFormat.Alpha,
            PixelType.UnsignedByte, fontTex.Data);
    }


    public class Provider
    {
        private static Dictionary<string, FontFaceW> FaceMap = new Dictionary<string, FontFaceW>();

        public static FontFaceW GetFromFile(string fname, float size, int faceIndex)
        {
            string key = GetKey(fname, size, faceIndex);

            FontFaceW face;

            if (FaceMap.TryGetValue(key, out face)) {
                return face;
            }

            face = new FontFaceW();
            face.SetFont(fname, faceIndex);
            face.SetSize(size);

            FaceMap.Add(key, face);

            return face;
        }

        public static FontFaceW GetFromResource(string uri, float size, int faceIndex)
        {
            string key = GetKey(uri, size, faceIndex);

            FontFaceW face;

            if (FaceMap.TryGetValue(key, out face))
            {
                return face;
            }

            face = new FontFaceW();
            face.SetResourceFont(uri, faceIndex);
            face.SetSize(size);

            FaceMap.Add(key, face);

            return face;
        }

        public static void Dispose()
        {
            FaceMap.Clear();
        }

        private static string GetKey(string name, float size, int faceIndex)
        {
            return name + "_" + size.ToString() + "_" + faceIndex.ToString();
        }
    }
}
