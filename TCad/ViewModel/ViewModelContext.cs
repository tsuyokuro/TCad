using Plotter.Controller;
using Plotter;

namespace TCad.ViewModel
{
    public abstract class ViewModelContext
    {
        protected PlotterController mController;
        public PlotterController Controller
        {
            get => mController;
        }


        protected ICadMainWindow mMainWindow;
        public ICadMainWindow MainWindow
        {
            get => mMainWindow;
        }

        public void Redraw()
        {
            ThreadUtil.RunOnMainThread(mController.Redraw, true);
        }

        public abstract void DrawModeUpdated(DrawTools.DrawMode mode);
    }
}
