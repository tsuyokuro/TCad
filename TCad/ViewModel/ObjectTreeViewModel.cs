using TCad.Controls;
using TCad.Dialogs;
using Plotter;
using Plotter.Settings;

namespace TCad.ViewModel
{
    public class ObjectTreeViewModel
    {
        ViewModelContext mVMContext;

        ICadObjectTree mObjectTree;

        public ICadObjectTree ObjectTree
        {
            set
            {
                if (mObjectTree != null)
                {
                    mObjectTree.CheckChanged -= CheckChanged;
                    mObjectTree.ItemCommand -= ItemCommand;
                }

                mObjectTree = value;

                if (mObjectTree != null)
                {
                    mObjectTree.CheckChanged += CheckChanged;
                    mObjectTree.ItemCommand += ItemCommand;
                }
            }

            get => mObjectTree;
        }

        public ObjectTreeViewModel(ViewModelContext context)
        {
            mVMContext = context;

            mVMContext.Controller.Callback.UpdateObjectTree = UpdateTreeView;

            mVMContext.Controller.Callback.SetObjectTreePos = SetTreeViewPos;

            mVMContext.Controller.Callback.FindObjectTreeItemIndex = FindTreeViewItemIndex;
        }

        private void CheckChanged(CadObjTreeItem item)
        {
            mVMContext.Controller.CurrentFigure =
                TreeViewUtil.GetCurrentFigure(item, mVMContext.Controller.CurrentFigure);

            mVMContext.Redraw();
        }

        private void UpdateTreeView(bool remakeTree)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                UpdateTreeViewProc(remakeTree);
            }, true);
        }

        private void UpdateTreeViewProc(bool remakeTree)
        {
            if (mObjectTree == null)
            {
                return;
            }

            mObjectTree.Update(remakeTree, SettingsHolder.Settings.FilterObjectTree, mVMContext.Controller.CurrentLayer);
        }

        private void SetTreeViewPos(int index)
        {
            if (mObjectTree == null)
            {
                return;
            }

            ThreadUtil.RunOnMainThread(() => {
                mObjectTree.SetPos(index);
            }, true);
        }

        private int FindTreeViewItemIndex(uint id)
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
            if (!(treeItem is CadFigTreeItem))
            {
                return;
            }

            CadFigTreeItem figItem = (CadFigTreeItem)treeItem;

            if (cmd == CadFigTreeItem.ITEM_CMD_CHANGE_NAME)
            {
                CadFigure fig = figItem.Fig;

                InputStringDialog dlg = new InputStringDialog();

                dlg.Message = TCad.Properties.Resources.string_input_fig_name;

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
        }
    }
}
