using CadDataTypes;
using System;
using System.Collections.Generic;
using HalfEdgeNS;

using MyCollections;


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

namespace Plotter.Serializer;

public delegate T MpLayerCreator<T>(SerializeContext context, CadLayer layer);

public delegate T MpFigCreator<T>(SerializeContext context, CadFigure fig, bool withChild);

public delegate T MpVertexCreator<T>(CadVertex v);

public delegate T MpVector3Creator<T>(vector3_t v);

public delegate T MpHeFaceCreator<T>(HeFace heFace);

public delegate T MpHalfEdgeCreator<T>(HalfEdge he);


public abstract class MpLayer
{
    public abstract CadLayer Restore(DeserializeContext dsc, Dictionary<uint, CadFigure> dic);
}

public abstract class MpFigure
{
    public abstract CadFigure Restore(DeserializeContext dsc);
}


public interface MpVertex {
    public CadVertex Restore();
}

public interface MpVector3
{
    public abstract vector3_t Restore();
}

public abstract class MpHeFace
{
    public abstract HeFace Restore(Dictionary<uint, HalfEdge> dic);
}

public class MpUtil
{
    //=========================================================================
    //
    // Convert Cad Object to MessagePack Object
    //

    public static List<TMpLayer> LayerListToMp<TMpLayer>(
        SerializeContext sc,
        List<CadLayer> src,
        MpLayerCreator<TMpLayer> creator
        )
    {
        List<TMpLayer> ret = new();
        for (int i = 0; i < src.Count; i++)
        {
            ret.Add(creator(sc, src[i]));
        }

        return ret;
    }

    public static List<TMpFig> FigureListToMp<TMpFig>(
        SerializeContext sc,
        List<CadFigure> figList,
        MpFigCreator<TMpFig> creator,
        bool withChild = false
        )
    {
        List<TMpFig> ret = new List<TMpFig>();
        for (int i = 0; i < figList.Count; i++)
        {
            ret.Add(creator(sc, figList[i], withChild));
        }

        return ret;
    }


    public static List<TMpFig> FigureMapToMp<TMpFig> (
        SerializeContext sc,
        Dictionary<uint, CadFigure> figMap,
        MpFigCreator<TMpFig> creator,
        bool withChild = false
        )
    {
        List<TMpFig> ret = new List<TMpFig>();
        foreach (CadFigure fig in figMap.Values)
        {
            ret.Add(creator(sc, fig, withChild));
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
        VertexList v,
        MpVertexCreator<TMpVertex> creator
        )
    {
        List<TMpVertex> ret = new List<TMpVertex>();
        for (int i = 0; i < v.Count; i++)
        {
            ret.Add(creator(v[i]));
        }

        return ret;
    }

    public static List<TMpVector3> Vector3ListToMp<TMpVector3>(
        Vector3List v,
        MpVector3Creator<TMpVector3> creator
        )
    {
        List<TMpVector3> ret = new List<TMpVector3>();
        for (int i = 0; i < v.Count; i++)
        {
            ret.Add(creator(v[i]));
        }

        return ret;
    }

    public static List<TMpHeFace> HeFaceListToMp<TMpHeFace>(
        FlexArray<HeFace> list,
        MpHeFaceCreator<TMpHeFace> creator
        )
    {
        List<TMpHeFace> ret = new();
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(creator(list[i]));
        }

        return ret;
    }

    public static List<TMpHalfEdge> HalfEdgeListToMp<TMpHalfEdge>(
        List<HalfEdge> list, MpHalfEdgeCreator<TMpHalfEdge> creator)
    {
        List <TMpHalfEdge > ret = new();
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(creator(list[i]));
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
