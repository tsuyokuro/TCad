using CadDataTypes;

namespace Plotter;

public struct MarkSegment
{
    public FigureSegment FigSeg;

    public CadFigure Figure
    {
        get
        {
            return FigSeg.Figure;
        }
    }

    public uint FigureID
    {
        get
        {
            return FigSeg.FigureID;
        }
    }

    public int PtIndexA
    {
        get
        {
            return FigSeg.Index0;
        }
    }

    public CadVertex pA
    {
        get
        {
            return FigSeg.Point0;
        }
    }

    public int PtIndexB
    {
        get
        {
            return FigSeg.Index1;
        }
    }

    public CadVertex pB
    {
        get
        {
            return FigSeg.Point1;
        }
    }


    public CadLayer Layer;

    public uint LayerID
    {
        get
        {
            if (Layer == null)
            {
                return 0;
            }

            return Layer.ID;
        }
    }

    public vector3_t CrossPoint;


    public vector3_t CenterPoint
    {
        get
        {
            return CadMath.CenterPoint(FigSeg.Point0.vector, FigSeg.Point1.vector);
        }
    }

    public vcompo_t Distance;

    public bool Valid { get { return FigureID != 0; } }

    public void dump(string name = "MarkSeg")
    {
        Log.pl(name + " {");
        Log.Indent++;
        FigSeg.dump("FSegment");
        Log.Indent--;
        Log.pl("}");
    }


    public void Clean()
    {
        CrossPoint = VectorExt.InvalidVector3;
    }

    public bool IsSelected()
    {
        if (Figure == null)
        {
            return false;
        }

        return Figure.IsPointSelected(PtIndexA) && Figure.IsPointSelected(PtIndexB);
    }
}
