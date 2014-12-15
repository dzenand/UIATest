using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TestUIA
{
    public static class User32
    {
        private const string DllImportPath = "user32.dll";

        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWMINIMIZED = 2;

        [Flags]
        public enum WindowStyles : uint
        {
            OverlappedWindow = Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
            PopupWindow = Popup | Border | SysMenu,
            Caption = Border | DlgFrame,
            ExOverlappedWindow = (ExWindowEdge | ExClientEdge),
            ExPaletteWindow = (ExWindowEdge | ExToolWindow | ExTopmost),

            Overlapped = 0x00000000,
            Popup = 0x80000000,
            Child = 0x40000000,
            Minimize = 0x20000000,
            Visible = 0x10000000,
            Disabled = 0x08000000,
            ClipSiblings = 0x04000000,
            ClipChildren = 0x02000000,
            Maximize = 0x01000000,
            Border = 0x00800000,
            DlgFrame = 0x00400000,
            VerticalScroll = 0x00200000,
            HorizontalScroll = 0x00100000,
            SysMenu = 0x00080000,
            ThickFrame = 0x00040000,
            Group = 0x00020000,
            Tabstop = 0x00010000,

            MinimizeBox = 0x00020000,
            MaximizeBox = 0x00010000,

            Tiled = Overlapped,
            Iconic = Minimize,
            Sizebox = ThickFrame,
            TiledWindow = OverlappedWindow,

            ChildWindow = Child,

            //Extended Window Styles

            ExDlgModalFrame = 0x00000001,
            ExNoparentNotify = 0x00000004,
            ExTopmost = 0x00000008,
            ExAcceptFiles = 0x00000010,
            ExTransparent = 0x00000020,

            ExMdiChild = 0x00000040,
            ExToolWindow = 0x00000080,
            ExWindowEdge = 0x00000100,
            ExClientEdge = 0x00000200,
            ExContextHelp = 0x00000400,

            ExRight = 0x00001000,
            ExLeft = 0x00000000,
            ExRtlReading = 0x00002000,
            ExLtrReading = 0x00000000,
            ExLeftScrollbar = 0x00004000,
            ExRightScrollbar = 0x00000000,

            ExControlParent = 0x00010000,
            ExStaticEdge = 0x00020000,
            ExAppWindow = 0x00040000,


            ExLayered = 0x00080000,

            ExNoinheritLayout = 0x00100000, // Disable inheritence of mirroring by children
            ExLayoutRtl = 0x00400000, // Right to left mirroring

            ExComposited = 0x02000000,
            ExNoActivate = 0x08000000
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms632611(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public uint length;
            public uint flags;
            public uint showCmd;
            public Win32Point ptMinPosition;
            public Win32Point ptMaxPosition;
            public Win32Rect rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowInformation
        {
            public uint cbSize;
            public Win32Rect rcWindow;
            public Win32Rect rcClient;
            public WindowStyles dwStyle;
            public WindowStyles dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            /// <summary>
            /// Allows automatic initialization of "cbSize" with "new WindowInformation(null/true/false)".
            /// </summary>
            /// <param name="filler"></param>
            public WindowInformation(Boolean? filler)
                : this()
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WindowInformation)));
            }
        }

        [Flags]
        public enum WindowFromPointFlags : uint
        {
            /// <summary>
            /// Does not skip any child windows
            /// </summary>
            CWP_ALL = 0x0000,
            /// <summary>
            /// Skips invisible child windows
            /// </summary>
            CWP_SKIPINVISIBLE = 0x0001,
            /// <summary>
            /// Skips disabled child windows
            /// </summary>
            CWP_SKIPDISABLED = 0x0002,
            /// <summary>
            /// Skips transparent child windows
            /// </summary>
            CWP_SKIPTRANSPARENT = 0x0004
        }

        [DllImport(DllImportPath, SetLastError = true, EntryPoint = "IsChild")]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [DllImport(DllImportPath, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Win32Point position);

        [DllImport(DllImportPath, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetClassName(IntPtr windowHandle, StringBuilder lpClassName, int nMaxCount);

        [DllImport(DllImportPath, SetLastError = true)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport(DllImportPath, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);

        [DllImport(DllImportPath, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(DllImportPath, SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Win32Rect rect);

        [DllImport(DllImportPath, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowInfo(IntPtr hWnd, ref WindowInformation windowInformation);

        [DllImport(DllImportPath, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport(DllImportPath, CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ChildWindowFromPointEx")]
        public static extern IntPtr ChildWindowFromPointEx(IntPtr hWndParent, Win32Point point, uint flags);
    }
}