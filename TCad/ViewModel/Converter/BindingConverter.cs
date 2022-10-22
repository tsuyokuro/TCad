using Plotter;
using Plotter.Controller;

namespace TCad.ViewModel;

public class SelectModeConverter : EnumBoolConverter<SelectModes> { }
public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
public class MeasureModeConverter : EnumBoolConverter<MeasureModes> { }
public class ViewModeConverter : EnumBoolConverter<ViewModes> { }
public class DrawModeConverter : EnumBoolConverter<DrawTools.DrawMode> { }
