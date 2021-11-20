using OpenTK;
using System.Collections.Generic;

namespace Plotter.Controller
{
    // Actions for DB

    public partial class PlotterController
    {
        public void ClearAll()
        {
            PageSize = new PaperPageSize();

            mDB.ClearAll();
            HistoryMan.Clear();

            UpdateLayerList();
            UpdateObjectTree(true);
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

            CadLayer layer = mDB.GetLayer(layerID);

            if (layer == null) return;

            CadOpeList opeList = layer.Clear();

            HistoryMan.foward(opeList);
        }

        public void AddLayer(string name)
        {
            CadLayer layer = mDB.NewLayer();

            layer.Name = name;

            CurrentLayer = layer;

            mDB.LayerList.Add(layer);

            UpdateLayerList();

            ItConsole.println("Layer added.  Name:" + layer.Name + " ID:" + layer.ID);
        }

        public void RemoveLayer(uint id)
        {
            if (mDB.LayerList.Count == 1)
            {
                return;
            }

            CadLayer layer = mDB.GetLayer(id);

            if (layer == null)
            {
                return;
            }

            int index = mDB.LayerIndex(id);

            int nextCurrentIdx = -1;

            if (CurrentLayer.ID == id)
            {
                nextCurrentIdx = mDB.LayerIndex(CurrentLayer.ID);
            }

            CadOpeRemoveLayer ope = new CadOpeRemoveLayer(layer, index);
            HistoryMan.foward(ope);

            mDB.RemoveLayer(id);

            if (nextCurrentIdx >= 0)
            {
                if (nextCurrentIdx > mDB.LayerList.Count - 1)
                {
                    nextCurrentIdx = mDB.LayerList.Count - 1;
                }

                CurrentLayer = mDB.LayerList[nextCurrentIdx];
            }

            UpdateLayerList();
            ItConsole.println("Layer removed.  Name:" + layer.Name + " ID:" + layer.ID);
        }
        #endregion

        public void SelectFigure(uint figID)
        {
            CadFigure fig = DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            fig.Select();
        }

        public void SelectFigureById(uint id, int idx, bool clearSelect = true)
        {
            CadFigure fig = mDB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            if (idx >= fig.PointCount)
            {
                return;
            }

            if (clearSelect)
            {
                ClearSelection();
            }

            if (idx >= 0)
            {
                fig.SelectPointAt(idx, true);
            }
            else
            {
                fig.Select();
            }

            CurrentFigure = fig;
        }

        public void MovePointsFromStored(List<CadFigure> figList, Vector3d d)
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
                fig.MoveSelectedPointsFromStored(DC, d);
            }
        }

        public void Remove()
        {
            StartEdit();

            RemoveSelectedPoints();

            EndEdit();
        }

        public void InsPoint()
        {
            StartEdit();
            if (InsPointToLastSelectedSeg())
            {
                EndEdit();
            }
            else
            {
                AbendEdit();
            }
        }

        public void Copy()
        {
            PlotterClipboard.CopyFiguresAsBin(this);
        }

        public void Paste()
        {
            ClearSelection();

            PlotterClipboard.PasteFiguresAsBin(this);
            UpdateObjectTree(true);
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

            Callback.UpdateObjectTree(true);
        }

        #endregion
    }
}