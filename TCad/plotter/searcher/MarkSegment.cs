using CadDataTypes;
using OpenTK.Mathematics;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

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

    public vector3_t CrossPointScrn;

    public vector3_t CenterPoint
    {
        get
        {
            return CadMath.CenterPoint(FigSeg.Point0.vector, FigSeg.Point1.vector);
        }
    }

    public vcompo_t Distance;

    public bool Valid { get { return FigureID != 0; } }

    public void dump(string name= "MarkSeg")
    {
        DOut.pl(name + " {");
        DOut.Indent++;
        FigSeg.dump("FSegment");
        DOut.Indent--;
        DOut.pl("}");
    }

    public bool Update()
    {
        if (FigSeg.Figure == null)
        {
            return true;
        }

        if (PtIndexA >= FigSeg.Figure.PointList.Count)
        {
            return false;
        }

        if (PtIndexB >= FigSeg.Figure.PointList.Count)
        {
            return false;
        }

        return true;
    }

    public void Clean()
    {
        CrossPoint = VectorExt.InvalidVector3;
        CrossPointScrn = VectorExt.InvalidVector3;
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
