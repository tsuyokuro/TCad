using System;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public class Gridding
    {
        private Vector3d mGridSize;

        public Vector3d GridSize
        {
            set
            {
                mGridSize = value;
            }

            get
            {
                return mGridSize;
            }
        }

        public double Range = 8;

        public Vector3d MatchW;
        public Vector3d MatchD;

        public Gridding()
        {
            GridSize = new Vector3d(10, 10, 10);
        }

        public void Clear()
        {

        }

        public void CopyFrom(Gridding g)
        {
            mGridSize = g.mGridSize;
            Range = g.Range;
        }

        public void Check(DrawContext dc, Vector3d scrp)
        {
            Vector3d p = dc.DevPointToWorldPoint(scrp);

            p.X = (long)((p.X + Math.Sign(p.X) * (GridSize.X / 2.0)) / GridSize.X) * GridSize.X;
            p.Y = (long)((p.Y + Math.Sign(p.Y) * (GridSize.Y / 2.0)) / GridSize.Y) * GridSize.Y;
            p.Z = (long)((p.Z + Math.Sign(p.Z) * (GridSize.Z / 2.0)) / GridSize.Z) * GridSize.Z;

            MatchW = p;
            MatchD = dc.WorldPointToDevPoint(p);
        }


        /**
         * 画面上での間隔が min より大きくなるように間引く為のサイズの
         * 倍率を求める
         */
        public double Decimate(DrawContext dc, Gridding grid, double min)
        {
            double scaleX = 1.0;
            double scaleY = 1.0;
            double scaleZ = 1.0;

            double gridSizeX = grid.GridSize.X;
            double gridSizeY = grid.GridSize.Y;
            double gridSizeZ = grid.GridSize.Z;

            Vector3d devLen;
            double t = 1;
            double d;

            double devLenX;
            double devLenY;
            double devLenZ;

            // X axis
            devLen = dc.WorldVectorToDevVector(new Vector3d(gridSizeX, 0, 0));

            devLenX = Math.Max(Math.Abs(devLen.X), Math.Abs(devLen.Y));
            if (devLenX != 0 && devLenX < min)
            {
                d = Math.Ceiling(min / devLenX) * devLenX;
                t = d / devLenX;
            }

            if (t > scaleX)
            {
                scaleX = t;
            }


            // Y axis
            devLen = dc.WorldVectorToDevVector(new Vector3d(0, gridSizeY, 0));

            devLenY = Math.Max(Math.Abs(devLen.X), Math.Abs(devLen.Y));
            if (devLenY != 0 && devLenY < min)
            {
                d = Math.Ceiling(min / devLenY) * devLenY;
                t = d / devLenY;
            }

            if (t > scaleY)
            {
                scaleY = t;
            }


            // Z axis
            devLen = dc.WorldVectorToDevVector(new Vector3d(0, 0, gridSizeZ));

            devLenZ = Math.Max(Math.Abs(devLen.X), Math.Abs(devLen.Y));

            if (devLenZ != 0 && devLenZ < min)
            {
                d = Math.Ceiling(min / devLenZ) * devLenZ;
                t = d / devLenZ;
            }

            if (t > scaleZ)
            {
                scaleZ = t;
            }

            return Math.Max(Math.Max(scaleX, scaleY), scaleZ);
        }
    }
}
