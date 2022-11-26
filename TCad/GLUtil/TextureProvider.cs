using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GLUtil;

public class TextureProvider
{
    private static TextureProvider sInstance;

    private List<int> mTextures = new List<int>();

    public static TextureProvider Instance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            if (sInstance == null)
            {
                sInstance = new TextureProvider();
            }

            return sInstance;
        }
    }

    public int GetNew()
    {
        int textureID = GL.GenTexture();
        mTextures.Add(textureID);
        return textureID;
    }

    public void RemoveAll()
    {
        for (int i=0;i<mTextures.Count;i++)
        {
            GL.DeleteTexture(mTextures[i]);
        }
        mTextures.Clear();
    }

    public void Remove(int name)
    {
        if (mTextures.Contains(name))
        {
            GL.DeleteTexture(name);
            mTextures.Remove(name);
        }
    }
}
