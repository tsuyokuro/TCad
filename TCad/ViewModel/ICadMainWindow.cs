using TCad.MainView;
using TCad.Plotter;

namespace TCad.ViewModel;

public interface ICadMainWindow
{
    void OpenPopupMessage(string text, UITypes.MessageType messageType);
    void ClosePopupMessage();
    void SetPlotterView(IPlotterView view);
    void DrawModeChanged(DrawModes drawMode);
}
