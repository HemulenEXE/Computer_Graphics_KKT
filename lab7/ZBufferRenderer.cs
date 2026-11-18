
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
            return Render(
                faces,
                width,
                height,
                baseColor,
                outlineColor,
                ShadingMode.Gouraud,
                new Point(4, 5, 6),
                0.25,
                0.75);
        }

        public static Bitmap Render(
            IList<ProjectedFace> faces,
            int width,
            int height,
            Color baseColor,
            Color outlineColor,
            ShadingMode shadingMode,
            Point lightPosition,
            double ambientStrength,
            double diffuseStrength)
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
                Point[]? vertexNormals = face.VertexNormals;
                Point[]? vertexPositions = face.VertexPositions;
                if (face.Points == null ||
                    face.DepthValues == null ||
                    face.Points.Length < 3 ||
                    face.Points.Length != face.DepthValues.Length)
                    continue;

                if (face.VertexBrightness == null ||
                    face.VertexBrightness.Length != face.Points.Length)
                    continue;
                if (shadingMode == ShadingMode.Phong &&
                    (vertexNormals == null ||
                     vertexNormals.Length != face.Points.Length ||
                     vertexPositions == null ||
                     vertexPositions.Length != face.Points.Length))
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
                        face.VertexBrightness[i + 1],
                        shadingMode == ShadingMode.Phong ? vertexNormals![0] : null,
                        shadingMode == ShadingMode.Phong ? vertexNormals![i] : null,
                        shadingMode == ShadingMode.Phong ? vertexNormals![i + 1] : null,
                        shadingMode == ShadingMode.Phong ? vertexPositions![0] : null,
                        shadingMode == ShadingMode.Phong ? vertexPositions![i] : null,
                        shadingMode == ShadingMode.Phong ? vertexPositions![i + 1] : null,
                        lightPosition,
                        ambientStrength,
                        diffuseStrength);
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
            double brightness2,
            Point? normal0,
            Point? normal1,
            Point? normal2,
            Point? position0,
            Point? position1,
            Point? position2,
            Point lightPosition,
            double ambientStrength,
            double diffuseStrength)
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
                        double brightness = 0;
                        if (normal0 != null && normal1 != null && normal2 != null
                            && position0 != null && position1 != null && position2 != null)
                        {
                            double nx, ny, nz;
                            double worldX, worldY, worldZ;
                            if (perspective)
                            {
                                nx = (a * normal0.X * d0 + b * normal1.X * d1 + c * normal2.X * d2) / interpolated;
                                ny = (a * normal0.Y * d0 + b * normal1.Y * d1 + c * normal2.Y * d2) / interpolated;
                                nz = (a * normal0.Z * d0 + b * normal1.Z * d1 + c * normal2.Z * d2) / interpolated;
                                worldX = (a * position0.X * d0 + b * position1.X * d1 + c * position2.X * d2) / interpolated;
                                worldY = (a * position0.Y * d0 + b * position1.Y * d1 + c * position2.Y * d2) / interpolated;
                                worldZ = (a * position0.Z * d0 + b * position1.Z * d1 + c * position2.Z * d2) / interpolated;
                            }
                            else
                            {
                                nx = a * normal0.X + b * normal1.X + c * normal2.X;
                                ny = a * normal0.Y + b * normal1.Y + c * normal2.Y;
                                nz = a * normal0.Z + b * normal1.Z + c * normal2.Z;
                                worldX = a * position0.X + b * position1.X + c * position2.X;
                                worldY = a * position0.Y + b * position1.Y + c * position2.Y;
                                worldZ = a * position0.Z + b * position1.Z + c * position2.Z;
                            }

                            double normalLength = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                            double lx = lightPosition.X - worldX;
                            double ly = lightPosition.Y - worldY;
                            double lz = lightPosition.Z - worldZ;
                            double lightLength = Math.Sqrt(lx * lx + ly * ly + lz * lz);
                            double lambert = 0;
                            if (normalLength > 1e-12 && lightLength > 1e-12)
                            {
                                lambert = Math.Max(0,
                                    (nx * lx + ny * ly + nz * lz) / (normalLength * lightLength));
                            }

                            double ambient = Math.Max(0, Math.Min(1, ambientStrength));
                            double diffuse = Math.Max(0, Math.Min(1 - ambient, diffuseStrength));
                            brightness = ambient + diffuse * lambert;
                        }
                        else
                        {
                            // Интерполяция интенсивности по барицентрическим координатам — шейдинг Гуро.
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
