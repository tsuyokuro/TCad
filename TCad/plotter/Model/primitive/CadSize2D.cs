
namespace Plotter;

public struct CadSize2D
{
    public vcompo_t Width;
    public vcompo_t Height;

    public CadSize2D(vcompo_t w, vcompo_t h)
    {
        Width = w;
        Height = h;
    }

    public static CadSize2D operator *(CadSize2D me, vcompo_t f)
    {
        return new CadSize2D(me.Width * f, me.Height * f);
    }
}
