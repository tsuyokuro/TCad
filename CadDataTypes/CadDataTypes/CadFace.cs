using MyCollections;

using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace CadDataTypes;

public class CadFace
{
    public FlexArray<int> VList;

    public CadFace()
    {
        VList = new FlexArray<int>(3);
    }

    public CadFace(FlexArray<int> vl)
    {
        VList = new FlexArray<int>(vl);
    }

    public CadFace(params int[] args)
    {
        VList = new FlexArray<int>(args.Length);
        for (int i=0; i< args.Length; i++)
        {
            VList.Add(args[i]);
        }
    }

    public CadFace(CadFace src)
    {
        VList = new FlexArray<int>(src.VList);
    }
}
