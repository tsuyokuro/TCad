using TCad.MathFunctions;

namespace Plotter;

public struct Centroid
{
    public bool IsInvalid;
    public vcompo_t Area;
    public vector3_t Point;

    // 三角形から作成
    public static Centroid Create(vector3_t p0, vector3_t p1, vector3_t p2)
    {
        Centroid ret = default(Centroid);
        ret.set(p0, p1, p2);
        return ret;
    }

    // 三角形で設定
    public void set(vector3_t p0, vector3_t p1, vector3_t p2)
    {
        Area = CadMath.TriangleArea(p0, p1, p2);
        Point = CadMath.TriangleCentroid(p0, p1, p2);
    }

    // 二つの重心情報から重心を求める
    public Centroid Merge(Centroid c1)
    {
        return Merge(this, c1);
    }

    // 二つの重心情報から重心を求める
    public static Centroid Merge(Centroid c0, Centroid c1)
    {
        vector3_t gpt = default;

        vcompo_t ratio = c1.Area / (c0.Area + c1.Area);

        gpt.X = (c1.Point.X - c0.Point.X) * ratio + c0.Point.X;
        gpt.Y = (c1.Point.Y - c0.Point.Y) * ratio + c0.Point.Y;
        gpt.Z = (c1.Point.Z - c0.Point.Z) * ratio + c0.Point.Z;

        Centroid ret = default(Centroid);

        ret.Area = c0.Area + c1.Area;
        ret.Point = gpt;

        return ret;
    }
}
