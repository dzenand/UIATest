using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace TestUIA
{
    [DebuggerDisplay("Left:{Left} Top:{Top} Width:{Width} Height:{Height}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Rect
    {
        private readonly int _left;
        private readonly int _top;
        private readonly int _right;
        private readonly int _bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Win32Rect"/> struct.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        public Win32Rect(int left, int top, int right, int bottom)
        {
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
        }

        /// <summary>
        /// Gets the x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Gets the y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top
        {
            get { return _top; }
        }

        /// <summary>
        /// Gets the x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right
        {
            get { return _right; }
        }

        /// <summary>
        /// Gets the y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom
        {
            get { return _bottom; }
        }

        public static implicit operator Rect(Win32Rect r)
        {
            if (r.Width <= 0 || r.Height <= 0)
                return Rect.Empty;
            return new Rect(r._left, r._top, r.Width, r.Height);
        }

        public int Width
        {
            get { return _right - _left; }
        }

        public int Height
        {
            get { return _bottom - _top; }

        }
    }
}