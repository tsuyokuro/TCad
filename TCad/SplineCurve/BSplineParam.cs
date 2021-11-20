
namespace SplineCurve
{
    public class BSplineParam
    {
        // 次数
        public int Degree = 3;

        // 分割数
        public int DivCnt = 0;

        // 出力Point数
        public int OutputCnt = 0;

        // Knot数
        public int KnotCnt;

        public double[] Knots;

        public double LowKnot = 0;

        public double HighKnot = 0;

        public double Step = 0;

        // i: Knot番号
        // t: 媒介変数
        public double BasisFunc(int i, double t)
        {
            return BSpline.BasisFunc(i, Degree, t, Knots);
        }

        public void Setup(int degree, int ctrlCnt, int divCnt, bool passOnEdge)
        {
            Degree = degree;
            //CtrlCnt = ctrlCnt;
            KnotCnt = ctrlCnt + Degree + 1;
            DivCnt = divCnt;
            OutputCnt = DivCnt + 1;

            CreateDefaultKnots(passOnEdge);

            LowKnot = Knots[Degree];
            HighKnot = Knots[ctrlCnt];
            Step = (HighKnot - LowKnot) / (double)DivCnt;
        }

        public void CreateDefaultKnots(bool passOnEdge)
        {
            Knots = new double[KnotCnt];

            double x = 0.0;

            for (int i = 0; i < KnotCnt; i++)
            {
                if (passOnEdge && (i < Degree || i > (KnotCnt - Degree - 2)))
                {
                    Knots[i] = x;
                }
                else
                {
                    Knots[i] = x;
                    x += 1.0;
                }
            }
        }
    }
}

