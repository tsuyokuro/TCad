using System.Collections.Generic;
using CadDataTypes;
using OpenTK;
using Plotter.Controller;

namespace Plotter
{
    public class NearPointSearcher
    {
        public abstract class Result
        {
            public double Dist = double.MaxValue;
            public CadVertex WoldPoint;

            public abstract string ToInfoString();

            public Result(CadVertex wp, double dist)
            {
                WoldPoint = wp;
                Dist = dist;
            }
        }

        List<Result> ResultList = new List<Result>();

        public struct SegmentItem
        {
            public CadLayer Layer;
            public CadFigure Fig;
            public int SegIndex;
            public CadSegment ScrSegment;
        }

        private PlotterController Controller;

        private DrawContext DC;

        List<SegmentItem> SegList = new List<SegmentItem>();

        public CadVertex TargetPoint = CadVertex.InvalidValue;

        public double Range = 128;

        public NearPointSearcher(PlotterController controller)
        {
            Controller = controller;
            DC = Controller.DC;
        }

        public List<Result> Search(CadVertex p, double range)
        {
            TargetPoint = p;
            Range = range;

            ResultList.Clear();

            SegList.Clear();

            CheckZeroPoint();

            Controller.DB.ForEachEditableFigure(CheckFig);

            CheckCross();

            ResultList.Sort((a, b) =>
            {
                return (int)(a.Dist * 1000 - b.Dist * 1000);
            });

            DOut.pl($"ResultList.Count:{ResultList.Count}");

            return ResultList;
        }

        void CheckZeroPoint()
        {
            CadVertex p = DC.WorldPointToDevPoint(CadVertex.Zero);

            CadVertex d = p - TargetPoint;

            double dist = d.Norm2D();

            if (dist > Range)
            {
                return;
            }

            Result res = new ResultZero(dist);
            ResultList.Add(res);
        }

        void CheckFig(CadLayer layer, CadFigure fig)
        {
            int n = fig.PointCount;
            for (int i = 0; i < n; i++)
            {
                CadVertex cp = fig.PointList[i];

                CadVertex p = DC.WorldPointToDevPoint(cp);

                CadVertex d = p - TargetPoint;

                double dist = d.Norm2D();

                if (dist > Range)
                {
                    continue;
                }

                Result res = new ResultPoint(cp, dist, fig, i);
                ResultList.Add(res);
            }

            //
            // Create segment list that in range.
            // And check center point of segment
            //
            // 範囲内の線分リスト作成
            // ついでに中点のチェックも行う
            //

            n = fig.SegmentCount;

            for (int i = 0; i < n; i++)
            {
                CadSegment seg = fig.GetSegmentAt(i);

                CadVertex pw = (seg.P1 - seg.P0) / 2 + seg.P0;
                CadVertex ps = DC.WorldPointToDevPoint(pw);

                double dist = (ps - TargetPoint).Norm2D();

                if (dist <= Range)
                {
                    Result res = new ResultSegCenter(pw, dist, fig, i);
                    ResultList.Add(res);
                }

                CadVertex p0 = DC.WorldPointToDevPoint(seg.P0);
                CadVertex p1 = DC.WorldPointToDevPoint(seg.P1);

                double d = CadMath.DistancePointToSeg(p0.vector, p1.vector, TargetPoint.vector);

                if (d > Range)
                {
                    continue;
                }

                SegmentItem segItem = new SegmentItem();

                segItem.Layer = layer;
                segItem.Fig = fig;
                segItem.SegIndex = i;
                segItem.ScrSegment = new CadSegment(p0, p1);

                SegList.Add(segItem);
            }
        }

        private void CheckCross()
        {
            int i = 0;
            for (; i < SegList.Count; i++)
            {
                SegmentItem seg0 = SegList[i];

                int j = i + 1;
                for (; j < SegList.Count; j++)
                {
                    SegmentItem seg1 = SegList[j];

                    if (!CheckCrossSegSegScr(seg0, seg1))
                    {
                        continue;
                    }

                    Vector3d cv = CrossLineScr(seg0, seg1);

                    if (cv.IsInvalid())
                    {
                        continue;
                    }

                    CadVertex dv = cv - TargetPoint;

                    double dist = dv.Norm2D();

                    if (dist > Range)
                    {
                        continue;
                    }

                    if (IsSegVertex(cv, seg0) || IsSegVertex(cv, seg1))
                    {
                        continue;
                    }

                    Result res = new ResultCross(new CadVertex(DC.DevPointToWorldPoint(cv)), dist, seg0, seg1);
                    ResultList.Add(res);
                }
            }
        }

        private bool IsSegVertex(Vector3d v, SegmentItem seg)
        {
            return (seg.ScrSegment.P0.vector.Equals(v)) || (seg.ScrSegment.P1.vector.Equals(v));
        }

        private bool CheckCrossSegSegScr(SegmentItem seg0, SegmentItem seg1)
        {
            return CadMath.CheckCrossSegSeg2D(
                seg0.ScrSegment.P0.vector,
                seg0.ScrSegment.P1.vector,
                seg1.ScrSegment.P0.vector,
                seg1.ScrSegment.P1.vector);

        }

        private Vector3d CrossLineScr(SegmentItem seg0, SegmentItem seg1)
        {
            return CadMath.CrossLine2D(
                seg0.ScrSegment.P0.vector,
                seg0.ScrSegment.P1.vector,
                seg1.ScrSegment.P0.vector,
                seg1.ScrSegment.P1.vector);
        }


        #region Result types
        public class ResultZero : Result
        {
            public ResultZero(double dist)
                : base(CadVertex.Zero, dist)
            {
            }

            public override string ToInfoString()
            {
                return $"Zero";
            }
        }

        public class ResultPoint : Result
        {
            public CadFigure Fig = null;
            public int PointIndex = -1;

            public ResultPoint(CadVertex wp, double dist, CadFigure fig, int index)
                : base(wp, dist)
            {
                Fig = fig;
                PointIndex = index;
            }

            public override string ToInfoString()
            {
                return $"Vertex FigID={Fig.ID} PointIndex={PointIndex}";
            }
        }

        public class ResultSegCenter : Result
        {
            public CadFigure Fig = null;
            public int SegIndex;

            public ResultSegCenter(CadVertex wp, double dist, CadFigure fig, int segIndex)
                : base(wp, dist)
            {
                Fig = fig;
                SegIndex = segIndex;
            }

            public override string ToInfoString()
            {
                return $"Center point FigID={Fig.ID} SegIndex={SegIndex}";
            }
        }

        public class ResultCross : Result
        {
            public SegmentItem Seg0 = default;
            public SegmentItem Seg1 = default;

            public ResultCross(CadVertex wp, double dist, SegmentItem seg0, SegmentItem seg1)
                : base(wp, dist)
            {
                WoldPoint = wp;
                Dist = dist;
                Seg0 = seg0;
                Seg1 = seg1;
            }

            public override string ToInfoString()
            {
                return $"Cross point FigID={Seg0.Fig.ID} Index={Seg0.SegIndex} - FigID={Seg1.Fig.ID} Index={Seg1.SegIndex}";
            }
        }
        #endregion
    }
}
