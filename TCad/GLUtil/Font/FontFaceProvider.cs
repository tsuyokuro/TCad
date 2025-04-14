using System.Collections.Generic;

namespace GLFont;

// FontFaceWのキャッシュを管理するクラス
public class FontFaceProvider
{
    private Dictionary<string, FontFaceW> FaceMap = new Dictionary<string, FontFaceW>();

    private static FontFaceProvider Instance_;

    public static FontFaceProvider Instance {
        get
        {
            if (Instance_ == null)
            {
                Instance_ = new FontFaceProvider();
            }
            return Instance_;
        }
    }

    public FontFaceW FromFile(string fname, float size, int faceIndex)
    {
        string key = GetKey(fname, size, faceIndex);

        FontFaceW face;

        if (FaceMap.TryGetValue(key, out face)) {
            return face;
        }

        face = FontFaceW.FromFile(fname, size, faceIndex);

        FaceMap.Add(key, face);

        return face;
    }

    public FontFaceW FromResource(string uri, float size, int faceIndex)
    {
        string key = GetKey(uri, size, faceIndex);

        FontFaceW face;

        if (FaceMap.TryGetValue(key, out face))
        {
            return face;
        }

        face = FontFaceW.FromResource(uri, size, faceIndex);

        FaceMap.Add(key, face);

        return face;
    }

    public void Dispose()
    {
        FaceMap.Clear();
    }

    private string GetKey(string name, float size, int faceIndex)
    {
        return name + "_" + size.ToString() + "_" + faceIndex.ToString();
    }
}
