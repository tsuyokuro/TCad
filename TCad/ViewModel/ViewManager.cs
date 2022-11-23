using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using System.ComponentModel;

namespace TCad.ViewModel;

public class ViewManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public IPlotterViewModel mContext;

    private IPlotterView mPlotterView = null;
    public IPlotterView PlotterView
    {
        get => mPlotterView;
    }

    public DrawContext DrawContext
    {
        get => mPlotterView.DrawContext;
    }

    private PlotterViewGL PlotterViewGL1 = null;

    private ViewModes mViewMode = ViewModes.NONE;
    public ViewModes ViewMode
    {
        set
        {
            bool changed = ChangeViewMode(value);
            if (changed)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewMode)));
            }
        }

        get => mViewMode;
    }

    public ViewManager(IPlotterViewModel context)
    {
        mContext = context;
    }

    public void SetupViews()
    {
        DOut.plx("in");

        PlotterViewGL1 = PlotterViewGL.Create(mContext);

        ViewMode = ViewModes.FRONT;

        DOut.plx("out");
    }

    public void SetWorldScale(double scale)
    {
        PlotterViewGL1.SetWorldScale(scale);
    }

    public void DrawModeUpdated(DrawModes mode)
    {
        PlotterViewGL1.DrawModeUpdated(mode);
    }

    public void ResetCamera()
    {
        DrawContext dc = mPlotterView.DrawContext;

        switch (mViewMode)
        {
            case ViewModes.FRONT:
            case ViewModes.BACK:
            case ViewModes.TOP:
            case ViewModes.BOTTOM:
            case ViewModes.RIGHT:
            case ViewModes.LEFT:
                dc.SetViewOrg(
                    new Vector3d(
                            dc.ViewWidth / 2, dc.ViewHeight / 2, 0
                        ));
                break;

            case ViewModes.FREE:
                Vector3d eye = new Vector3d(0, 0, DrawContextGL.DEFAULT_EYE_Z);
                Vector3d lookAt = Vector3d.Zero;
                Vector3d up = Vector3d.UnitY;
                dc.SetCamera(eye, lookAt, up);
                break;
        }
    }

    private bool ChangeViewMode(ViewModes newMode)
    {
        if (mViewMode == newMode)
        {
            return false;
        }

        mViewMode = newMode;

        DrawContext currentDC = mPlotterView?.DrawContext;
        DrawContext nextDC = mPlotterView?.DrawContext;
        IPlotterView view = mPlotterView;

        switch (mViewMode)
        {
            case ViewModes.FRONT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                    Vector3d.Zero, Vector3d.UnitY);
                nextDC = view.DrawContext;
                break;

            case ViewModes.BACK:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                    Vector3d.Zero, Vector3d.UnitY);

                nextDC = view.DrawContext;
                break;

            case ViewModes.TOP:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                    Vector3d.Zero, -Vector3d.UnitZ);

                nextDC = view.DrawContext;
                break;

            case ViewModes.BOTTOM:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                    Vector3d.Zero, Vector3d.UnitZ);

                nextDC = view.DrawContext;
                break;

            case ViewModes.RIGHT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                    Vector3d.Zero, Vector3d.UnitY);

                nextDC = view.DrawContext;
                break;

            case ViewModes.LEFT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                    Vector3d.Zero, Vector3d.UnitY);

                nextDC = view.DrawContext;
                break;

            case ViewModes.FREE:
                PlotterViewGL1.EnablePerse(true);
                view = PlotterViewGL1;
                nextDC = view.DrawContext;
                break;
        }

        if (currentDC != null) currentDC.Deactive();
        if (nextDC != null) nextDC.Active();

        SetView(view);
        mContext.Redraw();
        return true;
    }

    private void SetView(IPlotterView view)
    {
        mPlotterView = view;

        mContext.Controller.DC = view.DrawContext;

        mContext.MainWindow.SetPlotterView(view);
    }
}
