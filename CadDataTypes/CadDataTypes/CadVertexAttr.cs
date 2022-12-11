//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using System.Reflection.Metadata;


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


namespace CadDataTypes;

public struct CadVertexAttr
{
    public static byte COLOR1_VALID = 0x01;
    public static byte COLOR2_VALID = 0x02;
    public static byte NORMAL_VALID = 0x04;

    public byte Flags
    {
        get;
        set;
    }

    public Color4 Color1
    {
        get;
        set;
    }

    public Color4 Color2
    {
        get;
        set;
    }

    public vector3_t Normal
    {
        get;
        set;
    }

    public bool IsColor1Valid
    {
        set => Flags = value ? (byte)(Flags | COLOR1_VALID) : (byte)(Flags & ~COLOR1_VALID);
        get => (Flags & COLOR1_VALID) != 0;
    }

    public bool IsColor2Valid
    {
        set => Flags = value ? (byte)(Flags | COLOR2_VALID) : (byte)(Flags & ~COLOR2_VALID);
        get => (Flags & COLOR1_VALID) != 0;
    }

    public bool IsNormalValid
    {
        set => Flags = value ? (byte)(Flags | NORMAL_VALID) : (byte)(Flags & ~NORMAL_VALID);
        get => (Flags & NORMAL_VALID) != 0;
    }
}
