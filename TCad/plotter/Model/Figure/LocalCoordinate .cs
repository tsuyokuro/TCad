using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct LocalCoordinate
    {
        public Vector3d BasePoint;

        public LocalCoordinate(Vector3d v = default)
        {
            BasePoint = v;
        }

        Vector3d Trans(Vector3d vector)
        {
            return vector + BasePoint;
        }
    }
}
