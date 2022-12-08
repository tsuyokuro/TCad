using OpenTK.Mathematics;
using System;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

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

        public SnapInfo(CadCursor cursor, vector3_t snapPoint, vcompo_t dist = vcompo_t.MaxValue)
        {
            Cursor = cursor;
            SnapPoint = snapPoint;
            Distance = dist;
            IsPointMatch = false;
            PriorityMatch = MatchType.NONE;
        }
    }
}
