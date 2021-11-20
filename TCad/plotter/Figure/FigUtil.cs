using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;
using MessagePack;
using OpenTK;
using Plotter.Serializer;
using Plotter.Serializer.v1001;

namespace Plotter
{
    public class FigUtil
    {
        public static void MoveSelectedPointsFromStored(CadFigure fig, DrawContext dc, Vector3d delta)
        {
            if (fig.StoreList == null)
            {
                return;
            }

            for (int i = 0; i < fig.StoreList.Count; i++)
            {
                CadVertex op = fig.StoreList[i];

                if (!op.Selected)
                {
                    continue;
                }

                if (i < fig.PointList.Count)
                {
                    fig.PointList[i] = op + delta;
                }
            }
        }

        public static void MoveAllPoints(CadFigure fig, Vector3d delta)
        {
            CadUtil.MovePoints(fig.PointList, delta);
        }

        public static CadRect GetContainsRect(CadFigure fig)
        {
            return CadUtil.GetContainsRect(fig.PointList);
        }

        public static CadRect GetContainsRectScrn(CadFigure fig, DrawContext dc)
        {
            return CadUtil.GetContainsRectScrn(dc, fig.PointList);
        }

        public static VertexList GetPoints(CadFigure fig, int curveSplitNum)
        {
            return fig.PointList;
        }

        public static CadVertex GetPointAt(CadFigure fig, int idx)
        {
            return fig.PointList[idx];
        }

        public static void SetPointAt(CadFigure fig, int index, CadVertex pt)
        {
            fig.PointList[index] = pt;
        }

        public static void SelectPointAt(CadFigure fig, int index, bool sel)
        {
            CadVertex p = fig.PointList[index];
            p.Selected = sel;
            fig.PointList[index] = p;
        }

        public static CadSegment GetSegmentAt(CadFigure fig, int n)
        {
            if (n < fig.PointList.Count - 1)
            {
                return new CadSegment(fig.PointList[n], fig.PointList[n + 1]);
            }

            if (n == fig.PointList.Count - 1 && fig.IsLoop)
            {
                return new CadSegment(fig.PointList[n], fig.PointList[0]);
            }

            throw new System.ArgumentException("GetSegmentAt", "bad index");
        }

        public static FigureSegment GetFigSegmentAt(CadFigure fig, int n)
        {
            if (n < fig.PointList.Count - 1)
            {
                return new FigureSegment(fig, n, n, n + 1);
            }

            if (n == fig.PointList.Count - 1 && fig.IsLoop)
            {
                return new FigureSegment(fig, n, n, 0);
            }

            throw new System.ArgumentException("GetFigSegmentAt", "bad index");
        }

        public static int SegmentCount(CadFigure fig)
        {
            int cnt = fig.PointList.Count - 1;

            if (fig.IsLoop)
            {
                cnt++;
            }

            return cnt;
        }

        public static CadFigure GetRootFig(CadFigure src)
        {
            while (true)
            {
                if (src.Parent == null)
                {
                    return src;
                }

                src = src.Parent;
            }
        }

        public static string DumpString(CadFigure fig, string margin)
        {
            string s="";

            s += margin + "ID:" + fig.ID.ToString() + "\n";
            s += margin + "Point:[\n";
            for (int i=0; i<fig.PointList.Count; i++)
            {
                CadVertex v = fig.PointList[i];
                s += margin + "  " + string.Format("{0},{1},{2}\n", v.X, v.Y, v.Z);
            }
            s += margin + "]\n";

            s += margin + "Children:[\n";
            if (fig.ChildList != null)
            {
                for (int i = 0; i < fig.ChildList.Count; i++)
                {
                    CadFigure c = fig.ChildList[i];
                    s += DumpString(c, margin + "  ");
                }
            }

            s += margin + "]\n";

            return s;
        }

        public static List<CadFigure> GetRootFigList(List<CadFigure> srcList)
        {
            HashSet<CadFigure> set = new HashSet<CadFigure>();

            foreach (CadFigure fig in srcList)
            {
                set.Add(FigUtil.GetRootFig(fig));
            }

            List<CadFigure> ret = new List<CadFigure>();

            ret.AddRange(set);

            return ret;
        }

        //public static CadFigure Clone(CadFigure src)
        //{
        //    MpFigure_v1002 mpf = MpFigure_v1002.Create(src, false);

        //    byte[] data = MessagePackSerializer.Serialize(mpf);

        //    MpFigure_v1002 mpfCopy = MessagePackSerializer.Deserialize<MpFigure_v1002>(data);

        //    CadFigure fig = mpfCopy.Restore();

        //    fig.ID = 0;

        //    return fig;
        //}

        public static CadFigure Clone(CadFigure src)
        {
            byte[] data = MpUtil.FigToBin(src, false);

            CadFigure fig = MpUtil.BinToFig(data);

            fig.ID = 0;

            return fig;
        }


        public static void CopyTo(CadFigure src, CadFigure dst)
        {
            MpFigure_v1002 mpf = MpFigure_v1002.Create(src, false);

            byte[] data = MessagePackSerializer.Serialize(mpf);

            MpFigure_v1002 mpfCopy = MessagePackSerializer.Deserialize<MpFigure_v1002>(data);

            uint id = dst.ID;

            mpfCopy.RestoreTo(dst);

            dst.ID = id;
        }
    }
}