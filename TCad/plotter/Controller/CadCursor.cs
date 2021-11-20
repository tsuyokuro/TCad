using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public struct CadCursor
    {
        public Vector3d Pos;
        public Vector3d DirX;
        public Vector3d DirY;

        public Vector3d StorePos;

        public static CadCursor Create()
        {
            CadCursor cc = default(CadCursor);

            cc.DirX = Vector3d.UnitX;
            cc.DirY = Vector3d.UnitY;

            return cc;
        }

        public static CadCursor Create(Vector3d pixp)
        {
            CadCursor cc = default(CadCursor);

            cc.Pos = pixp;
            cc.DirX = Vector3d.UnitX;
            cc.DirY = Vector3d.UnitY;

            return cc;
        }

        public void Store()
        {
            StorePos = Pos;
        }

        public Vector3d DistanceX(Vector3d pixp)
        {
            Vector3d a1 = Pos;
            Vector3d a2 = Pos + DirY;

            Vector3d b1 = pixp;
            Vector3d b2 = pixp + DirX;

            Vector3d c = CadMath.CrossLine2D(a1, a2, b1, b2);

            return pixp - c;
        }

        public Vector3d DistanceY(Vector3d pixp)
        {
            Vector3d a1 = Pos;
            Vector3d a2 = Pos + DirX;

            Vector3d b1 = pixp;
            Vector3d b2 = pixp + DirY;

            Vector3d c = CadMath.CrossLine2D(a1, a2, b1, b2);

            return pixp - c;
        }
    }
}