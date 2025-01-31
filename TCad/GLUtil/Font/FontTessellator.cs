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
    public static FontPoly TessellateRaw(GlyphSlot glyph, int div, Tessellator tesse)
    {
        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        byte[] tags = outline.Tags;

        List<List<int>> contourList = new();
        List<vector3_t> vertexList = new List<vector3_t>();

        vector3_t cv = new();

        int idx = 0;
        for (int i = 0; i < outline.ContoursCount; i++)
        {
            List<int> contour = new();

            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = points[idx];
                cv.X = (vcompo_t)fv.X;
                cv.Y = (vcompo_t)fv.Y;
                cv.Z = 0;

                vertexList.Add(cv);
                contour.Add(idx);

                idx++;
            }

            contourList.Add(contour);
        }

        FontPoly fontPoly = default;

        fontPoly.Mesh = tesse?.Tessellate(contourList, vertexList);
        fontPoly.ContourList = contourList;
        fontPoly.VertexList = vertexList;

        return fontPoly;
    }

    public static FontPoly Tessellate(GlyphSlot glyph, int steps, Tessellator tesse)
    {
        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        byte[] tags = outline.Tags;

        List<List<int>> contList = new();
        List<vector3_t> vertexList = new List<vector3_t>();

        int start = 0;
        for (int contIdx = 0; contIdx < outline.ContoursCount; contIdx++)
        {
            List<int> contour = new();

            int end = outline.Contours[contIdx];
            int num = end - start + 1;

            vector3_t prev;
            vector3_t next = FTV2vector3_t(points[start]);
            vector3_t cur = FTV2vector3_t(points[((num - 1) % num) + start]);

            for (int i=0; i<num; i++)
            {
                int idx = start + i;

                prev = cur;
                cur = next;
                next = FTV2vector3_t(points[((i + 1) % num) + start]);

                if ((tags[idx] & 0x01) != 0) // On Curve
                {
                    vertexList.Add(cur);
                    contour.Add(vertexList.Count - 1);
                }
                else if ((tags[idx] & 0x03) == 0) // Off Curve
                {
                    vector3_t prev2 = prev;
                    vector3_t next2 = next;

                    // Previous point is either the real previous point (an "on"
                    // point), or the midpoint between the current one and the
                    // previous "conic off" point.
                    if ((tags[((i - 1 + num) % num) + start] & 0x01) == 0)
                    {
                        prev2 = (cur + prev) * (vcompo_t)(0.5);
                        vertexList.Add(prev2);
                        contour.Add(vertexList.Count - 1);
                    }

                    // Next point is either the real next point or the midpoint.
                    if ((tags[((i + 1) % num) + start] & 0x01) == 0)
                    {
                        next2 = (cur + next) * (vcompo_t)(0.5);
                    }

                    evaluateQuadraticCurve(prev2, cur, next2, steps,
                                            vertexList, contour);
                }
                else if ((tags[idx] & 0x02) != 0) // Bézier Curve
                {
                    vector3_t next2 = FTV2vector3_t(points[((i + 2) % num) + start]);

                    evaluateCubicCurve(prev, cur, next, next2, steps,
                                        vertexList, contour);
                }
            }

            contList.Add(contour);
            start = end + 1;
        }

        FontPoly fontPoly = default;

        fontPoly.ContourList = contList;
        fontPoly.VertexList = vertexList;
        fontPoly.Mesh = tesse?.Tessellate(contList, vertexList);

        return fontPoly;
    }

    private static vector3_t FTV2vector3_t(FTVector ftv)
    {
        vector3_t cv = new();
        cv.X = (vcompo_t)ftv.X;
        cv.Y = (vcompo_t)ftv.Y;
        cv.Z = 0;

        return cv;
    }

    private static void evaluateQuadraticCurve(vector3_t A, vector3_t B, vector3_t C, int steps,
        List<vector3_t> vertexList, List<int> contour)
    {
        for (int i = 1; i < steps; i++)
        {
            vcompo_t t = (vcompo_t)i / (vcompo_t)steps;

            vector3_t U = ((vcompo_t)(1.0) - t) * A + t * B;
            vector3_t V = ((vcompo_t)(1.0) - t) * B + t * C;

            vector3_t v = ((vcompo_t)(1.0) - t) * U + t * V;

            vertexList.Add(v);
            contour.Add(vertexList.Count - 1);
        }
    }

    private static void evaluateCubicCurve(vector3_t A, vector3_t B, vector3_t C, vector3_t D, int steps,
        List<vector3_t> vertexList, List<int> contour)
    {
        for (int i = 0; i < steps; i++)
        {
            vcompo_t t = (vcompo_t)i / (vcompo_t)steps;

            vector3_t U = (1.0f - t) * A + t * B;
            vector3_t V = (1.0f - t) * B + t * C;
            vector3_t W = (1.0f - t) * C + t * D;

            vector3_t M = ((vcompo_t)(1.0) - t) * U + t * V;
            vector3_t N = ((vcompo_t)(1.0) - t) * V + t * W;

            vector3_t v = (1.0f - t) * M + t * N;

            vertexList.Add(v);
            contour.Add(vertexList.Count - 1);
        }
    }



    public static FontPoly TessellateTest(GlyphSlot glyph, int div, Tessellator tesse)
    {
        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        // 1:非制御点 
        byte[] tags = outline.Tags;

        List<List<int>> contList = new();
        List<vector3_t> vertexList = new List<vector3_t>();

        vector3_t cv = new();

        List<vector3_t> curvePoints = new();

        vector3_t lastPoint = default;

        int idx = 0;
        for (int i = 0; i < outline.ContoursCount; i++)
        {
            List<int> contour = new();

            curvePoints.Clear();
            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = points[idx];
                cv.X = (vcompo_t)fv.X;
                cv.Y = (vcompo_t)fv.Y;
                cv.Z = 0;

                if (tags[idx] == 1) // On curve. It is not Control Point 
                {
                    if (curvePoints.Count > 0)
                    {
                        curvePoints.Add(cv);
                        List<vector3_t> svlist = BSpline2D(lastPoint, curvePoints, 4);
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
                List<vector3_t> svlist = BSpline2D(lastPoint, curvePoints, 4);
                for (int k = 0; k < svlist.Count; k++)
                {
                    vertexList.Add(svlist[k]);
                    contour.Add(vertexList.Count - 1);
                }
            }

            contList.Add(contour);
        }

        //return tesse.Tessellate(contourList, vertexList);
        FontPoly fontPoly = new();

        fontPoly.ContourList = contList;
        fontPoly.VertexList = vertexList;
        fontPoly.Mesh = tesse?.Tessellate(contList, vertexList);

        return fontPoly;
    }

    private static vcompo_t BSplineBasis(vcompo_t p1, vcompo_t p2, vcompo_t p3, vcompo_t t)
    {
        vcompo_t x, a;
        a = 1f - t;
        x = a * a * p1
            + 2f * a * t * p2
            + t * t * p3;

        return x;
    }

    public static List<vector3_t> BSpline2D(vector3_t startPoint, List<vector3_t> points, int splitNum)
    {
        List<vector3_t> controlPoints = points;

        List<vector3_t> onCurvePoints = new(controlPoints.Count + 1);

        vector3_t v = default;
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


        List<vector3_t> bSplinePoints = new(onCurvePoints.Count + controlPoints.Count * (splitNum - 1));

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

    public static List<vector3_t> BSpline2D(List<vector3_t> points, int splitNum)
    {
        List<vector3_t> controlPoints = points;
        int cpStart = 1;

        List<vector3_t> onCurvePoints = new(controlPoints.Count);

        vector3_t v = default;
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


        List<vector3_t> bSplinePoints = new(onCurvePoints.Count + controlPoints.Count * (splitNum - 1));

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
