using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TestUIA.Common;
using Timer = System.Threading.Timer;

namespace TestUIA.Cache
{
    public sealed class VisualCacheInvalidationExecutant : ICacheInvalidationExecutant
    {
        private Rectangle _windowRectangle;
        private IntPtr _windowHandle;
        private int _processId;

        private Timer _timer;
        private bool _state = false;


        public event EventHandler<InvalidateEventArgs> Invalidate;

        public Rectangle RegionOfInterest
        {
            get
            {
                return _windowRectangle;
            }
        }

        public Func<Rectangle, bool> Filter
        {
            get
            {
                return null;
            }
        }

        public bool Init(int processId, IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _processId = processId;

            _timer = new Timer(TimerPoll, null, TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.3));
            return true;
        }

        private void TimerPoll(object state)
        {
            var windowRect = new Win32Rect();
            if (!User32.GetWindowRect(_windowHandle, ref windowRect))
            {
                OnFullInvalidate();
            }

            var rectangle = new System.Drawing.Rectangle(
                windowRect.Left, 
                windowRect.Top, 
                windowRect.Width,
                windowRect.Height);

            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.IntersectsWith(rectangle))
                {
                    Rectangle rectangle1;                    
                    Rectangle rectangle2;
                    var cellWidth = screen.Bounds.Width / 2.0;
                    var cellHeight = screen.Bounds.Height / 2.0;


                    if (_state)
                    {
                        rectangle1 = new Rectangle(
                            screen.Bounds.X,
                            screen.Bounds.Y,
                            cellWidth,
                            cellHeight);
                        rectangle2 = new Rectangle(
                            screen.Bounds.X + cellWidth,
                            screen.Bounds.Y + cellHeight,
                            cellWidth,
                            cellHeight);
                    }
                    else
                    {
                        rectangle1 = new Rectangle(
                            screen.Bounds.X + cellWidth,
                            screen.Bounds.Y,
                            cellWidth,
                            cellHeight);
                        rectangle2 = new Rectangle(
                            screen.Bounds.X,
                            screen.Bounds.Y + cellHeight,
                            cellWidth,
                            cellHeight);                        
                    }

                    _state = !_state;
                    OnRectanglesChanged(new[] { rectangle1, rectangle2 });
                    return;
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public void OnRectanglesChanged(IEnumerable<Rectangle> rectangles)
        {
            var timestamp = DateTime.UtcNow;
            var rectsOfWindow = new List<Rectangle>();
            foreach (var rectangle in rectangles)
            {
                rectangle.Intersection(_windowRectangle);

                var foundWindowHandle = NativeWindowUtils.DesktopChildWindowFromPoint(rectangle.Center);
                if (foundWindowHandle == _windowHandle || User32.IsChild(foundWindowHandle, _windowHandle))
                {
                    rectsOfWindow.Add(rectangle);
                }
            }

            if (rectsOfWindow.Count != 0)
            {
                OnInvalidate(rectsOfWindow, timestamp);
            }
        }

        public void UpdateRegionOfInterest()
        {
            var windowRect = new Win32Rect();
            if (!User32.GetWindowRect(_windowHandle, ref windowRect))
            {
                OnFullInvalidate();
            }

            _windowRectangle = new Rectangle(windowRect);
        }

        private void OnInvalidate(IEnumerable<Rectangle> changedRectangles, DateTime timestampUtc)
        {
            try
            {
                var handler = Invalidate;
                if (handler != null)
                    handler(this, new VisualInvalidateEventArgs(changedRectangles, timestampUtc));
            }
            catch
            {
            }
        }

        private void OnFullInvalidate()
        {
            try
            {
                var handler = Invalidate;
                if (handler != null)
                    handler(this, new VisualFullInvalidateEventArgs());
            }
            catch
            {
            }
        }
    }

    public sealed class VisualInvalidateEventArgs : PartialInvalidateEventArgs
    {
        public VisualInvalidateEventArgs(IEnumerable<Rectangle> changedRectangles, DateTime timestampUtc)
            : base(timestampUtc)
        {
            ChangedRectangles = changedRectangles;
        }

        public IEnumerable<Rectangle> ChangedRectangles { get; private set; }
    }

    public sealed class VisualFullInvalidateEventArgs : InvalidateEventArgs
    {
    }
}
