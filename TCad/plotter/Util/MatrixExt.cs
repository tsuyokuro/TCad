namespace Plotter;

public static class MatrixExt
{
    public static matrix4_t Inv(this matrix4_t m)
    {
        return matrix4_t.Invert(m);
    }

    public static void dump(this matrix4_t m, string name = "")
    {
        Log.pl(nameof(matrix4_t) + " " + name + " {");
        Log.Indent++;
        Log.pl(m.M11.ToString() + ", " + m.M12.ToString() + ", " + m.M13.ToString() + ", " + m.M14.ToString());
        Log.pl(m.M21.ToString() + ", " + m.M22.ToString() + ", " + m.M23.ToString() + ", " + m.M24.ToString());
        Log.pl(m.M31.ToString() + ", " + m.M32.ToString() + ", " + m.M33.ToString() + ", " + m.M34.ToString());
        Log.pl(m.M41.ToString() + ", " + m.M42.ToString() + ", " + m.M43.ToString() + ", " + m.M44.ToString());
        Log.Indent--;
        Log.pl("}");
    }
}
