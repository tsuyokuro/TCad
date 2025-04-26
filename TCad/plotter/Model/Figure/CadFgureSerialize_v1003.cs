using CadDataTypes;

using TCad.Plotter.Serializer;
using System;
using System.Drawing;
using TCad.Plotter.Serializer;
using TCad.Plotter.Serializer.v1003;
using TCad.Logger;

namespace TCad.Plotter.Model.Figure;

//=============================================================================
// CaFigure
//
public abstract partial class CadFigure
{
    public virtual MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpSimpleGeometricData_v1003 geo = new MpSimpleGeometricData_v1003();
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1003>(PointList);
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
    public override MpGeometricData_v1003 GeometricDataToMp_v1003(SerializeContext sc)
    {
        MpPictureGeometricData_v1003 geo = new MpPictureGeometricData_v1003();
        geo.FilePathName = FilePathName;
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1003>(PointList);
        if (sc.SerializeType == SerializeType.JSON)
        {
            geo.Base64 = Convert.ToBase64String(SrcData, 0, SrcData.Length);
            geo.Bytes = null;
        }
        else
        {
            geo.Base64 = null;
            geo.Bytes = new byte[SrcData.Length];
            SrcData.CopyTo(geo.Bytes, 0);
        }


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

        if (dsc.SerializeType == SerializeType.JSON)
        {
            SrcData = Convert.FromBase64String(g.Base64);
        }
        else
        {
            SrcData = new byte[g.Bytes.Length];
            g.Bytes.CopyTo(SrcData, 0);
        }

        Image image = ImageUtil.ByteArrayToImage(SrcData);
        mBitmap = new Bitmap(image);
        mBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
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
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1003>(PointList);
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
            Log.tpl("#### GeometricDataFromMp_v1003 OLD data !!!!! ####");
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
