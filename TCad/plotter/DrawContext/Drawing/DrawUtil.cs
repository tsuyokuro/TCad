using Plotter;
using TCad.MathFunctions;
using TCad.Plotter.DrawToolSet;

namespace TCad.Plotter.Drawing;

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

        vector3_t normal = CadMath.OuterProduct(tmp, d);  // 回転軸

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
