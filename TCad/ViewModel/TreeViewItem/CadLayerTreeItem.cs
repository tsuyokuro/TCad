//#define DEFAULT_DATA_TYPE_DOUBLE
using TCad.Controls;
using Plotter;
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


namespace TCad.ViewModel;

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
