using System;
using System.Collections.Generic;

namespace Plotter
{
    public struct FigureBelong
    {
        public CadLayer Layer;
        public int Index;
    }

    public class CadObjectDB
    {
        public const uint Version = 0x00010000;

        #region "Manage Layer"

        public uint CurrentLayerID
        {
            get => mCurrentLayer == null ? 0 : mCurrentLayer.ID;
            set => mCurrentLayer = GetLayer(value);
        }

        private CadLayer mCurrentLayer;
        public CadLayer CurrentLayer
        {
            get => mCurrentLayer;
            set => mCurrentLayer = value;
        }

        private Dictionary<uint, CadLayer> mLayerIdMap = new Dictionary<uint, CadLayer>();
        public Dictionary<uint, CadLayer> LayerMap
        {
            get => mLayerIdMap;
            set => mLayerIdMap = value;
        }

        private IdProvider mLayerIdProvider = new IdProvider();
        public IdProvider LayerIdProvider
        {
            get => mLayerIdProvider;
        }


        private List<CadLayer> mLayerList = new List<CadLayer>();
        public List<CadLayer> LayerList
        {
            get => mLayerList;
            set => mLayerList = value;
        }


        public CadLayer GetLayer(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            CadLayer layer;
            mLayerIdMap.TryGetValue(id, out layer);
            return layer;
        }

        public CadLayer NewLayer()
        {
            CadLayer layer = new CadLayer();
            AddLayer(layer);
            return layer;
        }

        public uint AddLayer(CadLayer layer)
        {
            layer.ID = mLayerIdProvider.getNew();
            mLayerIdMap.Add(layer.ID, layer);
            return layer.ID;
        }

        public uint InserLayer(CadLayer layer, int index)
        {
            if (layer.ID == 0)
            {
                layer.ID = mLayerIdProvider.getNew();
            }

            mLayerIdMap.Add(layer.ID, layer);
            mLayerList.Insert(index, layer);

            return layer.ID;
        }

        public void RemoveLayer(uint id)
        {
            mLayerIdMap.Remove(id);
            mLayerList.RemoveAll(a => a.ID == id);
        }

        public int LayerIndex(uint id)
        {
            int idx = 0;
            foreach (CadLayer layer in mLayerList)
            {
                if (layer.ID == id)
                {
                    return idx;
                }

                idx++;
            }

            return -1;
        }
        
        #endregion



        #region "Manage Figure"
        private Dictionary<uint, CadFigure> mFigureIdMap = new Dictionary<uint, CadFigure>();
        public Dictionary<uint, CadFigure> FigureMap
        {
            get => mFigureIdMap;
            set => mFigureIdMap = value;
        }

        IdProvider mFigIdProvider = new IdProvider();
        public IdProvider FigIdProvider
        {
            get => mFigIdProvider;
        }


        public CadFigure GetFigure(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            CadFigure fig;
            mFigureIdMap.TryGetValue(id, out fig);

            return fig;
        }

        public CadFigure NewFigure(CadFigure.Types type)
        {
            CadFigure fig = CadFigure.Create(type);

            AddFigure(fig);
            return fig;
        }

        public uint AddFigure(CadFigure fig)
        {
            fig.ID = mFigIdProvider.getNew();
            mFigureIdMap.Add(fig.ID, fig);

            return fig.ID;
        }

        public void RelaseFigure(uint id)
        {
            mFigureIdMap.Remove(id);
        }

        #endregion Manage Figure


        
        #region Walk
        //public static Func<CadLayer, bool> EditableLayerFilter = (layer) =>
        //{
        //    if (layer.Locked) return false;
        //    if (!layer.Visible) return false;

        //    return true;
        //};

        //public void ForEachEditableFigure(Action<CadLayer, CadFigure> walk)
        //{
        //    mLayerList.ForEach(layer =>
        //    {
        //        if (!EditableLayerFilter(layer))
        //        {
        //            return;
        //        }

        //        layer.ForEachFig(fig =>
        //        {
        //            walk(layer, fig);
        //        });
        //    });
        //}


        private void FigureAction(CadLayer layer, CadFigure fig, Action<CadLayer, CadFigure> action)
        {
            action(layer, fig);

            if (fig.ChildList == null)
            {
                return;
            }

            foreach (CadFigure c in fig.ChildList)
            {
                FigureAction(layer, c, action);
            }
        }

        public void ForEachEditableFigure(Action<CadLayer, CadFigure> action)
        {
            foreach (CadLayer layer in mLayerList)
            {
                if (layer.Locked) continue;
                if (!layer.Visible) continue;

                foreach (CadFigure fig in layer.FigureList)
                {
                    FigureAction(layer, fig, action);
                }
            }
        }

        #endregion Walk

        #region Query
        public List<uint> GetSelectedFigIDList()
        {
            List<uint> idList = new List<uint>();

            HashSet<uint> idSet = new HashSet<uint>();

            foreach (CadLayer layer in LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint() || fig.IsSelected)
                    {
                        idSet.Add(fig.ID);
                        if (fig.Parent != null)
                        {
                            idSet.Add(fig.Parent.ID);
                        }
                    }
                });
            }

            idList.AddRange(idSet);

            return idList;
        }

        public List<CadFigure> GetSelectedFigList()
        {
            List<CadFigure> list = new List<CadFigure>();

            HashSet<CadFigure> fset = new HashSet<CadFigure>();

            foreach (CadLayer layer in LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint() || fig.IsSelected)
                    {
                        fset.Add(fig);
                        if (fig.Parent != null)
                        {
                            fset.Add(fig.Parent);
                        }
                    }
                });
            }

            list.AddRange(fset);

            return list;
        }
        #endregion Query

        public void ClearAll()
        {
            LayerMap.Clear();
            LayerIdProvider.Reset();
            LayerList.Clear();
            FigureMap.Clear();
            FigIdProvider.Reset();

            GC.Collect();

            CadLayer layer = NewLayer();

            LayerList.Add(layer);

            CurrentLayerID = layer.ID;
            CurrentLayer = layer;
        }

        #region "For debug"
        public void dump()
        {
            DOut.pl(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            DOut.Indent++;

            {
                List<uint> ids = new List<uint>(mLayerIdMap.Keys);

                DOut.pl("Layer map {");
                DOut.Indent++;
                foreach (uint id in ids)
                {
                    CadLayer layer = mLayerIdMap[id];
                    layer.sdump();
                }
                DOut.Indent--;
                DOut.pl("}");
            }

            {
                DOut.pl("Layer list {");
                DOut.Indent++;
                foreach (CadLayer layer in mLayerList)
                {
                    layer.dump();
                }
                DOut.Indent--;
                DOut.pl("}");
            }

            dumpFigureMap();

            DOut.Indent--;
            DOut.pl("}");
        }

        public void dumpFigureMap()
        {
            List<uint> ids = new List<uint>(mFigureIdMap.Keys);

            DOut.pl("Figure map {");
            DOut.Indent++;
            foreach (uint id in ids)
            {
                CadFigure fig = mFigureIdMap[id];
                fig.Dump("fig");
            }
            DOut.Indent--;
            DOut.pl("}");
        }

        #endregion
    }
}