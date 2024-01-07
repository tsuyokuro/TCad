//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using System.ComponentModel;



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
        Log.plx("in");

        PlotterViewGL1 = PlotterViewGL.Create(mContext);

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
        mContext.MainWindow.DrawModeChanged(mode);
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
                    vector3_t.UnitZ * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);
                nextDC = view.DrawContext;
                break;

            case ViewModes.BACK:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -vector3_t.UnitZ * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);

                nextDC = view.DrawContext;
                break;

            case ViewModes.TOP:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    vector3_t.UnitY * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, -vector3_t.UnitZ);

                nextDC = view.DrawContext;
                break;

            case ViewModes.BOTTOM:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -vector3_t.UnitY * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitZ);

                nextDC = view.DrawContext;
                break;

            case ViewModes.RIGHT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    vector3_t.UnitX * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);

                nextDC = view.DrawContext;
                break;

            case ViewModes.LEFT:
                PlotterViewGL1.EnablePerse(false);
                view = PlotterViewGL1;
                view.DrawContext.SetCamera(
                    -vector3_t.UnitX * DrawContext.STD_EYE_DIST,
                    vector3_t.Zero, vector3_t.UnitY);

                nextDC = view.DrawContext;
                break;

            case ViewModes.FREE:
                PlotterViewGL1.EnablePerse(true);
                view = PlotterViewGL1;
                nextDC = view.DrawContext;
                break;
        }

        if (currentDC != null) currentDC.Deactivate();
        if (nextDC != null) nextDC.Activate();

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
