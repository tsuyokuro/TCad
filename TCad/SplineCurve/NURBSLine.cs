//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK;
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


namespace SplineCurve;

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

    public vcompo_t[] Weights;

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

    private vector3_t CalcPoint(vcompo_t t)
    {
        vector3_t linePoint = vector3_t.Zero;
			vcompo_t weight = 0f;

        vcompo_t bs;

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
            vcompo_t t = p * BSplineP.Step + BSplineP.LowKnot;
            if (t >= BSplineP.HighKnot)
            {
                t = BSplineP.HighKnot - BSpline.Epsilon;
            }

            vl.Add( new CadVertex(CalcPoint(t)) );
        }
    }

    public void SetDefaultWeights()
    {
        Weights = new vcompo_t[CtrlDataCnt];

        for (int i = 0; i < Weights.Length; ++i)
        {
            Weights[i] = 1f;
        }
    }

    public vcompo_t GetWeight(int u, int v)
    {
        return Weights[v * CtrlDataCnt + u];
    }

    public void SetWeight(int u, int v, vcompo_t val)
    {
        Weights[v * CtrlDataCnt + u] = val;
    }
}
