using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace TestUIA
{
    public static class MouseHelper
    {
        public static Point GetCurrentMousePosition()
        {
            Win32Point lpPoint;
            if (!User32.GetCursorPos(out lpPoint))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return lpPoint;
        }
    }
}