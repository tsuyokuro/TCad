using CadDataTypes;
using OpenTK;

namespace SplineCurve
{
    public class NurbsSurface
    {
        // Control pointの数
        public int UCtrlCnt = 5;
        public int VCtrlCnt = 5;

        // Control pointのデータ数
        public int UCtrlDataCnt = 5;
        public int VCtrlDataCnt = 5;

        // 制御点リスト
        public VertexList CtrlPoints = null;

        // Weight情報
        public double[] Weights;

        public int[] CtrlOrder;

        public BSplineParam UBSpline = new BSplineParam();
        public BSplineParam VBSpline = new BSplineParam();

        // U方向の出力されるPointの数
        public int UOutCnt
        {
            get { return UBSpline.OutputCnt; }
        }

        // V方向の出力されるPointの数
        public int VOutCnt
        {
            get { return VBSpline.OutputCnt; }
        }

        public NurbsSurface()
        {
        }

        public NurbsSurface(
            int deg,
            int uCtrlCnt, int vCtrlCnt,
            int uDivCnt, int vDivCnt,
            bool uedge, bool vedge,
            bool uclose, bool vclose
            )
        {
            Setup(
                deg,
                uCtrlCnt, vCtrlCnt,
                uDivCnt, vDivCnt,
                uedge, vedge,
                uclose, vclose
                );
        }

        public void Setup(
            int deg,
            int uCtrlCnt, int vCtrlCnt,
            int uDivCnt, int vDivCnt,
            bool uedge, bool vedge,
            bool uclose, bool vclose
            )
        {
            UCtrlDataCnt = uCtrlCnt;
            VCtrlDataCnt = vCtrlCnt;

            UCtrlCnt = uCtrlCnt;
            if (uclose)
            {
                UCtrlCnt += deg;
            }

            VCtrlCnt = vCtrlCnt;
            if (vclose)
            {
                VCtrlCnt += deg;
            }

            UBSpline.Setup(deg, UCtrlCnt, uDivCnt, uedge);
            VBSpline.Setup(deg, VCtrlCnt, vDivCnt, vedge);

            SetDefaultWeights();
        }

        public void SetupDefaultCtrlOrder()
        {
            int ucnt = UCtrlCnt;
            int vcnt = VCtrlCnt;
            int udcnt = UCtrlDataCnt;
            int vdcnt = VCtrlDataCnt;

            int[] order = new int[ucnt * vcnt];

            for (int j = 0; j < vcnt; j++)
            {
                int ds = (j % vdcnt) * udcnt;
                int s = j * ucnt;

                for (int i = 0; i < ucnt; i++)
                {
                    order[s + i] = ds + (i % udcnt);
                }
            }

            CtrlOrder = order;
        }

        public void SetDefaultWeights()
        {
            Weights = new double[UCtrlDataCnt*VCtrlDataCnt];

            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = 1.0;
            }
        }

        private Vector3d CalcPoint(double u, double v)
        {
            Vector3d pt = Vector3d.Zero;

            double weight = 0f;

            int sp;

            int vcnt = VCtrlCnt;
            int ucnt = UCtrlCnt;

            for (int j = 0; j < vcnt; ++j)
            {
                sp = ucnt * j;

                for (int i = 0; i < ucnt; ++i)
                {
                    double ubs = UBSpline.BasisFunc(i, u);
                    double vbs = VBSpline.BasisFunc(j, v);

                    int cp = CtrlOrder[sp + i];

                    pt += (ubs * vbs * Weights[cp]) * CtrlPoints[cp].vector;

                    weight += ubs * vbs * Weights[cp];
                }
            }

            return pt / weight;
        }

        public double GetWeight(int u, int v)
        {
            return Weights[v * UCtrlDataCnt + u];
        }

        public void SetWeight(int u, int v, double val)
        {
            Weights[v * UCtrlDataCnt + u] = val;
        }

        public void Eval(VertexList vl)
        {
            double u;
            double v;

            for (int j = 0; j <= VBSpline.DivCnt; ++j)
            {
                v = j * VBSpline.Step + VBSpline.LowKnot;
                if (v >= VBSpline.HighKnot)
                {
                    v = VBSpline.HighKnot - BSpline.Epsilon;
                }

                for (int i = 0; i <= UBSpline.DivCnt; ++i)
                {
                    u = i * UBSpline.Step + UBSpline.LowKnot;
                    if (u >= UBSpline.HighKnot)
                    {
                        u = UBSpline.HighKnot - BSpline.Epsilon;
                    }

                    vl.Add(new CadVertex(CalcPoint(u, v)));
                }
            }
        }
    }
}
