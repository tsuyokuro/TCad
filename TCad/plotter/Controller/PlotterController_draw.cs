
using CadDataTypes;
using MyCollections;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Settings;

namespace Plotter.Controller;

public partial class PlotterController
{
    public void PushToView()
    {
        DC.PushToView();
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
        dc.PushToView();
    }

    //public void RedrawOnMainThread()
    //{
    //    ThreadUtil.RunOnMainThread(Redraw, false);
    //}

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

        DrawExtendSnapPoint(dc);

        DrawAccordingState(dc);

        DrawTop(dc);
    }

    public void DrawSelRect(DrawContext dc, Vector3d p0, Vector3d p1)
    {
        dc.Drawing.DrawRectScrn(dc.GetPen(DrawTools.PEN_TEMP_FIGURE), p0, p1);
    }

    public void DrawFiguresRaw(DrawContext dc)
    {
        dc.OptionSet.Update();
        DrawOption normal_dp = dc.OptionSet.Normal;

        foreach (CadLayer layer in mDB.LayerList)
        {
            if (!layer.Visible) continue;

            foreach (CadFigure fig in layer.FigureList)
            {
                fig.DrawEach(dc, normal_dp);
            }
        }
    }

    #region private
    private void DrawTop(DrawContext dc)
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
            dc.Drawing.DrawCrossScrn(dc.GetPen(DrawTools.PEN_AXIS), dc.WorldPointToDevPoint(Vector3d.Zero), 8);
        }

        dc.Drawing.DrawPageFrame(PageSize.Width, PageSize.Height, Vector3d.Zero);
        DrawGrid(dc);
    }

    FlexArray<CadFigure> AlphaFigList = new FlexArray<CadFigure>(100);
    FlexArray<CadFigure> AlphaFigListCurrentLayer = new FlexArray<CadFigure>(100);

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
            foreach (CadLayer layer in mDB.LayerList)
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
            dc.Drawing.DrawGrid(mGridding);
        }
    }

    private void DrawSelectedItems(DrawContext dc)
    {
        foreach (CadLayer layer in mDB.LayerList)
        {
            dc.Drawing.DrawSelected(layer.FigureList);
        }
    }

    private void DrawLastPoint(DrawContext dc)
    {
        dc.Drawing.DrawMarkCursor(
            dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER),
            LastDownPoint,
            ControllerConst.MARK_CURSOR_SIZE);

        if (ObjDownPoint.IsValid())
        {
            dc.Drawing.DrawMarkCursor(
                dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER2),
                ObjDownPoint,
                ControllerConst.MARK_CURSOR_SIZE);
        }
    }

    private void DrawDragLine(DrawContext dc)
    {
        if (State != States.DRAGING_POINTS)
        {
            return;
        }

        dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_DRAG_LINE),
            LastDownPoint, dc.DevPointToWorldPoint(CrossCursor.Pos));
    }

    private void DrawCrossCursor(DrawContext dc)
    {
        dc.Drawing.DrawCrossCursorScrn(CrossCursor, dc.GetPen(DrawTools.PEN_CROSS_CURSOR));

        if (CursorLocked)
        {
            dc.Drawing.DrawCrossScrn(
                dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT),
                CrossCursor.Pos,
                ControllerConst.CURSOR_LOCK_MARK_SIZE);
        }
    }

    private void DrawAccordingState(DrawContext dc)
    {
        CurrentState.Draw(dc);

        if (mInteractCtrl.IsActive)
        {
            mInteractCtrl.Draw(dc, SnapPoint);
        }
    }

    private void DrawHighlightPoint(DrawContext dc)
    {
        dc.Drawing.DrawHighlightPoints(HighlightPointList);
    }

    private void DrawHighlightSeg(DrawContext dc)
    {
        foreach (MarkSegment markSeg in HighlightSegList)
        {
            CadFigure fig = mDB.GetFigure(markSeg.FigureID);
            fig.DrawSeg(dc, dc.GetPen(DrawTools.PEN_MATCH_SEG), markSeg.PtIndexA, markSeg.PtIndexB);
        }
    }


    private void DrawExtendSnapPoint(DrawContext dc)
    {
        if (ExtendSnapPointList.Count > 0)
        {
            dc.Drawing.DrawExtSnapPoints(ExtendSnapPointList, dc.GetPen(DrawTools.PEN_EXT_SNAP));
        }
    }
    #endregion
}
