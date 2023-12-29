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

//public struct PointPair
//{
//    public CadVertex P0;
//    public CadVertex P1;

//    public PointPair(CadVertex p0, CadVertex p1)
//    {
//        P0 = p0;
//        P1 = p1;
//    }
//}

public struct FigureSegment
{
    public CadFigure Figure;
    public int SegIndex;
    public int Index0;
    public int Index1;

    public static FigureSegment InvalidValue = new FigureSegment(null, -1, -1, -1);

    public uint FigureID
    {
        get
        {
            if (Figure == null)
            {
                return 0;
            }

            return Figure.ID;
        }
    }

    public CadVertex Point0
    {
        get
        {
            return Figure.GetPointAt(Index0);
        }

    }

    public CadVertex Point1
    {
        get
        {
            return Figure.GetPointAt(Index1);
        }
    }

    public CadVertex StoredPoint0
    {
        get
        {
            return Figure.GetStorePointAt(Index0);
        }

    }

    public CadVertex StoredPoint1
    {
        get
        {
            return Figure.GetStorePointAt(Index1);
        }
    }

    public CadSegment Segment
    {
        get
        {
            return Figure.GetSegmentAt(SegIndex);
        }

    }

    public FigureSegment(CadFigure fig, int segIndex, int a, int b)
    {
        Figure = fig;
        SegIndex = segIndex;
        Index0 = a;
        Index1 = b;
    }

    public void dump(string name = "FigureSegment")
    {
        Log.pl(name + "{");
        Log.Indent++;
        Log.pl("FigureID:" + Figure.ID.ToString());
        Log.pl("SegIndex:" + SegIndex.ToString());
        Log.pl("Index0:" + Index0.ToString());
        Log.pl("Index1:" + Index1.ToString());
        Log.Indent--;
        Log.pl("}");

    }
}
