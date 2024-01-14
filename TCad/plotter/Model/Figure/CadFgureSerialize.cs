using Plotter.Serializer;
using System.Windows.Media.Media3D;
using CadDataTypes;
using System.Drawing;
using System.IO;
using System;



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
