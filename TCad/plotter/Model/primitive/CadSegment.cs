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

public struct CadSegment
{
    public bool Valid
    {
        set
        {
            P0.Valid = value;
        }

        get
        {
            return P0.Valid;
        }
    }

    public CadVertex P0;
    public CadVertex P1;

    public CadSegment(CadVertex a, CadVertex b)
    {
        P0 = a;
        P1 = b;
    }

    public void dump(string name = "FigureSegment")
    {
        Log.pl(name + "{");
        Log.Indent++;
        Log.pl("Valid:" + Valid.ToString());
        P0.dump("P0");
        P1.dump("P1");
        Log.Indent--;
        Log.pl("}");
    }
}
