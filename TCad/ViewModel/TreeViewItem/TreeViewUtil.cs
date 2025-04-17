using Plotter;
using TCad.Controls;

namespace TCad.ViewModel;

public static class TreeViewUtil
{
    public static CadFigure GetCurrentFigure(CadObjTreeItem item, CadFigure currentFig)
    {
        CadFigure fig = null;

        //DOut.pl("TreeViewUtil.GetCurrentFigure " + item.GetType().ToString());

        if (item is CadFigTreeItem)
        {
            CadFigTreeItem figItem = (CadFigTreeItem)item;

            if (figItem.IsChecked)
            {
                fig = figItem.Fig;
            }
            else
            {
                if (currentFig == figItem.Fig)
                {
                    fig = null;
                }
            }
        }
        else if (item is CadPointTreeItem)
        {
            CadPointTreeItem ptItem = (CadPointTreeItem)item;

            if (ptItem.Parent != null)
            {
                if (ptItem.Parent.IsChecked)
                {
                    fig = ptItem.Fig;
                }
                else
                {
                    if (currentFig == ptItem.Fig)
                    {
                        fig = null;
                    }
                }
            }
        }

        return fig;
    }
}
