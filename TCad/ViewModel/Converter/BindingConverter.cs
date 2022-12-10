//#define DEFAULT_DATA_TYPE_DOUBLE
using Plotter;
using Plotter.Controller;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad.ViewModel;

public class SelectModeConverter : EnumBoolConverter<SelectModes> { }
public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
public class MeasureModeConverter : EnumBoolConverter<MeasureModes> { }
public class ViewModeConverter : EnumBoolConverter<ViewModes> { }
public class DrawModeConverter : EnumBoolConverter<DrawModes> { }
