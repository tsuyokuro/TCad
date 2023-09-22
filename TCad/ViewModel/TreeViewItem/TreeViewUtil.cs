//#define DEFAULT_DATA_TYPE_DOUBLE
using TCad.Controls;
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

public static class TreeViewUtil
{
    public static CadFigure GetCurrentFigure(CadObjTreeItem item, CadFigure currentFig)
    {
        CadFigure fig = null;

        //DOut.pl("TreeViewUtil.GetCurrentFigure " + item.GetType().ToString());

        if (item is CadFigTreeItem)
        {
            CadFigTreeItem figItem = (CadFigTreeItem)item;

            if (figItem.IsChecked)
            {
                fig = figItem.Fig;
            }
            else
            {
                if (currentFig == figItem.Fig)
                {
                    fig = null;
                }
            }
        }
        else if (item is CadPointTreeItem)
        {
            CadPointTreeItem ptItem = (CadPointTreeItem)item;

            if (ptItem.Parent != null)
            {
                if (ptItem.Parent.IsChecked)
                {
                    fig = ptItem.Fig;
                }
                else
                {
                    if (currentFig == ptItem.Fig)
                    {
                        fig = null;
                    }
                }
            }
        }

        return fig;
    }
}
