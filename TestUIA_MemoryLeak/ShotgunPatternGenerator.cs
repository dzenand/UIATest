using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TestUIA.Common;

namespace TestUIA
{
    public class ShotgunPatternGenerator
    {
        private readonly int _numberOfSteps;
        private readonly double _stepAngle;
        private readonly Random _random;

        private int _step;
        private bool _evenRound;

        public ShotgunPatternGenerator(int numberOfSteps)
        {
            _numberOfSteps = numberOfSteps;
            _stepAngle = Math.PI / numberOfSteps;
            _step = 0;
            _evenRound = true;
            _random = new Random();
        }

        public void Reset()
        {
            _step = 0;
            _evenRound = true;
        }

        public IList<Point> NextPattern(Rectangle rectangle)
        {
            var cellSizeWidth = rectangle.Size.Width / 8.0;
            var cellSizeHeight = rectangle.Size.Height / 8.0;
            var cellSize = new Point(cellSizeWidth, cellSizeHeight);
            var center = rectangle.Center;

            IList<Point> searchPoints;
            if (_evenRound)
                searchPoints = new List<Point>()
                {
                    new Point(0, cellSize.Y * (2 + GetRandom())),
                    new Point(cellSize.X * GetRandom(), 0),
                    new Point(0, -cellSize.Y * (2 + GetRandom())),
                    new Point(-cellSize.X * GetRandom(), 0),
                };
            else
                searchPoints = new List<Point>()
                {
                    new Point(0, cellSize.Y * (3 + GetRandom())),
                    new Point(cellSize.X * (1 + GetRandom()), 0),
                    new Point(0, -cellSize.Y * (3 + GetRandom())),
                    new Point(-cellSize.X * (1 + GetRandom()), 0),
                };

            var rotateAngle = _stepAngle * (_step + GetRandom());

            searchPoints = RotatePoints(searchPoints, rotateAngle);
            searchPoints = ChangePointsScreenOrigin(searchPoints, center);
            searchPoints.Add(center);

            GoToNextStep();

            return searchPoints;
        }

        private IList<Point> RotatePoints(IList<Point> points, double angle)
        {
            if (angle == 0)
                return points;

            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            var rotatedPoints = new List<Point>(points.Count);
            foreach (var point in points)
            {
                var x = (cos * point.X) - (sin * point.Y);
                var y = (sin * point.X) + (cos * point.Y);

                rotatedPoints.Add(new Point(x, y));
            }

            return rotatedPoints;
        }

        private IList<Point> ChangePointsScreenOrigin(IList<Point> points, Point origin)
        {
            return points.Select(p => new Point(p.X + origin.X, origin.Y - p.Y)).ToList();
        }

        private void GoToNextStep()
        {
            if (_step < _numberOfSteps - 1)
                _step++;
            else
            {
                _evenRound = !_evenRound;
                _step = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetRandom()
        {
            return _random.NextDouble();
        }
    }
}