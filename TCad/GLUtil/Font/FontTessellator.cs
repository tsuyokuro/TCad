using CadDataTypes;
using GLUtil;
using OpenGL.GLU;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static GLUtil.Tessellator;

namespace GLFont;

internal class FontTessellator
{
    public static CadMesh TessellateRaw(GlyphSlot glyph, double scale, int div, Tessellator tesse)
    {
        if (scale == 0) scale = 100.0;

        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        byte[] tags = outline.Tags;

        List<Tessellator.IndexList> contourList = new();
        List<Vector3d> vertexList = new List<Vector3d>();

        Vector3d cv = new();

        int idx = 0;
        for (int i = 0; i < outline.ContoursCount; i++)
        {
            Tessellator.IndexList contour = new();

            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = points[idx];
                cv.X = fv.X * scale;
                cv.Y = fv.Y * scale;
                cv.Z = 0;

                vertexList.Add(cv);
                contour.Add(idx);

                idx++;
            }

            contourList.Add(contour);
        }

        return tesse.Tessellate(contourList, vertexList);
    }

    public static CadMesh Tessellate(GlyphSlot glyph, double scale, int steps, Tessellator tesse,
    out List<Tessellator.IndexList> cl, out List<Vector3d> vl)
    {
        if (scale == 0) scale = 100.0;

        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        byte[] tags = outline.Tags;

        List<Tessellator.IndexList> contList = new();
        List<Vector3d> vertexList = new List<Vector3d>();

        int start = 0;
        for (int contIdx = 0; contIdx < outline.ContoursCount; contIdx++)
        {
            Tessellator.IndexList contour = new();

            int end = outline.Contours[contIdx];
            int num = end - start + 1;

            Vector3d prev;
            Vector3d next = FTV2Vector3d(points[start], scale);
            Vector3d cur = FTV2Vector3d(points[((num - 1) % num) + start], scale);

            for (int i=0; i<num; i++)
            {
                int idx = start + i;

                prev = cur;
                cur = next;
                next = FTV2Vector3d(points[((i + 1) % num) + start], scale);

                if (tags[idx] == 1) // FT_Curve_Tag_On
                {
                    vertexList.Add(cur);
                    contour.Add(vertexList.Count - 1);
                }
                else if (tags[idx] == 0) // FT_Curve_Tag_Conic
                {
                    Vector3d prev2 = prev;
                    Vector3d next2 = next;

                    // Previous point is either the real previous point (an "on"
                    // point), or the midpoint between the current one and the
                    // previous "conic off" point.
                    if (tags[((i - 1 + num) % num) + start] == 0)
                    {
                        prev2 = (cur + prev) * 0.5;
                        vertexList.Add(prev2);
                        contour.Add(vertexList.Count - 1);
                    }

                    // Next point is either the real next point or the midpoint.
                    if (tags[((i + 1) % num) + start] == 0)
                    {
                        next2 = (cur + next) * 0.5;
                    }

                    List<Vector3d> evl = evaluateQuadraticCurve(prev2, cur, next2, steps);

                    for (int k = 0; k < evl.Count; k++)
                    {
                        vertexList.Add(evl[k]);
                        contour.Add(vertexList.Count - 1);
                    }
                }
                else if (tags[idx] == 2) // Bézier Curve
                {
                    List<Vector3d> evl = evaluateCubicCurve(prev, cur, next,
                                        FTV2Vector3d(points[((i + 2) % num) + start], scale), steps);

                    for (int k = 0; k < evl.Count; k++)
                    {
                        vertexList.Add(evl[k]);
                        contour.Add(vertexList.Count - 1);
                    }
                }
            }

            contList.Add(contour);
            start = end + 1;
        }

        cl = contList;
        vl = vertexList;

        return tesse.Tessellate(contList, vertexList);
    }

    private static Vector3d FTV2Vector3d(FTVector ftv, double scale)
    {
        Vector3d cv = new();
        cv.X = ftv.X * scale;
        cv.Y = ftv.Y * scale;
        cv.Z = 0;

        return cv;
    }

    private static List<Vector3d> evaluateQuadraticCurve(Vector3d A, Vector3d B, Vector3d C, int steps)
    {
        List<Vector3d> curvePoints = new();

        for (int i = 1; i < steps; i++)
        {
            double t = (double)i / (double)steps;

            Vector3d U = (1.0 - t) * A + t * B;
            Vector3d V = (1.0 - t) * B + t * C;

            Vector3d v = (1.0 - t) * U + t * V;

            curvePoints.Add(v);
        }

        return curvePoints;
    }

    private static List<Vector3d> evaluateCubicCurve(Vector3d A, Vector3d B, Vector3d C, Vector3d D, int steps)
    {
        List<Vector3d> curvePoints = new();

        for (int i = 0; i < steps; i++)
        {
            double t = (double)i / (double)steps;

            Vector3d U = (1.0f - t) * A + t * B;
            Vector3d V = (1.0f - t) * B + t * C;
            Vector3d W = (1.0f - t) * C + t * D;

            Vector3d M = (1.0 - t) * U + t * V;
            Vector3d N = (1.0 - t) * V + t * W;

            Vector3d v = (1.0f - t) * M + t * N;

            curvePoints.Add(v);
        }

        return curvePoints;
    }



    public static CadMesh TessellateTest(GlyphSlot glyph, double scale, int div, Tessellator tesse,
        out List<Tessellator.IndexList> cl, out List<Vector3d> vl)
    {
        if (scale == 0) scale = 100.0;

        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        // 1:非制御点 
        byte[] tags = outline.Tags;

        List<Tessellator.IndexList> contList = new();
        List<Vector3d> vertexList = new List<Vector3d>();

        Vector3d cv = new();

        List<Vector3d> curvePoints = new();

        Vector3d lastPoint = default;

        int idx = 0;
        for (int i = 0; i < outline.ContoursCount; i++)
        {
            Tessellator.IndexList contour = new();

            curvePoints.Clear();
            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = points[idx];
                cv.X = fv.X * scale;
                cv.Y = fv.Y * scale;
                cv.Z = 0;

                if (tags[idx] == 1) // On curve. It is not Control Point 
                {
                    if (curvePoints.Count > 0)
                    {
                        curvePoints.Add(cv);
                        List<Vector3d> svlist = BSpline2D(lastPoint, curvePoints, 4);
                        for (int k = 0; k < svlist.Count; k++)
                        {
                            vertexList.Add(svlist[k]);
                            contour.Add(vertexList.Count - 1);
                        }

                        curvePoints.Clear();
                    }
                    else
                    {
                        vertexList.Add(cv);
                        contour.Add(vertexList.Count - 1);
                    }

                    lastPoint = cv;
                }
                else if (tags[idx] == 0) // B-Spline
                {
                    curvePoints.Add(cv);
                }
                else if (tags[idx] == 2) // Bézier Curve
                {
                    curvePoints.Add(cv);
                }

                idx++;
            }

            if (curvePoints.Count > 0)
            {
                List<Vector3d> svlist = BSpline2D(lastPoint, curvePoints, 4);
                for (int k = 0; k < svlist.Count; k++)
                {
                    vertexList.Add(svlist[k]);
                    contour.Add(vertexList.Count - 1);
                }
            }

            contList.Add(contour);
        }

        //return tesse.Tessellate(contourList, vertexList);

        cl = contList;
        vl = vertexList;
        return null;
    }

    private static double BSplineBasis(double p1, double p2, double p3, double t)
    {
        double x, a;
        a = 1f - t;
        x = a * a * p1
            + 2f * a * t * p2
            + t * t * p3;

        return x;
    }

    public static List<Vector3d> BSpline2D(Vector3d startPoint, List<Vector3d> points, int splitNum)
    {
        List<Vector3d> controlPoints = points;

        List<Vector3d> onCurvePoints = new(controlPoints.Count + 1);

        Vector3d v = default;
        v.Z = 0;

        onCurvePoints.Add(startPoint);

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            v.X = (controlPoints[i].X + controlPoints[i + 1].X) / 2f;
            v.Y = (controlPoints[i].Y + controlPoints[i + 1].Y) / 2f;
            v.Z = 0;
            onCurvePoints.Add(v);
        }
        v = points[points.Count -1];
        v.Z = 0;

        onCurvePoints.Add(v);


        List<Vector3d> bSplinePoints = new(onCurvePoints.Count + controlPoints.Count * (splitNum - 1));

        bSplinePoints.Add(onCurvePoints[0]);

        float t = 0, dt = 1f / splitNum;

        for (int i = 0; i < controlPoints.Count; i++)
        {
            for (int j = 1; j <= splitNum; j++)
            {
                t = dt * j;
                v.X = BSplineBasis(onCurvePoints[i].X, controlPoints[i].X, onCurvePoints[i + 1].X, t);
                v.Y = BSplineBasis(onCurvePoints[i].Y, controlPoints[i].Y, onCurvePoints[i + 1].Y, t);
                v.Z = 0;
                bSplinePoints.Add(v);
            }
        }

        return bSplinePoints;
    }

    public static List<Vector3d> BSpline2D(List<Vector3d> points, int splitNum)
    {
        List<Vector3d> controlPoints = points;
        int cpStart = 1;

        List<Vector3d> onCurvePoints = new(controlPoints.Count);

        Vector3d v = default;
        v.Z = 0;

        onCurvePoints.Add(points[0]); // Start point

        for (int i = cpStart; i < controlPoints.Count - 1; i++)
        {
            v.X = (controlPoints[i].X + controlPoints[i + 1].X) / 2f;
            v.Y = (controlPoints[i].Y + controlPoints[i + 1].Y) / 2f;
            v.Z = 0;
            onCurvePoints.Add(v);
        }

        v = points[points.Count - 1];
        v.Z = 0;
        onCurvePoints.Add(v); // End poiint


        List<Vector3d> bSplinePoints = new(onCurvePoints.Count + controlPoints.Count * (splitNum - 1));

        bSplinePoints.Add(onCurvePoints[0]);

        float t = 0;
        float dt = 1f / splitNum;

        int k = 0;
        for (int i = cpStart; i < controlPoints.Count; i++)
        {
            for (int j = 1; j <= splitNum; j++)
            {
                t = dt * j;
                v.X = BSplineBasis(onCurvePoints[k].X, controlPoints[i].X, onCurvePoints[k + 1].X, t);
                v.Y = BSplineBasis(onCurvePoints[k].Y, controlPoints[i].Y, onCurvePoints[k + 1].Y, t);
                v.Z = 0;
                bSplinePoints.Add(v);
            }
            k++;
        }

        return bSplinePoints;
    }
}