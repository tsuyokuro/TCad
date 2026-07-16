//#define USE_FORMATTED_TEXT

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TCad.Logger;
using TCad.Util;

namespace TCad.Controls.CadConsole;

public partial class CadConsoleView : FrameworkElement
{
    public Brush Background
    {
        get
        {
            return Palette.Brushes[Palette.DefaultBColor];
        }
        set
        {
            Palette.Brushes[Palette.DefaultBColor] = value;
        }
    }

    public Brush Foreground
    {
        get
        {
            return Palette.Brushes[Palette.DefaultFColor];
        }
        set
        {
            Palette.Brushes[Palette.DefaultFColor] = value;
        }
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

    public string Colors
    {
        get => Palette.ToStr();
        set
        {
            Palette.FromStr(value);
            UpdateView();
        }
    }

    protected Brush mSelectedBackground = new SolidColorBrush(Color.FromArgb(255, 68, 141, 214));
    public Brush SelectedBackground
    {
        get => mSelectedBackground;
        set => mSelectedBackground = value;
    }

    protected double mSelectedBackgroundOpacity = 0.8;
    public double SelectedBackgroundOpacity
    {
        get => mSelectedBackgroundOpacity;
        set => mSelectedBackgroundOpacity = value;
    }

    protected double mLineHeight = 1;
    protected double LineHeight
    {
        get => mLineHeight;
        set => mLineHeight = value;
    }

    protected FontFamily mFontFamily = null;
    public FontFamily FontFamily
    {
        get => mFontFamily;
        set
        {
            mFontFamily = value;
            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            if (!mTypeface.TryGetGlyphTypeface(out mGlyphTypeface))
            {
                mGlyphTypeface = null;
            }

            RecalcMetrics();
        }
    }

    protected Typeface mTypeface;
    protected Typeface Typeface
    {
        get => mTypeface;
    }

    protected GlyphTypeface mGlyphTypeface;
    protected GlyphTypeface GlyphTypeface
    {
        get => mGlyphTypeface;
    }

    protected double mFontSize = 10.0;
    public double FontSize
    {
        get => mFontSize;
        set
        {
            mFontSize = value;
            RecalcMetrics();
        }
    }

    public int MaxLine
    {
        get => mList.BufferSize;
        set
        {
            if (value <= 0)
            {
                value = 1;
            }

            RawSel.Reset();
            Sel.Reset();

            mList.CreateBuffer(value);
        }
    }


    protected int mTopIndex = 0;

    protected bool mIsLoaded = false;

    protected ScrollViewer mScrollViewer;

    protected FastRingBuffer<TextLine> mList = new();

    protected AnsiPalette Palette = new();

    protected TextAttr DefaultAttr = default;

    protected TextAttr CurrentAttr = default;

    protected Pen FocusedBorderPen = new(
            new SolidColorBrush(Color.FromArgb(0xff, 0x56, 0x9D, 0xE5)), 1.5);

    protected double CW = 1;

    protected double CWF = 2;

    protected double CH = 1;

    private TextRange RawSel = new();
    private TextRange Sel = new();
    private bool Selecting = false;

    private AutoScroller mAutoScroller;

    public CadConsoleView()
    {
        Focusable = true;

        Loaded += CadConsoleView_Loaded;

        GotFocus += CadConsoleView_GotFocus;
        LostFocus += CadConsoleView_LostFocus;

        SizeChanged += CadConsoleView_SizeChanged;

        RawSel.Reset();
        Sel.Reset();
    }

    public bool ProcessKeyEvent(KeyEventArgs e)
    {
        if (!e.IsUp)
        {
            return false;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.C || e.Key == Key.Insert)
            {
                CopySelected(this, null);
                return true;
            }
            else if (e.Key == Key.X)
            {
                Clear();
                return true;
            }
        }

        return false;
    }

    public void ScrollToEnd()
    {
        if (mScrollViewer == null)
        {
            return;
        }

        mScrollViewer.ScrollToEnd();
    }

    private void CadConsoleView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SizeChanged -= CadConsoleView_SizeChanged;
        RecalcSize();
        SizeChanged += CadConsoleView_SizeChanged;
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

        if (mList.BufferSize == 0)
        {
            mList.CreateBuffer(200);
        }

        if (FontFamily == null)
        {
            FontFamily = new FontFamily("MS Gothic");
            //Uri uri = new("pack://application:,,,/Fonts/");
            //FontFamily = new FontFamily(uri, "./mplus-1m-light.ttf#M+ 1m light");
        }

        FrameworkElement parent = (FrameworkElement)Parent;

        if (parent is ScrollViewer)
        {
            mScrollViewer = (ScrollViewer)parent;

            // XAMLで VerticalScrollBarButtonHeight を設定していると作用しない
            // Resources\ScrollBarStyle.xaml
            // CustomScrollBarStyleで設定している
            //mScrollViewer.Resources.Add(SystemParameters.VerticalScrollBarButtonHeightKey, 32.0);
        }

        if (mScrollViewer != null)
        {
            mScrollViewer.ScrollChanged += Scroll_ScrollChanged;
        }

        mAutoScroller = new(this);
        mAutoScroller.Scroll = AutoScrollEvent;

        DefaultAttr.FColor = Palette.DefaultFColor;
        DefaultAttr.BColor = Palette.DefaultBColor;

        CurrentAttr = DefaultAttr;

        RecalcMetrics();
    }

    private void CadConsoleView_Loaded(object sender, RoutedEventArgs e)
    {
        Log.plx("");
        mIsLoaded = true;

        NewLine();

        UpdateView();

        SetContextMenu();
    }

    private void AutoScrollEvent(double dx, double dy)
    {
        mScrollViewer.ScrollToVerticalOffset(mScrollViewer.VerticalOffset + dy);
    }


#if USE_FORMATTED_TEXT
    private void RecalcMetrics()
    {
        if (Typeface == null)
        {
            return;
        }

        FormattedText ft = GetFormattedText("A", Foreground);

        if (ft != null)
        {
            CW = ft.Width;
            CH = ft.Height;

            FormattedText ftk = GetFormattedText("漢", Foreground);
            if (ftk != null)
            {
                CWF = ftk.Width;
            }
            else
            {
                CWF = CW * 2;
            }

            LineHeight = CH;
        }
        else
        {
            CW = 1;
            CH = 1;
            CWF = 1;
            LineHeight = 1;
        }
    }
#else
    private void RecalcMetrics()
    {
        if (Typeface == null)
        {
            return;
        }

        double w = 0;
        double h = 0;

        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;


        ushort i = 0;
        if (mGlyphTypeface.CharacterToGlyphMap.TryGetValue('A', out i))
        {
            w = mGlyphTypeface.AdvanceWidths[i] * FontSize;
            h = (mGlyphTypeface.Height) * FontSize;
        }

        if (w != 0 && h != 0)
        {
            CW = w;
            CH = h;

            w = 0;

            if (mGlyphTypeface.CharacterToGlyphMap.TryGetValue('漢', out i))
            {
                w = mGlyphTypeface.AdvanceWidths[i] * FontSize;
            }

            if (w != 0)
            {
                CWF = w;
            }
            else
            {
                CWF = CW * 2;
            }

            LineHeight = CH;
        }
        else
        {
            CW = 1;
            CH = 1;
            CWF = 1;
            LineHeight = 1;
        }
    }
#endif

    private void CopySelected(object obj, RoutedEventArgs args)
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
        ContextMenu = new();

        ContextMenu.BorderBrush = Brushes.Black;
        ContextMenu.Padding = new Thickness(0, 1, 0, 1);

        MenuItem menuItem = new();

        menuItem.Header = CadConsoleRes.menu_copy;
        menuItem.Click += CopySelected;

        SetupMenuItem(menuItem);

        ContextMenu.Items.Add(menuItem);
    }

    private static void SetupMenuItem(MenuItem menuItem)
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

        mAutoScroller?.Start();

        base.OnMouseDown(e);
    }

    protected void StartSelect(Point p)
    {
        TextPos tp = PointToTextPos(p);

        Sel.Reset();

        RawSel.Start(tp.Row, tp.Col);
        Selecting = true;
    }

    public Regex WordRegex {
        get;
        set;
    } = new(@"([^ \t,:=/\\]+)");

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
        mAutoScroller?.End();

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

            Sel = TextRange.Normalized(RawSel);

            InvalidateVisual();

            //DOut.pl($"sr:{Sel.SP.Row} sc:{Sel.SP.Col} - er:{Sel.EP.Row} ec:{Sel.EP.Col}");
        }
    }

    protected TextPos PointToTextPos(Point p)
    {
        TextPos tp = new();

        int row = (int)(p.Y / LineHeight);

        row = Math.Min(row, mList.Count - 1);

        if (row < 0)
        {
            row = 0;
        }

        int col = PointToTextCol(p.X - TextLeftMargin, mList[row].Data, CW, CWF);

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
        if (c <= '\u007e' || // 英数字
            c == '\u00a5' || // \記号
            c == '\u203e' || // ~記号
            c >= '\uff61' && c <= '\uff9f' // 半角カナ
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
        Height = LineHeight * mList.Count;

        if (mScrollViewer != null)
        {
            if (Height < mScrollViewer.ActualHeight)
            {
                Height = mScrollViewer.ActualHeight;
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

        line.Parse(s, DefaultAttr);
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
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        double scrollOffset = 0;
        double dispHeight = ActualHeight;

        if (mScrollViewer != null)
        {
            scrollOffset = mScrollViewer.VerticalOffset;
            dispHeight = mScrollViewer.ActualHeight;
        }

        Point p = default;
        Rect rect = default;

        long topNumber = (long)(scrollOffset / LineHeight);

        double textOffset = 0;

        p.X = 0;
        p.Y = LineHeight * topNumber;

        Point tp;

        rect.X = 0;
        rect.Y = p.Y;
        rect.Width = ActualWidth;
        rect.Height = LineHeight;

        int n = (int)topNumber;

        double rangeY = scrollOffset + dispHeight;

        while (p.Y < rangeY)
        {
            if (n >= mList.Count)
            {
                break;
            }

            TextLine item = mList[n];
            n++;

            rect.Y = p.Y;

            dc.DrawRectangle(Background, null, rect);

            tp = p;

            tp.X = TextLeftMargin;
            tp.Y += textOffset;

            DrawText(dc, item, tp, n - 1);

            //DrawSelectedRange(dc, n - 1);

            p.Y += LineHeight;
        }

        if (p.Y < rangeY)
        {
            Rect sr = new(0, p.Y, ActualWidth, rangeY - p.Y);
            dc.DrawRectangle(Background, null, sr);
        }

        if (IsFocused)
        {
            Rect sr = new(0, scrollOffset + 1, ActualWidth, dispHeight - 1);
            dc.DrawRectangle(null, FocusedBorderPen, sr);
        }
    }


    protected void DrawText(DrawingContext dc, TextLine line, Point pt, int row)
    {
        TextSpan rowSpan = Sel.GetRowSpan(row);

        bool inRange = rowSpan.Len > 0;
        int selS = rowSpan.Start;
        int selE = rowSpan.Start + (rowSpan.Len - 1);

        //Log.pl($"row:{row} inRange:{inRange} sels:{selS} sele:{selE}");

        foreach (AttrSpan attr in line.Attrs)
        {
            int ps = attr.Start;
            int pe = ps + attr.Len - 1;

            TextAttr selTextAttr = new(attr.Attr.BColor, attr.Attr.FColor);

            bool notSel = !inRange || ps > selE || pe < selS;


            if (notSel)
            {
                string s = line.Data.Substring(ps, pe - ps + 1);
                pt = RenderText(dc, attr.Attr, s, pt, row);
                continue;
            }


            if (ps >= selS && pe <= selE)
            {
                string s = line.Data.Substring(ps, pe - ps + 1);
                pt = RenderText(dc, selTextAttr, s, pt, row);
            }

            else if (ps >= selS && pe > selE)
            {
                string s = line.Data.Substring(ps, selE - ps + 1);
                pt = RenderText(dc, selTextAttr, s, pt, row);


                s = line.Data.Substring(selE + 1, pe - selE);
                pt = RenderText(dc, attr.Attr, s, pt, row);
            }
            else if (ps < selS && pe <= selE)
            {
                string s = line.Data.Substring(ps, selS - ps);
                pt = RenderText(dc, attr.Attr, s, pt, row);


                s = line.Data.Substring(selS, pe - selS + 1);
                pt = RenderText(dc, selTextAttr, s, pt, row);
            }
            else if (ps < selS && pe > selE)
            {
                string s = line.Data.Substring(ps, selS - ps);
                pt = RenderText(dc, attr.Attr, s, pt, row);


                s = line.Data.Substring(selS, selE - selS + 1);
                pt = RenderText(dc, selTextAttr, s, pt, row);


                s = line.Data.Substring(selE + 1, pe - selE);
                pt = RenderText(dc, attr.Attr, s, pt, row);

            }
        }

        //foreach (AttrSpan attr in line.Attrs)
        //{
        //    string s = line.Data.Substring(attr.Start, attr.Len);
        //    pt = RenderText(dc, attr.Attr, s, pt, row);
        //}
    }

#if USE_FORMATTED_TEXT
    protected Point RenderText(
        DrawingContext dc, TextAttr attr, string s, Point pt, int row)
    {
        Brush foreground = Palette.Brushes[attr.FColor];

        FormattedText ft = GetFormattedText(s, foreground);

        Rect r = new(pt.X, row * LineHeight, ft.WidthIncludingTrailingWhitespace, LineHeight);

        Brush background = Palette.Brushes[attr.BColor];

        dc.DrawRectangle(background, null, r);

        Point tpt = pt;

        tpt.Y = pt.Y + (LineHeight - ft.Height) / 2;

        dc.DrawText(ft, tpt);
        pt.X += ft.WidthIncludingTrailingWhitespace;
        return pt;
    }
#else
    protected Point RenderText(
        DrawingContext dc, TextAttr attr, string s, Point pt, int row)
    {
        Brush foreground = Palette.Brushes[attr.FColor];

        double textHeight = mGlyphTypeface.Height * FontSize;
        double baseline = mGlyphTypeface.Baseline * FontSize;
        double originY = ((LineHeight - textHeight) / 2.0) + baseline;

        Point tpt = pt;

        tpt.Y = pt.Y + originY;

        (GlyphRun glyphRun, double width) = GetGlyphRun(mGlyphTypeface, FontSize, this, s, tpt);


        Rect r = new(pt.X, row * LineHeight, width, LineHeight);

        Brush background = Palette.Brushes[attr.BColor];

        dc.DrawRectangle(background, null, r);

        if (glyphRun != null)
        {
            dc.DrawGlyphRun(foreground, glyphRun);
        }

        pt.X += width;
        return pt;
    }
#endif

    protected void DrawSelectedRange(DrawingContext dc, int row)
    {
        if (Sel.IsValid && row >= Sel.SP.Row && row <= Sel.EP.Row)
        {
            Rect r = new(TextLeftMargin, row * LineHeight, 0, LineHeight);

            TextSpan ts = Sel.GetRowSpan(row, mList[row].Data.Length);

            //DOut.pl($"row:{row} ts.Start:{ts.Start} ts.Len{ts.Len}");

            double sp = TextColToPoint(ts.Start - 1, mList[row].Data, CW, CWF);
            double ep = TextColToPoint(ts.Start + ts.Len - 1, mList[row].Data, CW, CWF);

            r.X = sp + TextLeftMargin;
            r.Width = ep - sp;

            dc.PushOpacity(SelectedBackgroundOpacity);
            dc.DrawRectangle(SelectedBackground, null, r);
            dc.Pop();
        }
    }

    protected FormattedText GetFormattedText(string s, Brush brush)
    {
        FormattedText formattedText = new(s,
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        Typeface,
                                        FontSize,
                                        brush,
                                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
        return formattedText;
    }


    protected void UpdateView()
    {
        if (mIsLoaded)
        {
            InvalidateVisual();
        }
    }

    protected (GlyphRun glyphRun, double width) GetGlyphRun(
            GlyphTypeface typeface,
            double fontSize,
            Visual visual,
            string s,
            Point p)
    {
        if (s.Length == 0)
        {
            return (null, 0);
        }

        double totalWidth = 0;

        float pixelsPerDip = (float)VisualTreeHelper.GetDpi(visual).PixelsPerDip;
        var glyphIndices = new List<ushort>();
        var advanceWidths = new List<double>();
        foreach (char ch in s)
        {
            ushort glyphIndex = typeface.CharacterToGlyphMap[ch];
            glyphIndices.Add(glyphIndex);
            double width = typeface.AdvanceWidths[glyphIndex] * fontSize;
            advanceWidths.Add(width);
            totalWidth += width;
        }


        GlyphRun gr = new(
            typeface, 0, false, fontSize,
            pixelsPerDip,
            glyphIndices,
            p,
            advanceWidths,
            null, null, null, null, null, null
        );

        return (gr, totalWidth);

    }
}
