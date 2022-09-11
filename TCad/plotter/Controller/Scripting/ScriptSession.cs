namespace Plotter.Controller
{
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

        public CadOpeList OpeList
        {
            get => mCadOpeList;
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

            mCadOpeList.Add(ope);
        }

        public void Start(bool snapshotDB = false)
        {
            mCadOpeList.Clear();

            ResetFlags();

            StartWithSnapshotDB = snapshotDB;

            if (snapshotDB)
            {
                SnapShot = new CadOpeDBSnapShot();
                SnapShot.StoreBefore(Env.Controller.DB);
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
            } else {
                if (OpeList.Count() > 0)
                {
                    Env.Controller.HistoryMan.foward(OpeList);
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
            Env.RunOnMainThread(() =>
            {
                Env.Controller.UpdateObjectTree(remakeTree);
            });
        }

        public void Redraw()
        {
            Env.RunOnMainThread(() =>
            {
                Env.Controller.Clear();
                Env.Controller.DrawAll();
                Env.Controller.PushToView();
            });
        }
    }
}
