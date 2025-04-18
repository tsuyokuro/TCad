using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GLUtil;

public class TextureProvider
{
    private readonly List<int> mTextures = new();

    private static TextureProvider sInstance;

    public static TextureProvider Instance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            sInstance ??= new TextureProvider();
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
        for (int i = 0; i < mTextures.Count; i++)
        {
            GL.DeleteTexture(mTextures[i]);
        }
        mTextures.Clear();
    }

    public void Remove(int textureID)
    {
        if (mTextures.Contains(textureID))
        {
            GL.DeleteTexture(textureID);
            mTextures.Remove(textureID);
        }
    }
}
