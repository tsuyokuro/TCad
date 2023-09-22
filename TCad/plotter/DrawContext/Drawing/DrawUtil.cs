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


namespace Plotter;

public static class DrawUtil
{
    public static void DrawArrow(
        IDrawing drawing,
        in DrawPen pen,
        vector3_t pt0,
        vector3_t pt1,
        ArrowTypes type,
        ArrowPos pos,
        vcompo_t len,
        vcompo_t width)
    {
        drawing.DrawLine(pen, pt0, pt1);

        vector3_t d = pt1 - pt0;

        vcompo_t dl = d.Length;

        if (dl < (vcompo_t)(0.00001))
        {
            return;
        }


        vector3_t tmp = new vector3_t(dl, 0, 0);

        vcompo_t angle = vector3_t.CalculateAngle(tmp, d);

        vector3_t normal = CadMath.CrossProduct(tmp, d);  // 回転軸

        if (normal.Length < (vcompo_t)(0.0001))
        {
            normal = new vector3_t(0, 0, 1);
        }
        else
        {
            normal = normal.UnitVector();
            normal = CadMath.Normal(tmp, d);
        }

        CadQuaternion q = CadQuaternion.RotateQuaternion(normal, -angle);
        CadQuaternion r = q.Conjugate();

        ArrowHead a;

        if (pos == ArrowPos.END || pos == ArrowPos.START_END)
        {
            a = ArrowHead.Create(type, ArrowPos.END, len, width);

            a.Rotate(q, r);

            a += pt1;

            drawing.DrawLine(pen, a.p0.vector, a.p1.vector);
            drawing.DrawLine(pen, a.p0.vector, a.p2.vector);
            drawing.DrawLine(pen, a.p0.vector, a.p3.vector);
            drawing.DrawLine(pen, a.p0.vector, a.p4.vector);
        }

        if (pos == ArrowPos.START || pos == ArrowPos.START_END)
        {
            a = ArrowHead.Create(type, ArrowPos.START, len, width);

            a.Rotate(q, r);

            a += pt0;

            drawing.DrawLine(pen, a.p0.vector, a.p1.vector);
            drawing.DrawLine(pen, a.p0.vector, a.p2.vector);
            drawing.DrawLine(pen, a.p0.vector, a.p3.vector);
            drawing.DrawLine(pen, a.p0.vector, a.p4.vector);
        }
    }
}
