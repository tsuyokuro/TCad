using System.Collections.Generic;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public class CadRegion2D
{
    public vcompo_t X;
    public vcompo_t Y;
    public List<List<vcompo_t>> Data = new List<List<vcompo_t>>();
}
