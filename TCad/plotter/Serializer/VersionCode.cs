using MessagePack;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Plotter.Serializer;

[MessagePackObject]
[StructLayout(LayoutKind.Explicit)]
public struct VersionCode
{
    public static int CodeLength = 4;

    [Key(0)]
    [FieldOffset(0)]
    public byte C_0;

    [Key(1)]
    [FieldOffset(1)]
    public byte C_1;

    [Key(2)]
    [FieldOffset(2)]
    public byte C_2;

    [Key(3)]
    [FieldOffset(3)]
    public byte C_3;


    [IgnoreMember]
    public string Str
    {
        get
        {
            StringBuilder sb = new StringBuilder(32);
            sb.Append(C_0.ToString("x")); sb.Append(".");
            sb.Append(C_1.ToString("x")); sb.Append(".");
            sb.Append(C_2.ToString("x")); sb.Append(".");
            sb.Append(C_3.ToString("x"));

            return sb.ToString();
        }
    }

    [IgnoreMember]
    public byte[] Bytes
    {
        get
        {
            return new byte[] { C_0, C_1, C_2, C_3 };
        }
    }

    public VersionCode(byte f0, byte f1, byte f2, byte f3)
    {
        C_0 = f0; C_1 = f1; C_2 = f2; C_3 = f3;
    }
    public VersionCode(string v)
    {
        C_0 = 0;
        C_1 = 0;
        C_2 = 0;
        C_3 = 0;

        string[] vt = v.Split('.');
        if (vt.Length != 4)
        {
            return;
        }

        try
        {
            C_0 = byte.Parse(vt[0], NumberStyles.HexNumber);
            C_1 = byte.Parse(vt[1], NumberStyles.HexNumber);
            C_2 = byte.Parse(vt[2], NumberStyles.HexNumber);
            C_3 = byte.Parse(vt[3], NumberStyles.HexNumber);

        } catch (FormatException) {
            C_0 = 0xFF;
            C_1 = 0xFF;
            C_2 = 0xFF;
            C_3 = 0xFF;
        }
    }

    public bool Equals(byte[] bytes)
    {
        return bytes[0] == C_0 && bytes[1] == C_1 && bytes[2] == C_2 && bytes[3] == C_3;
    }
}
