using CadDataTypes;
using IronPython.Runtime;
using Plotter.Serializer.v1003;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Serializer
{
    public abstract class MpFigure
    {
        public abstract CadFigure Restore();
    }



    public class MpUtil
    {
        public static List<TMpVertex> VertexListToMp<TMpVertex>(
            VertexList v,
            Func<CadVertex, TMpVertex> creator
            )
        {
            List<TMpVertex> ret = new List<TMpVertex>();
            for (int i = 0; i < v.Count; i++)
            {
                ret.Add(creator(v[i]));
            }

            return ret;
        }


        public static List<uint> FigureListToIdList(List<CadFigure> figList)
        {
            List<uint> ret = new List<uint>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(figList[i].ID);
            }

            return ret;
        }


        public static List<TMpFig> FigureListToMp<TMpFig>(
            List<CadFigure> figList,
            Func<CadFigure, bool, TMpFig> creator,
            bool withChild = false) where TMpFig : MpFigure
        {
            List<TMpFig> ret = new List<TMpFig>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(creator(figList[i], withChild));
            }

            return ret;
        }


        public static List<CadFigure> FigureListFromMp<TMpFig>(List<TMpFig> list)
            where TMpFig : MpFigure
        {
            List<CadFigure> ret = new List<CadFigure>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<TMpFig> FigureMapToMp<TMpFig> (
            Dictionary<uint, CadFigure> figMap,
            Func<CadFigure, bool, TMpFig> creator,
            bool withChild = false) where TMpFig : MpFigure
        {
            List<TMpFig> ret = new List<TMpFig>();
            foreach (CadFigure fig in figMap.Values)
            {
                ret.Add(creator(fig, withChild));
            }

            return ret;
        }
    }
}
