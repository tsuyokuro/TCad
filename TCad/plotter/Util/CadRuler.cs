using System;
using System.Diagnostics;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public struct RulerInfo
    {
        public bool IsValid;
        public Vector3d CrossPoint;
        public double Distance;

        public CadRuler Ruler;
    }


    public struct CadRuler
    {
        public bool IsValid;
        public CadFigure Fig;
        public int Idx0;
        public int Idx1;

        public Vector3d P0
        {
            get
            {
                if (Fig.StoreList == null)
                {
                    return Fig.PointList[Idx0].vector;
                }
                else
                {
                    return Fig.StoreList[Idx0].vector;
                }
            }
        }

        public Vector3d P1
        {
            get
            {
                if (Fig.StoreList == null)
                {
                    return Fig.PointList[Idx1].vector;
                }
                else
                {
                    return Fig.StoreList[Idx1].vector;
                }
            }
        }

        public RulerInfo Capture(DrawContext dc, CadCursor cursor, double range)
        {
            RulerInfo ret = default(RulerInfo);

            Vector3d cwp = dc.DevPointToWorldPoint(cursor.Pos);

            Vector3d xfaceNormal = dc.DevVectorToWorldVector(cursor.DirX);
            Vector3d yfaceNormal = dc.DevVectorToWorldVector(cursor.DirY);

            Vector3d cx = CadMath.CrossPlane(P0, P1, cwp, xfaceNormal);
            Vector3d cy = CadMath.CrossPlane(P0, P1, cwp, yfaceNormal);

            if (!cx.IsValid() && !cy.IsValid())
            {
                return ret;
            }

            Vector3d p = VectorExt.InvalidVector3d;
            double mind = Double.MaxValue;

            StackArray<Vector3d> vtbl = default;

            vtbl[0] = cx;
            vtbl[1] = cy;
            vtbl.Length = 2;

            for (int i = 0; i < vtbl.Length; i++)
            {
                Vector3d v = vtbl[i];

                if (!v.IsValid())
                {
                    continue;
                }

                Vector3d devv = dc.WorldPointToDevPoint(v);

                double td = (devv - cursor.Pos).Norm();

                if (td < mind)
                {
                    mind = td;
                    p = v;
                }
            }

            if (!p.IsValid())
            {
                return ret;
            }

            if (mind > range)
            {
                return ret;
            }

            ret.IsValid = true;
            ret.CrossPoint = p;
            ret.Distance = mind;

            ret.Ruler = this;

            return ret;
        }

        public static CadRuler Create(CadFigure fig, int idx0, int idx1)
        {
            CadRuler ret = default(CadRuler);
            ret.Fig = fig;
            ret.Idx0 = idx0;
            ret.Idx1 = idx1;

            return ret;
        }
    }

    public class CadRulerSet
    {
        private CadRuler[] Ruler = new CadRuler[10];
        private int RCount = 0;
        private int MatchIndex = -1;

        public void Set(MarkPoint mkp)
        {
            CadFigure fig = mkp.Figure;
            int pointIndex = mkp.PointIndex;

            int cnt = fig.PointList.Count;

            if (cnt < 2)
            {
                return;
            }

            if (!fig.IsLoop)
            {
                if (pointIndex == cnt - 1)
                {
                    Ruler[RCount] = CadRuler.Create(fig, pointIndex - 1, pointIndex);
                    RCount++;
                    return;
                }
                else if (pointIndex == 0)
                {
                    Ruler[RCount] = CadRuler.Create(fig, 1, 0);
                    RCount++;
                    return;
                }
            }

            int idx0;
            int idx1;

            idx0 = (pointIndex + cnt - 1) % cnt;
            idx1 = pointIndex;

            Debug.Assert(idx0 >= 0 && idx0 < cnt);

            Ruler[RCount] = CadRuler.Create(fig, idx0, idx1);
            RCount++;

            idx0 = (pointIndex + 1) % cnt;
            idx1 = pointIndex;

            Debug.Assert(idx0 >= 0 && idx0 < cnt);

            Ruler[RCount] = CadRuler.Create(fig, idx0, idx1);
            RCount++;
        }

        public void Set(MarkSegment mks, DrawContext dc)
        {
            Ruler[RCount] = CadRuler.Create(mks.Figure, mks.PtIndexA, mks.PtIndexB);
            RCount++;
        }

        public RulerInfo Capture(DrawContext dc, CadCursor cursor, double rangePixel)
        {
            RulerInfo match = default(RulerInfo);
            RulerInfo ri = default(RulerInfo);

            double min = rangePixel;

            MatchIndex = -1;

            for (int i = 0; i < RCount; i++)
            {
                ri = Ruler[i].Capture(dc, cursor, rangePixel);

                if (ri.IsValid && ri.Distance < min)
                {
                    min = ri.Distance;
                    match = ri;
                    MatchIndex = i;
                }
            }

            return match;
        }

        public void Clear()
        {
            for (int i = 0; i < RCount; i++)
            {
                Ruler[i].IsValid = false;
            }

            RCount = 0;
        }
    }
}
