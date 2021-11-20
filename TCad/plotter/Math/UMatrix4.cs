// 4行4列の行列
using OpenTK;
using CadDataTypes;

namespace Plotter
{
    public struct UMatrix4
    {
        public Matrix4d Matrix;

        public double M11
        {
            set { Matrix.M11 = value; }
            get { return Matrix.M11; }
        }

        public double M12
        {
            set { Matrix.M12 = value; }
            get { return Matrix.M12; }
        }

        public double M13
        {
            set { Matrix.M13 = value; }
            get { return Matrix.M13; }
        }

        public double M14
        {
            set { Matrix.M14 = value; }
            get { return Matrix.M14; }
        }


        public double M21
        {
            set { Matrix.M21 = value; }
            get { return Matrix.M21; }
        }

        public double M22
        {
            set { Matrix.M22 = value; }
            get { return Matrix.M22; }
        }

        public double M23
        {
            set { Matrix.M23 = value; }
            get { return Matrix.M23; }
        }

        public double M24
        {
            set { Matrix.M24 = value; }
            get { return Matrix.M24; }
        }


        public double M31
        {
            set { Matrix.M31 = value; }
            get { return Matrix.M31; }
        }

        public double M32
        {
            set { Matrix.M32 = value; }
            get { return Matrix.M32; }
        }

        public double M33
        {
            set { Matrix.M33 = value; }
            get { return Matrix.M33; }
        }

        public double M34
        {
            set { Matrix.M34 = value; }
            get { return Matrix.M34; }
        }


        public double M41
        {
            set { Matrix.M41 = value; }
            get { return Matrix.M41; }
        }

        public double M42
        {
            set { Matrix.M42 = value; }
            get { return Matrix.M42; }
        }

        public double M43
        {
            set { Matrix.M43 = value; }
            get { return Matrix.M43; }
        }

        public double M44
        {
            set { Matrix.M44 = value; }
            get { return Matrix.M44; }
        }

        public UMatrix4(
            double a11, double a12, double a13, double a14,
            double a21, double a22, double a23, double a24,
            double a31, double a32, double a33, double a34,
            double a41, double a42, double a43, double a44
            )
        {
            Matrix = default(Matrix4d);

            M11 = a11; M12 = a12; M13 = a13; M14 = a14;
            M21 = a21; M22 = a22; M23 = a23; M24 = a24;
            M31 = a31; M32 = a32; M33 = a33; M34 = a34;
            M41 = a41; M42 = a42; M43 = a43; M44 = a44;
        }

        public UMatrix4(Matrix4d m4d)
        {
            Matrix = m4d;
        }

        public UMatrix4(double [] data)
        {
            // Row-major
            //Matrix = new Matrix4d(
            //    data[0], data[4], data[8], data[12],
            //    data[1], data[5], data[9], data[13],
            //    data[2], data[6], data[10], data[14],
            //    data[3], data[7], data[11], data[15]);

            // Column-major
            Matrix = new Matrix4d(
                data[0], data[1], data[2], data[3],
                data[4], data[5], data[6], data[7],
                data[8], data[9], data[10], data[11],
                data[12], data[13], data[14], data[15]);
        }

        public static UMatrix4 Scale(double s)
        {
            return Matrix4d.Scale(s);
        }

        public static UMatrix4 operator *(UMatrix4 m1, UMatrix4 m2)
        {
            return product(m1, m2);
        }

        public static implicit operator UMatrix4(Matrix4d m)
        {
            return new UMatrix4(m);
        }

        public static implicit operator Matrix4d(UMatrix4 m)
        {
            return m.Matrix;
        }

        // 通常OpenGLのVectorは、列Vectorらしいのだが、
        // Vector4d.Transformは、行Vectorとして扱うみたい
        //              |m11, m12, m13, m14|
        //              |m21, m22, m23, m24|
        // (x, y, z, w) |m31, m32, m33, m34|
        //              |m41, m42, m43, m44|
        public static Vector4d operator *(Vector4d v, UMatrix4 m)
        {
            return product(v, m);
        }

        public static Vector4d product(Vector4d p, UMatrix4 m)
        {
            Vector4d v = Vector4d.Transform(p, m.Matrix);
            return v;
        }

        public static UMatrix4 product(UMatrix4 m1, UMatrix4 m2)
        {
            UMatrix4 r = default(UMatrix4);

            r.Matrix = Matrix4d.Mult(m1.Matrix, m2.Matrix);
            return r;
        }

        public UMatrix4 Invert()
        {
            return new UMatrix4(Matrix4d.Invert(Matrix));
        }

        public static readonly UMatrix4 Unit = new UMatrix4
            (
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

        public void dump(string name = "")
        {
            DOut.pl(nameof(UMatrix4) + " " + name + " {");
            DOut.Indent++;
            DOut.pl(M11.ToString() + ", " + M12.ToString() + ", " + M13.ToString() + ", " + M14.ToString());
            DOut.pl(M21.ToString() + ", " + M22.ToString() + ", " + M23.ToString() + ", " + M24.ToString());
            DOut.pl(M31.ToString() + ", " + M32.ToString() + ", " + M33.ToString() + ", " + M34.ToString());
            DOut.pl(M41.ToString() + ", " + M42.ToString() + ", " + M43.ToString() + ", " + M44.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}