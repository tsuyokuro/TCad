using OpenTK.Mathematics;
using System;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter.Controller;

public partial class PlotterController
{
    public struct SnapInfo
    {
        public enum MatchType
        {
            NONE,
            X_MATCH,
            Y_MATCH,
            POINT_MATCH,
        }

        public CadCursor Cursor;
        public vector3_t SnapPoint;
        public vcompo_t Distance;

        public bool IsPointMatch;

        public MatchType PriorityMatch;

        public SnapInfo(CadCursor cursor, vector3_t snapPoint, vcompo_t dist = Double.MaxValue)
        {
            Cursor = cursor;
            SnapPoint = snapPoint;
            Distance = dist;
            IsPointMatch = false;
            PriorityMatch = MatchType.NONE;
        }
    }
}
