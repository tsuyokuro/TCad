using CadDataTypes;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public class CadFigurePicture : CadFigure
    {
        //  3-------------------2
        //  |                   |
        //  |                   |
        //  |                   |
        //  0-------------------1

        private Bitmap mBitmap;
        

        public CadFigurePicture()
        {
            Type = Types.PICTURE;
        }

        public void Setup(PaperPageSize pageSize, Vector3d pos, String path)
        {
            mBitmap = new Bitmap(Image.FromFile(path));

            mBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            double uw = mBitmap.Width;
            double uh = mBitmap.Height;

            double w;
            double h;

            if (pageSize.Width <= pageSize.Height)
            {
                w = pageSize.Width * 0.5;
                h = w * (uh / uw);
            }
            else
            {
                h = pageSize.Height * 0.5;
                w = h * (uw / uh);
            }

            CadVertex tv = (CadVertex)pos;
            CadVertex ov = tv;

            mPointList.Clear();

            mPointList.Add(tv);

            tv = ov;
            tv.X += w;
            mPointList.Add(tv);

            tv = ov;
            tv.X += w;
            tv.Y += h;
            mPointList.Add(tv);

            tv = ov;
            tv.Y += h;
            mPointList.Add(tv);
        }

        public override void AddPoint(CadVertex p)
        {
            mPointList.Add(p);
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

        public override void AddPointInCreating(DrawContext dc, CadVertex p)
        {
            PointList.Add(p);
        }

        public override void SetPointAt(int index, CadVertex pt)
        {
            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 4)
            {
                mPointList.Clear();
            }
        }

        public override void Draw(DrawContext dc)
        {
            DrawPicture(dc, dc.GetPen(DrawTools.PEN_DIMENTION));
        }

        public override void Draw(DrawContext dc, DrawParams dp)
        {
            DrawPicture(dc, dp.LinePen);
        }

        private void DrawPicture(DrawContext dc, DrawPen linePen)
        {
            ImageRenderer renderer = ImageRenderer.Provider.Get();

            Vector3d xv = (Vector3d)(mPointList[1] - mPointList[0]);
            Vector3d yv = (Vector3d)(mPointList[3] - mPointList[0]);

            renderer.Render(mBitmap, (Vector3d)mPointList[0], xv, yv);

            dc.Drawing.DrawLine(linePen, mPointList[0].vector, mPointList[1].vector);
            dc.Drawing.DrawLine(linePen, mPointList[1].vector, mPointList[2].vector);
            dc.Drawing.DrawLine(linePen, mPointList[2].vector, mPointList[3].vector);
            dc.Drawing.DrawLine(linePen, mPointList[3].vector, mPointList[0].vector);
        }


        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
        }

        public override void DrawSelected(DrawContext dc)
        {
            foreach (CadVertex p in PointList)
            {
                if (p.Selected)
                {
                    dc.Drawing.DrawSelectedPoint(p.vector, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                }
            }
        }

        public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
        {
            int cnt = PointList.Count;

            if (cnt < 1) return;

            if (cnt == 1)
            {
                //DrawDim(dc, PointList[0], tp, tp, pen);
                return;
            }

            //DrawDim(dc, PointList[0], PointList[1], tp, pen);
        }

        public override void StartCreate(DrawContext dc)
        {
        }

        public override void EndCreate(DrawContext dc)
        {
            if (PointList.Count < 3)
            {
                return;
            }

            CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1], PointList[2]);

            PointList[2] = PointList[2].SetVector(seg.P1.vector);
            PointList.Add(seg.P0);
        }

        public override void MoveSelectedPointsFromStored(DrawContext dc, Vector3d delta)
        {
            if (PointList[0].Selected && PointList[1].Selected &&
                PointList[2].Selected && PointList[3].Selected)
            {
                PointList[0] = StoreList[0] + delta;
                PointList[1] = StoreList[1] + delta;
                PointList[2] = StoreList[2] + delta;
                PointList[3] = StoreList[3] + delta;
                return;
            }

            if (PointList[2].Selected || PointList[3].Selected)
            {
                Vector3d v0 = StoreList[3].vector - StoreList[0].vector;

                if (v0.IsZero())
                {
                    // 移動方向が不定の場合
                    MoveSelectedPointWithHeight(dc, delta);
                    return;
                }

                Vector3d v0u = v0.UnitVector();

                double d = CadMath.InnerProduct(v0u, delta);

                Vector3d vd = v0u * d;

                CadVertex nv3 = StoreList[3] + vd;
                CadVertex nv2 = StoreList[2] + vd;

                if (nv3.EqualsThreshold(StoreList[0], 0.001) ||
                    nv2.EqualsThreshold(StoreList[1], 0.001))
                {
                    return;
                }

                PointList[3] = nv3;
                PointList[2] = nv2;

                return;
            }

            if (PointList[0].Selected || PointList[1].Selected)
            {
                Vector3d v0 = StoreList[0].vector;
                Vector3d v1 = StoreList[1].vector;
                Vector3d v2 = StoreList[2].vector;
                Vector3d v3 = StoreList[3].vector;

                Vector3d lv = v3 - v0;
                double h = lv.Norm();

                Vector3d planeNormal = CadMath.Normal(v0, v1, v2);

                Vector3d cp0 = v0;
                Vector3d cp1 = v1;

                if (PointList[0].Selected)
                {
                    cp0 = CadMath.CrossPlane(v0 + delta, v0, planeNormal);
                }

                if (PointList[1].Selected)
                {
                    cp1 = CadMath.CrossPlane(v1 + delta, v1, planeNormal);
                }

                if (cp0.EqualsThreshold(cp1, 0.001))
                {
                    return;
                }

                if (PointList[0].Selected)
                {
                    PointList[0] = PointList[0].SetVector(cp0);
                }

                if (PointList[1].Selected)
                {
                    PointList[1] = PointList[1].SetVector(cp1);
                }

                Vector3d normal = CadMath.Normal(cp0, cp0 + planeNormal, cp1);
                Vector3d d = normal * h;

                PointList[3] = PointList[3].SetVector(PointList[0] + d);
                PointList[2] = PointList[2].SetVector(PointList[1] + d);
            }
        }

        public override void InvertDir()
        {
        }

        // 高さが０の場合、移動方向が定まらないので
        // 投影座標系でz=0とした座標から,List[0] - List[1]への垂線を計算して
        // そこへ移動する
        private void MoveSelectedPointWithHeight(DrawContext dc, Vector3d delta)
        {
            CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1],
                StoreList[2] + delta);

            PointList[2] = PointList[2].SetVector(seg.P1.vector);
            PointList[3] = PointList[3].SetVector(seg.P0.vector);
        }

        public override void EndEdit()
        {
            base.EndEdit();

            if (PointList.Count == 0)
            {
                return;
            }

            CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1], PointList[2]);

            PointList[2] = PointList[2].SetVector(seg.P1.vector);
            PointList[3] = PointList[3].SetVector(seg.P0.vector);
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