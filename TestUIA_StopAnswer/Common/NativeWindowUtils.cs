using System;
using System.Windows;

namespace TestUIA.Common
{
    public static class NativeWindowUtils
    {
        public static IntPtr DesktopChildWindowFromPoint(Point searchPoint)
        {
            var desktopWindow = User32.GetDesktopWindow();
            return User32.ChildWindowFromPointEx(
                desktopWindow,
                new Win32Point((int)searchPoint.X, (int)searchPoint.Y),
                (uint)(User32.WindowFromPointFlags.CWP_SKIPTRANSPARENT |
                        User32.WindowFromPointFlags.CWP_SKIPINVISIBLE |
                        User32.WindowFromPointFlags.CWP_SKIPDISABLED));
        }         
    }
}