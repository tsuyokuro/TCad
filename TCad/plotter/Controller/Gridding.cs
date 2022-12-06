using OpenTK.Mathematics;
using System;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public class Gridding
{
    private vector3_t mGridSize;

    public vector3_t GridSize
    {
        set
        {
            mGridSize = value;
        }

        get
        {
            return mGridSize;
        }
    }

    public vcompo_t Range = 8;

    public vector3_t MatchW;
    public vector3_t MatchD;

    public Gridding()
    {
        GridSize = new vector3_t(10, 10, 10);
    }

    public void Clear()
    {

    }

    public void CopyFrom(Gridding g)
    {
        mGridSize = g.mGridSize;
        Range = g.Range;
    }

    public void Check(DrawContext dc, vector3_t scrp)
    {
        vector3_t p = dc.DevPointToWorldPoint(scrp);

        p.X = (long)((p.X + (vcompo_t)Math.Sign(p.X) * (GridSize.X / (vcompo_t)(2.0))) / GridSize.X) * GridSize.X;
        p.Y = (long)((p.Y + (vcompo_t)Math.Sign(p.Y) * (GridSize.Y / (vcompo_t)(2.0))) / GridSize.Y) * GridSize.Y;
        p.Z = (long)((p.Z + (vcompo_t)Math.Sign(p.Z) * (GridSize.Z / (vcompo_t)(2.0))) / GridSize.Z) * GridSize.Z;

        MatchW = p;
        MatchD = dc.WorldPointToDevPoint(p);
    }


    /**
     * 画面上での間隔が min より大きくなるように間引く為のサイズの
     * 倍率を求める
     */
    public vcompo_t Decimate(DrawContext dc, Gridding grid, vcompo_t min)
    {
        vcompo_t scaleX = (vcompo_t)(1.0);
        vcompo_t scaleY = (vcompo_t)(1.0);
        vcompo_t scaleZ = (vcompo_t)(1.0);

        vcompo_t gridSizeX = grid.GridSize.X;
        vcompo_t gridSizeY = grid.GridSize.Y;
        vcompo_t gridSizeZ = grid.GridSize.Z;

        vector3_t devLen;
        vcompo_t t = 1;
        vcompo_t d;

        vcompo_t devLenX;
        vcompo_t devLenY;
        vcompo_t devLenZ;

        // X axis
        devLen = dc.WorldVectorToDevVector(new vector3_t(gridSizeX, 0, 0));

        devLenX = (vcompo_t)Math.Max(Math.Abs(devLen.X), (vcompo_t)Math.Abs(devLen.Y));
        if (devLenX != 0 && devLenX < min)
        {
            d = (vcompo_t)Math.Ceiling(min / devLenX) * devLenX;
            t = d / devLenX;
        }

        if (t > scaleX)
        {
            scaleX = t;
        }


        // Y axis
        devLen = dc.WorldVectorToDevVector(new vector3_t(0, gridSizeY, 0));

        devLenY = (vcompo_t)Math.Max(Math.Abs(devLen.X), (vcompo_t)Math.Abs(devLen.Y));
        if (devLenY != 0 && devLenY < min)
        {
            d = (vcompo_t)Math.Ceiling(min / devLenY) * devLenY;
            t = d / devLenY;
        }

        if (t > scaleY)
        {
            scaleY = t;
        }


        // Z axis
        devLen = dc.WorldVectorToDevVector(new vector3_t(0, 0, gridSizeZ));

        devLenZ = (vcompo_t)Math.Max(Math.Abs(devLen.X), (vcompo_t)Math.Abs(devLen.Y));

        if (devLenZ != 0 && devLenZ < min)
        {
            d = (vcompo_t)Math.Ceiling(min / devLenZ) * devLenZ;
            t = d / devLenZ;
        }

        if (t > scaleZ)
        {
            scaleZ = t;
        }

        return (vcompo_t)Math.Max(Math.Max(scaleX, scaleY), scaleZ);
    }
}
