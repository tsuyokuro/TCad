using CadDataTypes;
using OpenTK.Mathematics;

namespace Plotter
{
    // 直方体の対角線を保持
    public struct CadRect
    {
        public Vector3d p0;
        public Vector3d p1;

        public void Normalize()
        {
            Vector3d minv = p0;
            Vector3d maxv = p0;

            if (p0.X < p1.X)
            {
                maxv.X = p1.X;
            }
            else
            {
                minv.X = p1.X;
            }

            if (p0.Y < p1.Y)
            {
                maxv.Y = p1.Y;
            }
            else
            {
                minv.Y = p1.Y;
            }

            if (p0.Z < p1.Z)
            {
                maxv.Z = p1.Z;
            }
            else
            {
                minv.Z = p1.Z;
            }

            p0 = minv;
            p1 = maxv;
        }

        public Vector3d Center()
        {
            Vector3d cv = default;

            cv.X = p0.X + ((p1.X - p0.X) / 2.0);
            cv.Y = p0.Y + ((p1.Y - p0.Y) / 2.0);
            cv.Z = p0.Z + ((p1.Z - p0.Z) / 2.0);

            return cv;
        }
    }
}