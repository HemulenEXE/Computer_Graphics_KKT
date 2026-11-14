
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace PolyhedraLab
{
    public static class ZBufferRenderer
    {
        /// <summary>
        /// Создаёт изображение с использованием Z-буфера.
        /// </summary>
        public static Bitmap Render(
            IList<ProjectedFace> faces,
            int width,
            int height,
            Color baseColor,
            Color outlineColor)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Некорректный размер изображения.");

            double[,] zBuffer = new double[width, height];

            // Изначально все пиксели свободны.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    zBuffer[x, y] = double.PositiveInfinity;
                }
            }

            Bitmap bitmap = new Bitmap(width,
                                       height,
                                       PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
            }

            if (faces == null)
                return bitmap;

            foreach (ProjectedFace face in faces)
            {
                if (face.Points == null ||
                    face.DepthValues == null ||
                    face.Points.Length < 3 ||
                    face.Points.Length != face.DepthValues.Length)
                    continue;

                if (face.VertexBrightness == null ||
                    face.VertexBrightness.Length != face.Points.Length)
                    continue;

                // Разбиваем выпуклый полигон на треугольники.
                // Вершины: 0,1,2; 0,2,3; 0,3,4...
                PointF p0 = face.Points[0];
                double d0 = face.DepthValues[0];

                for (int i = 1; i < face.Points.Length - 1; i++)
                {
                    RasterizeTriangle(
                        bitmap,
                        zBuffer,
                        p0,
                        face.Points[i],
                        face.Points[i + 1],
                        d0,
                        face.DepthValues[i],
                        face.DepthValues[i + 1],
                        face.IsPerspective,
                        baseColor,
                        face.VertexBrightness[0],
                        face.VertexBrightness[i],
                        face.VertexBrightness[i + 1]);
                }

                // Рисуем только внешние рёбра грани.
                // При этом также проверяем глубину.
                for (int i = 0; i < face.Points.Length; i++)
                {
                    int next = (i + 1) % face.Points.Length;

                    RasterizeEdge(
                        bitmap,
                        zBuffer,
                        face.Points[i],
                        face.Points[next],
                        face.DepthValues[i],
                        face.DepthValues[next],
                        face.IsPerspective,
                        outlineColor);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Растеризация треугольника.
        /// </summary>
        private static void RasterizeTriangle(
            Bitmap bitmap,
            double[,] zBuffer,
            PointF p0,
            PointF p1,
            PointF p2,
            double d0,
            double d1,
            double d2,
            bool perspective,
            Color baseColor,
            double brightness0,
            double brightness1,
            double brightness2)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Ограничивающий прямоугольник.
            int minX = Math.Max(0,
                (int)Math.Floor(Math.Min(
                    p0.X, Math.Min(p1.X, p2.X))));

            int maxX = Math.Min(width - 1,
                (int)Math.Ceiling(Math.Max(
                    p0.X, Math.Max(p1.X, p2.X))));

            int minY = Math.Max(0,
                (int)Math.Floor(Math.Min(
                    p0.Y, Math.Min(p1.Y, p2.Y))));

            int maxY = Math.Min(height - 1,
                (int)Math.Ceiling(Math.Max(
                    p0.Y, Math.Max(p1.Y, p2.Y))));

            double denominator =
                (p1.Y - p2.Y) * (p0.X - p2.X) +
                (p2.X - p1.X) * (p0.Y - p2.Y);

            if (Math.Abs(denominator) < 1e-12)
                return;

            const double epsilon = 1e-9;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // Проверяем центр пикселя.
                    double px = x + 0.5;
                    double py = y + 0.5;

                    // Барицентрические координаты.
                    double a =
                        ((p1.Y - p2.Y) * (px - p2.X) +
                         (p2.X - p1.X) * (py - p2.Y))
                        / denominator;

                    double b =
                        ((p2.Y - p0.Y) * (px - p2.X) +
                         (p0.X - p2.X) * (py - p2.Y))
                        / denominator;

                    double c = 1.0 - a - b;

                    // Точка находится вне треугольника.
                    if (a < -epsilon ||
                        b < -epsilon ||
                        c < -epsilon)
                        continue;

                    // Интерполируем глубину.
                    double interpolated = a * d0 + b * d1 + c * d2;

                    double depth;

                    if (perspective)
                    {
                        // Интерполировалась обратная глубина.
                        if (interpolated <= 0)
                            continue;

                        depth = 1.0 / interpolated;
                    }
                    else
                    {
                        depth = interpolated;
                    }

                    // Проверка Z-буфера.
                    if (depth < zBuffer[x, y])
                    {
                        zBuffer[x, y] = depth;
                        // Интерполяция интенсивности по барицентрическим координатам — шейдинг Гуро.
                        double brightness;
                        if (perspective)
                        {
                            brightness = (a * brightness0 * d0
                                        + b * brightness1 * d1
                                        + c * brightness2 * d2) / interpolated;
                        }
                        else
                        {
                            brightness = a * brightness0 + b * brightness1 + c * brightness2;
                        }
                        bitmap.SetPixel(x, y, Shade(baseColor, brightness));
                    }
                }
            }
        }

        /// <summary>
        /// Рисование ребра с проверкой глубины.
        /// </summary>
        private static void RasterizeEdge(
            Bitmap bitmap,
            double[,] zBuffer,
            PointF p0,
            PointF p1,
            double d0,
            double d1,
            bool perspective,
            Color color)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;

            int steps = (int)Math.Ceiling(Math.Max(Math.Abs(dx), Math.Abs(dy)));

            if (steps == 0)
                steps = 1;

            int width = bitmap.Width;
            int height = bitmap.Height;

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;

                int x = (int)Math.Round(p0.X + t * dx);
                int y = (int)Math.Round(p0.Y + t * dy);

                if (x < 0 || x >= width ||
                    y < 0 || y >= height)
                    continue;

                // Интерполяция глубины вдоль ребра.
                double interpolated = d0 * (1.0 - t) + d1 * t;

                double depth = perspective
                    ? 1.0 / interpolated
                    : interpolated;

                // Небольшой допуск нужен из-за погрешностей
                // вычисления глубины на границе грани.
                double tolerance = 1e-5 * Math.Max(1.0, Math.Abs(depth));

                if (depth <= zBuffer[x, y] + tolerance)
                {
                    bitmap.SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        /// Затемнение цвета согласно освещённости грани.
        /// </summary>
        private static Color Shade(
            Color baseColor,
            double brightness)
        {
            brightness = Math.Max(0.0,
                Math.Min(1.0, brightness));

            return Color.FromArgb(
                Clamp255(baseColor.R * brightness),
                Clamp255(baseColor.G * brightness),
                Clamp255(baseColor.B * brightness));
        }

        private static int Clamp255(double value)
        {
            return Math.Max(0, Math.Min(255, (int)value));
        }
    }
}
