using Plotter;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TCad.Controls.CadConsole;
using TCad.Util;

namespace TCad.Controls;

public partial class CadConsoleView : FrameworkElement
{
    #region Properties
    protected Brush mBackground = Brushes.White;
    public Brush Background
    {
        get => mBackground;
        set => mBackground = value;
    }

    protected Brush mForeground = Brushes.Black;
    public Brush Foreground
    {
        get => mForeground;
        set => mForeground = value;
    }

    protected Brush mSelectedBackground = Brushes.GreenYellow;
    public Brush SelectedBackground
    {
        get => mSelectedBackground;
        set => mSelectedBackground = value;
    }

    protected double mSelectedBackgroundOpacity = 0.3;
    public double SelectedBackgroundOpacity
    {
        get => mSelectedBackgroundOpacity;
        set => mSelectedBackgroundOpacity = value;
    }

    protected double mTextLeftMargin = 8.0;
    public double TextLeftMargin
    {
        get => mTextLeftMargin;
        set
        {
            mTextLeftMargin = value;
            UpdateView();
        }
    }

    protected double mLineHeight = 14.0;
    public double LineHeight
    {
        get => mLineHeight;
        set => mLineHeight = value;
    }

    protected string DefaultFontName = "MS Gothic";
    protected Typeface mTypeface;
    protected FontFamily mFontFamily = null;
    public FontFamily FontFamily
    {
        get => mFontFamily;
        set
        {
            mFontFamily = value;
            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.UltraLight, FontStretches.Normal);
            RecalcCharaSize();
        }
    }

    protected double mFontSize = 10.0;
    public double FontSize
    {
        get => mFontSize;
        set
        {
            mFontSize = value;
            RecalcCharaSize();
        }
    }

    #endregion

    protected int mTopIndex = 0;

    protected bool mIsLoaded = false;

    protected ScrollViewer Scroll;

    protected FastRingBuffer<TextLine> mList = new FastRingBuffer<TextLine>();

    protected AnsiPalette Palette = new AnsiPalette();

    protected TextAttr DefaultAttr = default;

    protected TextAttr CurrentAttr = default;

    protected Pen FocusedBorderPen = new Pen(
            new SolidColorBrush(Color.FromArgb(0xff, 0x56, 0x9D, 0xE5)), 1.5);

    protected double CW = 1;

    protected double CWF = 2;

    protected double CH = 1;

    private TextRange RawSel = new TextRange();
    private TextRange Sel = new TextRange();
    private bool Selecting = false;

    private AutoScroller mAutoScroller;

    public CadConsoleView()
    {
        mList.CreateBuffer(200);

        Focusable = true;

        Loaded += CadConsoleView_Loaded;

        GotFocus += CadConsoleView_GotFocus;
        LostFocus += CadConsoleView_LostFocus;

        KeyUp += CadConsoleView_KeyUp;

        SizeChanged += CadConsoleView_SizeChanged;

        RawSel.Reset();
        Sel.Reset();
    }

    private void CadConsoleView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SizeChanged -= CadConsoleView_SizeChanged;
        RecalcSize();
        SizeChanged += CadConsoleView_SizeChanged;
    }

    private void CadConsoleView_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.C || e.Key == Key.Insert)
            {
                CopySelected(this, null);
            }
            else if (e.Key == Key.X)
            {
                Clear();
            }
        }
    }

    private void CadConsoleView_LostFocus(object sender, RoutedEventArgs e)
    {
        UpdateView();
    }

    private void CadConsoleView_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Log.plx("");

        if (FontFamily == null)
        {
            //FontFamily = new FontFamily(DefaultFontName);
            Uri uri = new Uri("pack://application:,,,/Fonts/");
            FontFamily = new FontFamily(uri, "./mplus-1m-light.ttf#M+ 1m light");
        }

        FrameworkElement parent = (FrameworkElement)Parent;

        if (parent is ScrollViewer)
        {
            Scroll = (ScrollViewer)parent;
        }

        if (Scroll != null)
        {
            Scroll.ScrollChanged += Scroll_ScrollChanged;
        }
    }

    private void CadConsoleView_Loaded(object sender, RoutedEventArgs e)
    {
        Log.plx("");
        mIsLoaded = true;

        mAutoScroller = new AutoScroller(this);
        mAutoScroller.Scroll = AutoScrollEvent;

        Palette.Brushes[Palette.DefaultFColor] = mForeground;
        Palette.Brushes[Palette.DefaultBColor] = mBackground;

        DefaultAttr.FColor = Palette.DefaultFColor;
        DefaultAttr.BColor = Palette.DefaultBColor;

        CurrentAttr = DefaultAttr;

        RecalcCharaSize();

        NewLine();

        UpdateView();

        SetContextMenu();
    }

    private void AutoScrollEvent(double dx, double dy)
    {
        Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + dy);
    }

    private void RecalcCharaSize()
    {
        if (mTypeface == null)
        {
            return;
        }

        FormattedText ft = GetFormattedText("A", mForeground);

        if (ft != null)
        {
            CW = ft.Width;
            CH = ft.Height;

            FormattedText ftk = GetFormattedText("漢", mForeground);
            if (ftk != null)
            {
                CWF = ftk.Width;
            }
            else
            {
                CWF = CW * 2;
            }
        }
        else
        {
            CW = 1;
            CH = 1;
            CWF = 1;
        }
    }

    private void CopySelected(Object obj, RoutedEventArgs args)
    {
        string copyString = GetSelectedString();

        if (copyString == null || copyString.Length == 0)
        {
            return;
        }

        Clipboard.SetDataObject(copyString, true);
    }

    private void SetContextMenu()
    {
        ContextMenu = new ContextMenu();

        ContextMenu.BorderBrush = Brushes.Black;
        ContextMenu.Padding = new Thickness(0, 1, 0, 1);

        MenuItem menuItem = new MenuItem();

        menuItem.Header = CadConsoleRes.menu_copy;
        menuItem.Click += CopySelected;

        SetupMenuItem(menuItem);

        ContextMenu.Items.Add(menuItem);
    }

    private void SetupMenuItem(MenuItem menuItem)
    {
        menuItem.Foreground = Brushes.White;
        menuItem.BorderThickness = new Thickness(0, 0, 0, 0);

        menuItem.MouseEnter += (sender, e) =>
        {
            menuItem.Foreground = Brushes.Black;
        };

        menuItem.MouseLeave += (sender, e) =>
        {
            menuItem.Foreground = Brushes.White;
        };
    }

    private void RemoveContextMenu()
    {
        if (ContextMenu != null)
        {
            ContextMenu.IsOpen = false;
        }

        ContextMenu = null;
    }

    //
    // MouseDown += handler ではなく、OnMouseDownをoverrideするように
    // しないと、Focus()を呼んでもすぐにLostFocusしてしまう
    // 上手くEventを処理済みに出来ないのかもしれない
    //
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Point p = e.GetPosition(this);

        if (e.ClickCount == 1)
        {
            StartSelect(p);
        }
        else if (e.ClickCount == 2)
        {
            SelectWord(p);
        }

        UpdateView();

        if (Focus())
        {
            e.Handled = true;
        }

        if (mAutoScroller != null)
        {
            mAutoScroller.Start();
        }

        base.OnMouseDown(e);
    }

    protected void StartSelect(Point p)
    {
        TextPos tp = PointToTextPos(p);

        Sel.Reset();

        RawSel.Start(tp.Row, tp.Col);
        Selecting = true;
    }

    private Regex WordRegex = new Regex(@"([^ \t,:=/\\]+)");

    protected void SelectWord(Point p)
    {
        TextPos tp = PointToTextPos(p);
        Sel.Reset();

        if (tp.Row >= mList.Count)
        {
            return;
        }

        TextLine item = mList[tp.Row];

        if (tp.Col >= item.Data.Length)
        {
            return;
        }

        MatchCollection matches = WordRegex.Matches(item.Data);

        foreach (Match match in matches)
        {
            int sp = match.Index;
            int ep = match.Index + match.Length - 1;

            if (tp.Col >= sp && tp.Col <= ep)
            {
                Sel.SP.Row = tp.Row;
                Sel.SP.Col = sp;
                Sel.EP.Row = tp.Row;
                Sel.EP.Col = ep;
                break;
            }
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (mAutoScroller != null)
        {
            mAutoScroller.End();
        }

        Point p = e.GetPosition(this);
        if (RawSel.IsEmpty())
        {
            if (p.X < TextLeftMargin)
            {
                TextPos tp = PointToTextPos(p);
                Sel.SP = tp;
                Sel.EP = tp;
                Sel.EP.Col = mList[tp.Row].Data.Length - 1;
                InvalidateVisual();
            }
        }

        Selecting = false;
        //DOut.pl($"Sel.IsValid:{Sel.IsValid}");
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (Selecting)
        {
            Point p = e.GetPosition(this);

            TextPos tp = PointToTextPos(p);

            RawSel.End(tp.Row, tp.Col);

            Sel = TextRange.Naormalized(RawSel);

            InvalidateVisual();

            //DOut.pl($"sr:{Sel.SP.Row} sc:{Sel.SP.Col} - er:{Sel.EP.Row} ec:{Sel.EP.Col}");
        }
    }

    protected TextPos PointToTextPos(Point p)
    {
        TextPos tp = new TextPos();

        int row = (int)(p.Y / mLineHeight);

        row = Math.Min(row, mList.Count - 1);

        if (row < 0)
        {
            row = 0;
        }

        int col = PointToTextCol(p.X - mTextLeftMargin, mList[row].Data, CW, CWF);

        tp.Row = row;
        tp.Col = col;

        return tp;
    }

    protected static int PointToTextCol(double x, string s, double cw, double cwf)
    {
        //return (int)(x / cw);

        int col = -1;

        double p = 0;

        int i = 0;
        for (; i < s.Length; i++)
        {
            char c = s[i];

            if (IsHankaku(c))
            {
                p += cw;
            }
            else
            {
                p += cwf;
            }

            if (p >= x)
            {
                col = i;
                break;
            }
        }

        if (col == -1)
        {
            col = s.Length - 1 + (int)((x - p) / cw);
        }

        return col;
    }

    protected static double TextColToPoint(int col, string s, double cw, double cwf)
    {
        //return (col + 1) * cw;

        double w = 0;

        if (col < 0)
        {
            return 0;
        }

        int endCol = s.Length - 1;

        int e = Math.Min(col, endCol);
        int i = 0;

        for (; i <= e; i++)
        {
            char c = s[i];

            if (IsHankaku(c))
            {
                w += cw;
            }
            else
            {
                w += cwf;
            }
        }

        if (col > endCol)
        {
            w += cw * (col - endCol);
        }

        return w;
    }

    protected static bool IsHankaku(char c)
    {
        if ((c <= '\u007e') || // 英数字
            (c == '\u00a5') || // \記号
            (c == '\u203e') || // ~記号
            (c >= '\uff61' && c <= '\uff9f') // 半角カナ
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void RecalcSize()
    {
        Height = mLineHeight * (double)(mList.Count);

        if (Scroll != null)
        {
            if (Height < Scroll.ActualHeight)
            {
                Height = Scroll.ActualHeight;
            }
        }
    }


    public void PrintLn(string s)
    {
        if (Dispatcher.CheckAccess())
        {
            Print(s);
            NewLine();
        }
        else
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Print(s);
                    NewLine();
                }));
            }
            catch { }
        }
    }

    public void Print(string s)
    {
        if (Dispatcher.CheckAccess())
        {
            PrintString(s);
        }
        else
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    PrintString(s);
                }));
            }
            catch { }
        }
    }

    private void PrintString(string s)
    {
        string[] lines = s.Split('\n');

        int i = 0;
        for (; i < lines.Length - 1; i++)
        {
            AppendString(lines[i]);
            NewLine();
        }

        AppendString(lines[i]);
        UpdateView();
    }

    private void NewLine()
    {
        int prevCnt = mList.Count;

        CurrentAttr = DefaultAttr;

        var line = new TextLine(DefaultAttr);
        mList.Add(line);

        //while (mList.Count > mMaxLine)
        //{
        //    mList.RemoveAt(0);
        //}

        if (prevCnt != mList.Count)
        {
            RecalcSize();
        }

        ScrollToEnd();
    }

    private void AppendString(string s)
    {
        int idx = mList.Count - 1;

        TextLine line;

        line = mList[idx];

        line.Parse(s);
    }

    public void PrintF(string format, params object[] args)
    {
        string s = string.Format(format, args);
        Print(s);
    }

    public void Clear()
    {
        if (Dispatcher.CheckAccess())
        {
            HandleClear();
        }
        else
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    HandleClear();
                }));
            }
            catch { }
        }
    }

    private void HandleClear()
    {
        mList.Clear();
        //RecalcSize();
        NewLine();
        UpdateView();
    }

    public string GetSelectedString()
    {
        string s = "";

        if (!Sel.IsValid)
        {
            return s;
        }

        TextSpan tr;

        int i = Sel.SP.Row;

        int end = Sel.EP.Row;

        for (; i <= end; i++)
        {
            TextLine item = mList[i];
            int strLen = item.Data.Length;

            tr = Sel.GetRowSpan(i, strLen);

            if (tr.Len > 0)
            {
                int len = Math.Min(strLen - tr.Start, tr.Len);
                s += mList[i].Data.Substring(tr.Start, len);
            }

            if (i < end)
            {
                s += "\n";
            }
        }

        return s;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double offset = 0;
        double dispHeight = ActualHeight;

        if (Scroll != null)
        {
            offset = Scroll.VerticalOffset;
            dispHeight = Scroll.ActualHeight;
        }

        Point p = default;
        Rect rect = default;

        long topNumber = (long)offset / (long)mLineHeight;

        double textOffset = (mLineHeight - mFontSize) / 2.0 - 3;

        p.X = 0;
        p.Y = mLineHeight * topNumber;

        Point tp;

        rect.X = 0;
        rect.Y = p.Y;
        rect.Width = ActualWidth;
        rect.Height = mLineHeight + 0.5;

        int n = (int)topNumber;

        double rangeY = offset + dispHeight;

        //DOut.pl($"sr:{Sel.SP.Row} sc:{Sel.SP.Col} - er:{Sel.EP.Row} ec:{Sel.EP.Col}");

        while (p.Y < rangeY)
        {
            if (n >= mList.Count)
            {
                break;
            }

            TextLine item = mList[n];
            n++;

            rect.Y = p.Y;

            dc.DrawRectangle(mBackground, null, rect);

            tp = p;

            tp.X = mTextLeftMargin;
            tp.Y += textOffset;

            DrawText(dc, item, tp, n - 1);

            DrawSelectedRange(dc, n - 1);

            p.Y += mLineHeight;
        }

        if (p.Y < rangeY)
        {
            Rect sr = new Rect(0, p.Y, ActualWidth, rangeY - p.Y);
            dc.DrawRectangle(mBackground, null, sr);
        }

        if (IsFocused)
        {
            Rect sr = new Rect(0, offset + 1, ActualWidth, dispHeight - 1);
            dc.DrawRectangle(null, FocusedBorderPen, sr);
        }
    }

    #region draw text

    protected void DrawText(DrawingContext dc, TextLine line, Point pt, int row)
    {
        foreach (AttrSpan attr in line.Attrs)
        {
            string s = line.Data.Substring(attr.Start, attr.Len);
            pt = RenderText(dc, attr.Attr, s, pt, row);
        }
    }

    protected Point RenderText(
        DrawingContext dc, TextAttr attr, string s, Point pt, int row)
    {
        Brush foreground = Palette.Brushes[attr.FColor];

        FormattedText ft = GetFormattedText(s, foreground);

        Rect r = new Rect(pt.X, row * mLineHeight, ft.WidthIncludingTrailingWhitespace, mLineHeight);

        Brush background = Palette.Brushes[attr.BColor];

        dc.DrawRectangle(background, null, r);

        Point tpt = pt;

        tpt.Y = pt.Y + (mLineHeight - ft.Height) / 2;

        dc.DrawText(ft, tpt);
        pt.X += ft.WidthIncludingTrailingWhitespace;
        return pt;
    }

    protected void DrawSelectedRange(DrawingContext dc, int row)
    {
        if (Sel.IsValid && row >= Sel.SP.Row && row <= Sel.EP.Row)
        {
            Rect r = new Rect(mTextLeftMargin, row * mLineHeight - 0.5, 0, mLineHeight);

            TextSpan ts = Sel.GetRowSpan(row, mList[row].Data.Length);

            //DOut.pl($"row:{row} ts.Start:{ts.Start} ts.Len{ts.Len}");

            double sp = TextColToPoint(ts.Start - 1, mList[row].Data, CW, CWF);
            double ep = TextColToPoint(ts.Start + ts.Len - 1, mList[row].Data, CW, CWF);

            r.X = sp + mTextLeftMargin;
            r.Width = ep - sp;

            dc.PushOpacity(mSelectedBackgroundOpacity);
            dc.DrawRectangle(mSelectedBackground, null, r);
            dc.Pop();
        }
    }
    #endregion

    protected FormattedText GetFormattedText(string s, Brush brush)
    {
        FormattedText formattedText = new FormattedText(s,
                                                  System.Globalization.CultureInfo.CurrentCulture,
                                                  FlowDirection.LeftToRight,
                                                  mTypeface,
                                                  mFontSize,
                                                  brush,
                                                  VisualTreeHelper.GetDpi(this).PixelsPerDip);
        return formattedText;
    }

    public void ScrollToEnd()
    {
        if (Scroll == null)
        {
            return;
        }

        Scroll.ScrollToEnd();
    }

    private void UpdateView()
    {
        if (mIsLoaded)
        {
            InvalidateVisual();
        }
    }
}
