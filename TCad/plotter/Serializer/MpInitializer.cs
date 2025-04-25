using MessagePack;

namespace TCad.plotter.Serializer;

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
