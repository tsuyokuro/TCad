using CadDataTypes;
using OpenTK.Mathematics;
using System.Collections.Generic;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace GLFont;

public struct FontPoly
{
    public CadMesh Mesh { get; set; } = null;
    public List<List<int>> ContourList { get; set; } = null;
    public List<vector3_t> VertexList { get; set; } = null;

    public FontPoly()
    {
    }

    public FontPoly(FontPoly src)
    {
        Mesh = new CadMesh(src.Mesh);

        ContourList = new();
        for (int i=0; i < src.ContourList.Count; i++)
        {
            List<int> cont = new(src.ContourList[i]);
            ContourList.Add(cont);
        }

        VertexList = new(src.VertexList);
    }
}
