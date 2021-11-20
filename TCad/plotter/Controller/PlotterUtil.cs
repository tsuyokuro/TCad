using CadDataTypes;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public class PlotterUtil
    {
        public static Centroid Centroid(List<CadFigure> figList)
        {
            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (CadFigure fig in figList)
            {
                Centroid t = fig.GetCentroid();

                if (cent.IsInvalid)
                {
                    cent = t;
                    continue;
                }

                if (t.IsInvalid)
                {
                    continue;
                }

                cent = cent.Merge(t);
            }

            return cent;
        }

        // 指定された図形の面積の総和を求める
        public static double Area(List<CadFigure> figList)
        {
            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (CadFigure fig in figList)
            {
                Centroid t = fig.GetCentroid();

                if (cent.IsInvalid)
                {
                    cent = t;
                    continue;
                }

                if (t.IsInvalid)
                {
                    continue;
                }

                cent = cent.Merge(t);
            }

            if (cent.IsInvalid)
            {
                return 0;
            }

            return cent.Area;
        }

        //
        // Calculate the intersection point in the screen coordinate system
        // スクリーン座標系での交点を求める
        //
        public static Vector3d CrossOnScreen(DrawContext dc, Vector3d wp00, Vector3d wp01, Vector3d wp10, Vector3d wp11)
        {
            Vector3d sp00 = dc.WorldPointToDevPoint(wp00);
            Vector3d sp01 = dc.WorldPointToDevPoint(wp01);
            Vector3d sp10 = dc.WorldPointToDevPoint(wp10);
            Vector3d sp11 = dc.WorldPointToDevPoint(wp11);

            Vector3d cp = CadMath.CrossLine2D(sp00, sp01, sp10, sp11);

            return cp;
        }
    }
}