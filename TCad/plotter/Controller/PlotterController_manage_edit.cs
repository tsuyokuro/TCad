//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK;
using OpenTK.Mathematics;
using System.Collections.Generic;



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


namespace Plotter.Controller;

public partial class PlotterController
{
    private CadOpeFigureSnapShotList mSnapShotList;

    private List<CadFigure> mEditFigList = new List<CadFigure>();


    public void ClearSelection()
    {
        CurrentFigure = null;

        LastSelPoint = null;
        LastSelSegment = null;

        foreach (CadLayer layer in mDB.LayerList)
        {
            layer.ClearSelectedFlags();
        }
    }

    public List<CadFigure> StartEdit()
    {
        mEditFigList = DB.GetSelectedFigList();
        StartEdit(mEditFigList);

        return mEditFigList;
    }

    public void StartEdit(List<CadFigure> targetList)
    {
        mSnapShotList = new CadOpeFigureSnapShotList();

        mSnapShotList.StoreBefore(targetList);

        foreach (CadFigure fig in targetList)
        {
            if (fig != null)
            {
                fig.StartEdit();
            }
        }
    }

    public void AbendEdit()
    {
        mSnapShotList = null;
    }

    public void EndEdit()
    {
        EndEdit(mEditFigList);
    }

    public void EndEdit(List<CadFigure> targetList)
    {
        foreach (CadFigure fig in targetList)
        {
            if (fig != null)
            {
                fig.EndEdit();
            }
        }

        CadOpeList root = new CadOpeList();

        CadOpeList rmOpeList = RemoveInvalidFigure();

        if (rmOpeList.OpeList.Count > 0)
        {
            root.Add(rmOpeList);
        }

        mSnapShotList.StoreAfter(DB);
        if (mSnapShotList.Count > 0)
        {
            root.Add(mSnapShotList);
        }

        if (root.Count > 0)
        {
            HistoryMan.foward(root);
        }

        mSnapShotList = null;
    }

    public void CancelEdit()
    {
        foreach (CadFigure fig in mEditFigList)
        {
            if (fig != null)
            {
                fig.CancelEdit();
            }
        }
    }

    private CadOpeList RemoveInvalidFigure()
    {
        CadOpeList opeList = new CadOpeList();

        int removeCnt = 0;

        foreach (CadLayer layer in mDB.LayerList)
        {
            IReadOnlyList<CadFigure> list = layer.FigureList;

            int i = list.Count - 1;

            for (; i >= 0; i--)
            {
                CadFigure fig = list[i];

                if (fig.IsGarbage())
                {
                    CadOpe ope = new CadOpeRemoveFigure(layer, fig.ID);
                    opeList.OpeList.Add(ope);

                    layer.RemoveFigureByIndex(i);

                    removeCnt++;
                }
            }
        }

        if (removeCnt > 0)
        {
            UpdateObjectTree(true);
        }

        return opeList;
    }

    public void MoveSelectedPoints(DrawContext dc, MoveInfo moveInfo)
    {
        List<uint> figIDList = DB.GetSelectedFigIDList();

        //delta.z = 0;

        foreach (uint id in figIDList)
        {
            CadFigure fig = mDB.GetFigure(id);
            if (fig != null)
            {
                fig.MoveSelectedPointsFromStored(dc, moveInfo);
            }
        }
    }

    public void Cancel()
    {
        if (CursorLocked)
        {
            CursorLocked = false;
        }

        if (InteractCtrl.IsActive)
        {
            InteractCtrl.Cancel();
        }

        CurrentState.Cancel();
    }
}
