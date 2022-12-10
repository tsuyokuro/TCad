//#define DEFAULT_DATA_TYPE_DOUBLE
using System;
using System.Windows.Input;
using Plotter;



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

public class CurrentFigCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        string cmd = parameter as string;

        if (cmd == "set_line_color")
        {
            return mViewModel.Controller.CurrentFigure != null;
        }
        else if (cmd == "set_fill_color")
        {
            if (mViewModel.Controller.CurrentFigure == null) return false;

            return mViewModel.Controller.CurrentFigure.Type == CadFigure.Types.MESH;
        }

        return true;
    }

    public void Execute(object parameter)
    {
        mViewModel.ExecCommand(parameter as string);
    }

    private IPlotterViewModel mViewModel;

    public CurrentFigCommand(IPlotterViewModel vm)
    {
        mViewModel = vm;
        CanExecuteChanged += (sender, e) => { /*DUMMY*/ };
    }

    public void UpdateCanExecute()
    {
        if (CanExecuteChanged != null)
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}

public class SimpleCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        mViewModel.ExecCommand(parameter as string);
    }

    private IPlotterViewModel mViewModel;

    public SimpleCommand(IPlotterViewModel vm)
    {
        mViewModel = vm;
        CanExecuteChanged += (sender, e) => { /*DUMMY*/ };
    }

    public void UpdateCanExecute()
    {
        if (CanExecuteChanged != null)
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
