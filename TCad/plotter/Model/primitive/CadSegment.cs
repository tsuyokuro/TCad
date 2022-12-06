using CadDataTypes;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

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
        DOut.pl(name + "{");
        DOut.Indent++;
        DOut.pl("Valid:" + Valid.ToString());
        P0.dump("P0");
        P1.dump("P1");
        DOut.Indent--;
        DOut.pl("}");
    }
}
