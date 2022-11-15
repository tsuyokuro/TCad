using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TCad.ScriptEditor;

public class BreakPointMargin : AbstractMargin
{
    private const int margin = 20;

    private HashSet<int> BreakPoints;

    private TextEditor mTextEditor;

    public BreakPointMargin(HashSet<int> bps)
    {
        BreakPoints = bps;
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(margin, 0);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        TextView textView = this.TextView;
        Size renderSize = this.RenderSize;
        if (textView != null && textView.VisualLinesValid)
        {
            foreach (VisualLine line in textView.VisualLines)
            {
                int lineNumber = line.FirstDocumentLine.LineNumber;
                if (BreakPoints.Contains(lineNumber))
                {
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
                    y -= textView.VerticalOffset;

                    double x = (renderSize.Width - 8) / 2;

                    y = (line.Height - 8) / 2 + y;

                    drawingContext.DrawRectangle(Brushes.Red, null, new Rect(x, y, 8, 8));
                }
            }
        }
    }

    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView != null)
        {
            oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
        }

        base.OnTextViewChanged(oldTextView, newTextView);

        if (newTextView != null)
        {
            newTextView.VisualLinesChanged += TextViewVisualLinesChanged;

            mTextEditor = newTextView.Services.GetService(typeof(TextEditor)) as TextEditor;
        }
        else
        {
            mTextEditor = null;
        }
        InvalidateVisual();
    }

    void TextViewVisualLinesChanged(object sender, EventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        TextViewPosition? pos = mTextEditor.GetPositionFromPoint(e.GetPosition(mTextEditor));

        if (!pos.HasValue)
        {
            return;
        }

        int line = pos.Value.Location.Line;

        DocumentLine docLine = mTextEditor.Document.GetLineByNumber(line);

        string s = mTextEditor.Document.GetText(docLine.Offset, docLine.Length);

        s = s.Trim();

        if (s.Length == 0)
        {
            return;
        }

        if (BreakPoints.Contains(line))
        {
            BreakPoints.Remove(line);
        }
        else
        {
            BreakPoints.Add(line);
        }

        InvalidateVisual();
    }
}

