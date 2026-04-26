using System.IO;

namespace TCad.Plotter;

public class ExtDataFileUtil
{
    public static string GetExternalDataDir(string fname)
    {
        string path = fname + "_external";
        return path;
    }
}
