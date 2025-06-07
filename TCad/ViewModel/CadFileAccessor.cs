using System.IO;
using TCad.Plotter;
using TCad.Plotter.Controller;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Serializer;

namespace TCad.ViewModel;

public class CadFileAccessor
{
    public static void SaveFile(string fname, IPlotterController controller)
    {
        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            SerializeContext sc = new(MpCadFile.CurrentVersion, SerializeType.JSON);
            SaveToMsgPackJsonFile(fname, controller);
        }
        else
        {
            SerializeContext sc = new(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
            SaveToMsgPackFile(fname, controller);
        }
    }

    public static void LoadFile(string fname, IPlotterController controller)
    {
        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            DeserializeContext dsc = new(MpCadFile.CurrentVersion, SerializeType.JSON);
            LoadFromMsgPackJsonFile(fname, controller);
        }
        else
        {
            DeserializeContext dsc = new(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
            LoadFromMsgPackFile(fname, controller);
        }

        controller.Redraw();
    }

    #region "MessagePack file access"

    private static void SaveToMsgPackFile(string fname, IPlotterController controller)
    {
        CadData cd = new(
            controller.DB,
            controller.DC.WorldScale,
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
        controller.PageSize = rcd.PageSize;
        controller.SetDB(rcd.DB);
    }


    private static void SaveToMsgPackJsonFile(string fname, IPlotterController controller)
    {
        CadData cd = new(
            controller.DB,
            controller.DC.WorldScale,
            controller.PageSize);

        MpCadFile.SaveAsJson(fname, cd);
    }

    private static void LoadFromMsgPackJsonFile(string fname, IPlotterController controller)
    {
        CadData cd = MpCadFile.LoadJson(fname);

        if (cd == null) return;

        CadData rcd = cd;

        controller.SetWorldScale(rcd.WorldScale);
        controller.PageSize = rcd.PageSize;
        controller.SetDB(rcd.DB);
    }
    #endregion
}
