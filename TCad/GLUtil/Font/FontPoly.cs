using CadDataTypes;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace GLFont;

internal class FontPoly
{
    public CadMesh Mesh { get; set; } = null;
    public List<List<int>> ContourList { get; set; } = null;
    public List<Vector3d> VertexList { get; set; } = null;
}
