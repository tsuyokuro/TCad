using System;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    // 円を点リストに展開
    public static class CircleExpander
    {
        public static void ForEachSegs(
            CadVertex cp, CadVertex pa, CadVertex pb,
            int splitCnt,
            Action<CadVertex, CadVertex> action)
        {
            CadVertex va = pa - cp;
            CadVertex vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }


            double dt = (2.0 * Math.PI) / (double)splitCnt;

            int div = splitCnt;

            Vector3d normal = CadMath.Normal(va.vector, vb.vector);

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, dt);
            CadQuaternion r = q.Conjugate();

            CadVertex p = va;
            CadVertex tp1 = pa;
            CadVertex tp2 = pa;


            int i = 0;
            for (; i < div - 1; i++)
            {
                CadQuaternion qp = CadQuaternion.FromPoint(p.vector);
                qp = r * qp;
                qp = qp * q;

                p.vector = qp.ToPoint();

                tp2 = p + cp;

                action(tp1, tp2);
                tp1 = tp2;
            }

            action(tp1, pa);
        }

        public static void Draw(
            CadVertex cp, CadVertex pa, CadVertex pb,
            int splitCnt,
            DrawContext dc, DrawPen pen)
        {
            CadVertex va = pa - cp;
            CadVertex vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }

            double dt = (2.0 * Math.PI) / (double)splitCnt;

            int div = splitCnt;

            Vector3d normal = CadMath.Normal(va.vector, vb.vector);

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, dt);
            CadQuaternion r = q.Conjugate();

            CadVertex p = va;
            CadVertex tp1 = pa;
            CadVertex tp2 = pa;


            int i = 0;
            for (; i < div - 1; i++)
            {
                CadQuaternion qp = CadQuaternion.FromPoint(p.vector);
                qp = r * qp;
                qp = qp * q;

                p.vector = qp.ToPoint();

                tp2 = p + cp;

                dc.Drawing.DrawLine(pen, tp1.vector, tp2.vector);
                tp1 = tp2;
            }

            dc.Drawing.DrawLine(pen, tp1.vector, pa.vector);
        }
    }
}
