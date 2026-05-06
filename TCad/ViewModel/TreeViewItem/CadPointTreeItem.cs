using CadDataTypes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using TCad.Controls;
using TCad.Plotter.Model.Figure;

namespace TCad.ViewModel;

class CadPointTreeItem : CadObjTreeItem
{
    public const string ITEM_CMD_COPY_DATA = "copy_data";


    public CadFigure Fig;
    public int Index;

    public static SolidColorBrush CheckedBackColor = new SolidColorBrush(Color.FromRgb(0x11, 0x46, 0x11));

    public override bool IsChecked
    {
        get
        {
            if (Index >= 0 && Index < Fig.PointCount)
            {
                return Fig.GetPointAt(Index).Selected;
            }

            return false;
        }

        set
        {
            CadVertex v = Fig.GetPointAt(Index);
            v.Selected = value;
            Fig.SetPointAt(Index, v);
        }
    }


    public override string Text
    {
        get
        {
            CadVertex v;

            if (Index >= 0 && Index < Fig.PointCount)
            {
                v = Fig.GetPointAt(Index);
                return $"{v.X.ToString("F2")}, {v.Y.ToString("F2")}, {v.Z.ToString("F2")}";
            }

            return "removed";
        }
    }


    public CadPointTreeItem(CadFigure fig, int idx)
    {
        Type = CadObjTreeItemType.LEAF;
        Fig = fig;
        Index = idx;
    }

    public override List<MenuItem> GetContextMenuItems()
    {
        List<MenuItem> list = new List<MenuItem>();

        MenuItem menuItem = new MenuItem();
        menuItem.Header = "Copy Data";
        menuItem.Tag = CreateContextMenuTag(ITEM_CMD_COPY_DATA);

        list.Add(menuItem);

        return list;
    }
}
