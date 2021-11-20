using System;
using System.Collections.Generic;
using System.Drawing;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public class CadFigureCircle : CadFigure
    {
        public override int SegmentCount
        {
            get => 0;
        }

        public override void InvertDir()
        {
            Normal = -Normal;
        }

        public CadFigureCircle()
        {
            Type = Types.CIRCLE;
        }

        public override void AddPointInCreating(DrawContext dc, CadVertex p)
        {
            mPointList.Add(p);
        }

        public override void AddPoint(CadVertex p)
        {
            mPointList.Add(p);
        }

        public override void SetPointAt(int index, CadVertex pt)
        {
            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.Clear();
        }

        public override void Draw(DrawContext dc)
        {
            drawCircle(dc, dc.GetPen(DrawTools.PEN_DEFAULT_FIGURE));
        }

        public override void Draw(DrawContext dc, DrawParams dp)
        {
            drawCircle(dc, dp.LinePen);
        }

        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
            //drawCircle(dc, pen);
        }

        public override void DrawSelected(DrawContext dc)
        {
            drawSelected_Circle(dc);
        }

        public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
        {
            if (PointList.Count <= 0)
            {
                return;
            }

            CadVertex cp = PointList[0];

            CadVertex a = tp;
            CadVertex b = new CadVertex(getRP(dc, cp, tp, true));

            CadVertex c = -(a - cp) + cp;
            CadVertex d = -(b - cp) + cp;

            CircleExpander.Draw(cp, a, b, 32, dc, pen);

            dc.Drawing.DrawLine(pen, cp.vector, a.vector);
            dc.Drawing.DrawLine(pen, cp.vector, b.vector);
            dc.Drawing.DrawLine(pen, cp.vector, c.vector);
            dc.Drawing.DrawLine(pen, cp.vector, d.vector);
        }

        private void drawCircle(DrawContext dc, DrawPen pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            if (PointList.Count == 1)
            {
                dc.Drawing.DrawCross(pen, PointList[0].vector, 2);
                if (PointList[0].Selected) dc.Drawing.DrawSelectedPoint(PointList[0].vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                return;
            }

            Vector3d normal = CadMath.Normal(PointList[0].vector, PointList[2].vector, PointList[1].vector);

            CircleExpander.Draw(PointList[0], PointList[1], PointList[2], 32, dc, pen);

            double size = dc.DevSizeToWoldSize(4);
            dc.Drawing.DrawCross(pen, PointList[0].vector, size);
        }

        private void drawSelected_Circle(DrawContext dc)
        {
            for (int i=0; i<PointList.Count; i++)
            {
                if (PointList[i].Selected)
                {
                    dc.Drawing.DrawSelectedPoint(
                        PointList[i].vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                }

            }
        }

        public override void StartCreate(DrawContext dc)
        {
            // NOP
        }

        public override void EndCreate(DrawContext dc)
        {
            if (PointCount < 2)
            {
                return;
            }

            CadVertex cp = mPointList[0];

            CadVertex a = mPointList[1];

            CadVertex b = new CadVertex(getRP(dc, cp, a, true));

            AddPoint(b);

            CadVertex c = -(a - cp) + cp;
            CadVertex d = -(b - cp) + cp;

            AddPoint(c);

            AddPoint(d);

            return;
        }

        public override void MoveSelectedPointsFromStored(DrawContext dc, Vector3d delta)
        {
            CadVertex cp = StoreList[0];

            if (cp.Selected)
            {
                mPointList[0] = cp + delta;
                mPointList[1] = StoreList[1] + delta;
                mPointList[2] = StoreList[2] + delta;
                mPointList[3] = StoreList[3] + delta;
                mPointList[4] = StoreList[4] + delta;
                return;
            }

            StackArray<CadVertex> vt = default;

            vt[0] = StoreList[1] - cp;
            vt[1] = StoreList[2] - cp;
            vt[2] = StoreList[3] - cp;
            vt[3] = StoreList[4] - cp;
            vt.Length = 4;

            if (vt[0].Norm() < 0.01)
            {
                return;
            }

            int ai = -1;

            for (int i = 0; i < 4; i++)
            {
                if (StoreList[i+1].Selected)
                {
                    ai = i;
                    break;
                }
            }

            if (ai < 0)
            {
                return;
            }

            int bi = (ai + 1) % 4;
            int ci = (ai + 2) % 4;
            int di = (ai + 3) % 4;

            Vector3d normal = CadMath.CrossProduct(vt[ai].vector, vt[bi].vector);
            normal = normal.UnitVector();

            vt[ai] += delta;

            CadVertex uva = vt[ai].UnitVector();
            CadVertex uvb = vt[bi].UnitVector();

            if (!uva.EqualsThreshold(uvb))
            {
                normal = CadMath.CrossProduct(vt[ai].vector, vt[bi].vector);

                if (normal.IsZero())
                {
                    return;
                }

                normal = normal.UnitVector();

            }

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, Math.PI / 2.0);
            CadQuaternion r = q.Conjugate();

            CadQuaternion qp = CadQuaternion.FromPoint(vt[ai].vector);
            qp = r * qp;
            qp = qp * q;

            vt[bi] = (CadVertex)qp.ToPoint();

            vt[ci] = -vt[ai];
            vt[di] = -vt[bi];

            CadVertex tmp;

            for (int i=0; i<vt.Length; i++)
            {
                tmp = vt[i];
                tmp.Selected = false;
                vt[i] = tmp;
            }

            tmp = vt[ai];
            tmp.Selected = true;
            vt[ai] = tmp;

            mPointList[1] = vt[0] + cp;
            mPointList[2] = vt[1] + cp;
            mPointList[3] = vt[2] + cp;
            mPointList[4] = vt[3] + cp;
        }

        public override Centroid GetCentroid()
        {
            Centroid ret = default;

            Vector3d cp = StoreList[0].vector;
            Vector3d rp = StoreList[1].vector;

            Vector3d d = rp - cp;

            double r = d.Norm();

            ret.Point = cp;
            ret.Area = r * r * Math.PI;

            return ret;
        }

        private Vector3d getRP(DrawContext dc, CadVertex cp, CadVertex p, bool isA)
        {
            if (p.Equals(cp))
            {
                return cp.vector;
            }


            Vector3d r = CadMath.CrossProduct(p.vector - cp.vector, dc.ViewDir);

            r = r.UnitVector();

            r = r * (p.vector - cp.vector).Norm() + cp.vector;

            return r;
        }

        public override CadSegment GetSegmentAt(int n)
        {
            return new CadSegment(CadVertex.InvalidValue, CadVertex.InvalidValue);
        }

        public override FigureSegment GetFigSegmentAt(int n)
        {
            return new FigureSegment(null, -1, -1, -1);
        }
    }
}