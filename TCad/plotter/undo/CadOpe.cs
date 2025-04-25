using CadDataTypes;
using Plotter;
using Plotter.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using TCad.plotter.Serializer;

namespace TCad.plotter.undo;

/**
* Item for history of user operation
* 
*/
public abstract class CadOpe
{
    protected CadOpe()
    {
    }

    public abstract void Undo(IPlotterController pc);
    public abstract void Redo(IPlotterController pc);

    public virtual void Dispose(IPlotterController pc)
    {
    }
}

public class CadOpeDBSnapShot : CadOpe
{
    public byte[] Before;
    public byte[] After;

    public CadOpeDBSnapShot() { }

    public void StoreBefore(CadObjectDB db)
    {
        Before = CopyUtil.DBToLz4(db);
        Log.pl(nameof(CadOpeDBSnapShot) + " StoreBefore data size:" + Before.Length);
    }

    public void StoreAfter(CadObjectDB db)
    {
        After = CopyUtil.DBToLz4(db);
        Log.pl(nameof(CadOpeDBSnapShot) + " StoreAfter data size:" + After.Length);
    }

    public override void Undo(IPlotterController pc)
    {
        CadObjectDB db = CopyUtil.Lz4BinRestoreDB(Before);
        pc.SetDB(db, false);
    }

    public override void Redo(IPlotterController pc)
    {
        CadObjectDB db = CopyUtil.Lz4BinRestoreDB(After);
        pc.SetDB(db, false);
    }
}

public class CadOpeFigureSnapShot : CadOpe
{
    public byte[] Before;
    public byte[] After;

    public uint FigureID = 0;

    public CadOpeFigureSnapShot() { }

    public void StoreBefore(CadFigure fig)
    {
        Before = CopyUtil.FigToLz4Bin(fig);
        FigureID = fig.ID;
    }

    public void StoreAfter(CadFigure fig)
    {
        After = CopyUtil.FigToLz4Bin(fig);
    }

    public override void Undo(IPlotterController pc)
    {
        CopyUtil.Lz4BinRestoreFig(Before, pc.DB);
    }

    public override void Redo(IPlotterController pc)
    {
        CopyUtil.Lz4BinRestoreFig(After, pc.DB);
    }
}

public class CadOpeFigureSnapShotList : CadOpe
{
    public List<CadOpeFigureSnapShot> SnapShotList = new List<CadOpeFigureSnapShot>();

    public int Count
    {
        get => SnapShotList.Count;
    }

    public CadOpeFigureSnapShotList()
    {

    }

    public void StoreBefore(List<CadFigure> figList)
    {
        for (int i = 0; i < figList.Count; i++)
        {
            CadOpeFigureSnapShot ss = new CadOpeFigureSnapShot();

            ss.StoreBefore(figList[i]);

            SnapShotList.Add(ss);
        }
    }

    public void StoreAfter(CadObjectDB db)
    {
        for (int i = 0; i < SnapShotList.Count; i++)
        {
            CadOpeFigureSnapShot ss = SnapShotList[i];
            ss.StoreAfter(db.GetFigure(ss.FigureID));
        }
    }

    public override void Undo(IPlotterController pc)
    {
        for (int i = 0; i < SnapShotList.Count; i++)
        {
            SnapShotList[i].Undo(pc);
        }
    }

    public override void Redo(IPlotterController pc)
    {
        for (int i = 0; i < SnapShotList.Count; i++)
        {
            SnapShotList[i].Redo(pc);
        }
    }
}


public class CadOpeList : CadOpe
{
    public List<CadOpe> OpeList { get; protected set; } = null;

    public int Count
    {
        get => OpeList.Count();
    }

    public CadOpeList()
    {
        OpeList = new List<CadOpe>();
    }

    public CadOpeList(List<CadOpe> list)
    {
        OpeList = new List<CadOpe>(list);
    }

    public void Clear()
    {
        OpeList.Clear();
    }

    public void Add(CadOpe ope)
    {
        OpeList.Add(ope);
    }

    public override void Undo(IPlotterController pc)
    {
        foreach (CadOpe ope in OpeList.Reverse<CadOpe>())
        {
            ope.Undo(pc);
        }
    }

    public override void Redo(IPlotterController pc)
    {
        foreach (CadOpe ope in OpeList)
        {
            ope.Redo(pc);
        }
    }
}

#region point base
public abstract class CadOpePointBase : CadOpe
{
    protected uint LayerID;
    protected uint FigureID;
    protected int PointIndex;

    public CadOpePointBase(
        uint layerID,
        uint figureID,
        int pointIndex)
    {
        LayerID = layerID;
        FigureID = figureID;
        PointIndex = pointIndex;
    }
}

public class CadOpeAddPoint : CadOpePointBase
{
    private CadVertex Point;

    public CadOpeAddPoint(
        uint layerID,
        uint figureID,
        int pointIndex,
        ref CadVertex pt)
        : base(layerID, figureID, pointIndex)
    {
        Point = pt;
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.RemovePointAt(PointIndex);
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.AddPoint(Point);
    }
}


public class CadOpeInsertPoints : CadOpePointBase
{
    private int InsertNum;

    private VertexList mPointList = null;

    public CadOpeInsertPoints(
        uint layerID,
        uint figureID,
        int startIndex,
        int insertNum)
        : base(layerID, figureID, startIndex)
    {
        InsertNum = insertNum;
    }

    public override void Undo(IPlotterController pc)
    {
        CadLayer layer = pc.DB.GetLayer(LayerID);
        CadFigure fig = pc.DB.GetFigure(FigureID);

        if (fig == null)
        {
            return;
        }

        int idx = PointIndex;
        int i = 0;

        if (mPointList == null)
        {
            mPointList = new VertexList();
        }

        mPointList.Clear();

        for (; i < InsertNum; i++)
        {
            mPointList.Add(fig.GetPointAt(idx + i));
        }

        fig.RemovePointsRange(idx, InsertNum);
    }

    public override void Redo(IPlotterController pc)
    {
        CadLayer layer = pc.DB.GetLayer(LayerID);
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.InsertPointsRange(PointIndex, mPointList);
    }
}
#endregion


#region Figure base
public abstract class CadOpeFigureBase : CadOpe
{
    protected uint LayerID;
    protected uint FigureID;

    public CadOpeFigureBase(
        uint layerID,
        uint figureID
        )
    {
        LayerID = layerID;
        FigureID = figureID;
    }
}

public class CadOpeSetClose : CadOpeFigureBase
{
    bool Close = false;

    public CadOpeSetClose(uint layerID, uint figureID, bool on)
        : base(layerID, figureID)
    {
        Close = on;
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.IsLoop = !Close;
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.IsLoop = Close;
    }
}

public class CadOpeAddFigure : CadOpeFigureBase
{
    public CadOpeAddFigure(uint layerID, uint figureID)
        : base(layerID, figureID)
    {
    }

    public override void Undo(IPlotterController pc)
    {
        CadLayer layer = pc.DB.GetLayer(LayerID);
        layer.RemoveFigureByID(pc.DB, FigureID);
    }

    public override void Redo(IPlotterController pc)
    {
        CadLayer layer = pc.DB.GetLayer(LayerID);
        CadFigure fig = pc.DB.GetFigure(FigureID);
        layer.AddFigure(fig);
    }

    public override void Dispose(IPlotterController pc)
    {
        pc.DB.RelaseFigure(FigureID);
    }
}

public class CadOpeRemoveFigure : CadOpeFigureBase
{
    int mFigureIndex = 0;

    public CadOpeRemoveFigure(CadLayer layer, uint figureID)
        : base(layer.ID, figureID)
    {
        int figIndex = layer.GetFigureIndex(figureID);
        mFigureIndex = figIndex;
    }

    public override void Undo(IPlotterController pc)
    {
        CadLayer layer = pc.DB.GetLayer(LayerID);
        CadFigure fig = pc.DB.GetFigure(FigureID);
        layer.InsertFigure(mFigureIndex, fig);
    }

    public override void Redo(IPlotterController pc)
    {
        CadLayer layer = pc.DB.GetLayer(LayerID);
        layer.RemoveFigureByID(pc.DB, FigureID);
    }
}
#endregion

public class CadOpeAddChildlen : CadOpe
{
    private uint ParentID = 0;
    private List<uint> ChildIDList = new List<uint>();

    public CadOpeAddChildlen(CadFigure parent, List<CadFigure> childlen)
    {
        ParentID = parent.ID;

        childlen.ForEach(a =>
        {
            ChildIDList.Add(a.ID);
        });
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);

        foreach (uint childID in ChildIDList)
        {
            parent.ChildList.RemoveAll(a => a.ID == childID);
            CadFigure fig = pc.DB.GetFigure(childID);
            fig.Parent = null;
        }
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);

        foreach (uint childID in ChildIDList)
        {
            CadFigure fig = pc.DB.GetFigure(childID);
            parent.AddChild(fig);
        }
    }
}

public class CadOpeRemoveChildlen : CadOpe
{
    private uint ParentID = 0;
    private List<uint> ChildIDList = new List<uint>();

    public CadOpeRemoveChildlen(CadFigure parent, List<CadFigure> childlen)
    {
        ParentID = parent.ID;

        childlen.ForEach(a =>
        {
            ChildIDList.Add(a.ID);
        });
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);

        foreach (uint childID in ChildIDList)
        {
            CadFigure fig = pc.DB.GetFigure(childID);
            parent.AddChild(fig);
        }
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);

        foreach (uint childID in ChildIDList)
        {
            parent.ChildList.RemoveAll(a => a.ID == childID);
            CadFigure fig = pc.DB.GetFigure(childID);
            fig.Parent = null;
        }
    }
}

public class CadOpeAddChild : CadOpe
{
    private uint ParentID = 0;
    private uint ChildID = 0;
    private int Index;

    public CadOpeAddChild(CadFigure parent, CadFigure child, int index)
    {
        ParentID = parent.ID;
        ChildID = child.ID;
        Index = index;
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);
        CadFigure child = pc.DB.GetFigure(ChildID);
        parent.ChildList.Remove(child);
        child.Parent = null;
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);
        CadFigure child = pc.DB.GetFigure(ChildID);
        parent.ChildList.Insert(Index, child);
        child.Parent = parent;
    }
}

public class CadOpeRemoveChild : CadOpe
{
    private uint ParentID = 0;
    private uint ChildID;
    private int Index;

    public CadOpeRemoveChild(CadFigure parent, CadFigure child, int index)
    {
        ParentID = parent.ID;
        ChildID = child.ID;
        Index = index;
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);
        CadFigure child = pc.DB.GetFigure(ChildID);
        parent.ChildList.Insert(Index, child);
        child.Parent = parent;
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure parent = pc.DB.GetFigure(ParentID);
        CadFigure child = pc.DB.GetFigure(ChildID);
        parent.ChildList.Remove(child);
        child.Parent = null;
    }
}

public class CadOpeChangeNormal : CadOpe
{
    private uint FigureID;
    private vector3_t NewNormal;
    private vector3_t OldNormal;

    public CadOpeChangeNormal(uint figID, vector3_t oldNormal, vector3_t newNormal)
    {
        FigureID = figID;
        OldNormal = oldNormal;
        NewNormal = newNormal;
    }

    public override void Undo(IPlotterController pc)
    {
    }

    public override void Redo(IPlotterController pc)
    {
    }
}

public class CadOpeInvertDir : CadOpe
{
    private uint FigureID;

    public CadOpeInvertDir(uint figID)
    {
        FigureID = figID;
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.InvertDir();
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.InvertDir();
    }
}

public class CadOpeRemoveLayer : CadOpe
{
    private CadLayer Layer;
    private int Index;

    public CadOpeRemoveLayer(CadLayer layer, int index)
    {
        Layer = layer;
        Index = index;
    }

    public override void Redo(IPlotterController pc)
    {
        pc.DB.RemoveLayer(Layer.ID, adjustCurrent: true);
    }

    public override void Undo(IPlotterController pc)
    {
        pc.DB.InsertLayer(Layer, Index);
    }
}

public class CadOpeChangeFigureList : CadOpe
{
    private CadLayer Layer;
    private List<CadFigure> OldList;
    private List<CadFigure> NewList;

    public CadOpeChangeFigureList(CadLayer layer, List<CadFigure> oldList, List<CadFigure> newList)
    {
        Layer = layer;
        OldList = oldList;
        NewList = newList;
    }

    public override void Redo(IPlotterController pc)
    {
        Layer.FigureList = NewList;
    }

    public override void Undo(IPlotterController pc)
    {
        Layer.FigureList = OldList;
    }
}

public class CadChangeFilgLinePen : CadOpe
{
    private DrawPen OldPen;
    private DrawPen NewPen;
    private uint FigureID;

    public CadChangeFilgLinePen(uint figureID, DrawPen oldPen, DrawPen newPen)
    {
        FigureID = figureID;
        OldPen = oldPen;
        NewPen = newPen;
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.LinePen = NewPen;
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.LinePen = OldPen;
    }
}

public class CadChangeFilgFillBrush : CadOpe
{
    private DrawBrush OldBrush;
    private DrawBrush NewBrush;
    private uint FigureID;

    public CadChangeFilgFillBrush(uint figureID, DrawBrush oldBrush, DrawBrush newBrush)
    {
        FigureID = figureID;
        OldBrush = oldBrush;
        NewBrush = newBrush;
    }

    public override void Redo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.FillBrush = NewBrush;
    }

    public override void Undo(IPlotterController pc)
    {
        CadFigure fig = pc.DB.GetFigure(FigureID);
        fig.FillBrush = OldBrush;
    }
}

