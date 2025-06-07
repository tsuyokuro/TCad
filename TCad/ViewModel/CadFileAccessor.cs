using System.IO;
using TCad.Plotter;
using TCad.Plotter.Controller;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Serializer;

namespace TCad.ViewModel;

public class CadFileAccessor
{
    public static void SaveFile(string fname, IPlotterViewModel vm)
    {
        if ((fname != null && vm.CurrentFileName != null) && fname != vm.CurrentFileName)
        {
            FileUtil.OverWriteExtData(vm.CurrentFileName, fname);
        }


        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            SerializeContext sc = new(MpCadFile.CurrentVersion, SerializeType.JSON);
            SaveExternalData(sc, vm.Controller.DB, fname);
            SaveToMsgPackJsonFile(fname, vm.Controller);
        }
        else
        {
            SerializeContext sc = new(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
            SaveExternalData(sc, vm.Controller.DB, fname);
            SaveToMsgPackFile(fname, vm.Controller);
        }
    }

    public static void LoadFile(string fname, IPlotterViewModel vm)
    {
        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            DeserializeContext dsc = new(MpCadFile.CurrentVersion, SerializeType.JSON);
            LoadFromMsgPackJsonFile(fname, vm.Controller);
            LoadExternalData(dsc, vm.Controller.DB, fname);
        }
        else
        {
            DeserializeContext dsc = new(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
            LoadFromMsgPackFile(fname, vm.Controller);
            LoadExternalData(dsc, vm.Controller.DB, fname);
        }

        vm.Controller.Redraw();
    }

    private static void SaveExternalData(SerializeContext sc, CadObjectDB db, string fname)
    {
        foreach (CadLayer layer in db.LayerList)
        {
            foreach (CadFigure fig in layer.FigureList)
            {
                SaveExternalData(sc, fig, fname);
            }
        }
    }

    private static void SaveExternalData(SerializeContext sc, CadFigure fig, string fname)
    {
        fig.SaveExternalFiles(sc, fname);

        foreach (CadFigure c in fig.ChildList)
        {
            SaveExternalData(sc, c, fname);
        }
    }

    private static void LoadExternalData(DeserializeContext dsc, CadObjectDB db, string fname)
    {
        foreach (CadLayer layer in db.LayerList)
        {
            foreach (CadFigure fig in layer.FigureList)
            {
                LoadExternalData(dsc, fig, fname);
            }
        }
    }

    private static void LoadExternalData(DeserializeContext dsc, CadFigure fig, string fname)
    {
        if (!File.Exists(fname))
        {
            return;
        }

        fig.LoadExternalFiles(dsc, fname);

        foreach (CadFigure c in fig.ChildList)
        {
            try
            {
                LoadExternalData(dsc, c, fname);
            }
            catch
            {
                continue;
            }
        }
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
        CadData? cd = MpCadFile.Load(fname);

        if (cd == null)
        {
            return;
        }

        CadData rcd = cd.Value;

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
        CadData? cd = MpCadFile.LoadJson(fname);

        if (cd == null) return;

        CadData rcd = cd.Value;

        controller.SetWorldScale(rcd.WorldScale);
        controller.PageSize = rcd.PageSize;
        controller.SetDB(rcd.DB);
    }
    #endregion
}
