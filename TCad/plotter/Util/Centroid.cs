using OpenTK;

namespace Plotter
{
    public struct Centroid
    {
        public bool IsInvalid;
        public double Area;
        public Vector3d Point;

        // 三角形から作成
        public static Centroid Create(Vector3d p0, Vector3d p1, Vector3d p2)
        {
            Centroid ret = default(Centroid);
            ret.set(p0, p1, p2);
            return ret;
        }

        // 三角形で設定
        public void set(Vector3d p0, Vector3d p1, Vector3d p2)
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
            Vector3d gpt = default;

            double ratio = c1.Area / (c0.Area + c1.Area);

            gpt.X = (c1.Point.X - c0.Point.X) * ratio + c0.Point.X;
            gpt.Y = (c1.Point.Y - c0.Point.Y) * ratio + c0.Point.Y;
            gpt.Z = (c1.Point.Z - c0.Point.Z) * ratio + c0.Point.Z;

            Centroid ret = default(Centroid);

            ret.Area = c0.Area + c1.Area;
            ret.Point = gpt;

            return ret;
        }
    }
}
