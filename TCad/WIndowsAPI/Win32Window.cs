using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using TCad.WindowsAPI;

namespace TCad.WIndowsAPI;

delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

public partial class Win32Window
{
    static partial class Native
    {
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

        [CustomMarshaller(typeof(WNDCLASSEXW), MarshalMode.UnmanagedToManagedIn, typeof(WindowClassMarshaler))]
        [CustomMarshaller(typeof(WNDCLASSEXW), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
        internal static unsafe class WindowClassMarshaler
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal unsafe struct WindowClassUnmanaged
            {
                public uint StructSize;
                public uint Style;
                public IntPtr WindowProcedure;
                public int ClassAdditionalBytes;
                public int WindowAdditionalBytes;
                public IntPtr Instance;
                public IntPtr Icon;
                public IntPtr Cursor;
                public IntPtr BackgroundBrush;
                public char* ClassMenuResourceName;
                public char* ClassName;
                public IntPtr SmallIcon;
            }

            internal static unsafe WNDCLASSEXW ConvertToManaged(WindowClassUnmanaged unmanaged)
            {
                return new()
                {
                    lpfnWndProc = unmanaged.WindowProcedure,
                    lpszMenuName = Win32WideCharArrToString(unmanaged.ClassMenuResourceName),
                    lpszClassName = Win32WideCharArrToString(unmanaged.ClassName),
                    // (remainder omitted, just simple copies)
                };
            }

            public static unsafe string Win32WideCharArrToString(char* unmanagedArr)
            {
                if (unmanagedArr == null) { return null; }
                int Length = 0;
                while (*(unmanagedArr + Length) != 0x0000) { Length++; }
                return Encoding.Unicode.GetString((byte*)unmanagedArr, Length * sizeof(char));
            }

            internal unsafe ref struct ManagedToUnmanagedIn
            {
                public static int BufferSize => sizeof(WindowClassUnmanaged);

                private byte* UnmanagedBufferStruct;
                private char* UnmanagedStrResourceName, UnmanagedStrClassName;

                public void FromManaged(WNDCLASSEXW managed, Span<byte> buffer)
                {
                    IntPtr WindowProcedure = Marshal.GetFunctionPointerForDelegate(managed.lpfnWndProc);
                    this.UnmanagedStrResourceName = (managed.lpszMenuName == null) ? null : (char*)Marshal.StringToHGlobalUni(managed.lpszMenuName);
                    this.UnmanagedStrClassName = (managed.lpszClassName == null) ? null : (char*)Marshal.StringToHGlobalUni(managed.lpszClassName);

                    WindowClassUnmanaged Result = new()
                    {
                        WindowProcedure = WindowProcedure,
                        ClassMenuResourceName = this.UnmanagedStrResourceName,
                        ClassName = this.UnmanagedStrClassName,
                        // (remainder omitted, just simple copies)
                    };

                    Span<byte> ResultByteView = MemoryMarshal.Cast<WindowClassUnmanaged, byte>(MemoryMarshal.CreateSpan(ref Result, 1));
                    Debug.Assert(buffer.Length >= ResultByteView.Length, "Target buffer isn't large enough to hold the struct data.");
                    ResultByteView.CopyTo(buffer);

                    this.UnmanagedBufferStruct = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
                }

                public readonly byte* ToUnmanaged() => this.UnmanagedBufferStruct;

                public void Free()
                {
                    if (this.UnmanagedStrResourceName != null)
                    {
                        Marshal.FreeHGlobal((nint)this.UnmanagedStrResourceName);
                        this.UnmanagedStrResourceName = null;
                    }
                    if (this.UnmanagedStrClassName != null)
                    {
                        Marshal.FreeHGlobal((nint)this.UnmanagedStrClassName);
                        this.UnmanagedStrClassName = null;
                    }
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;     // ウィンドウハンドル
            public uint message;    // メッセージID
            public IntPtr wParam;   // メッセージの最初のパラメータ
            public IntPtr lParam;   // メッセージの2番目のパラメータ
            public uint time;       // メッセージが投稿された時間
            public WinAPI.POINT pt;        // カーソル位置（画面座標）
            public uint lPrivate;   // 内部（プライベート）データ
        }

        //[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassExW")]
        //public static extern ushort RegisterClassEx([In] ref WNDCLASSEXW lpWndClass);


        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassExW")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
        public static partial ushort RegisterClassEx([MarshalUsing(typeof(WindowClassMarshaler))] WNDCLASSEXW classDefinition);


        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", SetLastError = true)]
        public static partial IntPtr CreateWindowEx(
            int dwExStyle,

            [MarshalAs(UnmanagedType.LPWStr)]
            string lpClassName,

            [MarshalAs(UnmanagedType.LPWStr)]
            string lpWindowName,

            UInt32 dwStyle,

            int x,
            int y,
            int nWidth,
            int nHeight,

            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );


        [LibraryImport("user32.dll")]
        public static partial sbyte GetMessage(
            out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool TranslateMessage(in MSG lpMsg);

        [LibraryImport("user32.dll")]
        public static partial IntPtr DispatchMessage(in MSG lpmsg);
    }

    public const string ClassName = "myWindowClass";

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

    private static readonly Lock lockObj = new();

    private static readonly Dictionary<IntPtr, Win32Window> HWndMap = [];

    private readonly WndProc delegWndProc = staticWndProc;

    public bool Create(string windowName)
    {
        lock (lockObj)
        {
            Native.WNDCLASSEXW wndClass = CreateWindowClass();

            if (WndClassRegisterResult == 0)
            {
                //WndClassRegisterResult = Native.RegisterClassEx(ref wndClass);
                WndClassRegisterResult = Native.RegisterClassEx(wndClass);
            }

            if (WndClassRegisterResult == 0)
            {
                LastError_ = WinAPI.GetLastError();
                return false;
            }

            string wndClassName = wndClass.lpszClassName;

            hWnd_ = Native.CreateWindowEx(
                0,
                wndClassName,
                windowName,
                WinAPI.WS_OVERLAPPEDWINDOW /* | WinAPI.WS_VISIBLE */,
                0, 0, 300, 400,
                IntPtr.Zero,
                IntPtr.Zero,
                wndClass.hInstance,
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

    private Native.WNDCLASSEXW CreateWindowClass()
    {
        Native.WNDCLASSEXW wndClass = new()
        {
            cbSize = (uint)Marshal.SizeOf<Native.WNDCLASSEXW>(),
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

        return wndClass;
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
        Native.MSG msg;
        while (Native.GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
        {
            Native.TranslateMessage(in msg);
            Native.DispatchMessage(in msg);
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

