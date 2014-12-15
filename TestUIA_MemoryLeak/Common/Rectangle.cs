using System;
using System.Runtime;
using System.Windows;

namespace TestUIA.Common
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public const double ZeroTolerance = 1e-6f;

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private static readonly Rectangle EmptyRectangle = new Rectangle(0, 0, 0, 0);

        public static readonly Rectangle Max = new Rectangle(-double.MaxValue / 2, -double.MaxValue / 2, double.MaxValue / 2, double.MaxValue / 2);

        public Rectangle(Point topLeft, Point bottomRight) : this(topLeft, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y) { }
        public Rectangle(Point topLeft, Size size) : this(topLeft, size.Width, size.Height) { }
        public Rectangle(Point topLeft, double width, double height) : this(topLeft.X, topLeft.Y, width, height) { }
        public Rectangle(Rect rect) : this(rect.X, rect.Y, rect.Width, rect.Height) { }
        public Rectangle(double x, double y, double width, double height)
        {
            // TODO: Removed this check since WPF Bindings have issues where a subtree is replaced and bounds for items are invalid (needs more investigation)
            //if ((width < 0.0) || (height < 0.0))
            //{
            //    throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            //}
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// An empty rectangle has a width and height of zero
        /// </summary>
        public static Rectangle Empty
        {
            get { return EmptyRectangle; }
        }

        public bool IsEmpty
        {
            get { return (_width <= 0.0); }
        }

        /// <summary>
        /// Indicates the top left point of the rectangle
        /// </summary>
        public Point Location
        {
            get { return new Point(_x, _y); }
            set
            {
                _x = value.X;
                _y = value.Y;
            }
        }

        /// <summary>
        /// The size of the rectangle
        /// </summary>
        public Size Size
        {
            get { return new Size(_width, _height); }
            set
            {
                _width = value.Width;
                _height = value.Height;
            }
        }

        /// <summary>
        /// The x coordinate of the top left point of the rectangle
        /// </summary>
        public double X
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _x; }
            set
            {
                _x = value;
            }
        }

        /// <summary>
        /// The y coordinate of the top left point of the rectangle
        /// </summary>
        public double Y
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        public double Width
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _width; }
            set
            {
                if (value < 0.0)
                {
                    throw new ArgumentException("Size_WidthCannotBeNegative");
                }
                _width = value;
            }
        }

        public double Height
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _height; }
            set
            {
                if (value < 0.0)
                {
                    throw new ArgumentException("Size_HeightCannotBeNegative");
                }
                _height = value;
            }
        }

        public double Left
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _x; }
        }

        public double Top
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _y; }
        }

        public double Right
        {
            get
            {
                return (_x + _width);
            }
        }

        public double Bottom
        {
            get
            {
                return (_y + _height);
            }
        }

        public Point TopLeft
        {
            get { return new Point(Left, Top); }
        }

        public Point TopRight
        {
            get { return new Point(Right, Top); }
        }

        public Point BottomLeft
        {
            get { return new Point(Left, Bottom); }
        }

        public Point BottomRight
        {
            get { return new Point(Right, Bottom); }
        }

        public Point Center
        {
            get { return new Point(X + Width / 2f, Y + Height / 2f); }
        }

        public double Area
        {
            get { return Width * Height; }
        }

        public bool Equals(Rectangle value)
        {
            return Equals(this, value);
        }

        public static bool operator ==(Rectangle rectangle1, Rectangle rectangle2)
        {
            return ((((Math.Abs(rectangle1.X - rectangle2.X) < ZeroTolerance) && (Math.Abs(rectangle1.Y - rectangle2.Y) < ZeroTolerance)) &&
                     (Math.Abs(rectangle1.Width - rectangle2.Width) < ZeroTolerance)) && (Math.Abs(rectangle1.Height - rectangle2.Height) < ZeroTolerance));
        }

        public static bool operator !=(Rectangle rectangle1, Rectangle rectangle2)
        {
            return !(rectangle1 == rectangle2);
        }

        public static bool Equals(Rectangle rectangle1, Rectangle rectangle2)
        {
            if (rectangle1.IsEmpty)
            {
                return rectangle2.IsEmpty;
            }
            return (((rectangle1.X.Equals(rectangle2.X) && rectangle1.Y.Equals(rectangle2.Y)) &&
                     rectangle1.Width.Equals(rectangle2.Width)) && rectangle1.Height.Equals(rectangle2.Height));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Rectangle))
            {
                return false;
            }
            var rectangle = (Rectangle)o;
            return Equals(this, rectangle);
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Width.GetHashCode();
                hash = hash * 23 + Height.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("X: {0:0.000} Y: {1:0.000} Width: {2:0.000} Height: {3:0.000}", _x, _y,
                                 _width, _height);
        }

        /// <summary>
        /// Is the point contained by the rectangle? This includes if the point is on the circumference
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test</param>
        /// <param name="y">The y-coordinate of the point to test</param>
        /// <returns>True if the point is inside the rectangle or on the circumference</returns>
        public bool Contains(double x, double y)
        {
            if (IsEmpty)
            {
                return false;
            }
            return ContainsInternal(x, y);
        }

        /// <summary>
        /// Is the rectangle contained by this rectangle? This includes if the rectangle's edge is coincident with this rectangles edge.
        /// </summary>
        /// <param name="rectangle">The rectangle to test</param>
        /// <returns>True if the point is inside the rectangle or on the circumference</returns>
        public bool Contains(Rectangle rectangle)
        {
            if (IsEmpty || rectangle.IsEmpty)
            {
                return false;
            }
            return ((((_x <= rectangle._x) && (_y <= rectangle._y)) &&
                     ((_x + _width) >= (rectangle._x + rectangle._width))) &&
                    ((_y + _height) >= (rectangle._y + rectangle._height)));
        }

        /// <summary>
        /// Does these rectangles intersect?
        /// </summary>
        /// <param name="rectangle">The rectangle to test</param>
        /// <returns>True if the rectangles intersect</returns>
        public bool IntersectsWith(Rectangle rectangle)
        {
            if (IsEmpty || rectangle.IsEmpty)
            {
                return false;
            }
            return ((((rectangle.Left <= Right) && (rectangle.Right >= Left)) &&
                     (rectangle.Top <= Bottom)) && (rectangle.Bottom >= Top));
        }

        /// <summary>
        /// Finds the intersection of the current rectangle and the specified rectangle, and stores the result as the current rectangle.
        /// </summary>
        /// <param name="rectangle">The other rectangle with which to find the intersection of</param>
        public void Intersection(Rectangle rectangle)
        {
            if (!IntersectsWith(rectangle))
            {
                this = Empty;
            }
            else
            {
                double num2 = Math.Max(Left, rectangle.Left);
                double num = Math.Max(Top, rectangle.Top);
                _width = Math.Max((Math.Min(Right, rectangle.Right) - num2), 0.0);
                _height = Math.Max((Math.Min(Bottom, rectangle.Bottom) - num), 0.0);
                _x = num2;
                _y = num;
            }
        }

        /// <summary>
        /// Finds the intersection of the two rectangles.
        /// </summary>
        /// <param name="rectangle1">The first rectangle with which to find the intersection of</param>
        /// <param name="rectangle2">The first rectangle with which to find the intersection of</param>
        /// <returns>The intersection</returns>
        public static Rectangle Intersection(Rectangle rectangle1, Rectangle rectangle2)
        {
            rectangle1.Intersection(rectangle2);
            return rectangle1;
        }

        /// <summary>
        /// Finds the union between this rectangle and the parameter rectangle and sets the result to this rect.
        /// </summary>
        /// <param name="rectangle">The rect to test with</param>
        public void Union(Rectangle rectangle)
        {
            if (IsEmpty)
            {
                this = rectangle;
            }
            else if (!rectangle.IsEmpty)
            {
                double num2 = Math.Min(Left, rectangle.Left);
                double num = Math.Min(Top, rectangle.Top);
                if ((double.IsPositiveInfinity(rectangle.Width)) || (double.IsPositiveInfinity(Width)))
                {
                    _width = double.PositiveInfinity;
                }
                else
                {
                    double num4 = Math.Max(Right, rectangle.Right);
                    _width = Math.Max((num4 - num2), 0.0);
                }
                if ((double.IsPositiveInfinity(rectangle.Height)) || (double.IsPositiveInfinity(Height)))
                {
                    _height = double.PositiveInfinity;
                }
                else
                {
                    double num3 = Math.Max(Bottom, rectangle.Bottom);
                    _height = Math.Max((num3 - num), 0.0);
                }
                _x = num2;
                _y = num;
            }
        }

        /// <summary>
        /// Finds the union between two rectangles.
        /// </summary>
        /// <param name="rectangle1">The first rect to test with</param>
        /// <param name="rectangle2">The second rect to test with</param>
        public static Rectangle Union(Rectangle rectangle1, Rectangle rectangle2)
        {
            rectangle1.Union(rectangle2);
            return rectangle1;
        }

        /// <summary>
        /// Move the rectangle by a specified offset
        /// </summary>
        /// <param name="offsetX">The amount to move in the x-axis</param>
        /// <param name="offsetY">The amount to move in the y-axis</param>
        public void Offset(double offsetX, double offsetY)
        {
            _x += offsetX;
            _y += offsetY;
        }

        /// <summary>
        /// Move the rectangle by a specified offset
        /// </summary>
        /// <param name="rectangle">The rectangle to offset</param>
        /// <param name="offsetX">The amount to move in the x-axis</param>
        /// <param name="offsetY">The amount to move in the y-axis</param>
        public static Rectangle Offset(Rectangle rectangle, double offsetX, double offsetY)
        {
            rectangle.Offset(offsetX, offsetY);
            return rectangle;
        }

        /// <summary>
        /// Inflate the rectangle in all directions be the specified size
        /// </summary>
        /// <param name="size">The size to inflate in all directions</param>
        public void Inflate(Size size)
        {
            Inflate(size.Width, size.Height);
        }

        /// <summary>
        /// Inflate the rectangle in all directions be the specified size
        /// </summary>
        /// <param name="width">The width to add both to the left and right side</param>
        /// <param name="height">The height to add to both the top and bottom of the rectangle</param>
        public void Inflate(double width, double height)
        {
            _x -= width;
            _y -= height;
            _width += width;
            _width += width;
            _height += height;
            _height += height;
            if ((_width < 0.0) || (_height < 0.0))
            {
                throw new InvalidOperationException("Width and/or height can not go below 0");
            }
        }

        /// <summary>
        /// Inflate the rectangle in all directions be the specified size
        /// </summary>
        /// <param name="rectangle">The rectangle to inflate</param>
        /// <param name="size">The size to inflate in all directions</param>
        public static Rectangle Inflate(Rectangle rectangle, Size size)
        {
            rectangle.Inflate(size.Width, size.Height);
            return rectangle;
        }

        /// <summary>
        /// Inflate the rectangle in all directions be the specified size
        /// </summary>
        /// <param name="rectangle">The rectangle to inflate</param>
        /// <param name="width">The width to add both to the left and right side</param>
        /// <param name="height">The height to add to both the top and bottom of the rectangle</param>
        public static Rectangle Inflate(Rectangle rectangle, double width, double height)
        {
            rectangle.Inflate(width, height);
            return rectangle;
        }

        /// <summary>
        /// Scales the rectangle around the origin.
        /// </summary>
        /// <param name="scaleX">Scaling factor in the x axis</param>
        /// <param name="scaleY">Scaling factor in the y axis</param>
        /// <param name="scaleOriginX">Scaling relative origin in the x axis (0-1)</param>
        /// <param name="scaleOriginY">Scaling relative origin in the y axis (0-1)</param>
        public void Scale(double scaleX, double scaleY, double scaleOriginX = 0, double scaleOriginY = 0)
        {
            if (!IsEmpty)
            {
                if (scaleX < 0.0 || scaleY < 0.0)
                {
                    throw new InvalidOperationException("Scale has to be above zero");
                }

                if (scaleOriginX < 0 || scaleOriginX > 1
                    || scaleOriginY < 0 || scaleOriginY > 1)
                {
                    throw new InvalidOperationException("Scale origins have to be in range from 0 to 1");
                }

                _x -= _width * scaleOriginX * (scaleX - 1);
                _y -= _height * scaleOriginY * (scaleY - 1);

                _width *= scaleX;
                _height *= scaleY;
            }
        }

        private bool ContainsInternal(double x, double y)
        {
            return ((((x >= _x) && ((x - _width) <= _x)) && (y >= _y)) && ((y - _height) <= _y));
        }

        public static implicit operator Rect(Rectangle r)
        {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }

        public static implicit operator Rectangle(Rect r)
        {
            return new Rectangle(r);
        }
    }
}