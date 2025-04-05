using MyCollections;
using OpenTK.Mathematics;
using Plotter.Settings;
using System.Windows.Controls;

namespace Plotter.Controller;

public partial class PlotterController
{
    public void UpdateView()
    {
        DC.UpdateView();
    }

    public void Redraw()
    {
        Redraw(DC);
    }

    public void Redraw(DrawContext dc)
    {
        dc.StartDraw();
        Clear(dc);
        DrawAll(dc);
        dc.EndDraw();
        dc.UpdateView();
    }

    public void Clear()
    {
        Clear(DC);
    }
    
    public void Clear(DrawContext dc = null)
    {
        dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));
    }

    public void DrawAll()
    {
        DrawAll(DC);
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

        foreach (CadLayer layer in DB.LayerList)
        {
            if (!layer.Visible) continue;

            foreach (CadFigure fig in layer.FigureList)
            {
                fig.DrawEach(dc, normal_dp);
            }
        }
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

        dc.Drawing.DrawPageFrame(PageSize.Width, PageSize.Height, vector3_t.Zero);
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

        lock (DB)
        {
            foreach (CadLayer layer in DB.LayerList)
            {
                if (!layer.Visible) continue;

                // Skip current layer.
                // It will be drawn at the end of this loop.
                if (layer == CurrentLayer) { continue; }

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
            if (CurrentLayer != null && CurrentLayer.Visible)
            {
                foreach (CadFigure fig in CurrentLayer.FigureList)
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

            foreach (CadFigure fig in TempFigureList)
            {
                if (fig.Type == CadFigure.Types.DIMENTION_LINE)
                {
                    continue;
                }

                fig.DrawEach(dc, temp_dp);
            }

            if (MeasureFigureCreator != null)
            {
                if (MeasureFigureCreator.Figure.Type != CadFigure.Types.DIMENTION_LINE)
                {
                    MeasureFigureCreator.Figure.Draw(dc, measure_dp);
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


            foreach (CadFigure fig in TempFigureList)
            {
                if (fig.Type != CadFigure.Types.DIMENTION_LINE)
                {
                    continue;
                }

                fig.DrawEach(dc, temp_dp);
            }

            if (MeasureFigureCreator != null)
            {
                if (MeasureFigureCreator.Figure.Type == CadFigure.Types.DIMENTION_LINE)
                {
                    MeasureFigureCreator.Figure.Draw(dc, measure_dp);
                }
            }
        }
    }

    private void DrawGrid(DrawContext dc)
    {
        if (SettingsHolder.Settings.SnapToGrid)
        {
            dc.Drawing.DrawGrid(Input.Grid);
        }
    }

    private void DrawSelectedItems(DrawContext dc)
    {
        DrawOption current_dp = dc.OptionSet.Current;
        DrawOption normal_dp = dc.OptionSet.Normal;

        dc.DisableLight();

        foreach (CadLayer layer in DB.LayerList)
        {
            foreach (CadFigure fig in layer.FigureList)
            {
                if (fig.Current)
                {
                    fig.DrawSelectedEach(DC, current_dp);
                }
                else
                {
                    fig.DrawSelectedEach(DC, normal_dp);
                }
            }
        }

        dc.EnableLight();
    }

    private void DrawLastPoint(DrawContext dc)
    {
        dc.Drawing.DrawMarkCursor(
            dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER),
            Input.LastDownPoint,
            ControllerConst.MARK_CURSOR_SIZE);

        if (Input.ObjDownPoint.IsValid())
        {
            dc.Drawing.DrawMarkCursor(
                dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER2),
                Input.ObjDownPoint,
                ControllerConst.MARK_CURSOR_SIZE);
        }
    }

    private void DrawDragLine(DrawContext dc)
    {
        if (StateID != ControllerStates.DRAGING_POINTS)
        {
            return;
        }

        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_DRAG_LINE),
            Input.LastDownPoint, dc.DevPointToWorldPoint(Input.CrossCursor.Pos));
    }

    private void DrawCrossCursor(DrawContext dc)
    {
        dc.Drawing.DrawCrossCursorScrn(Input.CrossCursor, dc.GetPen(DrawTools.PEN_CROSS_CURSOR));

        if (Input.CursorLocked)
        {
            dc.Drawing.DrawCrossScrn(
                dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT),
                Input.CrossCursor.Pos,
                ControllerConst.CURSOR_LOCK_MARK_SIZE);
        }
    }

    private void DrawCrossCursorShort(DrawContext dc)
    {
        dc.Drawing.DrawCrossCursorScrn(Input.CrossCursor, dc.GetPen(DrawTools.PEN_CROSS_CURSOR2), 12, 12);
    }

    private void DrawAccordingState(DrawContext dc)
    {
        CurrentState.Draw(dc);

        if (Input.InteractCtrl.IsActive)
        {
            Input.InteractCtrl.Draw(dc, Input.SnapPoint);
        }
    }

    private void DrawHighlightPoint(DrawContext dc)
    {
        dc.Drawing.DrawHighlightPoints(Input.HighlightPointList);
    }

    private void DrawHighlightSeg(DrawContext dc)
    {
        foreach (MarkSegment markSeg in Input.HighlightSegList)
        {
            CadFigure fig = DB.GetFigure(markSeg.FigureID);
            fig.DrawSeg(dc, dc.GetPen(DrawTools.PEN_MATCH_SEG), markSeg.PtIndexA, markSeg.PtIndexB);
        }
    }

    private void DrawLastSelSeg(DrawContext dc)
    {
        if (Input.LastSelSegment == null)
        {
            return;
        }

        CadFigure fig = DB.GetFigure(Input.LastSelSegment.Value.FigureID);
        fig.DrawSeg(
                dc, dc.GetPen(DrawTools.PEN_LAST_SEL_SEG),
                Input.LastSelSegment.Value.PtIndexA,
                Input.LastSelSegment.Value.PtIndexB);
    }

    private void DrawLastSelPoint(DrawContext dc)
    {
        if (Input.LastSelPoint == null)
        {
            return;
        }

        CadFigure fig = DB.GetFigure(Input.LastSelPoint.Value.FigureID);
        int idx = Input.LastSelPoint.Value.PointIndex;
        var point = fig.PointList[idx];


        dc.Drawing.DrawLastSelectedPoint(point.vector, dc.GetPen(DrawTools.PEN_LAST_SEL_POINT));
    }

    private void DrawExtendSnapPoint(DrawContext dc)
    {
        if (Input.ExtendSnapPointList.Count > 0)
        {
            dc.Drawing.DrawExtSnapPoints(Input.ExtendSnapPointList, dc.GetPen(DrawTools.PEN_EXT_SNAP));
        }
    }
    #endregion
}
