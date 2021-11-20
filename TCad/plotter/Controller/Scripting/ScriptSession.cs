namespace Plotter.Controller
{
    public class ScriptSession
    {
        ScriptEnvironment Env;

        private CadOpeList mCadOpeList = null;

        private bool NeedUpdateObjectTree = false;
        private bool NeedRemakeObjectTree = false;
        private bool NeedRedraw = false;

        public ScriptSession(ScriptEnvironment env)
        {
            Env = env;
        }

        public CadOpeList OpeList
        {
            get => mCadOpeList;
        }

        public void AddOpe(CadOpe ope)
        {
            mCadOpeList.Add(ope);
        }

        public void Start()
        {
            mCadOpeList = new CadOpeList();

            ResetFlags();
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
                Env.Controller.ReflectToView();
            });
        }
    }
}