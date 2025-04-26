using System.Collections.Generic;
using TCad.MathFunctions;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.Model.Figure;

namespace Plotter.Controller;

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
    public static vcompo_t Area(List<CadFigure> figList)
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
    public static vector3_t CrossOnScreen(DrawContext dc, vector3_t wp00, vector3_t wp01, vector3_t wp10, vector3_t wp11)
    {
        vector3_t sp00 = dc.WorldPointToDevPoint(wp00);
        vector3_t sp01 = dc.WorldPointToDevPoint(wp01);
        vector3_t sp10 = dc.WorldPointToDevPoint(wp10);
        vector3_t sp11 = dc.WorldPointToDevPoint(wp11);

        vector3_t cp = CadMath.CrossLine2D(sp00, sp01, sp10, sp11);

        return cp;
    }
}
