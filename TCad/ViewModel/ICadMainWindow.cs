using Plotter;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace TCad.ViewModel;

public interface ICadMainWindow
{
    void OpenPopupMessage(string text, UITypes.MessageType messageType);
    void ClosePopupMessage();
    void SetPlotterView(IPlotterView view);
}
