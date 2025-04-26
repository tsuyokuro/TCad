using TCad.Plotter;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TCad.ViewModel;

namespace TCad.Controls;

public interface ICadObjectTree
{
    event Action<CadObjTreeItem> StateChanged;
    event Action<CadObjTreeItem, string> ItemCommand;

    void Update(bool remakeTree, bool filter, CadLayer layer);
    int FindIndex(uint id);
    void SetPos(int index);
}


public class CadObjectTreeView : FrameworkElement, ICadObjectTree
{
    // Node BG
    public static readonly DependencyProperty BackgroundProp =
        DependencyProperty.Register("Background",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(Brushes.Black));
    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProp);
        set => SetValue(BackgroundProp, value);
    }


    // Node FG
    public static readonly DependencyProperty NodeFGProp =
        DependencyProperty.RegisterAttached("NodeFG",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(Brushes.White));
    public Brush NodeFG
    {
        get => (Brush)GetValue(NodeFGProp);
        set => SetValue(NodeFGProp, value);
    }


    //--------------------------------------------------------------------------------
    // Checked Node FG
    public static readonly DependencyProperty CheckedNodeFGProp =
        DependencyProperty.Register("CheckedNodeFG",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(Brushes.White));
    public Brush CheckedNodeFG
    {
        get => (Brush)GetValue(CheckedNodeFGProp);
        set => SetValue(CheckedNodeFGProp, value);
    }


    //--------------------------------------------------------------------------------
    // Checked Node BG
    public static readonly DependencyProperty CheckedNodeBGProp =
        DependencyProperty.Register("CheckedNodeBG",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x22, 0x8B, 0x22))));

    public Brush CheckedNodeBG
    {
        get => (Brush)GetValue(CheckedNodeBGProp);
        set => SetValue(CheckedNodeBGProp, value);
    }


    //--------------------------------------------------------------------------------
    // Leaf FG
    public static readonly DependencyProperty LeafFGProp =
        DependencyProperty.Register("LeafFG",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(Brushes.White));
    public Brush LeafFG
    {
        get => (Brush)GetValue(LeafFGProp);
        set => SetValue(LeafFGProp, value);
    }

    //--------------------------------------------------------------------------------
    // Checked Leaf FG
    public static readonly DependencyProperty CheckedLeafFGProp =
        DependencyProperty.Register("CheckedLeafFG",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(Brushes.White));
    public Brush CheckedLeafFG
    {
        get => (Brush)GetValue(CheckedLeafFGProp);
        set => SetValue(CheckedLeafFGProp, value);
    }


    //--------------------------------------------------------------------------------
    // Checked Leaf BG
    public static readonly DependencyProperty CheckedLeafBGProp =
        DependencyProperty.Register("CheckedLeafBG",
                                    typeof(Brush),
                                    typeof(CadObjectTreeView),
                                    new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x11, 0x46, 0x11))));

    public Brush CheckedLeafBG
    {
        get => (Brush)GetValue(CheckedLeafBGProp);
        set => SetValue(CheckedLeafBGProp, value);
    }



    static CadObjectTreeView()
    {
    }

    #region Event

    public event Action<CadObjTreeItem> StateChanged;

    protected virtual void NotifyStateChanged(CadObjTreeItem item)
    {
        if (StateChanged != null)
        {
            StateChanged(item);
        }
    }
    #endregion



    public double TextSize { get; set; } = 16.0;

    public double ItemHeight { get; set; } = 20.0;

    public double IndentSize { get; set; } = 12.0;

    public double SmallIndentSize { get; set; } = 4.0;


    public bool ShowRoot
    {
        get; set;
    } = false;


    protected CadObjTreeItem mRoot;

    public CadObjTreeItem Root
    {
        get { return mRoot; }
        set
        {
            AttachRoot(value);
        }
    }

    protected ScrollViewer Scroll;

    protected Typeface mTypeface;

    protected Typeface mPartsTypeface;

    protected ContextMenu mContextMenu;

    public event Action<CadObjTreeItem, string> ItemCommand;

    FormattedText mExpand;

    FormattedText mContract;

    public CadObjectTreeView()
    {
        mContextMenu = new ContextMenu();
        mContextMenu.BorderBrush = Brushes.Black;
        mContextMenu.Padding = new Thickness(0, 1, 0, 1);

        {
            FontFamily font;

            font = new FontFamily("Consolas");
            mTypeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            font = new FontFamily("Marlett");   // WindowsのCloseボタン等の部品がFontになったもの
            mPartsTypeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        }

        Loaded += CadObjectTree_Loaded;
        MouseDown += CadObjectTree_MouseDown;
    }

    protected void CadObjectTree_Loaded(object sender, RoutedEventArgs e)
    {
        FrameworkElement parent = (FrameworkElement)Parent;

        if (parent is ScrollViewer)
        {
            Scroll = (ScrollViewer)parent;
        }

        if (Scroll != null)
        {
            Scroll.ScrollChanged += Scroll_ScrollChanged;
        }

        CreateParts();
    }

    protected void CadObjectTree_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Point p = e.GetPosition(this);

        int idx = (int)(p.Y / ItemHeight);

        if (!ShowRoot)
        {
            idx++;
        }

        CadObjTreeItem item = mRoot.GetAt(idx);

        if (item == null)
        {
            return;
        }

        if (e.RightButton == MouseButtonState.Pressed)
        {
            mRoot.ForEachAll((v) =>
            {
                v.IsChecked = false;
            });

            item.IsChecked = true;
            NotifyStateChanged(item);

            if (ItemCommand != null)
            {
                mContextMenu.Items.Clear();

                List<MenuItem> list = item.GetContextMenuItems();

                if (list != null)
                {
                    foreach (MenuItem m in list)
                    {
                        SetupContextMenuItem(m);
                        mContextMenu.Items.Add(m);
                    }
                }

                if (mContextMenu.Items.Count > 0)
                {
                    ContextMenu = mContextMenu;
                }
                else
                {
                    ContextMenu = null;
                }

                base.OnMouseDown(e);
            }
        }
        else
        {
            int level = item.GetLevel();

            if (item.Children != null)
            {
                if (p.X > (level) * IndentSize)
                {
                    if (!CadKeyboard.IsCtrlKeyDown())
                    {
                        mRoot.ForEachAll((v) =>
                        {
                            v.IsChecked = false;
                        });
                    }

                    item.IsChecked = item.IsChecked == false;
                    NotifyStateChanged(item);
                }
                else
                {
                    item.IsExpand = item.IsExpand == false;
                    RecalcSize();
                }
            }
            else
            {
                if (!CadKeyboard.IsCtrlKeyDown())
                {
                    mRoot.ForEachAll((v) =>
                    {
                        v.IsChecked = false;
                    });
                }

                item.IsChecked = item.IsChecked == false;
                NotifyStateChanged(item);
            }
        }

        InvalidateVisual();
    }

    protected void ContextMenuClickHandler(object sender, RoutedEventArgs e)
    {
        MenuItem m = (MenuItem)sender;

        if (!(m.Tag is CadObjTreeItem.ContextMenuTag))
        {
            return;
        }

        CadObjTreeItem.ContextMenuTag tag = (CadObjTreeItem.ContextMenuTag)m.Tag;

        ItemCommand?.Invoke(tag.TreeItem, tag.Tag);
    }

    protected void SetupContextMenuItem(MenuItem menuItem)
    {
        menuItem.Foreground = Brushes.White;
        menuItem.BorderThickness = new Thickness(0, 0, 0, 0);

        menuItem.Click -= ContextMenuClickHandler;
        menuItem.Click += ContextMenuClickHandler;

        menuItem.MouseEnter += (sender, e) =>
        {
            menuItem.Foreground = Brushes.Black;
        };

        menuItem.MouseLeave += (sender, e) =>
        {
            menuItem.Foreground = Brushes.White;
        };
    }

    public void SetVPos(int pos)
    {
        if (Dispatcher.CheckAccess())
        {
            Scroll.ScrollToVerticalOffset(pos * ItemHeight);
        }
        else
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Scroll.ScrollToVerticalOffset(pos * ItemHeight);
            }));
        }
    }

    public int Find(Func<CadObjTreeItem, bool> match)
    {
        int idx = -1;
        int cnt = 0;

        mRoot.ForEach((item) =>
        {
            if (match(item))
            {
                idx = cnt;
                return false;
            }

            cnt++;
            return true;
        });

        if (!ShowRoot && idx >= 0)
        {
            idx--;
        }

        return idx;
    }

    protected void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        Size size = RenderSize;

        double offset = 0;

        double dispHeight = ActualHeight;

        if (Scroll != null)
        {
            offset = Scroll.VerticalOffset;
            dispHeight = Scroll.ActualHeight;
        }

        if (mRoot == null)
        {
            Rect sr = new Rect(0, 0, ActualWidth, dispHeight);
            dc.DrawRectangle(Background, null, sr);
            return;
        }

        double textOffset = (ItemHeight - TextSize) / 2.0;

        Point p = default(Point);
        Point tp = default(Point);
        Rect rect = default(Rect);

        Point mp = default(Point);

        long topNumber = (long)offset / (long)ItemHeight;

        if (!ShowRoot)
        {
            topNumber++;
        }

        p.X = 0;
        p.Y = ItemHeight * topNumber;

        if (ShowRoot)
        {
            p.Y = ItemHeight * topNumber;
        }
        else
        {
            p.Y = ItemHeight * (topNumber - 1);
        }

        rect.X = 0;
        rect.Y = p.Y;
        rect.Width = ActualWidth;
        rect.Height = ItemHeight + 1;

        long skip = topNumber;

        double rangeY = offset + dispHeight;

        int topLevel = 0;

        if (!ShowRoot)
        {
            topLevel = 1;
        }

        mRoot.ForEach((item, level) =>
        {
            skip--;
            if (skip >= 0)
            {
                return true;
            }

            FormattedText ft;

            rect.Y = p.Y;

            Brush fbrush;
            Brush bbrush = Background;


            if (item.Type == CadObjTreeItemType.NODE)
            {
                if (item.IsChecked)
                {
                    fbrush = CheckedNodeFG;
                    bbrush = CheckedNodeBG;
                }
                else
                {
                    fbrush = NodeFG;
                }
            }
            else
            {
                if (item.IsChecked)
                {
                    fbrush = CheckedLeafFG;
                    bbrush = CheckedLeafBG;
                }
                else
                {
                    fbrush = LeafFG;
                }
            }

            dc.DrawRectangle(bbrush, null, rect);

            if (item.Children != null)
            {
                p.X = IndentSize * (level - topLevel) + IndentSize;
            }
            else
            {
                p.X = IndentSize * (level - topLevel) + SmallIndentSize;
            }

            tp = p;

            tp.Y += textOffset;

            if (item.Children != null)
            {
                mp = tp;
                mp.X -= IndentSize;
                //mp.X += 4;

                if (item.IsExpand)
                {
                    dc.DrawText(mContract, mp);
                }
                else
                {
                    dc.DrawText(mExpand, mp);
                }
            }

            tp.X += 2;

            ft = GetText(item.Text, fbrush);
            dc.DrawText(ft, tp);

            p.Y += ItemHeight;

            if (p.Y < rangeY)
            {
                return true;
            }

            return false;
        }, 0);

        if (p.Y < rangeY)
        {
            Rect sr = new Rect(0, p.Y, ActualWidth, rangeY - p.Y);
            dc.DrawRectangle(Background, null, sr);
        }
    }

    protected FormattedText GetText(string s, Brush brush)
    {
        FormattedText formattedText = new FormattedText(s,
                                                  System.Globalization.CultureInfo.CurrentCulture,
                                                  System.Windows.FlowDirection.LeftToRight,
                                                  mTypeface,
                                                  TextSize,
                                                  brush,
                                                  VisualTreeHelper.GetDpi(this).PixelsPerDip);
        return formattedText;
    }

    protected FormattedText GetText(string s, Brush brush, Typeface typeFace, double size)
    {
        FormattedText formattedText = new FormattedText(s,
                                                  System.Globalization.CultureInfo.CurrentCulture,
                                                  System.Windows.FlowDirection.LeftToRight,
                                                  typeFace,
                                                  size,
                                                  brush,
                                                  VisualTreeHelper.GetDpi(this).PixelsPerDip);
        return formattedText;
    }

    protected void CreateParts()
    {
        mExpand = GetText("4", NodeFG, mPartsTypeface, TextSize + 2);
        mContract = GetText("6", NodeFG, mPartsTypeface, TextSize + 2);
    }

    public void AttachRoot(CadObjTreeItem root)
    {
        mRoot = root;

        mRoot.IsExpand = true;

        RecalcSize();
    }

    private void RecalcSize()
    {
        int tc = mRoot.GetTotalCount();
        Height = ItemHeight * (double)(tc + 2);
    }

    public void Redraw()
    {
        InvalidateVisual();
    }

    #region ICadObjectTree implements
    public void Update(bool remakeTree, bool filter, CadLayer layer)
    {
        if (filter)
        {
            CadLayerTreeItem item = new CadLayerTreeItem();
            item.AddChildren(layer, fig =>
            {
                return fig.HasSelectedPointInclueChild();
            });

            AttachRoot(item);
            Redraw();
        }
        else
        {
            if (remakeTree)
            {
                CadLayerTreeItem item = new CadLayerTreeItem(layer);
                AttachRoot(item);
                Redraw();
            }
            else
            {
                Redraw();
            }
        }
    }

    public int FindIndex(uint id)
    {
        int idx = Find((item) =>
        {
            if (item is CadFigTreeItem)
            {
                CadFigTreeItem figItem = (CadFigTreeItem)item;

                if (figItem.Fig.ID == id)
                {
                    return true;
                }
            }

            return false;
        });

        return idx;
    }

    public void SetPos(int index)
    {
        SetVPos(index);
    }

    #endregion
}
