using TCad.Plotter.undo;
using TCad.Logger;

namespace TCad.Plotter.Scripting;

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

        Log.pl(nameof(ScriptSession) + " AddOpe " + ope.GetType().Name);
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
            Env.Controller.Drawer.Clear();
            Env.Controller.Drawer.DrawAll();
            Env.Controller.Drawer.UpdateView();
        }, true);
    }
}
