using System;

namespace Plotter
{
    /**
     *  汎用行列
     *  
     */
    public class MatrixMN
    {
        public int RN = 0;
        public int CN = 0;

        public double[,] v; // RowNum, ColNum

        // 初期化例
        // MatrixMN m1 = new MatrixMN(new double[,]
        // {
        //     { 11, 12, 13 },
        //     { 21, 22, 23 },
        //     { 31, 32, 33 }
        // });

        public MatrixMN(double[,] a)
        {
            Attach(a);
        }

        public MatrixMN(int rownum, int colnum)
        {
            v = new double[rownum, colnum];
            RN = v.GetLength(0);
            CN = v.GetLength(1);
        }

        public void Set(MatrixMN m)
        {
            v = new double[m.RN, m.CN];
            RN = m.RN;
            CN = m.CN;

            v = new double[RN, CN];

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < RN; r++)
                {
                    v[r, c] = m.v[r, c];
                }
            }
        }

        public void Set(double[,] a)
        {
            v = a;
            RN = a.GetLength(0);
            CN = a.GetLength(1);

            v = new double[RN, CN];

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < RN; r++)
                {
                    v[r, c] = a[r, c];
                }
            }
        }

        public void Attach(double[,] a)
        {
            v = a;
            RN = v.GetLength(0);
            CN = v.GetLength(1);
        }

        public MatrixMN Product(MatrixMN right)
        {
            return Product(this, right);
        }

        public static MatrixMN operator *(MatrixMN m1, MatrixMN m2)
        {
            return Product(m1, m2);
        }

        public static MatrixMN Product(MatrixMN m1, MatrixMN m2)
        {
            if (m1.CN != m2.RN)
            {
                return null;
            }

            int row3 = Math.Min(m1.RN, m2.RN);
            int col3 = Math.Min(m1.CN, m2.CN);

            MatrixMN ret = new MatrixMN(row3, col3);

            int col1 = m1.CN;
            int row1 = m1.RN;

            int col2 = m2.CN;
            int row2 = m2.RN;


            for (int r = 0; r < row3; r++)
            {
                for (int c = 0; c < col3; c++)
                {
                    for (int k = 0; k < col1; k++)
                    {
                        ret.v[r, c] += m1.v[r, k] * m2.v[k, c];
                    }
                }
            }

            return ret;
        }

        public void dump()
        {
            DOut.pl(nameof(MatrixMN) + "{");
            DOut.Indent++;

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < CN; c++)
                {
                    DOut.p(v[r, c].ToString() + ",");
                }
                DOut.pl("");
            }

            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
