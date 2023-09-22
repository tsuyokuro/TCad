//#define DEFAULT_DATA_TYPE_DOUBLE
using TCad.Controls;
using CadDataTypes;
using System.Windows.Media;
using Plotter;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad.ViewModel;

class CadPointTreeItem : CadObjTreeItem
{
    public CadFigure Fig;
    public int Index;

    private static SolidColorBrush CheckedBackColor = new SolidColorBrush(Color.FromRgb(0x11, 0x46, 0x11));

    public override bool IsChecked
    {
        get
        {
            if (Index >=0 && Index < Fig.PointCount)
            {
                return Fig.GetPointAt(Index).Selected;
            }

            return false;
        }

        set
        {
            CadVertex v = Fig.GetPointAt(Index);
            v.Selected = value;
            Fig.SetPointAt(Index, v);
        }
    }

    public override string Text
    {
        get
        {
            CadVertex v;

            if (Index >= 0 && Index < Fig.PointCount)
            {
                v = Fig.GetPointAt(Index);
                return $"{v.X.ToString("F2")}, {v.Y.ToString("F2")}, {v.Z.ToString("F2")}";
            }

            return "removed";
        }
    }

    public override SolidColorBrush getForeColor()
    {
        return null;
    }

    public override SolidColorBrush getBackColor()
    {
        if (!IsChecked)
        {
            return null;
        }

        return CheckedBackColor;
    }

    public CadPointTreeItem(CadFigure fig, int idx)
    {
        Fig = fig;
        Index = idx;
    }
}
