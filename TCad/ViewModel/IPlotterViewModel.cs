using OpenTK.Mathematics;
using System.Collections.Generic;
using Plotter.Controller;

namespace TCad.ViewModel
{
    public interface UITypes
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
    }

    public interface IPlotterViewModel
    {
        string CurrentFileName
        {
            get;
            set;
        }

        void StateChanged(PlotterController sender, PlotterStateInfo si);

        void LayerListChanged(PlotterController sender, LayerListInfo layerListInfo);

        void CursorPosChanged(PlotterController sender, Vector3d pt, Plotter.Controller.CursorType type);

        void UpdateTreeView(bool remakeTree);

        void SetTreeViewPos(int index);

        int FindTreeViewItemIndex(uint id);

        void OpenPopupMessage(string text, UITypes.MessageType messageType);

        void ClosePopupMessage();

        void CursorLocked(bool locked);

        void ChangeMouseCursor(UITypes.MouseCursorType cursorType);

        List<string> HelpOfKey(string keyword);

        void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y);


        public static readonly IPlotterViewModel Dummy = new DummyPlotterViewModel();

        public class DummyPlotterViewModel : IPlotterViewModel
        {
            private static List<string> EmptyList = new List<string>();

            public string CurrentFileName { get => null; set { } }
            public void ChangeMouseCursor(UITypes.MouseCursorType cursorType) {}
            public void ClosePopupMessage() {}
            public void CursorLocked(bool locked) {}
            public void CursorPosChanged(PlotterController sender, Vector3d pt, Plotter.Controller.CursorType type) {}
            public int FindTreeViewItemIndex(uint id) { return -1; }
            public List<string> HelpOfKey(string keyword) { return EmptyList; }
            public void LayerListChanged(PlotterController sender, LayerListInfo layerListInfo) {}
            public void OpenPopupMessage(string text, UITypes.MessageType messageType) {}
            public void SetTreeViewPos(int index) { }
            public void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y) { }
            public void StateChanged(PlotterController sender, PlotterStateInfo si) { }
            public void UpdateTreeView(bool remakeTree) { }
        }
    }
}
