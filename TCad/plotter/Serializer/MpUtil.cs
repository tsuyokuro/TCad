using CadDataTypes;
using HalfEdgeNS;
using MyCollections;
using System;
using System.Collections.Generic;


namespace Plotter.Serializer;



public class MpUtil
{
    //=========================================================================
    //
    // Convert Cad Object to MessagePack Object
    //

    public static List<TMpLayer> LayerListToMp<TMpLayer>(
        SerializeContext sc,
        List<CadLayer> src
        ) where TMpLayer : IMpLayer, new()
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
        ) where TMpFig : IMpFigure, new()
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

    public static List<TMpFig> FigureMapToMp<TMpFig>(
        SerializeContext sc,
        Dictionary<uint, CadFigure> figMap,
        bool withChild = false
        ) where TMpFig : IMpFigure, new()
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
        ) where TMpVertex : IMpVertex, new()
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
        ) where TMpVector3 : IMpVector3, new()
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
        ) where TMpHeFace : IMpHeFace, new()
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
        ) where TMpHalfEdge : IMpHalfEdge, new()
    {
        List<TMpHalfEdge> ret = new();
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
        ) where TMpLayer : IMpLayer
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
        where TMpFig : IMpFigure
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
        List<TMpVertex> list) where TMpVertex : IMpVertex
    {
        VertexList ret = new VertexList(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore());
        }

        return ret;
    }
    public static Vector3List Vector3ListFromMp<TMpVector3>(
        List<TMpVector3> list) where TMpVector3 : IMpVector3
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
        ) where TMpHeFace : IMpHeFace
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
