using CadDataTypes;

namespace Plotter
{
    // 直方体の対角線を保持
    public struct CadRect
    {
        public CadVertex p0;
        public CadVertex p1;

        public void Normalize()
        {
            CadVertex minv = p0;
            CadVertex maxv = p0;

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
    }
}