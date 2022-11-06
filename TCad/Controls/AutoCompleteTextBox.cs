using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Text.RegularExpressions;
using Plotter;

namespace TCad.Controls;

public class CandidatePopup : Popup
{
    private Border mBorder = new Border();
    public Border Border
    {
        get => mBorder;
    }

    private ListBox mListBox = new ListBox();
    public ListBox ListBox
    {
        get => mListBox;
    }

    private ScrollViewer mScrollViewer = new ScrollViewer();
    public ScrollViewer ScrollViewer
    {
        get => mScrollViewer;
    }

    public ItemCollection Items
    {
        get => mListBox.Items;
    }

    public object SelectedItem
    {
        get => mListBox.SelectedItem;
    }

    public string SelectedItemText
    {
        get
        {
            if (mListBox.SelectedItem == null)
            {
                return null;
            }

            ListBoxItem item = (ListBoxItem)(mListBox.SelectedItem);

            return item.Content as string;
        }
    }

    public CandidatePopup()
    {
        Child = mBorder;
        mBorder.Child = mScrollViewer;
        mScrollViewer.Content = mListBox;

        StaysOpen = false;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Border.Background = ScrollViewer.Background;
    }
}


public class AutoCompleteTextBox : TextBox
{
    static AutoCompleteTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
    }

    private CandidatePopup mCandidatePopup = new CandidatePopup();

    public Style CandidateListBorderStyle
    {
        get => mCandidatePopup.Border.Style;
        set => mCandidatePopup.Border.Style = value;
    }

    public Style CandidateListScrollViewerStyle
    {
        get => mCandidatePopup.ScrollViewer.Style;
        set => mCandidatePopup.ScrollViewer.Style = value;
    }

    public Style CandidateListBoxStyle
    {
        get => mCandidatePopup.ListBox.Style;
        set => mCandidatePopup.ListBox.Style = value;
    }

    public Style CandidateListItemContainerStyle
    {
        get => mCandidatePopup.ListBox.ItemContainerStyle;
        set => mCandidatePopup.ListBox.ItemContainerStyle = value;
    }

    public int CandidateListItemFontSize
    {
        get;
        set;
    }

    public int CandidateListMaxRowCount
    {
        get;
        set;
    } = 0;


    private bool DisableCandidateList = false;

    public List<string> CandidateList
    {
        get;
        set;
    } = new List<string>();

    public TextHistory History
    {
        get;
        set;
    } = new TextHistory();

    public int CandidateWordMin
    {
        get;
        set;
    } = 2;

    public event Action<string> Determined;

    public AutoCompleteTextBox()
    {
    }

    public void Enter()
    {
        string v = Text.Trim('\r', '\n', ' ', '\t');

        if (v.Length == 0)
        {
            return;
        }

        History.Add(Text);

        Determined?.Invoke(Text);

        Text = "";
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        DOut.plx("in");

        mCandidatePopup.ListBox.MouseUp += CandidateListBox_MouseUp;
        mCandidatePopup.ListBox.PreviewKeyDown += CandidateListBox_PreviewKeyDown;

        mCandidatePopup.ListBox.PreviewLostKeyboardFocus += CandidateListBox_PreviewLostKeyboardFocus;

        Control c = mCandidatePopup.ListBox;
        if (CandidateListItemFontSize > 0)
        {
            c.FontSize = CandidateListItemFontSize;
        }
        else
        {
            c.FontSize = FontSize;
        }

        var formattedText = new FormattedText(
            "Ahj",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(c.FontFamily, c.FontStyle, c.FontWeight, c.FontStretch),
            c.FontSize,
            c.Foreground,
            VisualTreeHelper.GetDpi(c).PixelsPerDip
        );

        int rowCnt = CandidateListMaxRowCount;
        if (CandidateListMaxRowCount > 0)
        {
            mCandidatePopup.ScrollViewer.MaxHeight = ((int)formattedText.Height + 3) * rowCnt + 1;
        }

        DOut.plx("out");
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (mCandidatePopup.IsOpen)
        {
            if (e.Key == Key.Down)
            {
                mCandidatePopup.ListBox.Focus();
            }
            else if (e.Key == Key.Up)
            {
                mCandidatePopup.ListBox.Focus();
            }
            else if (e.Key == Key.Escape)
            {
                mCandidatePopup.IsOpen = false;
            }
            else if (e.Key == Key.Enter)
            {
                Enter();
            }
        }
        else
        {
            if (e.Key == Key.Enter)
            {
                Enter();
            }

            if (History != null)
            {
                if (e.Key == Key.Up)
                {
                    string s = History.Rewind();
                    DisableCandidateList = true;
                    Text = s;
                }
                else if (e.Key == Key.Down)
                {
                    string s = History.Forward();
                    DisableCandidateList = true;
                    Text = s;
                }
            }
        }
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (Check())
        {
            if (DisableCandidateList)
            {
                DisableCandidateList = false;
                return;
            }

            mCandidatePopup.PlacementTarget = this;
            mCandidatePopup.MinWidth = ActualWidth;
            mCandidatePopup.IsOpen = true;
        }
        else
        {
            mCandidatePopup.IsOpen = false;
        }
    }

    private void CandidateListBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!mCandidatePopup.IsOpen)
        {
            return;
        }

        bool focus = false;

        foreach (ListBoxItem item in mCandidatePopup.Items)
        {
            if (item.Equals(e.NewFocus))
            {
                focus = true;
            }
        }

        if (!focus)
        {
            ClosePopup();
        }
    }

    private void CandidateListBox_MouseUp(object sender, MouseButtonEventArgs e)
    {
        SetTextFromListBox();
        ClosePopup();
    }

    private void CandidateListBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (mCandidatePopup.IsOpen)
        {
            if (e.Key == Key.Enter)
            {
                SetTextFromListBox();
                ClosePopup();
            }
            else if (e.Key == Key.Left
                || e.Key == Key.Right
                || e.Key == Key.Back
                || e.Key == Key.Delete
                || (e.Key >= Key.A && e.Key <= Key.Z))
            {
                Focus();
            }
            else if (e.Key == Key.Escape)
            {
                ClosePopup();
            }
        }
    }

    public void CloseCandidatePopup()
    {
        mCandidatePopup.IsOpen = false;
    }

    private void ClosePopup()
    {
        mCandidatePopup.IsOpen = false;
        Focus();
        CaretIndex = Text.Length;
    }

    private bool SetTextFromListBox()
    {
        string s = mCandidatePopup.SelectedItemText;
        if (s == null)
        {
            return false;
        }

        string currentText = Text;

        currentText = currentText.Remove(mReplacePos, mReplaceLen);
        currentText = currentText.Insert(mReplacePos, s);

        Text = currentText;

        return true;
    }

    //Regex WordPattern = new Regex(@"[@a-zA-Z_0-9]+[\(]*");
    Regex WordPattern = new Regex(@"[@a-zA-Z_0-9]+");

    int mReplacePos = -1;

    int mReplaceLen = 0;

    private bool Check()
    {
        string currentText = Text;
        int cpos = CaretIndex;

        if (string.IsNullOrEmpty(currentText))
        {
            return false;
        }

        if (CandidateList == null)
        {
            return false;
        }

        string targetWord = null;

        MatchCollection mc = WordPattern.Matches(currentText);
        
        foreach(Match m in mc)
        {
            if (cpos >= m.Index && cpos <= m.Index + m.Length)
            {
                mReplacePos = m.Index;
                mReplaceLen = m.Length;
                targetWord = m.Value;

                break;
            }
        }

        //DOut.pl(targetWord);

        if (targetWord == null)
        {
            targetWord = currentText;
        }

        if (targetWord.Length < CandidateWordMin)
        {
            return false;
        }

        mCandidatePopup.Items.Clear();

        var tempList = new List<string>();

        foreach (string text in CandidateList)
        {
            if (text.IndexOf(targetWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
            //if (text.StartsWith(targetWord))
            {
                tempList.Add(text);
            }
        }

        tempList.Sort((a, b) =>
        {
            int idxA = a.IndexOf(targetWord, StringComparison.CurrentCultureIgnoreCase);
            int idxB = b.IndexOf(targetWord, StringComparison.CurrentCultureIgnoreCase);

            int r = idxA - idxB;

            if (r == 0)
            {
                return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
            }

            return r;
        });

        foreach (var str in tempList)
        {
            ListBoxItem item = new ListBoxItem();

            item.Content = str;

            mCandidatePopup.Items.Add(item);
        }

        return mCandidatePopup.Items.Count > 0;
    }


    public class TextHistory
    {
        private List<string> Data = new List<string>();
        int Pos = 0;

        private string empty = "";

        public void Add(string s)
        {
            Data.Add(s);
            Pos = Data.Count;
        }

        public string Rewind()
        {
            Pos--;

            if (Pos < 0)
            {
                Pos = 0;
                return empty;
            }

            return Data[Pos];
        }


        public string Forward()
        {
            Pos++;

            if (Pos >= Data.Count)
            {
                Pos = Data.Count;
                return empty;
            }

            return Data[Pos];
        }
    }
}
