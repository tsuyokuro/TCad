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
        public Vector3d BasePoint = default;

        public LocalCoordinate()
        {
        }

        Vector3d Trans(Vector3d vector)
        {
            return vector + BasePoint;
        }
    }
}
