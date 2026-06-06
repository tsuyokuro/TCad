using TCad.Plotter;
using TCad.Plotter.Controller;
using TCad.Plotter.Serializer;

namespace TCad.ViewModel;

public class CadFileAccessor
{
    public static void SaveFile(string fname, IPlotterController controller)
    {
        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            SaveToMsgPackJsonFile(fname, controller);
        }
        else
        {
            SaveToMsgPackFile(fname, controller);
        }
    }

    public static void LoadFile(string fname, IPlotterController controller)
    {
        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            LoadFromMsgPackJsonFile(fname, controller);
        }
        else
        {
            LoadFromMsgPackFile(fname, controller);
        }

        controller.Redraw();
    }

    #region "MessagePack file access"

    private static void SaveToMsgPackFile(string fname, IPlotterController controller)
    {
        CadData cd = new(
            controller.DB,
            controller.WorldScale,
            controller.PageSize);

        MpCadFile.Save(fname, cd);
    }

    private static void LoadFromMsgPackFile(string fname, IPlotterController controller)
    {
        CadData cd = MpCadFile.Load(fname);

        if (cd == null)
        {
            return;
        }

        CadData rcd = cd;

        controller.SetWorldScale(rcd.WorldScale);
        controller.SetPaperPageSize(rcd.PageSize);
        controller.SetDB(rcd.DB);
    }


    private static void SaveToMsgPackJsonFile(string fname, IPlotterController controller)
    {
        CadData cd = new(
            controller.DB,
            controller.WorldScale,
            controller.PageSize);

        MpCadFile.SaveAsJson(fname, cd);
    }

    private static void LoadFromMsgPackJsonFile(string fname, IPlotterController controller)
    {
        CadData cd = MpCadFile.LoadJson(fname);

        if (cd == null) return;

        CadData rcd = cd;

        controller.SetWorldScale(rcd.WorldScale);
        controller.SetPaperPageSize(rcd.PageSize);
        controller.SetDB(rcd.DB);
    }
    #endregion
}
