using TCad.Controls;
using CadDataTypes;
using System.Windows.Controls;
using System.Collections.Generic;
using Plotter;

namespace TCad.ViewModel;

class CadFigTreeItem : CadObjTreeItem
{
    public const string ITEM_CMD_CHANGE_NAME = "change_name";

    public CadFigure Fig;

    public override bool IsChecked
    {
        get
        {
            return HasSelectedPoint() || Fig.IsSelected;
        }

        set
        {
            Fig.IsSelected = value;
            Fig.Current = value;
            SelectAllPoints(value);
        }
    }

    public override string Text
    {
        get
        {
            if (Fig.Name == null)
            {
                return $"ID:{Fig.ID} {CadFigure.TypeName(Fig.Type)}";
            }
            else
            {
                return $"ID:{Fig.ID} {Fig.Name}({CadFigure.TypeName(Fig.Type)})";
            }
        }
    }


    public CadFigTreeItem(CadFigure fig)
    {
        Type = CadObjTreeItemType.NODE;

        Fig = fig;

        int idx = 0;

        for (int i=0; i < Fig.PointList.Count; i++) {
            CadVertex p = Fig.PointList[i];
            CadPointTreeItem pi = new CadPointTreeItem(fig, idx);
            Add(pi);
            idx++;
        }

        for (int i=0; i<Fig.ChildList.Count; i++)
        {
            CadFigure c = Fig.ChildList[i];
            CadFigTreeItem pi = new CadFigTreeItem(c);
            Add(pi);
        }
    }

    public override List<MenuItem> GetContextMenuItems()
    {
        List<MenuItem> list = new List<MenuItem>();

        MenuItem menuItem = new MenuItem();
        menuItem.Header = TCad.Properties.Resources.menu_change_fig_name;
        menuItem.Tag = CreateContextMenuTag(ITEM_CMD_CHANGE_NAME);

        list.Add(menuItem);

        return list;
    }

    private void SelectAllPoints(bool sel)
    {
        if (Children == null)
        {
            return;
        }

        for (int i=0; i<Children.Count; i++)
        {
            CadObjTreeItem c = Children[i];
            c.IsChecked = sel;
        }
    }

    private bool HasSelectedPoint()
    {
        bool ret = false;

        if (Children == null)
        {
            return ret;
        }

        int i;
        for (i=0; i<Children.Count;i++)
        {
            CadObjTreeItem c = Children[i];

            if (c.IsChecked)
            {
                ret = true;
                break;
            }
        }

        return ret;
    }
}
