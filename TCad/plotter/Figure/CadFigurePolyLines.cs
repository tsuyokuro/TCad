using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;
using CadDataTypes;
using Plotter.Settings;

namespace Plotter
{
    public class CadFigurePolyLines : CadFigure
    {
        protected bool RestrictionByNormal = false;

        public CadFigurePolyLines()
        {
            Type = Types.POLY_LINES;
        }

        public override void StartCreate(DrawContext dc)
        {
        }

        public override void EndCreate(DrawContext dc)
        {
        }

        public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
        {
        }

        public override void AddPointInCreating(DrawContext dc, CadVertex p)
        {
        }


        #region Point Move
        public override void MoveSelectedPointsFromStored(DrawContext dc, Vector3d delta)
        {
            //base.MoveSelectedPoints(dc, delta);

            if (Locked) return;

            Vector3d d;


            if (!IsSelectedAll() && mPointList.Count > 2 && RestrictionByNormal)
            {
                Vector3d vdir = dc.ViewDir;

                Vector3d a = delta;
                Vector3d b = delta + vdir;

                d = CadMath.CrossPlane(a, b, StoreList[0].vector, Normal);

                if (!d.IsValid())
                {
                    Vector3d nvNormal = CadMath.Normal(Normal, vdir);

                    double ip = CadMath.InnerProduct(nvNormal, delta);

                    d = nvNormal * ip;
                }
            }
            else
            {
                d = delta;
            }

            FigUtil.MoveSelectedPointsFromStored(this, dc, d);

            mChildList.ForEach(c =>
            {
                c.MoveSelectedPointsFromStored(dc, delta);
            });
        }

        public override void MoveAllPoints(Vector3d delta)
        {
            if (Locked) return;

            FigUtil.MoveAllPoints(this, delta);
        }
        #endregion


        public override int PointCount
        {
            get
            {
                return mPointList.Count;
            }
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 2)
            {
                mPointList.Clear();
            }
        }

        public override void AddPoint(CadVertex p)
        {
            mPointList.Add(p);
        }

        public override void Draw(DrawContext dc)
        {
            DrawPolyLines(dc, dc.GetPen(DrawTools.PEN_DEFAULT_FIGURE));
        }

        public override void Draw(DrawContext dc, DrawParams dp)
        {
            DrawPolyLines(dc, dp.LinePen);
        }

        public void DrawPolyLines(DrawContext dc, DrawPen pen)
        {
            if (mStoreList != null)
            {
                DrawLines(dc, dc.GetPen(DrawTools.PEN_OLD_FIGURE), mStoreList);
            }

            DrawLines(dc, pen, mPointList);

            if (SettingsHolder.Settings.DrawNormal && !Normal.IsZero())
            {
                Vector3d np0 = PointList[0].vector;
                Vector3d np1 = np0 + (Normal * 10);
                dc.Drawing.DrawArrow(dc.GetPen(DrawTools.PEN_NORMAL), np0, np1, ArrowTypes.CROSS, ArrowPos.END, 3, 3);
            }
        }

        public override void DrawSelected(DrawContext dc)
        {
            DrawSelectedLines(dc);
        }

        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
            CadVertex a = PointList[idxA];
            CadVertex b = PointList[idxB];

            dc.Drawing.DrawLine(pen, a.vector, b.vector);
        }

        public override void InvertDir()
        {
            mPointList.Reverse();
            Normal = -Normal;
        }

        struct DrawParam
        {
            public DrawContext DC;
            public DrawPen Pen;

            public DrawParam(DrawContext dc, DrawPen pen)
            {
                DC = dc;
                Pen = pen;
            }
        }

        struct DrawParam2
        {
            public DrawContext DC;
            public DrawPen Pen;
            public Vector3d PrevV;

            public DrawParam2(DrawContext dc, DrawPen pen, Vector3d p)
            {
                DC = dc;
                Pen = pen;
                PrevV = p;
            }
        }


        protected void DrawLines(DrawContext dc, DrawPen pen, VertexList pl)
        {
            int start = 0;
            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return;
            }

            if (Normal.IsZero())
            {
                Normal = CadUtil.TypicalNormal(pl);
            }

            CadVertex a;

            a = pl[start];

            if (cnt == 1)
            {
                dc.Drawing.DrawCross(pen, a.vector, 2);
                //if (a.Selected)
                //{
                //    dc.Drawing.DrawHighlightPoint(a.vector, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT));
                //}

                return;
            }

            //DrawParam dp = new DrawParam(dc, pen);
            //PolyLineExpander.ForEachSegs<DrawParam>(pl, IsLoop, 8, 
            //    (v0, v1, p) =>
            //    {
            //        p.DC.Drawing.DrawLine(p.Pen, v0.vector, v1.vector);
            //    }, dp);

            PolyLineExpander.Draw(pl, IsLoop, 8, dc, pen);
        }

        public override VertexList GetPoints(int curveSplitNum)
        {
            return PolyLineExpander.GetExpandList(mPointList, curveSplitNum);
        }

        private void DrawSelectedLines(DrawContext dc)
        {
            int i;
            int num = PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadVertex p = PointList[i];

                if (!p.Selected) continue;

                dc.Drawing.DrawSelectedPoint(p.vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));


                if (p.IsHandle)
                {
                    int idx = i + 1;

                    if (idx >= num) idx = 0;

                    CadVertex next = GetPointAt(idx);
                    if (!next.IsHandle)
                    {
                        // Draw handle
                        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_HANDLE_LINE), p.vector, next.vector);
                        dc.Drawing.DrawSelectedPoint(next.vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVertex prev = GetPointAt(idx);
                        if (!prev.IsHandle)
                        {
                            // Draw handle
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_HANDLE_LINE), p.vector, prev.vector);
                            dc.Drawing.DrawSelectedPoint(prev.vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }
                }
                else
                {
                    int idx = i + 1;

                    if (idx < PointCount)
                    {
                        CadVertex np = GetPointAt(idx);
                        if (np.IsHandle)
                        {
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p.vector, np.vector);
                            dc.Drawing.DrawSelectedPoint(np.vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVertex np = GetPointAt(idx);
                        if (np.IsHandle)
                        {
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p.vector, np.vector);
                            dc.Drawing.DrawSelectedPoint(np.vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }
                }
            }
        }

        public override void SetPointAt(int index, CadVertex pt)
        {
            mPointList[index] = pt;
        }

       public override void EndEdit()
        {
            base.EndEdit();
            RecalcNormal();
            //例外ハンドリングテスト用
            //CadVector v = mPointList[100];
        }

        public override Centroid GetCentroid()
        {
            if (PointList.Count == 0)
            {
                return default;
            }

            if (PointList.Count == 1)
            {
                return GetPointCentroid();
            }

            if (PointList.Count < 3)
            {
                return GetSegCentroid();
            }

            return GetPointListCentroid();
        }

        public override void RecalcNormal()
        {
            if (PointList.Count == 0)
            {
                return;
            }

            Vector3d prevNormal = Normal;

            Vector3d normal = CadUtil.TypicalNormal(PointList);

            if (CadMath.InnerProduct(prevNormal, normal) < 0)
            {
                normal *= -1;
            }

            Normal = normal;
        }

        private Centroid GetPointListCentroid()
        {
            Centroid ret = default;

            List<CadFigure> triangles = TriangleSplitter.Split(this);

            ret = CadUtil.TriangleListCentroid(triangles);

            return ret;
        }

        private Centroid GetPointCentroid()
        {
            Centroid ret = default;

            ret.Point = PointList[0].vector;
            ret.Area = 0;

            return ret;
        }

        private Centroid GetSegCentroid()
        {
            Centroid ret = default;

            Vector3d d = PointList[1].vector - PointList[0].vector;

            d /= 2.0;

            ret.Point = PointList[0].vector + d;
            ret.Area = 0;

            return ret;
        }
    }
}