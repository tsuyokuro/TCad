using CadDataTypes;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using TCad.ViewModel;

namespace Plotter.Controller
{
    public class PlotterCallback
    {
        public enum MessageType
        {
            INFO,
            INPUT,
            ERROR,
        }

        public enum MouseCursorType
        {
            NORMAL_ARROW,
            CROSS,
            HAND,
        }

        private IPlotterViewModel VM;

        public string CurrentFileName
        {
            get => VM.CurrentFileName;
            set => VM.CurrentFileName = value;
        }

        public PlotterCallback(IPlotterViewModel vm)
        {
            VM = vm;
        }

        public void StateChanged(PlotterController sender, PlotterStateInfo si)
        {
            VM?.StateChanged(sender, si);
        }

        public void LayerListChanged(PlotterController sender, LayerListInfo layerListInfo)
        {
            VM?.LayerListChanged(sender, layerListInfo);
        }

        public void CursorPosChanged(PlotterController sender, Vector3d pt, Plotter.Controller.CursorType type)
        {
            VM?.CursorPosChanged(sender, pt, type);
        }

        public void UpdateTreeView(bool remakeTree)
        {
            VM?.UpdateTreeView(remakeTree);
        }

        public void SetTreeViewPos(int index)
        {
            VM?.SetTreeViewPos(index);
        }

        public int FindTreeViewItemIndex(uint id)
        {
            if (VM == null) return -1;
            return VM.FindTreeViewItemIndex(id);
        }

        public void OpenPopupMessage(string text, PlotterCallback.MessageType messageType)
        {
            VM?.OpenPopupMessage(text, messageType);
        }

        public void ClosePopupMessage()
        {
            VM?.ClosePopupMessage();
        }

        public void CursorLocked(bool locked)
        {
            VM?.CursorLocked(locked);
        }

        public void ChangeMouseCursor(PlotterCallback.MouseCursorType cursorType)
        {
            VM?.ChangeMouseCursor(cursorType);
        }

        public List<string> HelpOfKey(string keyword)
        {
            return VM?.HelpOfKey(keyword);
        }

        public void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            VM?.ShowContextMenu(sender, menuInfo, x, y);
        }
    }
}
