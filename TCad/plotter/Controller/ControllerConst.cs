//#define DEFAULT_DATA_TYPE_DOUBLE



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


namespace Plotter.Controller;

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
    CREATING,
    MEASURING,
}

class ControllerConst
{
    public const vcompo_t MARK_CURSOR_SIZE = (vcompo_t)(10.0);

    public const vcompo_t CURSOR_LOCK_MARK_SIZE = (vcompo_t)(8.0);
}
