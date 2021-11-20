using System;
using System.Collections.Generic;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public class EditResult
    {
        public class Item
        {
            public uint LayerID { set; get; } = 0;

            private CadFigure mFigure;

            public CadFigure Figure
            {
                set
                {
                    mFigure = value;
                }

                get
                {
                    return mFigure;
                }
            }

            public uint FigureID
            {
                get
                {
                    if (mFigure == null)
                    {
                        return 0;
                    }

                    return mFigure.ID;
                }
            }

            public Item()
            {
            }

            public Item(uint layerID, CadFigure fig)
            {
                LayerID = layerID;
                Figure = fig;
            }
        }


        public List<Item> AddList = new List<Item>();
        public List<Item> RemoveList = new List<Item>();

        public bool isValid()
        {
            return AddList.Count > 0 || RemoveList.Count > 0;
        }

        public void clear()
        {
            AddList.Clear();
            RemoveList.Clear();
        }
    }

    class CadFigureCutter
    {
        public static EditResult Cut(CadObjectDB db, CadFigure fig, int sp)
        {
            EditResult result = new EditResult();

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return result;
            }

            if (fig.IsLoop)
            {
                return result;
            }

            int pcnt = fig.PointCount;

            int headNum = sp + 1;
            int tailNum = pcnt - sp;

            CadFigure headFig = null;
            CadFigure tailFig = null;

            if (headNum <= 1 || tailNum <= 1)
            {
                return result;
            }

            if (headNum >= 2)
            {
                headFig = db.NewFigure(CadFigure.Types.POLY_LINES);
                headFig.AddPoints(fig.PointList, 0, headNum);
            }

            if (tailNum >= 2)
            {
                tailFig = db.NewFigure(CadFigure.Types.POLY_LINES);
                tailFig.AddPoints(fig.PointList, sp, tailNum);
            }

            if (headFig != null)
            {
                result.AddList.Add(new EditResult.Item(fig.LayerID, headFig));
            }

            if (tailFig != null)
            {
                result.AddList.Add(new EditResult.Item(fig.LayerID, tailFig));
            }

            result.RemoveList.Add(new EditResult.Item(fig.LayerID, fig));

            return result;
        }
    }

    class CadSegmentCutter
    {
        public static EditResult CutSegment(CadObjectDB db, MarkSegment seg, Vector3d p)
        {
            EditResult result = new EditResult();

            if (seg.Figure.Type != CadFigure.Types.POLY_LINES)
            {
                return result;
            }

            CrossInfo ci = CadMath.PerpendicularCrossSeg(seg.pA.vector, seg.pB.vector, p);

            if (!ci.IsCross)
            {
                return result;
            }

            CadFigure org = db.GetFigure(seg.FigureID);

            int a = Math.Min(seg.PtIndexA, seg.PtIndexB);
            int b = Math.Max(seg.PtIndexA, seg.PtIndexB);


            CadFigure fa = db.NewFigure(CadFigure.Types.POLY_LINES);
            CadFigure fb = db.NewFigure(CadFigure.Types.POLY_LINES);

            fa.AddPoints(org.PointList, 0, a + 1);
            fa.AddPoint(new CadVertex(ci.CrossPoint));

            fb.AddPoint(new CadVertex(ci.CrossPoint));
            fb.AddPoints(org.PointList, b);

            if (org.IsLoop)
            {
                fb.AddPoint(fa.GetPointAt(0));
            }

            result.AddList.Add(new EditResult.Item(seg.LayerID, fa));
            result.AddList.Add(new EditResult.Item(seg.LayerID, fb));
            result.RemoveList.Add(new EditResult.Item(org.LayerID, org));

            return result;
        }
    }
}
