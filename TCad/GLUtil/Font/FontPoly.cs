using CadDataTypes;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace GLFont;

public struct FontPoly
{
    // 塗りつぶし用メッシュ
    public CadMesh Mesh { get; set; } = null;

    // 輪郭のインデックスリストのリスト
    // 例えば "い" なら、インデックスリストが2個格納される
    public List<List<int>> ContourList { get; set; } = null;

    // 輪郭の座標リスト
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
