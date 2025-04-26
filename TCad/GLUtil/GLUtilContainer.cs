using GLFont;
using GLUtil;

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
        var textureProvider = TextureProvider.Instance;


        ImageShader = new SingleServiceProvider<ImageShader>(() => new ImageShader());
        var imageShader = ImageShader.Instance;

        ImageRenderer = new SingleServiceProvider<ImageRenderer>(() =>
            new ImageRenderer(imageShader, textureProvider)
        );


        FontFaceProvider = new SingleServiceProvider<FontFaceProvider>(() =>
            new FontFaceProvider()
        );
        var fontFaceProvider = FontFaceProvider.Instance;


        FontShader = new SingleServiceProvider<FontShader>(() =>
            new FontShader());
        var fontShader = FontShader.Instance;

        FontRenderer = new SingleServiceProvider<FontRenderer>(() =>
            new FontRenderer(fontShader, textureProvider)
        );


        WireFrameShader = new SingleServiceProvider<WireFrameShader>(() =>
            new WireFrameShader()
        );
    }


    public static void DisposeServices()
    {
        FontRenderer.Instance.Dispose();
        FontShader.Instance.Dispose();
        FontFaceProvider.Instance.Dispose();

        ImageRenderer.Instance.Dispose();
        ImageShader.Instance.Dispose();

        WireFrameShader.Instance.Dispose();

        TextureProvider.Instance.RemoveAll();


        FontRenderer = null;
        FontShader = null;
        FontFaceProvider = null;

        ImageRenderer = null;
        ImageShader = null;

        WireFrameShader = null;

        TextureProvider = null;
    }
}
