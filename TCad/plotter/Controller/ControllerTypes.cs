using OpenTK.Mathematics;
using System.Collections.Generic;

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
