using CadDataTypes;
using System;
using System.Collections.Generic;

using TCad.Plotter.Assembler;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Searcher;
using TCad.Plotter.undo;
using TCad.Properties;
using TCad.Logger;

namespace TCad.Plotter.Controller;

// Edit figure functions

public class PlotterEditor
{
    private IPlotterController Controller;

    PlotterInput Input
    {
        get => Controller.Input;
    }

    public CadObjectDB DB
    {
        get => Controller.DB;
    }

    public HistoryManager HistoryMan
    {
        get => Controller.HistoryMan;
    }

    public CadLayer CurrentLayer
    {
        get => Controller.CurrentLayer;
    }

    public PlotterTaskRunner PlotterTaskRunner
    {
        get => Controller.PlotterTaskRunner;
    }

    DrawContext DC
    {
        get => Controller.DC;
    }

    public PlotterEditor(IPlotterController controller)
    {
        Controller = controller;
    }

    public bool ToBezier()
    {
        if (Input.LastSelSegment == null)
        {
            return false;
        }

        bool ret = ToBezier(Input.LastSelSegment.Value);

        if (ret)
        {
            Input.ClearSelection();
            Controller.UpdateObjectTree(true);
        }

        return ret;
    }

    public bool ToBezier(MarkSegment seg)
    {
        if (seg.FigureID == 0)
        {
            return false;
        }

        CadFigure fig = DB.GetFigure(seg.FigureID);

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
        if (Input.LastSelPoint == null)
        {
            return;
        }

        SeparateFigures(Input.LastSelPoint.Value.Figure, Input.LastSelPoint.Value.PointIndex);
        Input.ClearSelection();
    }

    public void SeparateFigures(CadFigure fig, int pointIdx)
    {
        var res = CadFigureCutter.Cut(DB, fig, pointIdx);

        if (!res.isValid())
        {
            return;
        }

        CadOpeList opeRoot = new CadOpeList();
        CadOpe ope;

        foreach (EditResult.Item ri in res.AddList)
        {
            CadLayer layer = DB.GetLayer(ri.LayerID);

            ope = new CadOpeAddFigure(ri.LayerID, ri.FigureID);
            opeRoot.OpeList.Add(ope);

            layer.AddFigure(ri.Figure);
        }

        foreach (EditResult.Item ri in res.RemoveList)
        {
            CadLayer layer = DB.GetLayer(ri.LayerID);

            ope = new CadOpeRemoveFigure(layer, ri.FigureID);
            opeRoot.OpeList.Add(ope);

            layer.RemoveFigureByID(ri.FigureID);
        }

        HistoryMan.foward(opeRoot);
    }

    public void BondFigures()
    {
        BondFigures(Input.CurrentFigure);
        Input.ClearSelection();
    }

    public void BondFigures(CadFigure fig)
    {
        var res = CadFigureBonder.Bond(DB, fig);

        if (!res.isValid())
        {
            return;
        }

        CadOpeList opeRoot = new CadOpeList();
        CadOpe ope;

        foreach (EditResult.Item ri in res.AddList)
        {
            CadLayer layer = DB.GetLayer(ri.LayerID);

            ope = new CadOpeAddFigure(ri.LayerID, ri.FigureID);
            opeRoot.OpeList.Add(ope);

            layer.AddFigure(ri.Figure);
        }

        foreach (EditResult.Item ri in res.RemoveList)
        {
            CadLayer layer = DB.GetLayer(ri.LayerID);

            ope = new CadOpeRemoveFigure(layer, ri.FigureID);
            opeRoot.OpeList.Add(ope);

            layer.RemoveFigureByID(ri.FigureID);
        }

        HistoryMan.foward(opeRoot);
    }

    public void CutSegment()
    {
        if (Input.LastSelSegment == null)
        {
            return;
        }

        MarkSegment ms = Input.LastSelSegment.Value;
        CutSegment(ms);
        Input.ClearSelection();
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

        var res = CadSegmentCutter.CutSegment(DB, ms, ms.CrossPoint);

        if (!res.isValid())
        {
            return;
        }

        CadOpeList opeRoot = new CadOpeList();
        CadOpe ope;

        foreach (EditResult.Item ri in res.AddList)
        {
            CadLayer layer = DB.GetLayer(ri.LayerID);

            ope = new CadOpeAddFigure(ri.LayerID, ri.FigureID);
            opeRoot.OpeList.Add(ope);

            layer.AddFigure(ri.Figure);
        }

        foreach (EditResult.Item ri in res.RemoveList)
        {
            CadLayer layer = DB.GetLayer(ri.LayerID);

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

                ope = new CadOpeSetClose(CurrentLayer.ID, id, isLoop);
                opeRoot.OpeList.Add(ope);
            }
        }

        HistoryMan.foward(opeRoot);
    }

    public void FlipWithVector()
    {
        List<CadFigure> target = Controller.GetSelectedRootFigureList();
        if (target.Count <= 0)
        {
            ItConsole.printFaile(
                Resources.warning_select_objects_before_flip);
            return;
        }

        PlotterTaskRunner.FlipWithInteractive(target);
    }

    public void FlipAndCopyWithVector()
    {
        List<CadFigure> target = Controller.GetSelectedRootFigureList();
        if (target.Count <= 0)
        {
            ItConsole.printFaile(
                Resources.warning_select_objects_before_flip_and_copy);
            return;
        }

        PlotterTaskRunner.FlipAndCopyWithInteractive(target);
    }

    public void CutMeshWithVector()
    {
        CadFigure target = Input.CurrentFigure;
        if (target == null)
        {
            ItConsole.printFaile("No Figure selected.");
            return;
        }

        PlotterTaskRunner.CutMeshWithInteractive(target);
    }

    public void RotateWithPoint()
    {
        List<CadFigure> target = Controller.GetSelectedRootFigureList();
        if (target.Count <= 0)
        {
            ItConsole.printFaile("No target figure.");
            return;
        }

        PlotterTaskRunner.RotateWithInteractive(target);
    }

    public void RemoveSelectedPoints()
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

        Input.ClearSelection();
        Controller.UpdateObjectTree(true);
    }


    public bool InsPointToLastSelectedSeg()
    {
        if (Input.LastSelSegment == null)
        {
            return false;
        }

        MarkSegment seg = Input.LastSelSegment.Value;

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


        if (fig.IsLoop)
        {
            if (ins0 == 0 && ins1 == fig.PointCount - 1)
            {
                ins = ins1 + 1;
            }
            else
            {
                ins = ins1;
            }
        }
        else
        {
            ins = ins1;
        }


        Log.pl($"ins={ins} pcnt={fig.PointCount}");

        fig.InsertPointAt(ins, (CadVertex)Input.LastDownPoint);

        Input.ClearSelection();

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

        CadFigure pointFig = DB.NewFigure(CadFigure.Types.POINT);
        pointFig.AddPoint((CadVertex)cent.Point);

        pointFig.EndCreate(DC);

        CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, pointFig.ID);
        HistoryMan.foward(ope);
        CurrentLayer.AddFigure(pointFig);

        string s = string.Format("({0:(vcompo_t)(0.000)},{1:(vcompo_t)(0.000)},{2:(vcompo_t)(0.000)})",
                           cent.Point.X, cent.Point.Y, cent.Point.Z);

        ItConsole.println("Centroid:" + s);
        ItConsole.println("Area:" + (cent.Area / 100).ToString() + "(㎠)");
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
}
