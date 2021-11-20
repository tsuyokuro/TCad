using OpenTK;
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

            root.Add(rmOpeList);

            mSnapShotList.StoreAfter(DB);
            root.Add(mSnapShotList);

            HistoryMan.foward(root);

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

        public void MoveSelectedPoints(Vector3d delta)
        {
            StartEdit();
            MoveSelectedPoints(null, delta);
            EndEdit();
        }

        private void MoveSelectedPoints(DrawContext dc, Vector3d delta)
        {
            List<uint> figIDList = DB.GetSelectedFigIDList();

            //delta.z = 0;

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    fig.MoveSelectedPointsFromStored(dc, delta);
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

            if (State == States.START_CREATE || State == States.CREATING)
            {
                State = States.SELECT;
                CreatingFigType = CadFigure.Types.NONE;

                NotifyStateChange();
            }
            else if (State == States.DRAGING_POINTS)
            {
                CancelEdit();

                State = States.SELECT;
                ClearSelection();
            }
            else if (State == States.MEASURING)
            {
                State = States.SELECT;
                mMeasureMode = MeasureModes.NONE;
                MeasureFigureCreator = null;

                NotifyStateChange();
            }
        }
    }
}
