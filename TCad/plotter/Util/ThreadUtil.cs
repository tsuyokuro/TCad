using System;
using System.Windows;

namespace Plotter;

public static class ThreadUtil
{
    public static void RunOnMainThread(Action action)
    {
        RunOnMainThread(action, true);
    }

    public static void RunOnMainThread(Action action, bool wait)
    {
        if (Application.Current.CheckAccess())
        {
            action();
            return;
        }

        if (wait)
        {
            Application.Current.Dispatcher.Invoke(action);
        }
        else
        {
            Application.Current.Dispatcher.InvokeAsync(action);
        }
    }
}
