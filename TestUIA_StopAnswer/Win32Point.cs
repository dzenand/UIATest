using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace TestUIA
{
    [DebuggerDisplay("X:{X} Y:{Y}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Point
    {
        private readonly int _x;
        private readonly int _y;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        public Win32Point(Point point)
            : this((int)point.X, (int)point.Y)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Win32Point"/> struct.
        /// </summary>
        /// <param name="x">The x-part of the coordinate.</param>
        /// <param name="y">The y-part of the coordinate.</param>
        public Win32Point(int x, int y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Gets the X part of the coordinate.
        /// </summary>
        public int X
        {
            get { return _x; }
        }

        /// <summary>
        /// Gets the Y-part of the coordinate.
        /// </summary>
        public int Y
        {
            get { return _y; }
        }

        public static implicit operator Point(Win32Point p)
        {
            return new Point(p._x, p._y);
        }
    }
}