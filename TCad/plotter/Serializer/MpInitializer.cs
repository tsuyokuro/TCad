using MessagePack;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter.Serializer;

[MessagePackObject]
public class MpDummy
{
    [Key("Value")]
    public int Value = 0;
}

public class MpInitializer
{
    public static void Init()
    {
        MpDummy v = new MpDummy();

        byte[] b = MessagePackSerializer.Serialize(v);

        v = MessagePackSerializer.Deserialize<MpDummy>(b);

        int a = v.Value;
    }
}
