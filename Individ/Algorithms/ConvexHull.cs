
using Individ.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Individ.Algorithms
{
    /// <summary>
    /// Реализует алгоритм построения выпуклой оболочки
    /// методом монотонной цепи.
    /// </summary>
    public class ConvexHull
    {
        /// <summary>
        /// Строит выпуклую оболочку для заданного множества точек.
        /// </summary>
        public static List<Point2D> Build(List<Point2D> points)
        {
            // Проверяем входные данные.
            if (points == null || points.Count <= 2)
            {
                return points == null
                    ? new List<Point2D>()
                    : new List<Point2D>(points);
            }

            // Сортируем точки сначала по X, затем по Y.
            var sortedPoints = points
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToList();

            // Удаляем повторяющиеся точки.
            sortedPoints = sortedPoints
                .GroupBy(p => new { p.X, p.Y })
                .Select(g => g.First())
                .ToList();

            // Повторная проверка количества точек.
            if (sortedPoints.Count <= 2)
            {
                return sortedPoints;
            }

            // Строим нижнюю часть выпуклой оболочки.
            List<Point2D> lower = new List<Point2D>();

            foreach (var point in sortedPoints)
            {
                while (lower.Count >= 2 &&
                       Cross(
                           lower[lower.Count - 2],
                           lower[lower.Count - 1],
                           point
                       ) <= 0)
                {
                    lower.RemoveAt(lower.Count - 1);
                }

                lower.Add(point);
            }

            // Строим верхнюю часть выпуклой оболочки.
            List<Point2D> upper = new List<Point2D>();

            foreach (var point in sortedPoints.AsEnumerable().Reverse())
            {
                while (upper.Count >= 2 &&
                       Cross(
                           upper[upper.Count - 2],
                           upper[upper.Count - 1],
                           point
                       ) <= 0)
                {
                    upper.RemoveAt(upper.Count - 1);
                }

                upper.Add(point);
            }

            // Удаляем повторяющиеся крайние точки.
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);

            // Объединяем нижнюю и верхнюю части.
            lower.AddRange(upper);

            return lower;
        }

        /// <summary>
        /// Вычисляет векторное произведение векторов AB и AC.
        /// </summary>
        private static double Cross(Point2D a, Point2D b, Point2D c)
        {
            return (b.X - a.X) * (c.Y - a.Y) -
                   (b.Y - a.Y) * (c.X - a.X);
        }
    }
}
