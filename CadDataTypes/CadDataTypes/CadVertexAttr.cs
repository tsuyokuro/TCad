using OpenTK.Mathematics;
using System.Reflection.Metadata;

namespace CadDataTypes;

public struct CadVertexAttr
{
    public Color4 Color;

    public vector3_t Normal;

    public CadVertexAttr()
    {
        Color.A = -1;
        Color.R = -1;
        Color.G = -1;
        Color.B = -1;

        Normal = vector3_t.Zero;
    }
}
