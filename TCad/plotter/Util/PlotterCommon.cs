using CadDataTypes;

namespace TCad.Plotter;

public class IdProvider
{
    private uint mCounter = 0;
    public uint Counter
    {
        get { return mCounter; }
        set { mCounter = value; }
    }

    public uint getNew()
    {
        return ++mCounter;
    }

    public void Reset()
    {
        mCounter = 0;
    }
}

public static class CadClipBoard
{
    public const string TypeNameJson = "List.CadFiguer.Json";
    public const string TypeNameBin = "List.CadFiguer.bin";
}

static class CadVectorExtensions
{
    public static void dump(this CadVertex v, string prefix = nameof(CadVertex))
    {
        Log.pl(prefix + "{");
        Log.Indent++;
        Log.pl("x:" + v.X.ToString());
        Log.pl("y:" + v.Y.ToString());
        Log.pl("z:" + v.Z.ToString());
        Log.Indent--;
        Log.pl("}");
    }
}
