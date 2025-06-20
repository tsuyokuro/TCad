using System.ComponentModel;
using TCad.Logger;
using TCad.MainView;
using TCad.Plotter;
using TCad.Plotter.Controller;
using TCad.Plotter.DrawContexts;

namespace TCad.ViewModel;

public enum ViewModes
{
    NONE,
    FRONT,
    BACK,
    TOP,
    BOTTOM,
    RIGHT,
    LEFT,
    FREE,
}

public class ViewManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;


    private IPlotterController Controller;

    private ICadMainWindow MainWindow;


    public IPlotterView View
    {
        get;
        private set;
    } = null;

    private PlotterViewGL PlotterViewGL1 = null;

    private ViewModes ViewMode_ = ViewModes.NONE;
    public ViewModes ViewMode
    {
        set
        {
            if (value == ViewMode_)
            {
                return;
            }

            ViewMode_ = value;

            ChangeViewMode(ViewMode_);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewMode)));
        }

        get => ViewMode_;
    }

    public ViewManager(ICadMainWindow mainWindow, IPlotterController controller)
    {
        MainWindow = mainWindow;
        Controller = controller;
    }

    public void SetupViews(PlotterViewModel viewModel)
    {
        Log.plx("in");

        PlotterViewGL1 = new PlotterViewGL(viewModel);

        ViewMode = ViewModes.FRONT;

        Log.plx("out");
    }

    public void SetWorldScale(vcompo_t scale)
    {
        PlotterViewGL1.SetWorldScale(scale);
    }

    public void DrawModeChanged(DrawModes mode)
    {
        PlotterViewGL1.DrawModeChanged(mode);
        MainWindow.DrawModeChanged(mode);
    }

    public void ResetCamera()
    {
        DrawContext dc = View.DrawContext;

        switch (ViewMode)
        {
            case ViewModes.FRONT:
            case ViewModes.BACK:
            case ViewModes.TOP:
            case ViewModes.BOTTOM:
            case ViewModes.RIGHT:
            case ViewModes.LEFT:
                dc.SetViewOrg(
                    new vector3_t(
                            dc.ViewWidth / 2, dc.ViewHeight / 2, 0
                        ));
                break;

            case ViewModes.FREE:
                vector3_t eye = new vector3_t(0, 0, DrawContextGL.DEFAULT_EYE_Z);
                vector3_t lookAt = vector3_t.Zero;
                vector3_t up = vector3_t.UnitY;
                dc.SetCamera(eye, lookAt, up);
                break;
        }
    }

    private void ChangeViewMode(ViewModes newMode)
    {
        IPlotterView view = View;

        switch (ViewMode_)
        {
            case ViewModes.FRONT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    vector3_t.UnitZ * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);
                break;

            case ViewModes.BACK:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -vector3_t.UnitZ * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);

                break;

            case ViewModes.TOP:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    vector3_t.UnitY * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, -vector3_t.UnitZ);

                break;

            case ViewModes.BOTTOM:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -vector3_t.UnitY * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitZ);

                break;

            case ViewModes.RIGHT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    vector3_t.UnitX * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);

                break;

            case ViewModes.LEFT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -vector3_t.UnitX * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);

                break;

            case ViewModes.FREE:
                PlotterViewGL1.EnablePerse(true);
                view = PlotterViewGL1;
                break;
        }

        SetView(view);
    }

    private void SetView(IPlotterView view)
    {
        View = view;

        Controller.DC = view.DrawContext;

        view.SetWorldScale(Controller.WorldScale);

        // OnPaintが呼ばれるためRedrawを呼ぶ必要はない
        MainWindow.SetPlotterView(view);
    }
}
