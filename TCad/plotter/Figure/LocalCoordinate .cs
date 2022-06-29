using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class LocalCoordinate
    {
        public Matrix4d Matrix;

        public LocalCoordinate()
        {
            Matrix = Matrix4d.Scale(1.0);
        }
    }
}
