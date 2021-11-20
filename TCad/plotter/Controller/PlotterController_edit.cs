using System;
using System.Collections.Generic;
using CadDataTypes;
using TCad.Properties;
using OpenTK;

namespace Plotter.Controller
{
    // Edit figure functions

    public partial class PlotterController
    {
        public bool ToBezier()
        {
            if (LastSelSegment == null)
            {
                return false;
            }

            bool ret = ToBezier(LastSelSegment.Value);

            if (ret)
            {
                ClearSelection();
                UpdateObjectTree(true);
            }

            return ret;
        }

        public bool ToBezier(MarkSegment seg)
        {
            if (seg.FigureID == 0)
            {
                return false;
            }

            CadFigure fig = mDB.GetFigure(seg.FigureID);

            int num = CadUtil.InsertBezierHandle(fig, seg.PtIndexA, seg.PtIndexB);

            bool ret = num > 0;

            if (ret)
            {
                CadOpe ope = new CadOpeInsertPoints(
                    fig.LayerID, fig.ID, seg.PtIndexA + 1, num);

                HistoryMan.foward(ope);
            }

            return ret;
        }

        public void SeparateFigures()
        {
            if (LastSelPoint == null)
            {
                return;
            }

            SeparateFigures(LastSelPoint.Value.Figure, LastSelPoint.Value.PointIndex);
            ClearSelection();
        }

        public void SeparateFigures(CadFigure fig, int pointIdx)
        {
            var res = CadFigureCutter.Cut(mDB, fig, pointIdx);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = new CadOpeList();
            CadOpe ope;

            foreach (EditResult.Item ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = new CadOpeAddFigure(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (EditResult.Item ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = new CadOpeRemoveFigure(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            HistoryMan.foward(opeRoot);
        }

        public void BondFigures()
        {
            BondFigures(CurrentFigure);
            ClearSelection();
        }

        public void BondFigures(CadFigure fig)
        {
            var res = CadFigureBonder.Bond(mDB, fig);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = new CadOpeList();
            CadOpe ope;

            foreach (EditResult.Item ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = new CadOpeAddFigure(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (EditResult.Item ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = new CadOpeRemoveFigure(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            HistoryMan.foward(opeRoot);
        }

        public void CutSegment()
        {
            if (LastSelSegment == null)
            {
                return;
            }

            MarkSegment ms = LastSelSegment.Value;
            CutSegment(ms);
            ClearSelection();
        }

        public void CutSegment(MarkSegment ms)
        {
            if (!ms.Valid)
            {
                return;
            }

            if (!ms.CrossPoint.IsValid())
            {
                return;
            }

            var res = CadSegmentCutter.CutSegment(mDB, ms, ms.CrossPoint);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = new CadOpeList();
            CadOpe ope;

            foreach (EditResult.Item ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = new CadOpeAddFigure(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (EditResult.Item ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = new CadOpeRemoveFigure(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            HistoryMan.foward(opeRoot);
        }

        public void SetLoop(bool isLoop)
        {
            List<uint> list = DB.GetSelectedFigIDList();

            CadOpeList opeRoot = new CadOpeList();
            CadOpe ope;

            foreach (uint id in list)
            {
                CadFigure fig = DB.GetFigure(id);

                if (fig.Type != CadFigure.Types.POLY_LINES)
                {
                    continue;
                }

                if (fig.IsLoop != isLoop)
                {
                    fig.IsLoop = isLoop;

                    if (isLoop)
                    {
                        fig.RecalcNormal();
                    }

                    ope = new CadOpeSetClose(CurrentLayer.ID, id, isLoop);
                    opeRoot.OpeList.Add(ope);
                }
            }

            HistoryMan.foward(opeRoot);
        }

        public void FlipWithVector()
        {
            List<CadFigure> target = GetSelectedRootFigureList();
            if (target.Count <= 0)
            {
                ItConsole.printFaile(
                    Resources.warning_select_objects_before_flip);
                return;
            }

            mPlotterTaskRunner.FlipWithInteractive(target);
        }

        public void FlipAndCopyWithVector()
        {
            List<CadFigure> target = GetSelectedRootFigureList();
            if (target.Count <= 0)
            {
                ItConsole.printFaile(
                    Resources.warning_select_objects_before_flip_and_copy);
                return;
            }

            mPlotterTaskRunner.FlipAndCopyWithInteractive(target);
        }

        public void CutMeshWithVector()
        {
            CadFigure target = CurrentFigure;
            if (target == null)
            {
                ItConsole.printFaile("No Figure selected.");
                return;
            }

            mPlotterTaskRunner.CutMeshWithInteractive(target);
        }

        public void RotateWithPoint()
        {
            List<CadFigure> target = GetSelectedRootFigureList();
            if (target.Count <= 0)
            {
                ItConsole.printFaile("No target figure.");
                return;
            }

            mPlotterTaskRunner.RotateWithInteractive(target);
        }

        private void RemoveSelectedPoints()
        {
            List<CadFigure> figList = DB.GetSelectedFigList();
            foreach (CadFigure fig in figList)
            {
                fig.RemoveSelected();
            }

            foreach (CadFigure fig in figList)
            {
                fig.RemoveGarbageChildren();
            }

            UpdateObjectTree(true);
        }

        public void FlipNormal()
        {
            List<uint> ids = DB.GetSelectedFigIDList();

            CadOpeList opeList = new CadOpeList();

            foreach (uint id in ids)
            {
                CadFigure fig = mDB.GetFigure(id);
                Vector3d old = fig.Normal;

                fig.Normal *= -1;

                CadOpe ope = new CadOpeChangeNormal(id, old, fig.Normal);
                opeList.Add(ope);
            }


            HistoryMan.foward(opeList);
        }

        public bool InsPointToLastSelectedSeg()
        {
            if (LastSelSegment == null)
            {
                return false;
            }

            MarkSegment seg = LastSelSegment.Value;

            CadFigure fig = DB.GetFigure(seg.FigureID);

            if (fig == null)
            {
                return false;
            }

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return false;
            }

            bool handle = false;

            handle |= fig.GetPointAt(seg.PtIndexA).IsHandle;
            handle |= fig.GetPointAt(seg.PtIndexB).IsHandle;

            if (handle)
            {
                return false;
            }

            int ins = 0;
            int ins0 = Math.Min(seg.PtIndexA, seg.PtIndexB);
            int ins1 = Math.Max(seg.PtIndexA, seg.PtIndexB);

            if (ins0 == 0 && ins1==fig.PointCount-1)
            {
                ins = ins1 + 1;
            }
            else
            {
                ins = ins1;
            }

            DOut.pl($"ins={ins} pcnt={fig.PointCount}");

            fig.InsertPointAt(ins, (CadVertex)LastDownPoint);

            ClearSelection();

            fig.SelectPointAt(ins, true);

            return true;
        }

        public void AddCentroid()
        {
            Centroid cent = PlotterUtil.Centroid(DB.GetSelectedFigList());

            if (cent.IsInvalid)
            {
                return;
            }

            CadFigure pointFig = mDB.NewFigure(CadFigure.Types.POINT);
            pointFig.AddPoint((CadVertex)cent.Point);

            pointFig.EndCreate(DC);

            CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, pointFig.ID);
            HistoryMan.foward(ope);
            CurrentLayer.AddFigure(pointFig);

            string s = string.Format("({0:0.000},{1:0.000},{2:0.000})",
                               cent.Point.X, cent.Point.Y, cent.Point.Z);

            ItConsole.println("Centroid:" + s);
            ItConsole.println("Area:" + (cent.Area / 100).ToString() + "(㎠)");
        }
    }
}
