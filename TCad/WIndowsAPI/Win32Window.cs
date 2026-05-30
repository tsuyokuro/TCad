using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using TCad.WindowsAPI;

namespace TCad.WIndowsAPI;

delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

public class Win32Window
{
    /*
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASSEXW
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public int style;

        public IntPtr lpfnWndProc;

        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;

        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszMenuName;

        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszClassName;

        public IntPtr hIconSm;
    }

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassEx")]
    public static extern System.UInt16 RegisterClassEx([In] ref WNDCLASSEXW lpWndClass);
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEXW
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;

        public IntPtr hIconSm;
    }

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassExW")]
    public static extern ushort RegisterClassEx([In] ref WNDCLASSEXW lpWndClass);


    public const string ClassName = "myClass";

    protected IntPtr hWnd_;
    public IntPtr hWnd
    {
        get => hWnd_;
    }

    protected uint LastError_;
    public uint LastError
    {
        get => LastError_;
    }

    private static ushort WndClassRegisterResult = 0;

    private static WNDCLASSEXW WindowClass;

    private static readonly object lockObj = new();

    private static readonly Dictionary<IntPtr, Win32Window> HWndMap = [];

    private readonly WndProc delegWndProc = staticWndProc;

    public bool Create(string windowName)
    {
        lock (lockObj)
        {
            if (WndClassRegisterResult == 0)
            {
                WndClassRegisterResult = RegisterWindowClass();
            }

            if (WndClassRegisterResult == 0)
            {
                LastError_ = WinAPI.GetLastError();
                return false;
            }

            string wndClass = WindowClass.lpszClassName;

            hWnd_ = WinAPI.CreateWindowEx(
                0,
                wndClass,
                windowName,
                WinAPI.WS_OVERLAPPEDWINDOW /* | WinAPI.WS_VISIBLE */,
                0, 0, 300, 400,
                IntPtr.Zero,
                IntPtr.Zero,
                WindowClass.hInstance,
                IntPtr.Zero);

            if (hWnd_ == ((IntPtr)0))
            {
                LastError_ = WinAPI.GetLastError();
                return false;
            }

            HWndMap.Add(hWnd, this);

            return true;
        }
    }

    private ushort RegisterWindowClass()
    {
        WindowClass = new()
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
            style = (uint)(WinAPI.CS_HREDRAW | WinAPI.CS_VREDRAW),
            hbrBackground = WinAPI.GetStockObject(WinAPI.BLACK_BRUSH),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = Marshal.GetHINSTANCE(this.GetType().Module),
            hIcon = IntPtr.Zero,
            hCursor = WinAPI.LoadCursor(IntPtr.Zero, (int)WinAPI.IDC_CROSS),
            lpszMenuName = null,
            lpszClassName = ClassName,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(delegWndProc),
            hIconSm = IntPtr.Zero
        };

        return RegisterClassEx(ref WindowClass);
    }


    public void ShowWindow()
    {
        WinAPI.ShowWindow(hWnd_, 1);
    }

    public void UpdateWindow()
    {
        WinAPI.UpdateWindow(hWnd_);
    }

    public void StartMessageLoop()
    {
        MSG msg;
        while (WinAPI.GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
        {
            WinAPI.TranslateMessage(ref msg);
            WinAPI.DispatchMessage(ref msg);
        }
    }

    public void Dispose()
    {
        HWndMap.Remove(hWnd);
    }

    private static IntPtr staticWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        Win32Window window;
        if (HWndMap.TryGetValue(hWnd, out window))
        {
            return window.thisWndProc(hWnd, msg, wParam, lParam);
        }
        else
        {
            return WinAPI.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    private IntPtr thisWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WinAPI.WM_PAINT:
                break;

            case WinAPI.WM_CLOSE:
                WinAPI.DestroyWindow(hWnd);
                break;

            case WinAPI.WM_DESTROY:
                Dispose();
                WinAPI.PostQuitMessage(0);
                break;

            default:
                break;
        }

        return WinAPI.DefWindowProc(hWnd, msg, wParam, lParam);
    }
}

