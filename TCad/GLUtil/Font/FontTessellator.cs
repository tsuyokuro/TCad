using CadDataTypes;
using GLUtil;
using OpenGL.GLU;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GLFont;

internal class FontTessellator
{
    public static CadMesh Tessellate(GlyphSlot glyph, double scale, int div, Tessellator tesse)
    {
        if (scale == 0) scale = 100.0;

        Outline outline = glyph.Outline;
        FTVector[] points = outline.Points;

        // 1:非制御点 
        byte[] tags = outline.Tags;

        List<Tessellator.Contour> contourList = new List<Tessellator.Contour>();
        List<Vector3d> vertexList = new List<Vector3d>();

        Vector3d cv = new();

        int idx = 0;
        for (int i = 0; i < outline.ContoursCount; i++)
        {
            Tessellator.Contour contour = new();

            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = points[idx];
                cv.X = fv.X * scale;
                cv.Y = fv.Y * scale;
                cv.Z = 0;

                vertexList.Add(cv);
                contour.IndexList.Add(idx);

                idx++;
            }

            contourList.Add(contour);
        }

        return tesse.Tessellate(contourList, vertexList);
    }
}
