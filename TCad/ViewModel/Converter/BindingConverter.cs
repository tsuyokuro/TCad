using Plotter;
using Plotter.Controller;
using TCad.Plotter.Model.Figure;

namespace TCad.ViewModel;

public class SelectModeConverter : EnumBoolConverter<SelectModes> { }
public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
public class MeasureModeConverter : EnumBoolConverter<MeasureModes> { }
public class ViewModeConverter : EnumBoolConverter<ViewModes> { }
public class DrawModeConverter : EnumBoolConverter<DrawModes> { }
