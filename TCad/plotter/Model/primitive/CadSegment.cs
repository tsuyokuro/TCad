using CadDataTypes;
using TCad.Logger;

namespace TCad.Plotter;

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
