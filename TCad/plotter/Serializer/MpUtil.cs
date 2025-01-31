using CadDataTypes;
using System;
using System.Collections.Generic;
using HalfEdgeNS;

using MyCollections;


namespace Plotter.Serializer;


public interface MpLayer
{
    public void Store(SerializeContext context, CadLayer layer);

    public CadLayer Restore(DeserializeContext dsc, Dictionary<uint, CadFigure> dic);
}

public interface MpFigure
{
    public void Store(SerializeContext sc, CadFigure fig, bool withChild);

    public CadFigure Restore(DeserializeContext dsc);
}

public interface MpVertex {
    public void Store(CadVertex v);

    public CadVertex Restore();
}

public interface MpVector3
{
    public void Store(vector3_t v);

    public vector3_t Restore();
}

public interface MpHeFace
{
    public void Store(HeFace heFace);

    public HeFace Restore(Dictionary<uint, HalfEdge> dic);
}

public interface MpHalfEdge
{
    public void Store(HalfEdge he);

    public HalfEdge Restore();
}

public class MpUtil
{
    //=========================================================================
    //
    // Convert Cad Object to MessagePack Object
    //

    public static List<TMpLayer> LayerListToMp<TMpLayer>(
        SerializeContext sc,
        List<CadLayer> src
        ) where TMpLayer : MpLayer, new()
    {
        List<TMpLayer> ret = new();
        for (int i = 0; i < src.Count; i++)
        {
            TMpLayer mp = new();
            mp.Store(sc, src[i]);
            ret.Add(mp);
        }

        return ret;
    }

    public static List<TMpFig> FigureListToMp<TMpFig>(
        SerializeContext sc,
        List<CadFigure> figList,
        bool withChild = false
        ) where TMpFig : MpFigure, new()
    {
        List<TMpFig> ret = new List<TMpFig>();
        for (int i = 0; i < figList.Count; i++)
        {
            TMpFig mp = new();
            mp.Store(sc, figList[i], withChild);

            ret.Add(mp);
        }

        return ret;
    }

    public static List<TMpFig> FigureMapToMp<TMpFig> (
        SerializeContext sc,
        Dictionary<uint, CadFigure> figMap,
        bool withChild = false
        ) where TMpFig : MpFigure, new()
    {
        List<TMpFig> ret = new List<TMpFig>();
        foreach (CadFigure fig in figMap.Values)
        {
            TMpFig mp = new();
            mp.Store(sc, fig, withChild);

            ret.Add(mp);
        }
        return ret;
    }

    //-------------------------------------------------------------------------

    public static List<uint> FigureListToIdList(List<CadFigure> figList)
    {
        List<uint> ret = new List<uint>();
        for (int i = 0; i < figList.Count; i++)
        {
            ret.Add(figList[i].ID);
        }

        return ret;
    }


    public static List<TMpVertex> VertexListToMp<TMpVertex>(
        VertexList v
        ) where TMpVertex : MpVertex, new ()
    {
        List<TMpVertex> ret = new List<TMpVertex>();
        for (int i = 0; i < v.Count; i++)
        {
            TMpVertex mp = new();
            mp.Store(v[i]);

            ret.Add(mp);
        }

        return ret;
    }

    public static List<TMpVector3> Vector3ListToMp<TMpVector3>(
        Vector3List v
        ) where TMpVector3 : MpVector3, new()
    {
        List<TMpVector3> ret = new List<TMpVector3>();
        for (int i = 0; i < v.Count; i++)
        {
            TMpVector3 mp = new();
            mp.Store(v[i]);

            ret.Add(mp);
        }

        return ret;
    }

    public static List<TMpHeFace> HeFaceListToMp<TMpHeFace>(
        FlexArray<HeFace> list
        ) where TMpHeFace : MpHeFace, new()
    {
        List<TMpHeFace> ret = new();
        for (int i = 0; i < list.Count; i++)
        {
            TMpHeFace mp = new();
            mp.Store(list[i]);

            ret.Add(mp);
        }

        return ret;
    }

    public static List<TMpHalfEdge> HalfEdgeListToMp<TMpHalfEdge>(
        List<HalfEdge> list
        ) where TMpHalfEdge : MpHalfEdge, new()
    {
        List <TMpHalfEdge > ret = new();
        for (int i = 0; i < list.Count; i++)
        {
            TMpHalfEdge mp = new();
            mp.Store(list[i]);

            ret.Add(mp);
        }

        return ret;
    }


    //=========================================================================
    //
    // Restore Cad Object from MessagePack Object
    //

    public static List<CadLayer> LayerListFromMp<TMpLayer>(
        DeserializeContext dsc,
        List<TMpLayer> src, Dictionary<uint, CadFigure> dic
        ) where TMpLayer : MpLayer
    {
        List<CadLayer> ret = new List<CadLayer>();
        for (int i = 0; i < src.Count; i++)
        {
            ret.Add(src[i].Restore(dsc, dic));
        }

        return ret;
    }


    public static List<CadFigure> FigureListFromMp<TMpFig>(
        DeserializeContext dsc,
        List<TMpFig> list)
        where TMpFig : MpFigure
    {
        List<CadFigure> ret = new List<CadFigure>();
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore(dsc));
        }

        return ret;
    }

    //-------------------------------------------------------------------------

    public static VertexList VertexListFromMp<TMpVertex>(
        List<TMpVertex> list) where TMpVertex : MpVertex
    {
        VertexList ret = new VertexList(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore());
        }

        return ret;
    }
    public static Vector3List Vector3ListFromMp<TMpVector3>(
        List<TMpVector3> list) where TMpVector3 : MpVector3
    {
        Vector3List ret = new Vector3List(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore());
        }

        return ret;
    }

    public static FlexArray<HeFace> HeFaceListFromMp<TMpHeFace>(
        List<TMpHeFace> list,
        Dictionary<uint, HalfEdge> dic
        ) where TMpHeFace : MpHeFace
    {
        FlexArray<HeFace> ret = new FlexArray<HeFace>();
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore(dic));
        }

        return ret;
    }


    //=========================================================================

    public static T[] ArrayClone<T>(T[] src)
    {
        T[] dst = new T[src.Length];

        Array.Copy(src, dst, src.Length);

        return dst;
    }
}
