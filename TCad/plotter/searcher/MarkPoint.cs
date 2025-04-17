using System;

namespace Plotter;

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

    public vector3_t Point;     // Match座標 (World座標系)

    public vector3_t PointScrn; // Match座標 (Screen座標系)

    public vcompo_t DistanceX;    // X距離 (Screen座標系)
    public vcompo_t DistanceY;    // Y距離 (Screen座標系)

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
        Log.pl(name + " {");
        if (Figure != null)
        {
            Log.pl($"FigID:{Figure.ID}");
        }
        Log.pl($"PointIndex:{PointIndex}");
        Point.dump("Point");
        PointScrn.dump("PointScrn");
        Log.pl("}");
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
