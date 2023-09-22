//#define DEFAULT_DATA_TYPE_DOUBLE
using System.IO;



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
