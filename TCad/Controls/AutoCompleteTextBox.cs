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

namespace TCad.Controls
{
    public class AutoCompleteTextBox : TextBox
    {
        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        public class TextEventArgs : EventArgs
        {
            public string Text;
        }

        private Popup mCandidatePopup = new Popup();

        private Border mCandidateBorder = new Border();

        private ListBox mCandidateListBox = new ListBox();

        private ScrollViewer mCandidateScrollViewer = new ScrollViewer();


        public Style CandidateListBorderStyle
        {
            get => mCandidateBorder.Style;
            set => mCandidateBorder.Style = value;
        }

        public Style CandidateListScrollViewerStyle
        {
            get => mCandidateScrollViewer.Style;
            set => mCandidateScrollViewer.Style = value;
        }

        public Style CandidateListBoxStyle
        {
            get => mCandidateListBox.Style;
            set => mCandidateListBox.Style = value;
        }

        public Style CandidateListItemContainerStyle
        {
            get => mCandidateListBox.ItemContainerStyle;
            set => mCandidateListBox.ItemContainerStyle = value;
        }

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

        public bool IsDropDownOpen
        {
            get
            {
                if (mCandidatePopup == null)
                {
                    return false;
                }

                return mCandidatePopup.IsOpen;
            }
        }

        public int CandidateWordMin
        {
            get;
            set;
        } = 2;

        public event Action<object, TextEventArgs> Determine;

        public AutoCompleteTextBox()
        {
            mCandidatePopup.MaxHeight = 200;
            mCandidatePopup.Child = mCandidateBorder;
            mCandidateBorder.Child = mCandidateScrollViewer;
            mCandidateScrollViewer.Content = mCandidateListBox;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            mCandidateBorder.Background = mCandidateScrollViewer.Background;

            mCandidateListBox.MouseUp += CandidateListBox_MouseUp;
            mCandidateListBox.PreviewKeyDown += CandidateListBox_PreviewKeyDown;

            mCandidateListBox.PreviewLostKeyboardFocus += CandidateListBox_PreviewLostKeyboardFocus;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Application.Current.MainWindow.Deactivated += MainWindow_Deactivated;
            }
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            ClosePopup();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (mCandidatePopup.IsOpen)
            {
                if (e.Key == Key.Down)
                {
                    mCandidateListBox.Focus();
                }
                else if (e.Key == Key.Up)
                {
                    mCandidateListBox.Focus();
                }
                else if (e.Key == Key.Escape)
                {
                    mCandidatePopup.IsOpen = false;
                }
                else if (e.Key == Key.Enter)
                {
                    NotifyDetermine();
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    NotifyDetermine();
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

        private void NotifyDetermine()
        {
            TextEventArgs ea = new TextEventArgs();

            string v = Text.Trim('\r', '\n', ' ', '\t');

            if (v.Length == 0)
            {
                return;
            }

            ea.Text = v;

            History.Add(Text);

            Determine(this, ea);
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

            foreach (ListBoxItem item in mCandidateListBox.Items)
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
                else if (e.Key == Key.Escape)
                {
                    ClosePopup();
                }
            }
        }

        private void ClosePopup()
        {
            mCandidatePopup.IsOpen = false;
            Focus();
            CaretIndex = Text.Length;
        }

        private bool SetTextFromListBox()
        {
            if (mCandidateListBox.SelectedItem == null)
            {
                return false;
            }

            ListBoxItem item = (ListBoxItem)(mCandidateListBox.SelectedItem);

            string s = (string)(item.Content);

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

            mCandidateListBox.Items.Clear();

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
                MatchCollection mca = WordPattern.Matches(a);
                MatchCollection mcb = WordPattern.Matches(a);

                return mca[0].Length - mcb[0].Length;
            });

            foreach (var str in tempList)
            {
                ListBoxItem item = new ListBoxItem();

                item.Content = str;

                mCandidateListBox.Items.Add(item);
            }

            return mCandidateListBox.Items.Count > 0;
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
}
