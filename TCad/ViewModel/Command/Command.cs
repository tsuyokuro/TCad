using System;
using System.Windows.Input;
using Plotter;

namespace TCad.ViewModel;

public class CurrentFigCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        string cmd = parameter as string;

        if (cmd == "set_line_color")
        {
            return mViewModel.Controller.Input.CurrentFigure != null;
        }
        else if (cmd == "set_fill_color")
        {
            if (mViewModel.Controller.Input.CurrentFigure == null) return false;

            return mViewModel.Controller.Input.CurrentFigure.Type == CadFigure.Types.MESH;
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
