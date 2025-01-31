using CadDataTypes;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Plotter;

public class TriangleSplitter
{
    public static List<Vector3List> Split(CadFigure fig, int curveSplitNum = 32)
    {
        CadVertex p0 = default;

        var triangles = new List<Vector3List>();

        int i1 = -1;

        int state = 0;

        Vector3List triangle;

        VertexList pointList = fig.GetPoints(curveSplitNum);

        i1 = CadUtil.FindMaxDistantPointIndex(p0, pointList);

        if (i1 == -1)
        {
            return triangles;
        }

        triangle = GetTriangleWithCenterPoint(pointList, i1);

        vector3_t tp0 = triangle[0];
        vector3_t tp1 = triangle[1];
        vector3_t tp2 = triangle[2];

        vector3_t dir = CadMath.Normal(tp1, tp0, tp2);
        vector3_t currentDir = vector3_t.Zero;

        while (pointList.Count > 3)
        {
            if (state == 0)
            {
                i1 = CadUtil.FindMaxDistantPointIndex(p0, pointList);
                if (i1 == -1)
                {
                    return triangles;
                }
            }

            triangle = GetTriangleWithCenterPoint(pointList, i1);

            tp0 = triangle[0];
            tp1 = triangle[1];
            tp2 = triangle[2];

            currentDir = CadMath.Normal(tp1, tp0, tp2);

            bool hasIn = ListContainsPointInTriangle(pointList, triangle);

            vcompo_t scala = CadMath.InnerProduct(dir, currentDir);

            if (!hasIn && (scala > 0))
            {
                triangles.Add(triangle);
                pointList.RemoveAt(i1);
                state = 0;
                continue;
            }

            if (state == 0)
            {
                state = 1;
                i1 = 0;
            }
            else if (state == 1)
            {
                i1++;
                if (i1 >= pointList.Count)
                {
                    break;
                }
            }
        }

        if (pointList.Count == 3)
        {
            triangle = new Vector3List(3);

            triangle.Add(pointList[0].vector);
            triangle.Add(pointList[1].vector);
            triangle.Add(pointList[2].vector);

            triangles.Add(triangle);
        }

        return triangles;
    }

    private static Vector3List GetTriangleWithCenterPoint(VertexList pointList, int cpIndex)
    {
        int i1 = cpIndex;
        int endi = pointList.Count - 1;

        int i0 = i1 - 1;
        int i2 = i1 + 1;

        if (i0 < 0) { i0 = endi; }
        if (i2 > endi) { i2 = 0; }

        var triangle = new Vector3List();

        CadVertex tp0 = pointList[i0];
        CadVertex tp1 = pointList[i1];
        CadVertex tp2 = pointList[i2];

        triangle.Add(tp0.vector);
        triangle.Add(tp1.vector);
        triangle.Add(tp2.vector);

        return triangle;
    }

    private static bool ListContainsPointInTriangle(VertexList check, Vector3List triangle)
    {
        var tps = triangle;

        foreach (CadVertex cp in check)
        {
            if (
                cp.vector.Equals(tps[0]) ||
                cp.vector.Equals(tps[1]) ||
                cp.vector.Equals(tps[2])
                )
            {
                continue;
            }

            bool ret = CadMath.IsPointInTriangle(
                                            cp.vector,
                                            tps[0],
                                            tps[1],
                                            tps[2]
                                            );
            if (ret)
            {
                return true;
            }
        }

        return false;
    }
}
