using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct MoveInfo
    {
        public Vector3d Start;

        public Vector3d Moved;

        public Vector3d MoveOrgScrnPoint;

        public Vector3d CursorScrnPoint;

        public Vector3d Delta;

        public MoveInfo(Vector3d start, Vector3d moved, Vector3d moveOrgScr, Vector3d cursorPos)
        {
            Start = start;
            Moved = moved;
            MoveOrgScrnPoint = moveOrgScr;
            CursorScrnPoint = cursorPos;
            Delta = Moved - start;
        }
        public MoveInfo()
        {
            Start = VectorExt.InvalidVector3d;
            Moved = VectorExt.InvalidVector3d;
            MoveOrgScrnPoint = VectorExt.InvalidVector3d;
            CursorScrnPoint = VectorExt.InvalidVector3d;

            Delta = Vector3d.Zero;
        }
    }
}
