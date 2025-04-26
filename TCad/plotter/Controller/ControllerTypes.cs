using System.Collections.Generic;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.searcher;

namespace Plotter.Controller;

public struct LayerListInfo
{
    public List<CadLayer> LayerList;
    public uint CurrentID;

    public LayerListInfo(List<CadLayer> layerList, uint currentID)
    {
        LayerList = layerList;
        CurrentID = currentID;
    }
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
