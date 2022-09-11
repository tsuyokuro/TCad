using OpenTK.Mathematics;
using System.Collections.Generic;
using Plotter.Controller;
using Plotter;

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

        PlotterController Controller
        {
            get;
        }

        ICadMainWindow MainWindow
        {
            get;
        }

        void Redraw();

        void DrawModeUpdated(DrawTools.DrawMode mode);


        void StateChanged(PlotterStateInfo si);

        void LayerListChanged(LayerListInfo layerListInfo);

        void CursorPosChanged(Vector3d pt, Plotter.Controller.CursorType type);

        void UpdateTreeView(bool remakeTree);

        void SetTreeViewPos(int index);

        int FindTreeViewItemIndex(uint id);

        void OpenPopupMessage(string text, UITypes.MessageType messageType);

        void ClosePopupMessage();

        void CursorLocked(bool locked);

        void ChangeMouseCursor(UITypes.MouseCursorType cursorType);

        List<string> HelpOfKey(string keyword);

        void ShowContextMenu(MenuInfo menuInfo, int x, int y);


        public static readonly IPlotterViewModel Dummy = new DummyPlotterViewModel();
    }


    public class DummyPlotterViewModel : IPlotterViewModel
    {
        private static List<string> EmptyList = new List<string>();

        public string CurrentFileName { get => null; set { } }

        public PlotterController Controller
        {
            get => null;
        }

        public ICadMainWindow MainWindow => throw new System.NotImplementedException();

        public void ChangeMouseCursor(UITypes.MouseCursorType cursorType) { }
        public void ClosePopupMessage() { }
        public void CursorLocked(bool locked) { }
        public void CursorPosChanged(Vector3d pt, Plotter.Controller.CursorType type) { }
        public void DrawModeUpdated(DrawTools.DrawMode mode) { }
        public int FindTreeViewItemIndex(uint id) { return -1; }
        public List<string> HelpOfKey(string keyword) { return EmptyList; }
        public void LayerListChanged(LayerListInfo layerListInfo) { }
        public void OpenPopupMessage(string text, UITypes.MessageType messageType) { }
        public void Redraw() { }
        public void SetTreeViewPos(int index) { }
        public void ShowContextMenu(MenuInfo menuInfo, int x, int y) { }
        public void StateChanged(PlotterStateInfo si) { }
        public void UpdateTreeView(bool remakeTree) { }
    }
}