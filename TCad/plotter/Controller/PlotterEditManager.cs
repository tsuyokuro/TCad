using System.Collections.Generic;
using TCad.plotter.undo;


namespace Plotter.Controller;

public class PlotterEditManager
{
    private IPlotterController Controller;

    private CadOpeFigureSnapShotList mSnapShotList;

    private List<CadFigure> mEditFigList = new List<CadFigure>();

    public CadObjectDB DB
    {
        get => Controller.DB;
    }

    public HistoryManager HistoryMan
    {
        get => Controller.HistoryMan;
    }


    public PlotterEditManager(IPlotterController controller)
    {
        Controller = controller;
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
            Controller.UpdateObjectTree(true);
        }

        return opeList;
    }


    public void Cancel()
    {
        Controller.Input.UnlockCursor();

        if (Controller.Input.InteractCtrl.IsActive)
        {
            Controller.Input.InteractCtrl.Cancel();
        }

        Controller.CurrentState.Cancel();
    }
}
