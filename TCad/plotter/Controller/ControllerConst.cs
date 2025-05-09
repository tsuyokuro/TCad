namespace TCad.Plotter.Controller;

public enum DrawModes
{
    LIGHT = 1,
    DARK = 2,
    PRINTER = 100,
}

public enum SelectModes
{
    POINT,
    OBJECT,
}

public enum MeasureModes
{
    NONE,
    POLY_LINE,
}

public enum CursorType
{
    TRACKING,
    LAST_DOWN,
}

public enum ViewModes
{
    NONE,
    FRONT,
    BACK,
    TOP,
    BOTTOM,
    RIGHT,
    LEFT,
    FREE,
}

public enum ControllerStates
{
    NONE,
    SELECT,
    RUBBER_BAND_SELECT,
    DRAGING_POINTS,
    DRAGING_VIEW_ORG,
    CREATE_FIGURE,
    MEASURING,
}

class ControllerConst
{
    public const vcompo_t MARK_CURSOR_SIZE = (vcompo_t)(10.0);

    public const vcompo_t CURSOR_LOCK_MARK_SIZE = (vcompo_t)(8.0);
}
