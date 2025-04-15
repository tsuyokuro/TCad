using CadDataTypes;
using OpenTK;
using OpenTK.Mathematics;

namespace SplineCurve;

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
    public vcompo_t[] Weights;

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
        Weights = new vcompo_t[UCtrlDataCnt*VCtrlDataCnt];

        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = (vcompo_t)(1.0);
        }
    }

    private vector3_t CalcPoint(vcompo_t u, vcompo_t v)
    {
        vector3_t pt = vector3_t.Zero;

        vcompo_t weight = 0f;

        int sp;

        int vcnt = VCtrlCnt;
        int ucnt = UCtrlCnt;

        for (int j = 0; j < vcnt; ++j)
        {
            sp = ucnt * j;

            for (int i = 0; i < ucnt; ++i)
            {
                vcompo_t ubs = UBSpline.BasisFunc(i, u);
                vcompo_t vbs = VBSpline.BasisFunc(j, v);

                int cp = CtrlOrder[sp + i];

                pt += (ubs * vbs * Weights[cp]) * CtrlPoints[cp].vector;

                weight += ubs * vbs * Weights[cp];
            }
        }

        return pt / weight;
    }

    public vcompo_t GetWeight(int u, int v)
    {
        return Weights[v * UCtrlDataCnt + u];
    }

    public void SetWeight(int u, int v, vcompo_t val)
    {
        Weights[v * UCtrlDataCnt + u] = val;
    }

    public void Eval(VertexList vl)
    {
        vcompo_t u;
        vcompo_t v;

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
