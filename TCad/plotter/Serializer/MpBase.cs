using CadDataTypes;
using TCad.Plotter;
using Plotter.Serializer;
using System.Collections.Generic;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Model.HalfEdgeModel;

namespace TCad.Plotter.Serializer;

public interface IMpLayer
{
    public void Store(SerializeContext context, CadLayer layer);

    public CadLayer Restore(DeserializeContext dsc, Dictionary<uint, CadFigure> dic);
}

public interface IMpFigure
{
    public void Store(SerializeContext sc, CadFigure fig, bool withChild);

    public CadFigure Restore(DeserializeContext dsc);
}

public interface IMpVertex
{
    public void Store(CadVertex v);

    public CadVertex Restore();
}

public interface IMpVector3
{
    public void Store(vector3_t v);

    public vector3_t Restore();
}

public interface IMpHeFace
{
    public void Store(HeFace heFace);

    public HeFace Restore(Dictionary<uint, HalfEdge> dic);
}

public interface IMpHalfEdge
{
    public void Store(HalfEdge he);

    public HalfEdge Restore();
}
