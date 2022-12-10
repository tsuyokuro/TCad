//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using System.Collections.Generic;



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

public struct LayerListInfo
{
    public List<CadLayer> LayerList;
    public uint CurrentID;
}

public enum StateChangedType
{
    CURRENT_FIG_CHANGED,
    CREATING_FIG_TYPE_CHANGED,
    MESURE_MODE_CHANGED,
    SELECTION_CHANGED,
}

public struct StateChangedParam
{
    public StateChangedType Type
    {
        get; set;
    }

    public StateChangedParam(StateChangedType type)
    {
        Type = type;
    }
}

public struct SelectContext
{
    public DrawContext DC;

    public vector3_t CursorScrPt;
    public vector3_t CursorWorldPt;

    public CadCursor Cursor;

    public bool PointSelected;
    public MarkPoint MarkPt;

    public bool SegmentSelected;
    public MarkSegment MarkSeg;
}
