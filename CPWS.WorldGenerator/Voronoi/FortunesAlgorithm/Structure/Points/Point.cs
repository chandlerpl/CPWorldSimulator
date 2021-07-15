using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points
{
    public class Point
    {
        private double x = 0;
        private double y = 0;
        private double z = 0;

        public virtual double X { get => x; set => x = value; }
        public virtual double Y { get => y; set => y = value; }

        //public Point() { }

        public const double Deg2Rad = Math.PI * 2F / 360F;

        public const double Rad2Deg = 1F / Deg2Rad;
        public Point()
        {
        }

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static double Dot(Point lhs, Point rhs) { return lhs.X * rhs.X + lhs.Y * rhs.Y; }

        public static double DistanceSquared(Point from, Point to) { return ((to.X - from.X) * (to.X - from.X)) + ((to.Y - from.Y) * (to.Y - from.Y)); }

        public static double Distance(Point from, Point to) { return Math.Sqrt(DistanceSquared(from, to)); }

        public static double Angle(Point from, Point to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            double dot = Clamp(Dot(from, to) / denominator, -1, 1);
            return Math.Acos(dot) * Rad2Deg;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public double sqrMagnitude { get { return X * X + Y * Y; } }

        public static Point operator +(Point a, Point b) { return new Point(a.X + b.X, a.Y + b.Y); }
        public static Point operator +(Point a, double b) { return new Point(a.X + b, a.Y + b); }
        public static Point operator /(Point a, Point b) { return new Point(a.X / b.X, a.Y / b.Y); }
        public static Point operator *(Point a, double b) { return new Point(a.X * b, a.Y * b); }
        public static Point operator *(Point a, Point b) { return new Point(a.X * b.X, a.Y * b.Y); }

        public static Point operator *(double a, Point b) { return new Point(b.X * a, b.Y * a); }
        public static Point operator /(Point a, double b) { return new Point(a.X / b, a.Y / b); }
        public static Point operator -(Point a, Point b) { return new Point(a.X - b.X, a.Y - b.Y); }
        public static Point operator -(Point a, double b) { return new Point(a.X - b, a.Y - b); }
        public static Point operator -(Point a) { return new Point(-a.X, -a.Y); }

        public const float kEpsilon = 0.00001F;
        public const float kEpsilonNormalSqrt = 1e-15f;
    }
}
