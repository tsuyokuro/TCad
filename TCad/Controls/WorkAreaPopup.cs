using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using TCad.WindowsAPI;

namespace TCad.Controls;

public class WorkAreaPopup : Popup
{
    private bool? _appliedTopMost;
    private bool _alreadyLoaded;
    private Window _parentWindow;

    public WorkAreaPopup()
    {
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(WorkAreaPopup), new FrameworkPropertyMetadata(typeof(Popup)));
        }

        Loaded += OnPopupLoaded;
    }

    private void IsOpenChanged()
    {
        var target = PlacementTarget;
        if (target == null)
            return;

        var win = Window.GetWindow(target);

        if (IsOpen)
        {
            if (win != null)
            {
                win.LocationChanged += OnFollowWindowChanged;
                win.SizeChanged += OnFollowWindowChanged;
            }
        }
        else
        {
            if (win != null)
            {
                win.LocationChanged -= OnFollowWindowChanged;
                win.SizeChanged -= OnFollowWindowChanged;
            }
        }
    }

    private void OnFollowWindowChanged(object sender, EventArgs e)
    {
        var offset = this.HorizontalOffset;

        // HorizontalOffsetなどのプロパティを一度変更しないと、ポップアップの位置が更新されないため、
        // 位置に関するプロパティを変えて戻す
        this.HorizontalOffset = offset + 0.1;
        this.HorizontalOffset = offset;
    }

    void OnPopupLoaded(object sender, RoutedEventArgs e)
    {
        if (_alreadyLoaded)
            return;

        _alreadyLoaded = true;
        _parentWindow = Window.GetWindow(this);
    }

    protected override void OnOpened(EventArgs e)
    {
        IsOpenChanged();
        SetTopmostState(false);
    }

    protected override void OnClosed(EventArgs e)
    {
        IsOpenChanged();
    }

    private void SetTopmostState(bool isTop)
    {
        if (_appliedTopMost.HasValue && _appliedTopMost == isTop)
        {
            return;
        }

        if (Child == null) return;

        var hwndSource = (PresentationSource.FromVisual(Child)) as HwndSource;

        if (hwndSource == null) return;
        var hwnd = hwndSource.Handle;

        WinAPI.RECT rect;

        if (!WinAPI.GetWindowRect(hwnd, out rect)) return;

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
