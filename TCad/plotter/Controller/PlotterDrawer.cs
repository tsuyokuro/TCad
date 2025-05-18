using MyCollections;
using TCad.Plotter.Settings;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Searcher;
using TCad.Plotter.Drawing;

namespace TCad.Plotter.Controller;


public class PlotterDrawer
{
    IPlotterController Controller;

    public PlotterDrawer(IPlotterController controller)
    {
        Controller = controller;
    }

    public void Redraw()
    {
        Redraw(Controller.DC);
    }

    public void Redraw(DrawContext dc)
    {
        dc.StartDraw();
        Clear(dc);
        DrawAll(dc);
        dc.EndDraw();

        UpdateView();
    }

    public void Clear()
    {
        Clear(Controller.DC);
    }

    public void Clear(DrawContext dc = null)
    {
        dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));
    }

    public void DrawAll()
    {
        DrawAll(Controller.DC);
    }

    public void DrawAll(DrawContext dc)
    {
        DrawBase(dc);

        DrawDragLine(dc);

        DrawCrossCursor(dc);

        DrawFigures(dc);

        DrawSelectedItems(dc);

        DrawLastPoint(dc);

        DrawHighlightPoint(dc);

        DrawHighlightSeg(dc);

        DrawLastSelSeg(dc);

        DrawLastSelPoint(dc);

        DrawExtendSnapPoint(dc);

        DrawAccordingState(dc);

        DrawTop(dc);

        DrawCrossCursorShort(dc);
    }

    public void DrawFiguresRaw(DrawContext dc)
    {
        dc.OptionSet.Update();
        DrawOption normal_dp = dc.OptionSet.Normal;

        foreach (CadLayer layer in Controller.DB.LayerList)
        {
            if (!layer.Visible) continue;

            foreach (CadFigure fig in layer.FigureList)
            {
                fig.DrawEach(dc, normal_dp);
            }
        }
    }

    public void UpdateView()
    {
        Controller.ViewModel.ViewManager.View.SwapBuffers();
    }

    #region private
    private static void DrawTop(DrawContext dc)
    {
        if (SettingsHolder.Settings.DrawAxis && SettingsHolder.Settings.DrawAxisLabel)
        {
            dc.Drawing.DrawAxisLabel();
        }

        if (SettingsHolder.Settings.DrawCompass)
        {
            dc.Drawing.DrawCompass();
        }
    }

    private void DrawBase(DrawContext dc)
    {
        if (SettingsHolder.Settings.DrawAxis)
        {
            dc.Drawing.DrawAxis();
        }
        else
        {
            dc.Drawing.DrawCrossScrn(dc.GetPen(DrawTools.PEN_AXIS), dc.WorldPointToDevPoint(vector3_t.Zero), 8);
        }

        dc.Drawing.DrawPageFrame(Controller.PageSize.Width, Controller.PageSize.Height, vector3_t.Zero);
        DrawGrid(dc);
    }

    FlexArray<CadFigure> AlphaFigList = new(100);
    FlexArray<CadFigure> AlphaFigListCurrentLayer = new(100);

    private void DrawFigures(DrawContext dc)
    {
        if (dc == null) return;

        dc.OptionSet.Update();

        DrawOption pale_dp = dc.OptionSet.Pale;
        DrawOption temp_dp = dc.OptionSet.Temp;
        DrawOption current_dp = dc.OptionSet.Current;
        DrawOption measure_dp = dc.OptionSet.Measure;
        DrawOption normal_dp = dc.OptionSet.Normal;

        AlphaFigList.Clear();
        AlphaFigListCurrentLayer.Clear();

        lock (Controller.DB)
        {
            foreach (CadLayer layer in Controller.DB.LayerList)
            {
                if (!layer.Visible) continue;

                // Skip current layer.
                // It will be drawn at the end of this loop.
                if (layer == Controller.CurrentLayer) { continue; }

                foreach (CadFigure fig in layer.FigureList)
                {
                    if (fig.Type == CadFigure.Types.DIMENTION_LINE)
                    {
                        AlphaFigList.Add(fig);
                        continue;
                    }

                    if (fig.Current)
                    {
                        fig.DrawEach(dc, current_dp);
                    }
                    else
                    {
                        fig.DrawEach(dc, pale_dp);
                    }
                }
            }

            // Draw current layer at last
            if (Controller.CurrentLayer != null && Controller.CurrentLayer.Visible)
            {
                foreach (CadFigure fig in Controller.CurrentLayer.FigureList)
                {
                    if (fig.Type == CadFigure.Types.DIMENTION_LINE)
                    {
                        AlphaFigListCurrentLayer.Add(fig);
                        continue;
                    }

                    if (fig.Current)
                    {
                        fig.DrawEach(dc, current_dp);
                    }
                    else
                    {
                        fig.DrawEach(dc, normal_dp);
                    }
                }
            }

            foreach (CadFigure fig in Controller.TempFigureList)
            {
                if (fig.Type == CadFigure.Types.DIMENTION_LINE)
                {
                    continue;
                }

                fig.DrawEach(dc, temp_dp);
            }

            if (Controller.MeasureFigureCreator != null)
            {
                if (Controller.MeasureFigureCreator.Figure.Type != CadFigure.Types.DIMENTION_LINE)
                {
                    Controller.MeasureFigureCreator.Figure.Draw(dc, measure_dp);
                }
            }

            // Alpha指定があるFigureを描画
            foreach (CadFigure fig in AlphaFigList)
            {
                if (fig.Current)
                {
                    fig.DrawEach(dc, current_dp);
                }
                else
                {
                    fig.DrawEach(dc, pale_dp);
                }
            }

            foreach (CadFigure fig in AlphaFigListCurrentLayer)
            {
                if (fig.Current)
                {
                    fig.DrawEach(dc, current_dp);
                }
                else
                {
                    fig.DrawEach(dc, normal_dp);
                }
            }


            foreach (CadFigure fig in Controller.TempFigureList)
            {
                if (fig.Type != CadFigure.Types.DIMENTION_LINE)
                {
                    continue;
                }

                fig.DrawEach(dc, temp_dp);
            }

            if (Controller.MeasureFigureCreator != null)
            {
                if (Controller.MeasureFigureCreator.Figure.Type == CadFigure.Types.DIMENTION_LINE)
                {
                    Controller.MeasureFigureCreator.Figure.Draw(dc, measure_dp);
                }
            }
        }
    }

    private void DrawGrid(DrawContext dc)
    {
        if (SettingsHolder.Settings.SnapToGrid)
        {
            dc.Drawing.DrawGrid(Controller.Input.Grid);
        }
    }

    private void DrawSelectedItems(DrawContext dc)
    {
        DrawOption current_dp = dc.OptionSet.Current;
        DrawOption normal_dp = dc.OptionSet.Normal;

        dc.DisableLight();

        foreach (CadLayer layer in Controller.DB.LayerList)
        {
            foreach (CadFigure fig in layer.FigureList)
            {
                if (fig.Current)
                {
                    fig.DrawSelectedEach(Controller.DC, current_dp);
                }
                else
                {
                    fig.DrawSelectedEach(Controller.DC, normal_dp);
                }
            }
        }

        dc.EnableLight();
    }

    private void DrawLastPoint(DrawContext dc)
    {
        dc.Drawing.DrawMarkCursor(
            dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER),
            Controller.Input.LastDownPoint,
            DrawSizes.MarkCursorSize);

        if (Controller.Input.ObjDownPoint.IsValid())
        {
            dc.Drawing.DrawMarkCursor(
                dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER2),
                Controller.Input.ObjDownPoint,
                DrawSizes.MarkCursorSize);
        }
    }

    private void DrawDragLine(DrawContext dc)
    {
        if (Controller.StateID != ControllerStateID.DRAGING_POINTS)
        {
            return;
        }

        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_DRAG_LINE),
            Controller.Input.LastDownPoint, dc.DevPointToWorldPoint(Controller.Input.CrossCursor.Pos));
    }

    private void DrawCrossCursor(DrawContext dc)
    {
        dc.Drawing.DrawCrossCursorScrn(Controller.Input.CrossCursor, dc.GetPen(DrawTools.PEN_CROSS_CURSOR));

        if (Controller.Input.CursorLocked)
        {
            dc.Drawing.DrawCrossScrn(
                dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT),
                Controller.Input.CrossCursor.Pos,
                DrawSizes.CursorLockMarkSize);
        }
    }

    private void DrawCrossCursorShort(DrawContext dc)
    {
        dc.Drawing.DrawCrossCursorScrn(Controller.Input.CrossCursor, dc.GetPen(DrawTools.PEN_CROSS_CURSOR2), 12, 12);
    }

    private void DrawAccordingState(DrawContext dc)
    {
        Controller.CurrentState.Draw(dc);

        if (Controller.Input.InteractCtrl.IsActive)
        {
            Controller.Input.InteractCtrl.Draw(dc, Controller.Input.SnapPoint);
        }
    }

    private void DrawHighlightPoint(DrawContext dc)
    {
        dc.Drawing.DrawHighlightPoints(Controller.Input.HighlightPointList);
    }

    private void DrawHighlightSeg(DrawContext dc)
    {
        foreach (MarkSegment markSeg in Controller.Input.HighlightSegList)
        {
            CadFigure fig = Controller.DB.GetFigure(markSeg.FigureID);
            fig.DrawSeg(dc, dc.GetPen(DrawTools.PEN_MATCH_SEG), markSeg.PtIndexA, markSeg.PtIndexB);
        }
    }

    private void DrawLastSelSeg(DrawContext dc)
    {
        if (Controller.Input.LastSelSegment == null)
        {
            return;
        }

        CadFigure fig = Controller.DB.GetFigure(Controller.Input.LastSelSegment.Value.FigureID);
        fig.DrawSeg(
                dc, dc.GetPen(DrawTools.PEN_LAST_SEL_SEG),
                Controller.Input.LastSelSegment.Value.PtIndexA,
                Controller.Input.LastSelSegment.Value.PtIndexB);
    }

    private void DrawLastSelPoint(DrawContext dc)
    {
        if (Controller.Input.LastSelPoint == null)
        {
            return;
        }

        CadFigure fig = Controller.DB.GetFigure(Controller.Input.LastSelPoint.Value.FigureID);
        int idx = Controller.Input.LastSelPoint.Value.PointIndex;
        var point = fig.PointList[idx];


        dc.Drawing.DrawLastSelectedPoint(point.vector, dc.GetPen(DrawTools.PEN_LAST_SEL_POINT));
    }

    private void DrawExtendSnapPoint(DrawContext dc)
    {
        if (Controller.Input.ExtendSnapPointList.Count > 0)
        {
            dc.Drawing.DrawExtSnapPoints(Controller.Input.ExtendSnapPointList, dc.GetPen(DrawTools.PEN_EXT_SNAP));
        }
    }
    #endregion
}
