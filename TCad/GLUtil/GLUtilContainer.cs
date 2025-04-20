using GLFont;
using GLUtil;

class GLUtilContainer
{
    public static IServiceProvider<TextureProvider> TextureProvider {
        get; private set;
    }

    public static IServiceProvider<ImageShader> ImageShader
    {
        get; private set;
    }

    public static IServiceProvider<ImageRenderer> ImageRenderer
    {
        get; private set;
    }

    public static IServiceProvider<WireFrameShader> WireFrameShader
    {
        get; private set;
    }

    public static IServiceProvider<FontFaceProvider> FontFaceProvider
    {
        get; private set;
    }

    public static IServiceProvider<FontShader> FontShader
    {
        get; private set;
    }

    public static IServiceProvider<FontRenderer> FontRenderer
    {
        get; private set;
    }

    static GLUtilContainer()
    {
        TextureProvider = new SingleServiceProvider<TextureProvider>(() =>
        {
            return new TextureProvider();
        });

        ImageRenderer = new SingleServiceProvider<ImageRenderer>(() => {
            return new ImageRenderer();
        });

        ImageShader = new SingleServiceProvider<ImageShader>(() => {
            return new ImageShader();
        });

        WireFrameShader = new SingleServiceProvider<WireFrameShader>(() => {
            return new WireFrameShader();
        });

        FontFaceProvider = new SingleServiceProvider<FontFaceProvider>(() => {
            return new FontFaceProvider();
        });

        FontShader = new SingleServiceProvider<FontShader>(() => {
            return new FontShader();
        });

        FontRenderer = new SingleServiceProvider<FontRenderer>(() => {
            return new FontRenderer();
        });
    }
}
