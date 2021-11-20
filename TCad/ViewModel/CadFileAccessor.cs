using Plotter.Serializer;
using Plotter.Controller;

namespace TCad.ViewModel
{
    public class CadFileAccessor
    {
        public static void SaveFile(string fname, PlotterViewModel vm)
        {
            if (fname.EndsWith(".kjs") || fname.EndsWith(".txt"))
            {
                SaveToMsgPackJsonFile(fname, vm);
            }
            else
            {
                SaveToMsgPackFile(fname, vm);
            }
        }

        public static void LoadFile(string fname, PlotterViewModel vm)
        {
            if (fname.EndsWith(".kjs") || fname.EndsWith(".txt"))
            {
                LoadFromMsgPackJsonFile(fname, vm);
            }
            else
            {
                LoadFromMsgPackFile(fname, vm);
            }
        }

        #region "MessagePack file access"

        private static void SaveToMsgPackFile(string fname, PlotterViewModel vm)
        {
            PlotterController pc = vm.Controller;

            CadData cd = new CadData(
                                pc.DB,
                                pc.DC.WorldScale,
                                pc.PageSize
                                );

            MpCadFile.Save(fname, cd);
        }

        private static void LoadFromMsgPackFile(string fname, PlotterViewModel vm)
        {
            PlotterController pc = vm.Controller;

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


        private static void SaveToMsgPackJsonFile(string fname, PlotterViewModel vm)
        {
            PlotterController pc = vm.Controller;

            CadData cd = new CadData(
                pc.DB,
                pc.DC.WorldScale,
                pc.PageSize);


            MpCadFile.SaveAsJson(fname, cd);
        }

        private static void LoadFromMsgPackJsonFile(string fname, PlotterViewModel vm)
        {
            CadData? cd = MpCadFile.LoadJson(fname);

            if (cd == null)
            {
                return;
            }

            CadData rcd = cd.Value;

            vm.SetWorldScale(rcd.WorldScale);

            PlotterController pc = vm.Controller;

            pc.PageSize = rcd.PageSize;

            pc.SetDB(rcd.DB);
        }
        #endregion
    }
}