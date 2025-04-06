using Plotter.Serializer;
using Plotter.Controller;
using Plotter;
using System.IO;

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
            SerializeContext sc = new SerializeContext(MpCadFile.CurrentVersion, SerializeType.JSON);
            SaveExternalData(sc, vm.Controller.DB, fname);
            SaveToMsgPackJsonFile(fname, vm);
        }
        else
        {
            SerializeContext sc = new SerializeContext(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
            SaveExternalData(sc, vm.Controller.DB, fname);
            SaveToMsgPackFile(fname, vm);
        }
    }

    public static void LoadFile(string fname, IPlotterViewModel vm)
    {
        if (fname.EndsWith(".txt") || fname.EndsWith(".json"))
        {
            DeserializeContext dsc = new DeserializeContext(MpCadFile.CurrentVersion, SerializeType.JSON);
            LoadFromMsgPackJsonFile(fname, vm);
            LoadExternalData(dsc, vm.Controller.DB, fname);
        }
        else
        {
            DeserializeContext dsc = new DeserializeContext(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
            LoadFromMsgPackFile(fname, vm);
            LoadExternalData(dsc, vm.Controller.DB, fname);
        }

        vm.Controller.Drawer.Redraw();
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
            try {
                LoadExternalData(dsc, c, fname);
            }
            catch
            {
                continue;
            }
        }
    }


    #region "MessagePack file access"

    private static void SaveToMsgPackFile(string fname, IPlotterViewModel vm)
    {
        IPlotterController pc = vm.Controller;

        CadData cd = new CadData(
                            pc.DB,
                            pc.DC.WorldScale,
                            pc.PageSize
                            );

        MpCadFile.Save(fname, cd);
    }

    private static void LoadFromMsgPackFile(string fname, IPlotterViewModel vm)
    {
        IPlotterController pc = vm.Controller;

        CadData? cd = MpCadFile.Load(fname);

        if (cd == null)
        {
            return;
        }

        CadData rcd = cd.Value;


        vm.SetWorldScale(rcd.WorldScale);

        pc.PageSize = rcd.PageSize;

        pc.SetDB(rcd.DB);
    }


    private static void SaveToMsgPackJsonFile(string fname, IPlotterViewModel vm)
    {
        IPlotterController pc = vm.Controller;

        CadData cd = new CadData(
            pc.DB,
            pc.DC.WorldScale,
            pc.PageSize);


        MpCadFile.SaveAsJson(fname, cd);
    }

    private static void LoadFromMsgPackJsonFile(string fname, IPlotterViewModel vm)
    {
        CadData? cd = MpCadFile.LoadJson(fname);

        if (cd == null) return;

        CadData rcd = cd.Value;

        vm.SetWorldScale(rcd.WorldScale);

        IPlotterController pc = vm.Controller;

        pc.PageSize = rcd.PageSize;

        pc.SetDB(rcd.DB);
    }
    #endregion
}
