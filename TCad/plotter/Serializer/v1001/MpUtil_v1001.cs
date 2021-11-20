using HalfEdgeNS;
using MyCollections;
using System;
using System.Collections.Generic;
using CadDataTypes;


namespace Plotter.Serializer.v1001
{
    public partial class MpUtil_v1001
    {
        public static MpCadData_v1001 CreateMpCadData_v1001(CadData cd)
        {
            MpCadData_v1001 data = MpCadData_v1001.Create(cd.DB);

            data.ViewInfo.WorldScale = cd.WorldScale;

            data.ViewInfo.PaperSettings.Set(cd.PageSize);

            return data;
        }

        public static CadData CreateCadData_v1001(MpCadData_v1001 mpcd)
        {
            CadData cd = new CadData();

            MpViewInfo_v1001 viewInfo = mpcd.ViewInfo;

            double worldScale = 0;

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
                worldScale = 1.0;
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

        public static List<MpVertex_v1001> VertexListToMp(VertexList v)
        {
            List<MpVertex_v1001> ret = new List<MpVertex_v1001>();
            for (int i=0; i<v.Count; i++)
            {
                ret.Add(MpVertex_v1001.Create(v[i]));
            }

            return ret;
        }

        public static List<uint> FigureListToIdList(List<CadFigure> figList )
        {
            List<uint> ret = new List<uint>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(figList[i].ID);
            }

            return ret;
        }

        public static VertexList VertexListFromMp(List<MpVertex_v1001> list)
        {
            VertexList ret = new VertexList(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpVector3d_v1001> Vector3dListToMp(Vector3dList v)
        {
            List<MpVector3d_v1001> ret = new List<MpVector3d_v1001>();
            for (int i = 0; i < v.Count; i++)
            {
                ret.Add(MpVector3d_v1001.Create(v[i]));
            }

            return ret;
        }

        public static Vector3dList Vector3dListFromMp(List<MpVector3d_v1001> list)
        {
            Vector3dList ret = new Vector3dList(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpFigure_v1001> FigureListToMp_v1001(List<CadFigure> figList, bool withChild = false)
        {
            List<MpFigure_v1001> ret = new List<MpFigure_v1001>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(MpFigure_v1001.Create(figList[i], withChild));
            }

            return ret;
        }

        public static List<CadFigure> FigureListFromMp_v1001(List<MpFigure_v1001> list)
        {
            List<CadFigure> ret = new List<CadFigure>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpFigure_v1001> FigureMapToMp_v1001(
            Dictionary<uint, CadFigure> figMap, bool withChild = false)
        {
            List<MpFigure_v1001> ret = new List<MpFigure_v1001>();
            foreach (CadFigure fig in figMap.Values)
            {
                ret.Add(MpFigure_v1001.Create(fig, withChild));
            }

            return ret;
        }

        public static List<MpHeFace_v1001> HeFaceListToMp(FlexArray<HeFace> list)
        {
            List<MpHeFace_v1001> ret = new List<MpHeFace_v1001>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(MpHeFace_v1001.Create(list[i]));
            }

            return ret;
        }


        public static List<MpLayer_v1001> LayerListToMp(List<CadLayer> src)
        {
            List<MpLayer_v1001> ret = new List<MpLayer_v1001>();
            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(MpLayer_v1001.Create(src[i]));
            }

            return ret;
        }

        public static List<CadLayer> LayerListFromMp(
            List<MpLayer_v1001> src, Dictionary<uint, CadFigure> dic)
        {
            List<CadLayer> ret = new List<CadLayer>();
            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(src[i].Restore(dic));
            }

            return ret;
        }

        public static FlexArray<HeFace> HeFaceListFromMp(
            List<MpHeFace_v1001> list,
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

        public static List<MpHalfEdge_v1001> HalfEdgeListToMp(List<HalfEdge> list)
        {
            List<MpHalfEdge_v1001> ret = new List<MpHalfEdge_v1001>();
            for (int i=0; i<list.Count; i++)
            {
                ret.Add(MpHalfEdge_v1001.Create(list[i]));
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
}
