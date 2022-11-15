using TCad.ViewModel;
using Plotter;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TCad.Controls;

public interface ICadObjectTree
{
    event Action<CadObjTreeItem> CheckChanged;
    event Action<CadObjTreeItem, string> ItemCommand;

    void Update(bool remakeTree, bool filter, CadLayer layer);
    int FindIndex(uint id);
    void SetPos(int index);
}


public class CadObjectTreeView : FrameworkElement, ICadObjectTree
{
    static CadObjectTreeView()
    {
    }

    #region Event

    public event Action<CadObjTreeItem> CheckChanged;

    protected virtual void OnCheckChanged(CadObjTreeItem item)
    {
        if (CheckChanged != null)
        {
            CheckChanged(item);
        }
    }
    #endregion

    public Brush Background
    {
        get
        {
            return mBackground;
        }
        set
        {
            mBackground = value;
        }
    }

    public Brush Foreground
    {
        get
        {
            return mForeground;
        }
        set
        {
            mForeground = value;
        }
    }

    public Brush CheckedBackground
    {
        get
        {
            return mCheckedBackground;
        }
        set
        {
            mCheckedBackground = value;
        }
    }

    public Brush CheckedForeground
    {
        get
        {
            return mCheckedForeground;
        }
        set
        {
            mCheckedForeground = value;
        }
    }

    public double TextSize
    {
        get
        {
            return mTextSize;
        }

        set
        {
            mTextSize = value;
        }
    }

    public double ItemHeight
    {
        get
        {
            return mItemHeight;
        }

        set
        {
            mItemHeight = value;
        }
    }

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

    //protected FontFamily mFontFamily;

    protected Typeface mTypeface;

    protected Typeface mPartsTypeface;


    protected Brush mForeground = Brushes.Black;

    protected Brush mBackground = Brushes.White;

    protected Brush mCheckedForeground = Brushes.White;

    protected Brush mCheckedBackground = new SolidColorBrush(Color.FromRgb(0x22,0x8B,0x22));


    protected double mItemHeight = 20.0;

    protected double mTextSize = 16.0;

    protected double mIndentSize = 12.0;

    protected double mSmallIndentSize = 4.0;

    protected ContextMenu mContextMenu;

    public event Action<CadObjTreeItem, string> ItemCommand; 

    FormattedText mExpand;

    FormattedText mContract;

    public CadObjectTreeView()
    {
        FontFamily font;

        mContextMenu = new ContextMenu();
        mContextMenu.BorderBrush = Brushes.Black;
        mContextMenu.Padding = new Thickness(0, 1, 0, 1);

        //font = new FontFamily("ＭＳ ゴシック");
        font = new FontFamily("Consolas");
        mTypeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        font = new FontFamily("Marlett");   // WindowsのCloseボタン等の部品がFontになったもの
        mPartsTypeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

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
        
        int idx = (int)(p.Y / mItemHeight);

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
            mRoot.ForEachAll((v)=>{
                v.IsChecked = false;
            });

            item.IsChecked = true;
            OnCheckChanged(item);

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
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                mRoot.ForEachAll((v) => {
                    v.IsChecked = false;
                });
            }

            int level = item.GetLevel();

            if (item.Children != null)
            {
                if (p.X > (level) * mIndentSize)
                {
                    item.IsChecked = item.IsChecked == false;
                    OnCheckChanged(item);
                }
                else
                {
                    item.IsExpand = item.IsExpand == false;
                    RecalcSize();
                }
            }
            else
            {
                item.IsChecked = item.IsChecked == false;
                OnCheckChanged(item);
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
            Scroll.ScrollToVerticalOffset(pos * mItemHeight);
        }
        else
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Scroll.ScrollToVerticalOffset(pos * mItemHeight);
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

        if (!ShowRoot && idx>=0)
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
            return;
        }

        double textOffset = (mItemHeight - mTextSize) / 2.0;

        Point p = default(Point);
        Point tp = default(Point);
        Rect rect = default(Rect);

        Point mp = default(Point);

        long topNumber = (long)offset / (long)mItemHeight;

        if (!ShowRoot)
        {
            topNumber++;
        }

        p.X = 0;
        p.Y = mItemHeight * topNumber;

        if (ShowRoot)
        {
            p.Y = mItemHeight * topNumber;
        }
        else
        {
            p.Y = mItemHeight * (topNumber - 1);
        }

        rect.X = 0;
        rect.Y = p.Y;
        rect.Width = ActualWidth;
        rect.Height = mItemHeight + 1;

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

            Brush fbrush = item.getForeColor();

            Brush bbrush = item.getBackColor();

            if (item.IsChecked)
            {
                fbrush = fbrush ?? mCheckedForeground;
                bbrush = bbrush ?? mCheckedBackground;
            }
            else
            {
                fbrush = fbrush ?? mForeground;
                bbrush = bbrush ?? mBackground;
            }

            dc.DrawRectangle(bbrush, null, rect);

            if (item.Children != null)
            {
                p.X = mIndentSize * (level - topLevel) + mIndentSize;
            }
            else
            {
                p.X = mIndentSize * (level - topLevel) + mSmallIndentSize;
            }

            tp = p;

            tp.Y += textOffset;

            if (item.Children != null)
            {
                mp = tp;
                mp.X -= mIndentSize;
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

            p.Y += mItemHeight;

            if (p.Y < rangeY)
            {
                return true;
            }

            return false;
        }, 0);

        if (p.Y < rangeY)
        {
            Rect sr = new Rect(0, p.Y, ActualWidth, rangeY - p.Y);
            dc.DrawRectangle(mBackground, null, sr);
        }
    }

    protected FormattedText GetText(string s, Brush brush)
    {
        FormattedText formattedText = new FormattedText(s,
                                                  System.Globalization.CultureInfo.CurrentCulture,
                                                  System.Windows.FlowDirection.LeftToRight,
                                                  mTypeface,
                                                  mTextSize,
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
        mExpand = GetText("4", mForeground, mPartsTypeface, mTextSize + 2);
        mContract = GetText("6", mForeground, mPartsTypeface, mTextSize + 2);
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
        Height = mItemHeight * (double)(tc + 2);
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
