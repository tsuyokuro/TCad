using Plotter.Serializer.v1003;
using Plotter.Serializer;
using System.Windows.Media.Media3D;
using CadDataTypes;
using System.Drawing;
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

    public virtual MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpSimpleGeometricData_v1003 geo = new MpSimpleGeometricData_v1003();
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1003>(PointList, MpVertex_v1003.Create);
        return geo;
    }

    public virtual void GeometricDataFromMp_v1003(DeserializeContext dsc, MpGeometricData_v1003 geo)
    {
        if (!(geo is MpSimpleGeometricData_v1003))
        {
            return;
        }

        MpSimpleGeometricData_v1003 g = (MpSimpleGeometricData_v1003)geo;

        mPointList = MpUtil.VertexListFromMp(g.PointList);
    }
}

//=============================================================================
// CaFigureMesh
//
public partial class CadFigureMesh : CadFigure
{
    public override MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpMeshGeometricData_v1003 mpGeo = new MpMeshGeometricData_v1003();
        mpGeo.HeModel = MpHeModel_v1003.Create(mHeModel);

        return mpGeo;
    }

    public override void GeometricDataFromMp_v1003(DeserializeContext dsc, MpGeometricData_v1003 mpGeo)
    {
        if (!(mpGeo is MpMeshGeometricData_v1003))
        {
            return;
        }

        MpMeshGeometricData_v1003 meshGeo = (MpMeshGeometricData_v1003)mpGeo;

        //mHeModel = meshGeo.HeModel.Restore();
        //mPointList = mHeModel.VertexStore;
        SetMesh(meshGeo.HeModel.Restore());
    }
}

//=============================================================================
// CadFigureNurbsLine
//
public partial class CadFigureNurbsLine : CadFigure
{
    public override MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpNurbsLineGeometricData_v1003 geo = new MpNurbsLineGeometricData_v1003();
        geo.Nurbs = MpNurbsLine_v1003.Create(Nurbs);
        return geo;
    }

    public override void GeometricDataFromMp_v1003(DeserializeContext dsc, MpGeometricData_v1003 geo)
    {
        if (!(geo is MpNurbsLineGeometricData_v1003))
        {
            return;
        }

        MpNurbsLineGeometricData_v1003 g = (MpNurbsLineGeometricData_v1003)geo;

        Nurbs = g.Nurbs.Restore();

        mPointList = Nurbs.CtrlPoints;

        NurbsPointList = new VertexList(Nurbs.OutCnt);
    }
}

//=============================================================================
// CadFigureNurbsSurface
//
public partial class CadFigureNurbsSurface : CadFigure
{
    public override MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpNurbsSurfaceGeometricData_v1003 geo = new MpNurbsSurfaceGeometricData_v1003();
        geo.Nurbs = MpNurbsSurface_v1003.Create(Nurbs);
        return geo;
    }

    public override void GeometricDataFromMp_v1003(DeserializeContext dsc, MpGeometricData_v1003 geo)
    {
        if (!(geo is MpNurbsSurfaceGeometricData_v1003))
        {
            return;
        }

        MpNurbsSurfaceGeometricData_v1003 g = (MpNurbsSurfaceGeometricData_v1003)geo;

        Nurbs = g.Nurbs.Restore();

        mPointList = Nurbs.CtrlPoints;

        NurbsPointList = new VertexList(Nurbs.UOutCnt * Nurbs.VOutCnt);

        NeedsEval = true;
    }
}

//=============================================================================
// CadFigurePicture
//
public partial class CadFigurePicture : CadFigure
{
    public override void SaveExternalFiles(SerializeContext sc, string fname)
    {
        if (OrgFilePathName == null)
        {
            return;
        }

        string name = Path.GetFileName(OrgFilePathName);

        string dpath = FileUtil.GetExternalDataDir(fname);

        Directory.CreateDirectory(dpath);

        string dpathName = Path.Combine(dpath, name);

        File.Copy(OrgFilePathName, dpathName, true);

        FilePathName = name;

        OrgFilePathName = null;
    }

    public override void LoadExternalFiles(DeserializeContext dsc, string fname)
    {
        string basePath = FileUtil.GetExternalDataDir(fname);
        string dfname = Path.Combine(basePath, FilePathName);

        mBitmap = new Bitmap(Image.FromFile(dfname));

        mBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
    }


    public override MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpPictureGeometricData_v1003 geo = new MpPictureGeometricData_v1003();
        geo.FilePathName = FilePathName;
        geo.PointList = MpUtil.VertexListToMp(PointList, MpVertex_v1003.Create);
        return geo;
    }

    public override void GeometricDataFromMp_v1003(DeserializeContext dsc, MpGeometricData_v1003 geo)
    {
        if (!(geo is MpPictureGeometricData_v1003))
        {
            return;
        }

        MpPictureGeometricData_v1003 g = (MpPictureGeometricData_v1003)geo;
        FilePathName = g.FilePathName;
        mPointList = MpUtil.VertexListFromMp(g.PointList);
    }
}

//=============================================================================
// CadFigurePolyLines
//
public partial class CadFigurePolyLines : CadFigure
{
    public override MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpPolyLinesGeometricData_v1003 geo = new();
        geo.IsLoop = IsLoop_;
        geo.PointList = MpUtil.VertexListToMp(PointList, MpVertex_v1003.Create);
        return geo;
    }

    public override void GeometricDataFromMp_v1003(DeserializeContext dsc, MpGeometricData_v1003 geo)
    {
        MpPolyLinesGeometricData_v1003 g = geo as MpPolyLinesGeometricData_v1003;
        if (g != null)
        {
            IsLoop_ = g.IsLoop;
            mPointList = MpUtil.VertexListFromMp(g.PointList);
            return;
        }

        MpSimpleGeometricData_v1003 g2 = geo as MpSimpleGeometricData_v1003;
        if (g2 != null)
        {
            DOut.tpl("#### GeometricDataFromMp_v1003 OLD data !!!!! ####");
            mPointList = MpUtil.VertexListFromMp(g2.PointList);
        }
    }
}


//=============================================================================
// CadFigureCircle
//
public partial class CadFigureCircle : CadFigure
{
    // No spcial data for Serialize
}


//=============================================================================
// CadFigureDimLine
//
public partial class CadFigureDimLine : CadFigure
{
    // No spcial data for Serialize
}


//=============================================================================
// CadFigureGroup
//
public partial class CadFigureGroup : CadFigure
{
    // No spcial data for Serialize
}


//=============================================================================
// CadFigurePoint
//
public partial class CadFigurePoint : CadFigure
{
    // No spcial data for Serialize
}
