using CadDataTypes;
using OpenTK;

namespace SplineCurve
{
    public class NurbsLine
    {
        // 制御点リスト
        public VertexList CtrlPoints = null;

        // Control point の数
        public int CtrlCnt = 0;

        // Control pointのリスト上での数
        public int CtrlDataCnt;

        public int[] CtrlOrder;


        // 出力されるPointの個数
        public int OutCnt
        {
            get
            {
                return BSplineP.OutputCnt;
            }
        }

        public BSplineParam BSplineP = new BSplineParam();

        public double[] Weights;

        public NurbsLine()
        {
        }

        public NurbsLine(
            int deg,
            int ctrlCnt,
            int divCnt,
            bool edge,
            bool close)
        {
            Setup(deg, ctrlCnt, divCnt, edge, close);
        }

        public void Setup(
            int deg,
            int ctrlCnt,
            int divCnt,
            bool edge,
            bool close)
        {
            CtrlDataCnt = ctrlCnt;

            CtrlCnt = ctrlCnt;
            if (close)
            {
                CtrlCnt += deg;
            }

            BSplineP.Setup(deg, CtrlCnt, divCnt, edge);

            SetDefaultWeights();
        }

        public void SetupDefaultCtrlOrder()
        {
            CtrlOrder = new int[CtrlCnt];

            for (int i = 0; i < CtrlCnt; i++)
            {
                CtrlOrder[i] = i % CtrlDataCnt;
            }
        }

        private Vector3d CalcPoint(double t)
        {
            Vector3d linePoint = Vector3d.Zero;
			double weight = 0f;

            double bs;

            int i;

            int di;

            for (i = 0; i < CtrlCnt; ++i)
            {
				bs = BSplineP.BasisFunc(i, t);

                di = CtrlOrder[i];

                linePoint += bs * Weights[di] * CtrlPoints[di].vector;

                weight += bs * Weights[di];
			}

            return linePoint / weight;
		}

        public void Eval(VertexList vl)
        {
            for (int p = 0; p <= BSplineP.DivCnt; ++p)
            {
                double t = p * BSplineP.Step + BSplineP.LowKnot;
                if (t >= BSplineP.HighKnot)
                {
                    t = BSplineP.HighKnot - BSpline.Epsilon;
                }

                vl.Add( new CadVertex(CalcPoint(t)) );
            }
        }

        public void SetDefaultWeights()
        {
            Weights = new double[CtrlDataCnt];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = 1f;
            }
        }

        public double GetWeight(int u, int v)
        {
            return Weights[v * CtrlDataCnt + u];
        }

        public void SetWeight(int u, int v, double val)
        {
            Weights[v * CtrlDataCnt + u] = val;
        }
    }
}