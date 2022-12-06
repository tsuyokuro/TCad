using Plotter;
using Plotter.Controller;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace TCad.ViewModel;

public class SelectModeConverter : EnumBoolConverter<SelectModes> { }
public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
public class MeasureModeConverter : EnumBoolConverter<MeasureModes> { }
public class ViewModeConverter : EnumBoolConverter<ViewModes> { }
public class DrawModeConverter : EnumBoolConverter<DrawModes> { }
