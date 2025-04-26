using System.Windows;

namespace TCad.Util;

class XamlResource
{
    public static void SetValue(string name, dynamic value)
    {
        var res = Application.Current.MainWindow.TryFindResource(name);
        if (res != null)
        {
            Application.Current.MainWindow.Resources[name] = value;
        }
    }


    public static dynamic GetValue(string name)
    {
        var res = Application.Current.MainWindow.TryFindResource(name);
        if (res != null)
        {
            return Application.Current.MainWindow.Resources[name];
        }

        return null;
    }
}
