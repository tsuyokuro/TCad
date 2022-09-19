using OpenTK;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        private CadOpeFigureSnapShotList mSnapShotList;

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
            EditFigList = DB.GetSelectedFigList();
            return StartEdit(EditFigList);
        }

        public List<CadFigure> StartEdit(List<CadFigure> targetList)
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

            return targetList;
        }

        public void AbendEdit()
        {
            mSnapShotList = null;
        }

        public void EndEdit()
        {
            EndEdit(EditFigList);
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
            foreach (CadFigure fig in EditFigList)
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

        public void MoveSelectedPoints(MoveInfo moveInfo)
        {
            StartEdit();
            MoveSelectedPoints(null, moveInfo);
            EndEdit();
        }

        private void MoveSelectedPoints(DrawContext dc, MoveInfo moveInfo)
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

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Cancel();
            }

            CurrentState.Cancel();
        }
    }
}
