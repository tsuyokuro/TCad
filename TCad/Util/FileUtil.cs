using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IronPython.Modules._ast;

namespace TCad.Util;

public class FileUtil
{
    public static string ExeDir()
    {
        var dir = AppContext.BaseDirectory;
        return dir.Trim('\\');
    }

    public static string PathNameOnExeDir(string fname)
    {
        return $"{ExeDir()}\\{fname}";
    }
}
