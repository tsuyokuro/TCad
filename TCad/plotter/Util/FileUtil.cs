using System.IO;
using TCad.ViewModel;

namespace Plotter;

public class FileUtil
{
    public static string GetExternalDataDir(string fname)
    {
        string path = fname + "_external";
        return path;
    }

    public static void OverWriteDir(string sdir, string tdir)
    {
        if (Directory.Exists(tdir))
        {
            Directory.Delete(tdir, true);
        }
        DirectoryCopy(sdir, tdir);
    }

    public static void OverWriteExtData(string sfname, string tfname)
    {
        string sdir = GetExternalDataDir(sfname);
        string tdir = GetExternalDataDir(tfname);
        OverWriteDir(sdir, tdir);
    }

    public static void DirectoryCopy(string sourcePath, string destinationPath)
    {
        DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
        if (sourceDirectory.Exists == false)
        {
            return;
        }

        DirectoryInfo destinationDirectory = new DirectoryInfo(destinationPath);
        if (destinationDirectory.Exists == false)
        {
            destinationDirectory.Create();
            destinationDirectory.Attributes = sourceDirectory.Attributes;
        }

        foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
        {
            fileInfo.CopyTo(destinationDirectory.FullName + @"\" + fileInfo.Name, true);
        }

        foreach (DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
        {
            DirectoryCopy(directoryInfo.FullName, destinationDirectory.FullName + @"\" + directoryInfo.Name);
        }
    }
}
