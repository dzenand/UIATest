using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TestUIA.Common
{
    public static class WindowInformationUtils
    {
        public static string GetClassName(IntPtr hWnd)
        {
            var classNameBuilder = new StringBuilder(1024);
            if (User32.GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity) != 0) 
                return classNameBuilder.ToString();

            Debug.WriteLine("Unable to get class name for {0}. Win32 exception: {1}.", hWnd, new Win32Exception(Marshal.GetLastWin32Error()));
            return null;
        }
    }
}