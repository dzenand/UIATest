using System;
using System.Threading;
using System.Windows;
using TestUIA.Common;

namespace TestUIA.Cache
{
    public class WindowEventsCacheInvalidationExecutant : DisposableBase, ICacheInvalidationExecutant
    {
        private const int PrimeNumber = 239;

        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

        private IntPtr _windowHandle;
        private int _processId;
        private Timer _windowStatePollTimer;
        private WindowStateInfo _lastWindowStateInfo;

        public WindowEventsCacheInvalidationExecutant()
        {
            _lastWindowStateInfo = null;
        }

        public event EventHandler<InvalidateEventArgs> Invalidate;

        public bool Init(int processId, IntPtr windowHandle)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("WindowEventsCacheInvalidationExecutant");

            _processId = processId;
            _windowHandle = windowHandle;
            if (!TryGetWindowState(out _lastWindowStateInfo))
                return false;

            _windowStatePollTimer = new Timer(WindowStatePoll, null, PollInterval, Timeout.InfiniteTimeSpan);

            return true;
        }

        protected override void DisposeManagedResources()
        {
            if (_windowStatePollTimer != null)
            {
                _windowStatePollTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }

            base.DisposeManagedResources();
        }

        private static bool IsValidWindow(int processId, IntPtr windowHandle)
        {
            var isValid = false;
            if (User32.IsWindow(windowHandle))
            {
                var windowProcessId = GetWindowProcessId(windowHandle);
                isValid = processId.Equals(windowProcessId);
            }

            return isValid;
        }

        private static int GetWindowProcessId(IntPtr windowHandle)
        {
            uint processId;

            if (User32.GetWindowThreadProcessId(windowHandle, out processId) != 0)
                return (int)processId;

            return 0;
        }

        private void WindowStatePoll(object state)
        {
            if (IsDisposed)
                return;

            WindowStateInfo windowStateInfo;
            if (!TryGetWindowState(out windowStateInfo) || !windowStateInfo.Equals(_lastWindowStateInfo))
            {
                OnInvalidate();
            }

            _lastWindowStateInfo = windowStateInfo;

            if (!IsDisposed)
                _windowStatePollTimer.Change(PollInterval, Timeout.InfiniteTimeSpan);
        }

        private bool TryGetWindowState(out WindowStateInfo windowStateInfo)
        {
            windowStateInfo = new WindowStateInfo();
            if (IsValidWindow(_processId, _windowHandle))
            {
                var windowPlacement = new User32.WINDOWPLACEMENT();
                if (User32.GetWindowPlacement(_windowHandle, ref windowPlacement))
                {
                    if (windowPlacement.showCmd == User32.SW_SHOWMAXIMIZED)
                        windowStateInfo.State = WindowState.Maximized;
                    else if (windowPlacement.showCmd == User32.SW_SHOWMINIMIZED)
                        windowStateInfo.State = WindowState.Minimized;
                    else
                    {
                        windowStateInfo.State = WindowState.Normal;
                        var rect = new Win32Rect();
                        if (!User32.GetWindowRect(_windowHandle, ref rect))
                            return false;

                        windowStateInfo.Rect = rect;
                    }

                    var info = new User32.WindowInformation(null);
                    if (!User32.GetWindowInfo(_windowHandle, ref info))
                        return false;

                    windowStateInfo.WindowStatus = info.dwWindowStatus;
                    windowStateInfo.WindowStyles = info.dwStyle;
                    windowStateInfo.WindowExStyles = info.dwExStyle;

                    return true;
                }
            }

            return false;
        }

        private void OnInvalidate()
        {
            var handler = Invalidate;
            if (handler != null)
                handler(this, new WindowEventsCacheInvalidateEventArgs());
        }

        private enum WindowState
        {
            Maximized,

            Minimized,

            Normal
        }

        private class WindowStateInfo
        {
            public WindowState State { get; set; }

            public Rect Rect { get; set; }

            public uint WindowStatus { get; set; }

            public User32.WindowStyles WindowStyles { get; set; }

            public User32.WindowStyles WindowExStyles { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as WindowStateInfo;
                if (other == null)
                    return false;

                return (other.State == State) &&
                    Rect.Equals(other.Rect) &&
                    (other.WindowStatus == WindowStatus) &&
                    (other.WindowStyles == WindowStyles) &&
                    (other.WindowExStyles == WindowExStyles);
            }

            public override int GetHashCode()
            {
                return State.GetHashCode() ^
                    (Rect.GetHashCode() * PrimeNumber) ^
                    (WindowStatus.GetHashCode() * PrimeNumber) ^
                    (WindowStyles.GetHashCode() * PrimeNumber) ^
                    (WindowExStyles.GetHashCode() * PrimeNumber);
            }
        }
    }

    public sealed class WindowEventsCacheInvalidateEventArgs : InvalidateEventArgs
    {
    }
}