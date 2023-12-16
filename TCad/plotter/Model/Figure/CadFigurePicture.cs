//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Serializer.v1002;
using Plotter.Serializer.v1003;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Path = System.IO.Path;



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

public class CadFigurePicture : CadFigure
{
    //  3-------------------2
    //  |                   |
    //  |                   |
    //  |                   |
    //  0-------------------1

    private Bitmap mBitmap;

    public string OrgFilePathName;
    public string FilePathName;

    public CadFigurePicture()
    {
        Type = Types.PICTURE;

        if (mBitmap == null)
        {
            
        }
    }

    public void Setup(PaperPageSize pageSize, vector3_t pos, string path)
    {
        OrgFilePathName = path;

        mBitmap = new Bitmap(Image.FromFile(path));

        mBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

        vcompo_t uw = mBitmap.Width;
        vcompo_t uh = mBitmap.Height;

        vcompo_t w;
        vcompo_t h;

        if (pageSize.Width <= pageSize.Height)
        {
            w = pageSize.Width * (vcompo_t)(0.5);
            h = w * (uh / uw);
        }
        else
        {
            h = pageSize.Height * (vcompo_t)(0.5);
            w = h * (uw / uh);
        }

        CadVertex tv = (CadVertex)pos;
        CadVertex ov = tv;

        mPointList.Clear();

        mPointList.Add(tv);

        tv = ov;
        tv.X += w;
        mPointList.Add(tv);

        tv = ov;
        tv.X += w;
        tv.Y += h;
        mPointList.Add(tv);

        tv = ov;
        tv.Y += h;
        mPointList.Add(tv);
    }

    public override void AddPoint(CadVertex p)
    {
        mPointList.Add(p);
    }

    public override Centroid GetCentroid()
    {
        if (PointList.Count == 0)
        {
            return default;
        }

        if (PointList.Count == 1)
        {
            return GetPointCentroid();
        }

        if (PointList.Count < 3)
        {
            return GetSegCentroid();
        }

        return GetPointListCentroid();
    }

    public override void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        PointList.Add(p);
    }

    public override void SetPointAt(int index, CadVertex pt)
    {
        mPointList[index] = pt;
    }

    public override void RemoveSelected()
    {
        mPointList.RemoveAll(a => a.Selected);

        if (PointCount < 4)
        {
            mPointList.Clear();
        }
    }

    public override void Draw(DrawContext dc, DrawOption dp)
    {
        DrawPicture(dc, dp.LinePen);
    }

    private void DrawPicture(DrawContext dc, DrawPen linePen)
    {
        if (mBitmap == null)
        {
            return;
        }

        ImageRenderer renderer = ImageRenderer.Provider.Get();

        vector3_t xv = (vector3_t)(mPointList[1] - mPointList[0]);
        vector3_t yv = (vector3_t)(mPointList[3] - mPointList[0]);

        renderer.Render(mBitmap, (vector3_t)mPointList[0], xv, yv);

        dc.Drawing.DrawLine(linePen, mPointList[0].vector, mPointList[1].vector);
        dc.Drawing.DrawLine(linePen, mPointList[1].vector, mPointList[2].vector);
        dc.Drawing.DrawLine(linePen, mPointList[2].vector, mPointList[3].vector);
        dc.Drawing.DrawLine(linePen, mPointList[3].vector, mPointList[0].vector);
    }


    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
        foreach (CadVertex p in PointList)
        {
            if (p.Selected)
            {
                dc.Drawing.DrawSelectedPoint(p.vector, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
            }
        }
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
        int cnt = PointList.Count;

        if (cnt < 1) return;

        if (cnt == 1)
        {
            //DrawDim(DC, PointList[0], tp, tp, pen);
            return;
        }

        //DrawDim(DC, PointList[0], PointList[1], tp, pen);
    }

    public override void StartCreate(DrawContext dc)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
        if (PointList.Count < 3)
        {
            return;
        }

        CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1], PointList[2]);

        PointList[2] = PointList[2].SetVector(seg.P1.vector);
        PointList.Add(seg.P0);
    }

    public override void MoveSelectedPointsFromStored(DrawContext dc, MoveInfo moveInfo)
    {
        int cnt = 0;
        foreach (CadVertex vertex in PointList)
        {
            if (vertex.Selected)
            {
                cnt++;
            }
        }

        vector3_t delta = moveInfo.Delta;

        if (cnt >= 3)
        {
            PointList[0] = StoreList[0] + delta;
            PointList[1] = StoreList[1] + delta;
            PointList[2] = StoreList[2] + delta;
            PointList[3] = StoreList[3] + delta;
            return;
        }

        if (cnt == 1)
        {
            if (PointList[0].Selected)
            {


                return;
            }

            if (PointList[1].Selected)
            {
                return;
            }
        }
        else if (cnt == 2)
        {

        }
    }

    public override void InvertDir()
    {
    }

    // 高さが０の場合、移動方向が定まらないので
    // 投影座標系でz=0とした座標から,List[0] - List[1]への垂線を計算して
    // そこへ移動する
    private void MoveSelectedPointWithHeight(DrawContext dc, vector3_t delta)
    {
        CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1],
            StoreList[2] + delta);

        PointList[2] = PointList[2].SetVector(seg.P1.vector);
        PointList[3] = PointList[3].SetVector(seg.P0.vector);
    }

    public override void EndEdit()
    {
        base.EndEdit();

        if (PointList.Count == 0)
        {
            return;
        }

        CadSegment seg = CadUtil.PerpSeg(PointList[0], PointList[1], PointList[2]);

        PointList[2] = PointList[2].SetVector(seg.P1.vector);
        PointList[3] = PointList[3].SetVector(seg.P0.vector);
    }


    private Centroid GetPointListCentroid()
    {
        Centroid ret = default;

        List<Vector3List> triangles = TriangleSplitter.Split(this);

        ret = CadUtil.TriangleListCentroid(triangles);

        return ret;
    }

    private Centroid GetPointCentroid()
    {
        Centroid ret = default;

        ret.Point = PointList[0].vector;
        ret.Area = 0;

        return ret;
    }

    private Centroid GetSegCentroid()
    {
        Centroid ret = default;

        vector3_t d = PointList[1].vector - PointList[0].vector;

        d /= (vcompo_t)(2.0);

        ret.Point = PointList[0].vector + d;
        ret.Area = 0;

        return ret;
    }

    #region Serialize
    public override void SaveExternalFiles(string fname)
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

    public override void LoadExternalFiles(string fname)
    {
        string basePath = FileUtil.GetExternalDataDir(fname);
        string dfname = Path.Combine(basePath, FilePathName);

        mBitmap = new Bitmap(Image.FromFile(dfname));

        mBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
    }

    public override MpGeometricData_v1002 GeometricDataToMp_v1002()
    {
        MpSimpleGeometricData_v1002 geo = new MpSimpleGeometricData_v1002();
        geo.PointList = MpUtil_v1002.VertexListToMp(PointList);
        return geo;
    }

    public override void GeometricDataFromMp_v1002(MpGeometricData_v1002 geo)
    {
        if (!(geo is MpSimpleGeometricData_v1002))
        {
            return;
        }

        MpSimpleGeometricData_v1002 g = (MpSimpleGeometricData_v1002)geo;

        mPointList = MpUtil_v1002.VertexListFromMp(g.PointList);
    }


    public override MpGeometricData_v1003 GeometricDataToMp_v1003()
    {
        MpPictureGeometricData_v1003 geo = new MpPictureGeometricData_v1003();
        geo.FilePathName = FilePathName;
        geo.PointList = MpUtil_v1003.VertexListToMp(PointList);
        return geo;
    }

    public override void GeometricDataFromMp_v1003(MpGeometricData_v1003 geo)
    {
        if (!(geo is MpPictureGeometricData_v1003))
        {
            return;
        }

        MpPictureGeometricData_v1003 g = (MpPictureGeometricData_v1003)geo;
        FilePathName = g.FilePathName;
        mPointList = MpUtil_v1003.VertexListFromMp(g.PointList);
    }
    #endregion
}
