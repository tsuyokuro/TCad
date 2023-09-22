//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace Plotter;

public static class MatrixExt
{
    public static matrix4_t Inv(this matrix4_t m)
    {
        return matrix4_t.Invert(m);
    }

    public static void dump(this matrix4_t m, string name = "")
    {
        DOut.pl(nameof(matrix4_t) + " " + name + " {");
        DOut.Indent++;
        DOut.pl(m.M11.ToString() + ", " + m.M12.ToString() + ", " + m.M13.ToString() + ", " + m.M14.ToString());
        DOut.pl(m.M21.ToString() + ", " + m.M22.ToString() + ", " + m.M23.ToString() + ", " + m.M24.ToString());
        DOut.pl(m.M31.ToString() + ", " + m.M32.ToString() + ", " + m.M33.ToString() + ", " + m.M34.ToString());
        DOut.pl(m.M41.ToString() + ", " + m.M42.ToString() + ", " + m.M43.ToString() + ", " + m.M44.ToString());
        DOut.Indent--;
        DOut.pl("}");
    }
}
