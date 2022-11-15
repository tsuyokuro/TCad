using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter;

public static class MatrixExt
{
    public static Matrix4d Inv(this Matrix4d m)
    {
        return Matrix4d.Invert(m);
    }

    public static void dump(this Matrix4d m, string name = "")
    {
        DOut.pl(nameof(Matrix4d) + " " + name + " {");
        DOut.Indent++;
        DOut.pl(m.M11.ToString() + ", " + m.M12.ToString() + ", " + m.M13.ToString() + ", " + m.M14.ToString());
        DOut.pl(m.M21.ToString() + ", " + m.M22.ToString() + ", " + m.M23.ToString() + ", " + m.M24.ToString());
        DOut.pl(m.M31.ToString() + ", " + m.M32.ToString() + ", " + m.M33.ToString() + ", " + m.M34.ToString());
        DOut.pl(m.M41.ToString() + ", " + m.M42.ToString() + ", " + m.M43.ToString() + ", " + m.M44.ToString());
        DOut.Indent--;
        DOut.pl("}");
    }
}
