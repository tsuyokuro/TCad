using OpenTK;
using System;

namespace Plotter
{
    public struct MarkPoint : IEquatable<MarkPoint>
    {
        public bool IsValid;

        public CadLayer Layer;

        public uint LayerID
        {
            get
            {
                if (Layer == null)
                {
                    return 0;
                }

                return Layer.ID;
            }
        }

        public CadFigure Figure;

        public uint FigureID
        {
            get
            {
                if (Figure == null)
                {
                    return 0;
                }

                return Figure.ID;
            }
        }

        public int PointIndex;

        public Vector3d Point;     // Match座標 (World座標系)

        public Vector3d PointScrn; // Match座標 (Screen座標系)

        public double DistanceX;    // X距離 (Screen座標系)
        public double DistanceY;    // Y距離 (Screen座標系)

        public void reset()
        {
            this = default;

            IsValid = false;

            Figure = null;

            DistanceX = CadConst.MaxValue;
            DistanceY = CadConst.MaxValue;
        }

        public bool IsSelected()
        {
            if (Figure == null)
            {
                return false;
            }

            return Figure.IsPointSelected(PointIndex);
        }

        public bool update()
        {
            if (Figure == null)
            {
                return true;
            }

            if (PointIndex >= Figure.PointList.Count)
            {
                return false;
            }

            return true;
        }

        public void dump(string name = "MarkPoint")
        {
            DOut.pl(name + " {");
            if (Figure != null)
            {
                DOut.pl($"FigID:{Figure.ID}");
            }
            DOut.pl($"PointIndex:{PointIndex}");
            Point.dump("Point");
            PointScrn.dump("PointScrn");
            DOut.pl("}");
        }

        public bool Equals(MarkPoint other)
        {
            return Layer == other.Layer &&
                Figure == other.Figure &&
                PointIndex == other.PointIndex &&
                Point.Equals(other.Point);
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode() + 
                (int)(Layer == null ? 0 : Layer.ID) +
                (int)(Figure == null ? 0 : Figure.ID);
        }
    }
}
