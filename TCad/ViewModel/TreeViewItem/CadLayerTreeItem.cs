using TCad.Controls;
using Plotter;
using System;

namespace TCad.ViewModel
{
    class CadLayerTreeItem : CadObjTreeItem
    {
        public CadLayer Layer;

        public override bool IsChecked
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public override string Text
        {
            get
            {
                return $"ID:{Layer.ID} {Layer.Name}";
            }
        }

        public CadLayerTreeItem(CadLayer layer)
        {
            AddChildren(layer);
        }

        public CadLayerTreeItem()
        {
        }

        public void AddChildren(CadLayer layer)
        {
            Layer = layer;

            for (int i = 0; i < Layer.FigureList.Count; i++)
            {
                CadFigure fig = Layer.FigureList[i];

                CadObjTreeItem item = new CadFigTreeItem(fig);
                Add(item);
            }
        }

        public void AddChildren(CadLayer layer, Func<CadFigure, bool> filterFunc)
        {
            Layer = layer;

            for (int i = 0; i < Layer.FigureList.Count; i++)
            {
                CadFigure fig = Layer.FigureList[i];

                if (!filterFunc(fig))
                {
                    continue;
                }

                CadObjTreeItem item = new CadFigTreeItem(fig);
                Add(item);
            }
        }
    }
}
