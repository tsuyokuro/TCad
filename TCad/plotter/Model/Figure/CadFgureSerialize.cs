using TCad.Plotter.Serializer;

namespace TCad.Plotter.Model.Figure;

//=============================================================================
// CaFigure
//
public abstract partial class CadFigure
{
    public virtual void SaveExternalFiles(SerializeContext sc, string fname)
    {
    }

    public virtual void LoadExternalFiles(DeserializeContext dsc, string fname)
    {
    }
}


//=============================================================================
// CadFigurePicture
//
public partial class CadFigurePicture : CadFigure
{
    public override void SaveExternalFiles(SerializeContext sc, string fname)
    {
        //if (OrgFilePathName == null)
        //{
        //    return;
        //}

        //string name = Path.GetFileName(OrgFilePathName);

        //string dpath = FileUtil.GetExternalDataDir(fname);

        //Directory.CreateDirectory(dpath);

        //string dpathName = Path.Combine(dpath, name);

        //File.Copy(OrgFilePathName, dpathName, true);

        //FilePathName = name;

        //OrgFilePathName = null;
    }

    public override void LoadExternalFiles(DeserializeContext dsc, string fname)
    {
        //string basePath = FileUtil.GetExternalDataDir(fname);
        //string dfname = Path.Combine(basePath, FilePathName);

        //mBitmap = new Bitmap(Image.FromFile(dfname));

        //mBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
    }
}
