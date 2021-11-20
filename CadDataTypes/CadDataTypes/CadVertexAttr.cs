using OpenTK.Graphics;

namespace CadDataTypes
{
    public class CadVertexAttr
    {
        Color4 Color4
        {
            get; set;
        }

        CadVertex Normal
        {
            get; set;
        }

        bool HasColor
        {
            get; set;
        }

        bool HasNormal
        {
            get; set;
        }
    }
}
