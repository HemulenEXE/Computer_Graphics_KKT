using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace lab_6.Core
{
    public struct Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Point other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }

    public class Triangle
    {
        public Point P1;
        public Point P2;
        public Point P3;

        public Triangle(Point p1, Point p2, Point p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public bool ContainsPoint(Point p)
        {
            double d1 = Sign(p, P1, P2);
            double d2 = Sign(p, P2, P3);
            double d3 = Sign(p, P3, P1);

            bool has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        private double Sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public Point GetCircumcenter()
        {
            double x1 = P1.X, y1 = P1.Y;
            double x2 = P2.X, y2 = P2.Y;
            double x3 = P3.X, y3 = P3.Y;

            double D = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
            double ux = ((x1 * x1 + y1 * y1) * (y2 - y3) + (x2 * x2 + y2 * y2) * (y3 - y1) + (x3 * x3 + y3 * y3) * (y1 - y2)) / D;
            double uy = ((x1 * x1 + y1 * y1) * (x3 - x2) + (x2 * x2 + y2 * y2) * (x1 - x3) + (x3 * x3 + y3 * y3) * (x2 - x1)) / D;

            return new Point(ux, uy);
        }

        public double GetCircumradius()
        {
            return P1.DistanceTo(GetCircumcenter());
        }

        public bool IsPointInCircumcircle(Point p)
        {
            return p.DistanceTo(GetCircumcenter()) < GetCircumradius();
        }

        public bool HasEdge(Edge edge)
        {
            return (P1.Equals(edge.P1) && P2.Equals(edge.P2)) || (P1.Equals(edge.P2) && P2.Equals(edge.P1)) ||
                   (P2.Equals(edge.P1) && P3.Equals(edge.P2)) || (P2.Equals(edge.P2) && P3.Equals(edge.P1)) ||
                   (P3.Equals(edge.P1) && P1.Equals(edge.P2)) || (P3.Equals(edge.P2) && P1.Equals(edge.P1));
        }
    }

    public struct Edge
    {
        public Point P1;
        public Point P2;

        public Edge(Point p1, Point p2)
        {
            P1 = p1;
            P2 = p2;
        }
    }

    public static class Delaunay
    {
        public static List<Triangle> Triangulate(List<Point> points)
        {
            List<Triangle> triangulation = new List<Triangle>();

            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxX = points.Max(p => p.X);
            double maxY = points.Max(p => p.Y);

            double dx = maxX - minX;
            double dy = maxY - minY;
            double deltaMax = Math.Max(dx, dy);
            double midX = (minX + maxX) / 2;
            double midY = (minY + maxY) / 2;

            Point p1 = new Point(midX - 20 * deltaMax, midY - deltaMax);
            Point p2 = new Point(midX, midY + 20 * deltaMax);
            Point p3 = new Point(midX + 20 * deltaMax, midY - deltaMax);

            triangulation.Add(new Triangle(p1, p2, p3));

            foreach (var point in points)
            {
                List<Triangle> badTriangles = new List<Triangle>();
                foreach (var triangle in triangulation)
                {
                    if (triangle.IsPointInCircumcircle(point))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                List<Edge> polygon = new List<Edge>();
                foreach (var triangle in badTriangles)
                {
                    Edge[] edges = { new Edge(triangle.P1, triangle.P2), new Edge(triangle.P2, triangle.P3), new Edge(triangle.P3, triangle.P1) };
                    foreach (var edge in edges)
                    {
                        bool shared = false;
                        foreach (var other in badTriangles)
                        {
                            if (triangle == other) continue;
                            if (other.HasEdge(edge))
                            {
                                shared = true;
                                break;
                            }
                        }
                        if (!shared) polygon.Add(edge);
                    }
                }

                triangulation.RemoveAll(t => badTriangles.Contains(t));

                foreach (var edge in polygon)
                {
                    triangulation.Add(new Triangle(edge.P1, edge.P2, point));
                }
            }

            triangulation.RemoveAll(t => 
                t.P1.Equals(p1) || t.P1.Equals(p2) || t.P1.Equals(p3) ||
                t.P2.Equals(p1) || t.P2.Equals(p2) || t.P2.Equals(p3) ||
                t.P3.Equals(p1) || t.P3.Equals(p2) || t.P3.Equals(p3));

            return triangulation;
        }
    }
}
