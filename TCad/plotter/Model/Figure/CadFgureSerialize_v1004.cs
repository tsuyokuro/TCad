using CadDataTypes;
using Plotter.Serializer;
using System;
using System.Drawing;
using TCad.plotter.Serializer;
using TCad.plotter.Serializer.v1004;

namespace Plotter;

//=============================================================================
// CaFigure
//
public abstract partial class CadFigure
{
    public virtual MpGeometricData_v1004 GeometricDataToMp_v1004(SerializeContext sc)
    {
        MpSimpleGeometricData_v1004 geo = new MpSimpleGeometricData_v1004();
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1004>(PointList);
        return geo;
    }

    public virtual void GeometricDataFromMp_v1004(DeserializeContext dsc, MpGeometricData_v1004 geo)
    {
        if (!(geo is MpSimpleGeometricData_v1004))
        {
            return;
        }

        MpSimpleGeometricData_v1004 g = (MpSimpleGeometricData_v1004)geo;

        mPointList = MpUtil.VertexListFromMp(g.PointList);
    }
}

//=============================================================================
// CaFigureMesh
//
public partial class CadFigureMesh : CadFigure
{
    public override MpGeometricData_v1004 GeometricDataToMp_v1004(SerializeContext sc)
    {
        MpMeshGeometricData_v1004 mpGeo = new MpMeshGeometricData_v1004();
        mpGeo.HeModel = new MpHeModel_v1004();
        mpGeo.HeModel.Store(mHeModel);

        return mpGeo;
    }

    public override void GeometricDataFromMp_v1004(DeserializeContext dsc, MpGeometricData_v1004 mpGeo)
    {
        if (!(mpGeo is MpMeshGeometricData_v1004))
        {
            return;
        }

        MpMeshGeometricData_v1004 meshGeo = (MpMeshGeometricData_v1004)mpGeo;

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
    public override MpGeometricData_v1004 GeometricDataToMp_v1004(SerializeContext sc)
    {
        MpNurbsLineGeometricData_v1004 geo = new MpNurbsLineGeometricData_v1004();
        geo.Nurbs = new MpNurbsLine_v1004();
        geo.Nurbs.Store(Nurbs);
        return geo;
    }

    public override void GeometricDataFromMp_v1004(DeserializeContext dsc, MpGeometricData_v1004 geo)
    {
        if (!(geo is MpNurbsLineGeometricData_v1004))
        {
            return;
        }

        MpNurbsLineGeometricData_v1004 g = (MpNurbsLineGeometricData_v1004)geo;

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
    public override MpGeometricData_v1004 GeometricDataToMp_v1004(SerializeContext sc)
    {
        MpNurbsSurfaceGeometricData_v1004 geo = new MpNurbsSurfaceGeometricData_v1004();
        geo.Nurbs = new MpNurbsSurface_v1004();
        geo.Nurbs.Store(Nurbs);
        return geo;
    }

    public override void GeometricDataFromMp_v1004(DeserializeContext dsc, MpGeometricData_v1004 geo)
    {
        if (!(geo is MpNurbsSurfaceGeometricData_v1004))
        {
            return;
        }

        MpNurbsSurfaceGeometricData_v1004 g = (MpNurbsSurfaceGeometricData_v1004)geo;

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
    public override MpGeometricData_v1004 GeometricDataToMp_v1004(SerializeContext sc)
    {
        MpPictureGeometricData_v1004 geo = new MpPictureGeometricData_v1004();
        geo.FilePathName = FilePathName;
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1004>(PointList);
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

    public override void GeometricDataFromMp_v1004(DeserializeContext dsc, MpGeometricData_v1004 geo)
    {
        if (!(geo is MpPictureGeometricData_v1004))
        {
            return;
        }

        MpPictureGeometricData_v1004 g = (MpPictureGeometricData_v1004)geo;
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
    public override MpGeometricData_v1004 GeometricDataToMp_v1004(SerializeContext sc)
    {
        MpPolyLinesGeometricData_v1004 geo = new();
        geo.IsLoop = IsLoop_;
        geo.PointList = MpUtil.VertexListToMp<MpVertex_v1004>(PointList);
        return geo;
    }

    public override void GeometricDataFromMp_v1004(DeserializeContext dsc, MpGeometricData_v1004 geo)
    {
        MpPolyLinesGeometricData_v1004 g = geo as MpPolyLinesGeometricData_v1004;
        if (g != null)
        {
            IsLoop_ = g.IsLoop;
            mPointList = MpUtil.VertexListFromMp(g.PointList);
            return;
        }

        MpSimpleGeometricData_v1004 g2 = geo as MpSimpleGeometricData_v1004;
        if (g2 != null)
        {
            Log.tpl("#### GeometricDataFromMp_v1004 OLD data !!!!! ####");
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
