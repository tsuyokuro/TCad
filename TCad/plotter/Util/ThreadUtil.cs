using System;
using System.Windows;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

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
