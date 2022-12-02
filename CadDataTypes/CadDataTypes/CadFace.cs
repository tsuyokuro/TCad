using MyCollections;

using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

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
