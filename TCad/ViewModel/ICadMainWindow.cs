using Plotter;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace TCad.ViewModel;

public interface ICadMainWindow
{
    void OpenPopupMessage(string text, UITypes.MessageType messageType);
    void ClosePopupMessage();
    void SetPlotterView(IPlotterView view);
}
