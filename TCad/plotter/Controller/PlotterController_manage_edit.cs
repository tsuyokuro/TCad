using OpenTK;
using OpenTK.Mathematics;
using System.Collections.Generic;


namespace Plotter.Controller;

public partial class PlotterController
{
    private CadOpeFigureSnapShotList mSnapShotList;

    private List<CadFigure> mEditFigList = new List<CadFigure>();


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

        foreach (CadLayer layer in DB.LayerList)
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
            CadFigure fig = DB.GetFigure(id);
            if (fig != null)
            {
                fig.MoveSelectedPointsFromStored(dc, moveInfo);
            }
        }
    }

    public void Cancel()
    {
        Input.UnlockCursor();

        if (Input.InteractCtrl.IsActive)
        {
            Input.InteractCtrl.Cancel();
        }

        CurrentState.Cancel();
    }
}
