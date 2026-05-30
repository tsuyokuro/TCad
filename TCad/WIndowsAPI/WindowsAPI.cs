using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace TCad.WindowsAPI;

partial class WinAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT Reserved;
        public POINT MaxSize;
        public POINT MaxPosition;
        public POINT MinTrackSize;
        public POINT MaxTrackSize;
    }


    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter, int X,
        int Y, int cx, int cy, uint uFlags);


    public static partial class Monitor
    {
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct MONITORINFO
        {
            public int Size;
            public RECT MonitorRect;
            public RECT WorkRect;
            public uint Flags;
        };

        public const UInt32 MONITOR_DEFAULTTONULL = 0x00000000;
        public const UInt32 MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const UInt32 MONITOR_DEFAULTTONEAREST = 0x00000002;


        // HMONITOR MonitorFromWindow(
        //  [in] HWND hwnd,
        //  [in] DWORD dwFlags
        // );
        [LibraryImport("user32.dll")]
        public static partial IntPtr MonitorFromWindow(
            IntPtr hWnd, UInt32 flags);


        // BOOL GetMonitorInfoW(
        //   [in]  HMONITOR      hMonitor,
        //   [out] LPMONITORINFO lpmi
        // );
        [LibraryImport("User32.dll", EntryPoint = "GetMonitorInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpMonitorInfo);
    }

    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public static readonly IntPtr HWND_NOTOPMOST = new(-2);
    public static readonly IntPtr HWND_TOP = new(0);
    public static readonly IntPtr HWND_BOTTOM = new(1);

    // winuser.h

    public const UInt32 SWP_NOSIZE = 0x0001;
    public const UInt32 SWP_NOMOVE = 0x0002;
    public const UInt32 SWP_NOZORDER = 0x0004;
    public const UInt32 SWP_NOREDRAW = 0x0008;
    public const UInt32 SWP_NOACTIVATE = 0x0010;

    public const UInt32 SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */
    public const UInt32 SWP_SHOWWINDOW = 0x0040;
    public const UInt32 SWP_HIDEWINDOW = 0x0080;
    public const UInt32 SWP_NOCOPYBITS = 0x0100;
    public const UInt32 SWP_NOOWNERZORDER = 0x0200; /* Don’t do owner Z ordering */
    public const UInt32 SWP_NOSENDCHANGING = 0x0400; /* Don’t send WM_WINDOWPOSCHANGING */

    public const UInt32 TOPMOST_FLAGS =
      SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;

    // Window Messages
    public const int WM_SIZE = 0x0005;
    public const int WM_MOVE = 0x0003;
    public const int WM_ENTERSIZEMOVE = 0x0231;
    public const int WM_EXITSIZEMOVE = 0x0232;
    public const int WM_GETMINMAXINFO = 0x0024;


    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);



    public const UInt32 WS_OVERLAPPEDWINDOW = 0xcf0000;
    public const UInt32 WS_VISIBLE = 0x10000000;

    public const UInt32 CS_USEDEFAULT = 0x80000000;
    public const UInt32 CS_DBLCLKS = 8;
    public const UInt32 CS_VREDRAW = 1;
    public const UInt32 CS_HREDRAW = 2;

    public const UInt32 COLOR_WINDOW = 5;
    public const UInt32 COLOR_BACKGROUND = 1;

    public const UInt32 IDC_CROSS = 32515;
    public const UInt32 WM_DESTROY = 2;
    public const UInt32 WM_PAINT = 0x0f;
    public const UInt32 WM_LBUTTONUP = 0x0202;
    public const UInt32 WM_LBUTTONDBLCLK = 0x0203;
    public const UInt32 WM_CLOSE = 0x0010;


    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UpdateWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(IntPtr hWnd);


    [LibraryImport("user32.dll")]
    public static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll")]
    public static partial void PostQuitMessage(int nExitCode);

    [LibraryImport("user32.dll")]
    public static partial IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);


    public const int WHITE_BRUSH = 0;
    public const int BLACK_BRUSH = 4;

    [LibraryImport("gdi32.dll")]
    public static partial IntPtr GetStockObject(int fnObject);



    [LibraryImport("kernel32.dll")]
    public static partial uint GetLastError();

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AttachConsole(uint dwProcessId);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FreeConsole();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllocConsole();

    public const UInt32 STD_OUTPUT_HANDLE = 0xFFFFFFF5;

    [LibraryImport("kernel32.dll")]
    public static partial IntPtr GetStdHandle(UInt32 nStdHandle);

    [LibraryImport("kernel32.dll")]
    public static partial void SetStdHandle(UInt32 nStdHandle, IntPtr handle);



    [LibraryImport(
        "kernel32",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16,
        EntryPoint = "LoadLibraryW"
    )]
    public static partial IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FreeLibrary(IntPtr hModule);

    [LibraryImport(
        "kernel32",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Custom,
        StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller)
    )]
    public static partial IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
}


