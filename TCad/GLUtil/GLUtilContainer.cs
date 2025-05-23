using GLFont;
using GLUtil;
using TCad.Util;


class GLUtilContainer
{
    public static IServiceProvider<TextureProvider> TextureProvider { get; private set; }

    public static IServiceProvider<ImageShader> ImageShader { get; private set; }

    public static IServiceProvider<ImageRenderer> ImageRenderer { get; private set; }


    public static IServiceProvider<FontFaceProvider> FontFaceProvider { get; private set; }

    public static IServiceProvider<FontShader> FontShader { get; private set; }

    public static IServiceProvider<FontRenderer> FontRenderer { get; private set; }


    public static IServiceProvider<WireFrameShader> WireFrameShader { get; private set; }


    static GLUtilContainer()
    {
        CreateServices();
    }

    static void CreateServices()
    {
        TextureProvider = new SingleServiceProvider<TextureProvider>(() => new TextureProvider());
        var textureProvider = TextureProvider.Get();


        ImageShader = new SingleServiceProvider<ImageShader>(() => new ImageShader());
        var imageShader = ImageShader.Get();

        ImageRenderer = new SingleServiceProvider<ImageRenderer>(() =>
            new ImageRenderer(imageShader, textureProvider)
        );


        FontFaceProvider = new SingleServiceProvider<FontFaceProvider>(() =>
            new FontFaceProvider()
        );
        var fontFaceProvider = FontFaceProvider.Get();


        FontShader = new SingleServiceProvider<FontShader>(() =>
            new FontShader());
        var fontShader = FontShader.Get();

        FontRenderer = new SingleServiceProvider<FontRenderer>(() =>
            new FontRenderer(fontShader, textureProvider)
        );


        WireFrameShader = new SingleServiceProvider<WireFrameShader>(() =>
            new WireFrameShader()
        );
    }


    public static void DisposeServices()
    {
        FontRenderer.Get().Dispose();
        FontShader.Get().Dispose();
        FontFaceProvider.Get().Dispose();

        ImageRenderer.Get().Dispose();
        ImageShader.Get().Dispose();

        WireFrameShader.Get().Dispose();

        TextureProvider.Get().RemoveAll();


        FontRenderer = null;
        FontShader = null;
        FontFaceProvider = null;

        ImageRenderer = null;
        ImageShader = null;

        WireFrameShader = null;

        TextureProvider = null;
    }
}
