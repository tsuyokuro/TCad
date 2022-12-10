//#define DEFAULT_DATA_TYPE_DOUBLE


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


namespace Plotter.Scripting;

public class ScriptSession
{
    ScriptEnvironment Env;

    private CadOpeList mCadOpeList = null;

    private bool NeedUpdateObjectTree = false;
    private bool NeedRemakeObjectTree = false;
    private bool NeedRedraw = false;

    public bool StartWithSnapshotDB
    {
        get;
        private set;
    }

    private CadOpeDBSnapShot SnapShot;

    public ScriptSession(ScriptEnvironment env)
    {
        Env = env;
        mCadOpeList = new CadOpeList();
    }

    public void AddOpe(CadOpe ope)
    {
        if (ope == null)
        {
            return;
        }

        if (StartWithSnapshotDB)
        {
            return;
        }

        DOut.pl(nameof(ScriptSession) + " AddOpe " + ope.GetType().Name);
        mCadOpeList.Add(ope);
    }

    public void Start(bool snapshotDB = false)
    {
        ResetFlags();

        StartWithSnapshotDB = snapshotDB;

        if (snapshotDB)
        {
            SnapShot = new CadOpeDBSnapShot();
            SnapShot.StoreBefore(Env.Controller.DB);
        }
        else
        {
            mCadOpeList = new CadOpeList();
        }
    }

    public void End()
    {
        if (NeedUpdateObjectTree)
        {
            UpdateTV(NeedRemakeObjectTree);
        }

        if (NeedRedraw)
        {
            Redraw();
        }

        if (StartWithSnapshotDB)
        {
            SnapShot.StoreAfter(Env.Controller.DB);
            Env.Controller.HistoryMan.foward(SnapShot);
        }
        else
        {
            if (mCadOpeList?.Count > 0)
            {
                Env.Controller.HistoryMan.foward(mCadOpeList);
            }
        }
    }

    public void ResetFlags()
    {
        NeedUpdateObjectTree = false;
        NeedRemakeObjectTree = false;
        NeedRedraw = false;
    }

    public void PostUpdateObjectTree()
    {
        NeedUpdateObjectTree = true;
    }

    public void PostRemakeObjectTree()
    {
        NeedUpdateObjectTree = true;
        NeedRemakeObjectTree = true;
    }

    public void PostRedraw()
    {
        NeedRedraw = true;
    }

    public void UpdateTV(bool remakeTree)
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            Env.Controller.UpdateObjectTree(remakeTree);
        }, true);
    }

    public void Redraw()
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            Env.Controller.Clear();
            Env.Controller.DrawAll();
            Env.Controller.PushToView();
        }, true);
    }
}
