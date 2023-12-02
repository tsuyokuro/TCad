//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;



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

public static class ViewUtil
{
    public static void SetOrigin(DrawContext dc, int pixX, int pixY)
    {
        vector3_t op = new vector3_t(pixX, pixY, 0);

        dc.SetViewOrg(op);
    }

    public static void AdjustOrigin(DrawContext dc, vcompo_t pixX, vcompo_t pixY, int vw, int vh)
    {
        vcompo_t dx = vw / 2 - pixX;
        vcompo_t dy = vh / 2 - pixY;

        vector3_t d = new vector3_t(dx, dy, 0);

        dc.SetViewOrg(dc.ViewOrg + d);
    }

    public static void DpiUpDown(DrawContext dc, vcompo_t f)
    {
        vector3_t op = dc.ViewOrg;

        vector3_t center = new vector3_t(dc.ViewWidth / 2, dc.ViewHeight / 2, 0);

        vector3_t d = center - op;

        d *= f;

        op = center - d;


        dc.SetViewOrg(op);

        dc.UnitPerMilli *= f;
    }
}
