namespace TCad.Plotter;

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
