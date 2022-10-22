using System;
using System.Windows.Controls.Primitives;

using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace TCad.Controls;

public class PopupEx : Popup
{
    public static readonly DependencyProperty IsTopmostProperty =
        DependencyProperty.Register(
            "IsTopmost", typeof(bool), typeof(PopupEx), new FrameworkPropertyMetadata(false, OnIsTopmostChanged));

    private bool? _appliedTopMost;
    private bool _alreadyLoaded;
    private Window _parentWindow;

    public bool IsTopmost
    {
        get { return (bool)GetValue(IsTopmostProperty); }
        set { SetValue(IsTopmostProperty, value); }
    }

    public PopupEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupEx), new FrameworkPropertyMetadata(typeof(PopupEx)));
        PopupEx.IsOpenProperty.OverrideMetadata(typeof(PopupEx), new FrameworkPropertyMetadata(IsOpenChanged));

        Loaded += OnPopupLoaded;
    }

    private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as PopupEx;
        if (ctrl == null)
            return;

        var target = ctrl.PlacementTarget;
        if (target == null)
            return;

        var win = Window.GetWindow(target);

        if (e.OldValue != null && (bool)e.OldValue == true)
        {
            if (win != null)
            {
                win.LocationChanged -= ctrl.OnFollowWindowChanged;
                win.SizeChanged -= ctrl.OnFollowWindowChanged;
            }
        }

        if (e.NewValue != null && (bool)e.NewValue == true)
        {
            if (win != null)
            {
                win.LocationChanged += ctrl.OnFollowWindowChanged;
                win.SizeChanged += ctrl.OnFollowWindowChanged;
            }
        }
    }

    private void OnFollowWindowChanged(object sender, EventArgs e)
    {
        var offset = this.HorizontalOffset;

        // HorizontalOffsetなどのプロパティを一度変更しないと、ポップアップの位置が更新されないため、
        // 位置に関するプロパティを変えて戻す
        this.HorizontalOffset = offset + 1;
        this.HorizontalOffset = offset;
    }


    void OnPopupLoaded(object sender, RoutedEventArgs e)
    {
        if (_alreadyLoaded)
            return;

        _alreadyLoaded = true;

        if (Child != null)
        {
            Child.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnChildPreviewMouseLeftButtonDown), true);
        }

        _parentWindow = Window.GetWindow(this);

        if (_parentWindow == null)
            return;

        _parentWindow.Activated += OnParentWindowActivated;
        _parentWindow.Deactivated += OnParentWindowDeactivated;
    }

    void OnParentWindowActivated(object sender, EventArgs e)
    {
        SetTopmostState(true);
    }

    void OnParentWindowDeactivated(object sender, EventArgs e)
    {
        if (IsTopmost == false)
        {
            SetTopmostState(IsTopmost);
        }
    }

    void OnChildPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SetTopmostState(true);

        if (!_parentWindow.IsActive && IsTopmost == false)
        {
            _parentWindow.Activate();
        }
    }

    private static void OnIsTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var thisobj = (PopupEx)obj;

        thisobj.SetTopmostState(thisobj.IsTopmost);
    }

    protected override void OnOpened(EventArgs e)
    {
        SetTopmostState(IsTopmost);
    }

    private void SetTopmostState(bool isTop)
    {
        if (_appliedTopMost.HasValue && _appliedTopMost == isTop)
        {
            return;
        }

        if (Child == null)
            return;

        var hwndSource = (PresentationSource.FromVisual(Child)) as HwndSource;

        if (hwndSource == null)
            return;
        var hwnd = hwndSource.Handle;

        WinAPI.RECT rect;

        if (!WinAPI.GetWindowRect(hwnd, out rect))
            return;

        Debug.WriteLine("setting z-order " + isTop);

        if (isTop)
        {
            WinAPI.SetWindowPos(
                hwnd, WinAPI.HWND_TOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, WinAPI.TOPMOST_FLAGS);
        }
        else
        {
            WinAPI.SetWindowPos(
                hwnd, WinAPI.HWND_BOTTOM, rect.Left, rect.Top, (int)Width, (int)Height, WinAPI.TOPMOST_FLAGS);
            WinAPI.SetWindowPos(
                hwnd, WinAPI.HWND_TOP, rect.Left, rect.Top, (int)Width, (int)Height, WinAPI.TOPMOST_FLAGS);
            WinAPI.SetWindowPos(
                hwnd, WinAPI.HWND_NOTOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, WinAPI.TOPMOST_FLAGS);
        }

        _appliedTopMost = isTop;
    }

}
