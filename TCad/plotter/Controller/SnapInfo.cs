//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
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


namespace Plotter.Controller;

public struct SnapInfo
{
    public CadCursor Cursor;
    public vector3_t SnapPoint;

    public bool IsPointMatch {  get; set; }

    public PointSearcher PointSearcher;

    public SegSearcher SegSearcher;

    public SnapInfo(
        CadCursor cursor,
        vector3_t snapPoint,
        PointSearcher pointSearcher,
        SegSearcher segSearcher
        )
    {
        Cursor = cursor;
        SnapPoint = snapPoint;
        IsPointMatch = false;
        PointSearcher = pointSearcher;
        SegSearcher = segSearcher;
    }
}
