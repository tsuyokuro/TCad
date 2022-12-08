using MessagePack;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

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
