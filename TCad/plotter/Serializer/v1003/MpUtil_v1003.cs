//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using HalfEdgeNS;
using MyCollections;
using System;
using System.Collections.Generic;




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


namespace Plotter.Serializer.v1003;

public partial class MpUtil_v1003
{
    public static MpCadData_v1003 CreateMpCadData_v1003(CadData cd)
    {
        MpCadData_v1003 data = MpCadData_v1003.Create(cd.DB);

        data.ViewInfo.WorldScale = cd.WorldScale;

        data.ViewInfo.PaperSettings.Set(cd.PageSize);

        return data;
    }

    public static CadData CreateCadData_v1003(MpCadData_v1003 mpcd)
    {
        CadData cd = new CadData();

        MpViewInfo_v1003 viewInfo = mpcd.ViewInfo;

        vcompo_t worldScale = 0;

        PaperPageSize pps = null;

        if (viewInfo != null)
        {
            worldScale = viewInfo.WorldScale;

            if (viewInfo.PaperSettings != null)
            {
                pps = viewInfo.PaperSettings.GetPaperPageSize();
            }
        }


        if (worldScale == 0)
        {
            worldScale = (vcompo_t)(1.0);
        }

        cd.WorldScale = worldScale;


        if (pps == null)
        {
            pps = new PaperPageSize();
        }

        cd.PageSize = pps;

        cd.DB = mpcd.GetDB();

        return cd;
    }

    public static List<MpVertex_v1003> VertexListToMp(VertexList v)
    {
        List<MpVertex_v1003> ret = new List<MpVertex_v1003>();
        for (int i=0; i<v.Count; i++)
        {
            ret.Add(MpVertex_v1003.Create(v[i]));
        }

        return ret;
    }


    public static VertexList VertexListFromMp(List<MpVertex_v1003> list)
    {
        VertexList ret = new VertexList(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore());
        }

        return ret;
    }

    public static List<MpVector3_v1003> Vector3ListToMp(Vector3List v)
    {
        List<MpVector3_v1003> ret = new List<MpVector3_v1003>();
        for (int i = 0; i < v.Count; i++)
        {
            ret.Add(MpVector3_v1003.Create(v[i]));
        }

        return ret;
    }

    public static Vector3List Vector3ListFromMp(List<MpVector3_v1003> list)
    {
        Vector3List ret = new Vector3List(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore());
        }

        return ret;
    }


    public static List<MpHeFace_v1003> HeFaceListToMp(FlexArray<HeFace> list)
    {
        List<MpHeFace_v1003> ret = new List<MpHeFace_v1003>();
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(MpHeFace_v1003.Create(list[i]));
        }

        return ret;
    }


    public static List<MpLayer_v1003> LayerListToMp(List<CadLayer> src)
    {
        List<MpLayer_v1003> ret = new List<MpLayer_v1003>();
        for (int i = 0; i < src.Count; i++)
        {
            ret.Add(MpLayer_v1003.Create(src[i]));
        }

        return ret;
    }

    public static List<CadLayer> LayerListFromMp(
        List<MpLayer_v1003> src, Dictionary<uint, CadFigure> dic)
    {
        List<CadLayer> ret = new List<CadLayer>();
        for (int i = 0; i < src.Count; i++)
        {
            ret.Add(src[i].Restore(dic));
        }

        return ret;
    }

    public static FlexArray<HeFace> HeFaceListFromMp(
        List<MpHeFace_v1003> list,
        Dictionary<uint, HalfEdge> dic
        )
    {
        FlexArray<HeFace> ret = new FlexArray<HeFace>();
        for (int i = 0; i < list.Count; i++)
        {
            ret.Add(list[i].Restore(dic));
        }

        return ret;
    }

    public static List<MpHalfEdge_v1003> HalfEdgeListToMp(List<HalfEdge> list)
    {
        List<MpHalfEdge_v1003> ret = new List<MpHalfEdge_v1003>();
        for (int i=0; i<list.Count; i++)
        {
            ret.Add(MpHalfEdge_v1003.Create(list[i]));
        }

        return ret;
    }

    public static T[] ArrayClone<T>(T[] src)
    {
        T[] dst = new T[src.Length];

        Array.Copy(src, dst, src.Length);

        return dst;
    } 
}
