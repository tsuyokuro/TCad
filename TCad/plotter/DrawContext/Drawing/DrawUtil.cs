using OpenTK;
using System;

namespace Plotter
{
    public static class DrawUtil
    {
        public static void DrawArrow(
            Action<DrawPen, Vector3d, Vector3d> DrawLine,
            DrawPen pen,
            Vector3d pt0,
            Vector3d pt1,
            ArrowTypes type,
            ArrowPos pos,
            double len,
            double width)
        {
            DrawLine(pen, pt0, pt1);

            Vector3d d = pt1 - pt0;

            double dl = d.Length;

            if (dl < 0.00001)
            {
                return;
            }


            Vector3d tmp = new Vector3d(dl, 0, 0);

            double angle = Vector3d.CalculateAngle(tmp, d);

            Vector3d normal = CadMath.CrossProduct(tmp, d);  // 回転軸

            if (normal.Length < 0.0001)
            {
                normal = new Vector3d(0, 0, 1);
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

                DrawLine(pen, a.p0.vector, a.p1.vector);
                DrawLine(pen, a.p0.vector, a.p2.vector);
                DrawLine(pen, a.p0.vector, a.p3.vector);
                DrawLine(pen, a.p0.vector, a.p4.vector);
            }

            if (pos == ArrowPos.START || pos == ArrowPos.START_END)
            {
                a = ArrowHead.Create(type, ArrowPos.START, len, width);

                a.Rotate(q, r);

                a += pt0;

                DrawLine(pen, a.p0.vector, a.p1.vector);
                DrawLine(pen, a.p0.vector, a.p2.vector);
                DrawLine(pen, a.p0.vector, a.p3.vector);
                DrawLine(pen, a.p0.vector, a.p4.vector);
            }
        }
    }
}
