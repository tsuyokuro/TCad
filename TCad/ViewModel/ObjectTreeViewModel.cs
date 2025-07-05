using CadDataTypes;
using System.Windows;
using TCad.Controls;
using TCad.Dialogs;
using TCad.Plotter;
using TCad.Plotter.Controller;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Settings;

namespace TCad.ViewModel;

public class ObjectTreeViewModel
{
    protected IPlotterController Controller;

    protected ICadObjectTree mObjectTree;

    public ObjectTreeViewModel(IPlotterController controller)
    {
        Controller = controller;
    }

    public ICadObjectTree ObjectTree
    {
        set
        {
            if (mObjectTree != null)
            {
                mObjectTree.StateChanged -= StateChanged;
                mObjectTree.ItemCommand -= ItemCommand;
            }

            mObjectTree = value;

            if (mObjectTree != null)
            {
                mObjectTree.StateChanged += StateChanged;
                mObjectTree.ItemCommand += ItemCommand;
            }
        }

        get => mObjectTree;
    }

    private void StateChanged(CadObjTreeItem item)
    {
        Controller.Input.CurrentFigure =
            TreeViewUtil.GetCurrentFigure(item, Controller.Input.CurrentFigure);

        Controller.Redraw();
    }

    public void UpdateTreeView(bool remakeTree)
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            UpdateTreeViewProc(remakeTree);
        }, false);
    }

    private void UpdateTreeViewProc(bool remakeTree)
    {
        if (mObjectTree == null)
        {
            return;
        }

        mObjectTree.Update(remakeTree, SettingsHolder.Settings.FilterObjectTree, Controller.CurrentLayer);
    }

    public void SetTreeViewPos(int index)
    {
        if (mObjectTree == null)
        {
            return;
        }

        ThreadUtil.RunOnMainThread(() =>
        {
            mObjectTree.SetPos(index);
        }, true);
    }

    public int FindTreeViewItemIndex(uint id)
    {
        if (mObjectTree == null)
        {
            return -1;
        }

        int idx = mObjectTree.FindIndex(id);

        return idx;
    }

    public void ItemCommand(CadObjTreeItem treeItem, string cmd)
    {
        if (treeItem is CadFigTreeItem)
        {
            CadFigTreeItem figItem = (CadFigTreeItem)treeItem;

            if (cmd == CadFigTreeItem.ITEM_CMD_CHANGE_NAME)
            {
                CadFigure fig = figItem.Fig;

                InputStringDialog dlg = new()
                {
                    Message = Properties.Resources.string_input_fig_name
                };

                if (fig.Name != null)
                {
                    dlg.InputString = fig.Name;
                }

                bool? dlgRet = dlg.ShowDialog();

                if (dlgRet.Value)
                {
                    fig.Name = dlg.InputString;
                    UpdateTreeView(false);
                }
            }
            else if (cmd == CadFigTreeItem.ITEM_CMD_COPY)
            {
                CadFigure fig = figItem.Fig;
                PlotterClipboard.CopyFiguresAsBin([fig]);
            }
        }
        else if (treeItem is CadPointTreeItem)
        {
            CadPointTreeItem pointItem = (CadPointTreeItem)treeItem;
            if (cmd == CadPointTreeItem.ITEM_CMD_COPY_DATA)
            {
                CadFigure fig = pointItem.Fig;
                if (pointItem.Index >= 0 && pointItem.Index < fig.PointCount)
                {
                    CadVertex v = fig.GetPointAt(pointItem.Index);
                    var s = v.ToString();
                    Clipboard.SetText(v.ToString());
                }
            }
        }
    }
}
