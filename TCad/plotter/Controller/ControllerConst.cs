
namespace Plotter.Controller
{
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

    class ControllerConst
    {
        public const double MARK_CURSOR_SIZE = 10.0;

        public const double CURSOR_LOCK_MARK_SIZE = 8.0;
    }
}
