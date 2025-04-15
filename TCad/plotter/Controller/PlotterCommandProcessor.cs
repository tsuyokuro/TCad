using System.Collections.Generic;
using CadDataTypes;

namespace Plotter.Controller;

// Actions for DB

public class PlotterCommandProcessor
{
    IPlotterController Controller;

    public CadObjectDB DB
    {
        get => Controller.DB;
    }

    public CadLayer CurrentLayer
    {
        get => Controller.CurrentLayer;
        set => Controller.CurrentLayer = value;
    }

    public HistoryManager HistoryMan
    {
        get => Controller.HistoryMan;
    }

    DrawContext DC
    {
        get => Controller.DC;
    }

    public PlotterCommandProcessor(IPlotterController controller)
    {
        Controller = controller;
    }

    public void ClearAll()
    {
        Controller.ClearAll();
    }

    #region Layer
    public void SelectAllInCurrentLayer()
    {
        foreach (CadFigure fig in CurrentLayer.FigureList)
        {
            fig.Select();
        }
    }

    public void ClearLayer(uint layerID)
    {
        if (layerID == 0)
        {
            layerID = CurrentLayer.ID;
        }

        CadLayer layer = DB.GetLayer(layerID);

        if (layer == null) return;

        CadOpeList opeList = layer.Clear();

        HistoryMan.foward(opeList);
    }

    public void AddLayer(string name)
    {
        CadLayer layer = DB.NewLayer();

        layer.Name = name;

        CurrentLayer = layer;

        DB.LayerList.Add(layer);

        Controller.UpdateLayerList();

        ItConsole.println("Layer added.  Name:" + layer.Name + " ID:" + layer.ID);
    }

    public void RemoveLayer(uint id)
    {
        if (DB.LayerList.Count == 1)
        {
            return;
        }

        CadLayer layer = DB.GetLayer(id);

        if (layer == null)
        {
            return;
        }

        int index = DB.LayerIndex(id);

        int nextCurrentIdx = -1;

        if (CurrentLayer.ID == id)
        {
            nextCurrentIdx = DB.LayerIndex(CurrentLayer.ID);
        }

        CadOpeRemoveLayer ope = new CadOpeRemoveLayer(layer, index);
        HistoryMan.foward(ope);

        DB.RemoveLayer(id);

        if (nextCurrentIdx >= 0)
        {
            if (nextCurrentIdx > DB.LayerList.Count - 1)
            {
                nextCurrentIdx = DB.LayerList.Count - 1;
            }

            CurrentLayer = DB.LayerList[nextCurrentIdx];
        }

        Controller.UpdateLayerList();
        ItConsole.println("Layer removed.  Name:" + layer.Name + " ID:" + layer.ID);
    }
    #endregion


    public void MovePointsFromStored(List<CadFigure> figList, MoveInfo moveInfo)
    {
        if (figList == null)
        {
            return;
        }

        if (figList.Count == 0)
        {
            return;
        }

        foreach (CadFigure fig in figList)
        {
            fig.MoveSelectedPointsFromStored(DC, moveInfo);
        }
    }

    public void Remove()
    {
        Controller.EditManager.StartEdit();

        Controller.Editor.RemoveSelectedPoints();

        Controller.EditManager.EndEdit();
    }

    public void InsPoint()
    {
        Controller.EditManager.StartEdit();
        if (Controller.Editor.InsPointToLastSelectedSeg())
        {
            Controller.EditManager.EndEdit();
        }
        else
        {
            Controller.EditManager.AbendEdit();
        }
    }

    public void AddPointToCursorPos()
    {
        CadFigure fig = DB.NewFigure(CadFigure.Types.POINT);
        fig.AddPoint((CadVertex)Controller.Input.GetCursorPos());

        fig.EndCreate(DC);

        CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, fig.ID);

        CurrentLayer.AddFigure(fig);

        HistoryMan.foward(ope);
    }

    public void Copy()
    {
        PlotterClipboard.CopyFiguresAsBin(Controller);
    }

    public void Paste()
    {
        Controller.Input.ClearSelection();

        PlotterClipboard.PasteFiguresAsBin(Controller);
        Controller.UpdateObjectTree(true);
    }

    private struct ClusterInfo
    {
        public int Top;
        public int Bottom;

        public List<CadFigure> FigList;
        public List<CadFigure> SelFigList;

        public ClusterInfo(int cnt)
        {
            Top = cnt;
            Bottom = 0;

            FigList = new List<CadFigure>();
            SelFigList = new List<CadFigure>();
        }

        public void AddSelFigure(CadFigure fig, int foundIdx)
        {
            if (foundIdx < Top)
            {
                Top = foundIdx;
            }

            if (foundIdx > Bottom)
            {
                Bottom = foundIdx;
            }

            SelFigList.Add(fig);
        }
    }

    #region Object order

    private ClusterInfo SeparateSlectedFigs(List<CadFigure> figList)
    {
        int cnt = figList.Count;

        ClusterInfo ci = new ClusterInfo(cnt);

        for (int i=0; i<cnt; i++)
        {
            CadFigure fig = figList[i];

            if (fig.HasSelectedPointInclueChild())
            {
                ci.AddSelFigure(fig, i);
            }
            else
            {
                ci.FigList.Add(fig);
            }
        }

        return ci;
    }

    public void ObjOrderDown()
    {
        ClusterInfo ci = SeparateSlectedFigs(CurrentLayer.FigureList);

        if (ci.SelFigList.Count == 0) return;

        int ins = ci.Bottom - ci.SelFigList.Count + 2;

        if (ins > ci.FigList.Count) {
            return;
        }

        ci.FigList.InsertRange(ins, ci.SelFigList);

        ChangeLayerFigList(CurrentLayer, ci.FigList);
    }

    public void ObjOrderUp()
    {
        ClusterInfo ci = SeparateSlectedFigs(CurrentLayer.FigureList);

        if (ci.SelFigList.Count == 0) return;

        int ins = ci.Top - 1;

        if (ins < 0)
        {
            return;
        }

        ci.FigList.InsertRange(ins, ci.SelFigList);

        ChangeLayerFigList(CurrentLayer, ci.FigList);
    }

    public void ObjOrderBottom()
    {
        ClusterInfo ci = SeparateSlectedFigs(CurrentLayer.FigureList);

        if (ci.SelFigList.Count == 0) return;

        int ins = ci.FigList.Count;

        if (ins < 0)
        {
            return;
        }

        ci.FigList.InsertRange(ins, ci.SelFigList);

        ChangeLayerFigList(CurrentLayer, ci.FigList);
    }

    public void ObjOrderTop()
    {
        ClusterInfo ci = SeparateSlectedFigs(CurrentLayer.FigureList);

        if (ci.SelFigList.Count == 0) return;

        int ins = 0;

        ci.FigList.InsertRange(ins, ci.SelFigList);

        ChangeLayerFigList(CurrentLayer, ci.FigList);
    }

    private void ChangeLayerFigList(CadLayer layer, List<CadFigure> newFigList)
    {
        HistoryMan.foward(new CadOpeChangeFigureList(layer, layer.FigureList, newFigList));

        layer.FigureList = newFigList;

        Controller.ViewModel.UpdateTreeView(true);
    }

    #endregion
}
