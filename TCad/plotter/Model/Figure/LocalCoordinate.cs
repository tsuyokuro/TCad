using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public struct LocalCoordinate
{
    public vector3_t BasePoint;

    public LocalCoordinate(vector3_t v = default)
    {
        BasePoint = v;
    }

    vector3_t Trans(vector3_t vector)
    {
        return vector + BasePoint;
    }
}
