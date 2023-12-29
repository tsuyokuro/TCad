//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;



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


namespace Plotter;

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
