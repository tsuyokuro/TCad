//#define DEFAULT_DATA_TYPE_DOUBLE
using CadDataTypes;
using OpenTK.Mathematics;
using Plotter.Serializer.v1003;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Path = System.IO.Path;
using Plotter.Serializer;




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

public partial class CadFigurePicture : CadFigure
{
    //  3-------------------2
    //  |                   |
    //  |                   |
    //  |                   |
    //  0-------------------1

    private Bitmap mBitmap;

    private byte[] SrcData; 

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

        FileStream fs = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read);

        SrcData = new byte[fs.Length];

        fs.Read(SrcData, 0, SrcData.Length);

        fs.Close();

        //mBitmap = new Bitmap(Image.FromFile(path));

        Image image = ImageUtil.ByteArrayToImage(SrcData);

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

    public override FigureSegment GetFigSegmentAt(int n)
    {
        int a = n;
        int b = (n + 1) % 4;
        return new FigureSegment(this, n, a, b);
    }

    public override int SegmentCount
    {
        get
        {
            return 4;
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
        CadVertex a = PointList[idxA];
        CadVertex b = PointList[idxB];

        dc.Drawing.DrawLine(pen, a.vector, b.vector);
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
            return;
        }
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

        if (cnt >= 2)
        {
            PointList[0] = StoreList[0] + delta;
            PointList[1] = StoreList[1] + delta;
            PointList[2] = StoreList[2] + delta;
            PointList[3] = StoreList[3] + delta;
            return;
        }

        if (cnt == 1)
        {
            vector3_t d;

            vector3_t normal = CadMath.Normal(StoreList[0].vector, StoreList[1].vector, StoreList[2].vector);

            vector3_t vdir = dc.ViewDir;


            vector3_t a = vector3_t.Zero;
            vector3_t b = vdir;

            vector3_t d0 = CadMath.CrossPlane(a, b, StoreList[0].vector, normal);

            a = delta;
            b = delta + vdir;

            vector3_t d1 = CadMath.CrossPlane(a, b, StoreList[0].vector, normal);

            if (d0.IsValid() && d1.IsValid())
            {
                d = d1 - d0;
            }
            else
            {
                vector3_t nvNormal = CadMath.Normal(normal, vdir);

                vcompo_t ip = CadMath.InnerProduct(nvNormal, delta);

                d = nvNormal * ip;
            }

            AdjustPoints(GetTargetPointIndex(), d);
        }

        mChildList.ForEach(c =>
        {
            c.MoveSelectedPointsFromStored(dc, moveInfo);
        });
    }


    void AdjustPoints(int mIdx, vector3_t d)
    {
        int aIdx = (mIdx + 2) % 4; // 対角のIndex
        int bIdx = (mIdx + 1) % 4; // 次のIndex
        int cIdx = (mIdx + 3) % 4; // 前のIndex

        bool keepAspect = true;

        CrossInfo ci;
        if (keepAspect) { 
            ci = CadMath.PerpCrossLine(
                vector3_t.Zero,
                mPointList[aIdx].vector - mPointList[mIdx].vector,
                d
                );
            d = ci.CrossPoint;
        }

        mPointList[mIdx] = StoreList[mIdx] + d;

        ci = CadMath.PerpCrossLine(
            StoreList[aIdx].vector, StoreList[bIdx].vector,
            mPointList[mIdx].vector
            );
        mPointList[bIdx].vector = ci.CrossPoint;

        ci = CadMath.PerpCrossLine(
            StoreList[cIdx].vector, StoreList[aIdx].vector,
            mPointList[mIdx].vector
            );
        mPointList[cIdx].vector = ci.CrossPoint;
    }



    private int GetTargetPointIndex()
    {
        for (int i = 0; i < mPointList.Count; i++)
        {
            if (mPointList[i].Selected)
            {
                return i;
            }
        }

        return -1;
    }


    public override void InvertDir()
    {
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
}
