using System.Windows;
using Plotter.Controller;
using Plotter;
using System;

namespace TCad.ViewModel
{
    public interface ICadMainWindow
    {
        Window GetWindow();
        void SetCurrentFileName(string file_name);
        void OpenPopupMessage(string text, UITypes.MessageType messageType);
        void ClosePopupMessage();
        void SetPlotterView(IPlotterView view);
        void DrawModeUpdated(DrawTools.DrawMode drawMode);
    }
}
