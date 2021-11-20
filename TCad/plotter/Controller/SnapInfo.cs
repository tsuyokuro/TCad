using System;
using OpenTK;

namespace Plotter.Controller
{
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
            public Vector3d SnapPoint;
            public double Distance;

            public bool IsPointMatch;

            public MatchType PriorityMatch;

            public SnapInfo(CadCursor cursor, Vector3d snapPoint, double dist = Double.MaxValue)
            {
                Cursor = cursor;
                SnapPoint = snapPoint;
                Distance = dist;
                IsPointMatch = false;
                PriorityMatch = MatchType.NONE;
            }
        }
    }
}
