using CadDataTypes;
using OpenTK.Mathematics;
using System.Threading;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter.Controller;

public class InteractCtrl
{
    public enum States
    {
        NONE,
        CANCEL,
        CONTINUE,
        END,
    }

    private SemaphoreSlim Sem = new SemaphoreSlim(0, 1);

    public Vector3List PointList = new Vector3List();

    public States mState = States.NONE;
    public States State
    {
        get => mState;
        set => mState = value;
    }

    public bool IsActive => (mState == States.CONTINUE);

    public void Cancel()
    {
        mState = States.CANCEL;
        Sem.Release();
    }

    public void SetPoint(vector3_t v)
    {
        lock (PointList)
        {
            PointList.Add(v);
        }

        Sem.Release();
    }

    public void Start()
    {
        mState = States.CONTINUE;

        lock (PointList)
        {
            PointList.Clear();
        }
    }

    public void End()
    {
        mState = States.END;
    }

    public States WaitPoint()
    {
        Sem.Wait();
        return mState;
    }

    public void Draw(DrawContext dc, vector3_t tp)
    {
        if (PointList.Count == 0)
        {
            return;
        }

        vector3_t p0 = PointList[0];
        vector3_t p1;

        for (int i = 1; i < PointList.Count; i++)
        {
            p1 = PointList[i];

            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_DEFAULT_FIGURE), p0, p1);

            p0 = p1;
        }

        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_TEMP_FIGURE), p0, tp);
    }
}
