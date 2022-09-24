using OpenTK.Mathematics;
using System.Reflection.Metadata;


namespace CadDataTypes
{
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

        public Vector3d Normal
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
            set => Flags = value ? (byte)(Flags | COLOR1_VALID) : (byte)(Flags & ~COLOR1_VALID);
            get => (Flags & COLOR1_VALID) != 0;
        }

        public bool IsNormalValid
        {
            set => Flags = value ? (byte)(Flags | NORMAL_VALID) : (byte)(Flags & ~NORMAL_VALID);
            get => (Flags & NORMAL_VALID) != 0;
        }
    }
}
