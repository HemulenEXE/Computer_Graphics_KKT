using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;

namespace PolyhedraLab
{
    public class MatrixOperations
    {
        // Матрица смещения
        private double[,] TranslationMatrix(double dx, double dy, double dz)
        {
            double[,] translationMatrix = new double[4, 4];

            for (int i = 0; i < 4; i++)
                translationMatrix[i, i] = 1;

            translationMatrix[3, 0] = dx;
            translationMatrix[3, 1] = dy;
            translationMatrix[3, 2] = dz;

            return translationMatrix;
        }

        // Матрица поворота вокруг оси OX
        private double[,] RotationXMatrix(double degrees)
        {
            double radians = degrees * (Math.PI / 180.0);
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            double[,] matrix = new double[4, 4];

            matrix[0, 0] = 1;
            matrix[1, 1] = c;
            matrix[1, 2] = s;
            matrix[2, 1] = -s;
            matrix[2, 2] = c;
            matrix[3, 3] = 1;

            return matrix;
        }

        // Матрица поворота вокруг оси OY
        private double[,] RotationYMatrix(double degrees)
        {
            double radians = degrees * (Math.PI / 180.0);
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            double[,] matrix = new double[4, 4];

            matrix[0, 0] = c;
            matrix[0, 2] = -s;
            matrix[1, 1] = 1;
            matrix[2, 0] = s;
            matrix[2, 2] = c;
            matrix[3, 3] = 1;

            return matrix;
        }

        // Матрица поворота вокруг оси OZ
        private double[,] RotationZMatrix(double degrees)
        {
            double radians = degrees * (Math.PI / 180.0);
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            double[,] matrix = new double[4, 4];

            matrix[0, 0] = c;
            matrix[0, 1] = s;
            matrix[1, 0] = -s;
            matrix[1, 1] = c;
            matrix[2, 2] = 1;
            matrix[3, 3] = 1;

            return matrix;
        }
        //Матрица поворота вокруг произвольной прямой l, заданной двумя точками
        private double[,] RotationLMatrix(Point a, Point b, double degrees)
        {
            double radians = degrees * (Math.PI / 180.0);
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);

            //Находим единичный направляющий вектор
            double l = b.X - a.X;
            double m = b.Y - a.Y;
            double n = b.Z - a.Z;
            double length = Math.Sqrt(l * l + m * m + n * n);
            if (length < 1e-12)
                throw new ArgumentException("Точки прямой не должны совпадать.");
            l /= length;
            m /= length;
            n /= length;

            double[,] matrix = new double[4, 4];

            matrix[0, 0] = l * l + c * (1 - l * l);
            matrix[0, 1] = l * (1 - c) * m + n * s;
            matrix[0, 2] = l * (1 - c) * n - m * s;
            matrix[1, 0] = l * (1 - c) * m - n * s;
            matrix[1, 1] = m * m + c * (1 - m * m);
            matrix[1, 2] = m * (1 - c) * n + l * s;
            matrix[2, 0] = l * (1 - c) * n + m * s;
            matrix[2, 1] = m * (1 - c) * n - l * s;
            matrix[2, 2] = n * n + c * (1 - n * n);
            matrix[3, 3] = 1;

            return matrix;

        }

        // Матрица масштабирования:alpha - масштабирование вдоль OX,beta - вдоль OY, gamma - вдоль OZ
        private double[,] ScaleMatrix(double alpha, double beta, double gamma)
        {
            double[,] matrix = new double[4, 4];

            matrix[0, 0] = alpha;
            matrix[1, 1] = beta;
            matrix[2, 2] = gamma;
            matrix[3, 3] = 1;

            return matrix;
        }

        // Матрица отражения относительно координатной плоскости
        private double[,] ReflectionMatrix(string plane)
        {
            double[,] matrix = new double[4, 4];

            matrix[0, 0] = 1;
            matrix[1, 1] = 1;
            matrix[2, 2] = 1;
            matrix[3, 3] = 1;

            switch (plane.ToUpperInvariant())
            {
                // Плоскость XY: меняется знак Z
                case "XY":
                    matrix[2, 2] = -1;
                    break;

                // Плоскость XZ: меняется знак Y
                case "XZ":
                    matrix[1, 1] = -1;
                    break;

                // Плоскость YZ: меняется знак X
                case "YZ":
                    matrix[0, 0] = -1;
                    break;

                default:
                    throw new ArgumentException("Плоскость должна быть XY, XZ или YZ.");
            }

            return matrix;
        }

        // Умножение матриц произвольного размера
        private double[,] MultMatrix(double[,] m1, double[,] m2)
        {
            int row1 = m1.GetLength(0);
            int row2 = m2.GetLength(0);
            int col1 = m1.GetLength(1);
            int col2 = m2.GetLength(1);

            if (col1 != row2)
                throw new ArgumentException("Умножение невозможно: число столбцов матрицы 1 " + "должно совпадать с числом строк матрицы 2.");

            double[,] result = new double[row1, col2];

            for (int i = 0; i < row1; i++)
            {
                for (int j = 0; j < col2; j++)
                {
                    double sum = 0;

                    for (int k = 0; k < col1; k++)
                        sum += m1[i, k] * m2[k, j];

                    result[i, j] = sum;
                }
            }

            return result;
        }

        // Преобразование точки в матрицу-строку
        private double[,] PointToMatrix(Point p)
        {
            double[,] matrix = new double[1, 4];

            matrix[0, 0] = p.X;
            matrix[0, 1] = p.Y;
            matrix[0, 2] = p.Z;
            matrix[0, 3] = 1;

            return matrix;
        }

        // Поиск центра многогранника: центр ограничивающего параллелепипеда
        private Point FindCenter(List<Point> vertices)
        {
            if (vertices == null || vertices.Count == 0)
                throw new ArgumentException("Список вершин пуст.");

            double minX = vertices[0].X;
            double maxX = vertices[0].X;
            double minY = vertices[0].Y;
            double maxY = vertices[0].Y;
            double minZ = vertices[0].Z;
            double maxZ = vertices[0].Z;

            foreach (Point point in vertices)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);

                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);

                minZ = Math.Min(minZ, point.Z);
                maxZ = Math.Max(maxZ, point.Z);
            }

            return new Point(
                (minX + maxX) / 2,
                (minY + maxY) / 2,
                (minZ + maxZ) / 2);
        }

        // Применение матрицы преобразования ко всем вершинам
        private List<Point> TransformPoints(List<Point> vertices, double[,] matrix)
        {
            List<Point> result = new List<Point>();

            foreach (Point p in vertices)
            {
                double[,] point = PointToMatrix(p);
                double[,] newPoint = MultMatrix(point, matrix);

                result.Add(new Point(
                    newPoint[0, 0],
                    newPoint[0, 1],
                    newPoint[0, 2]));
            }

            return result;
        }

        // Смещение многогранника на dx, dy, dz
        public List<Point> TranslationPoints(List<Point> vertices, float dx, float dy, float dz)
        {
            return TransformPoints(vertices, TranslationMatrix(dx, dy, dz));
        }

        // Поворот относительно начала координат вокруг выбранной оси
        public List<Point> RotationPoints(List<Point> vertices, char axis, float degrees)
        {
            double[,] matrix;

            switch (char.ToUpperInvariant(axis))
            {
                case 'X':
                    matrix = RotationXMatrix(degrees);
                    break;
                case 'Y':
                    matrix = RotationYMatrix(degrees);
                    break;
                case 'Z':
                    matrix = RotationZMatrix(degrees);
                    break;
                default:
                    throw new ArgumentException("Ось должна быть X, Y или Z.");
            }

            return TransformPoints(vertices, matrix);
        }

        // Поворот вокруг прямой, проходящей через заданную точку параллельно выбранной координатной оси
        public List<Point> RotationPoints(List<Point> vertices, Point point, char axis, float degrees)
        {
            double[,] m1 = TranslationMatrix(-point.X, -point.Y, -point.Z);

            double[,] m2;

            switch (char.ToUpperInvariant(axis))
            {
                case 'X':
                    m2 = RotationXMatrix(degrees);
                    break;
                case 'Y':
                    m2 = RotationYMatrix(degrees);
                    break;
                case 'Z':
                    m2 = RotationZMatrix(degrees);
                    break;
                default:
                    throw new ArgumentException("Ось должна быть X, Y или Z.");
            }

            double[,] m3 = TranslationMatrix(point.X, point.Y, point.Z);

            double[,] matrix = MultMatrix(m1, m2);
            matrix = MultMatrix(matrix, m3);

            return TransformPoints(vertices, matrix);
        }

        // Поворот вокруг прямой, параллельной оси и проходящей через центр многогранника
        public List<Point> RotationCenterPoints(List<Point> vertices, char axis, float degrees)
        {
            Point center = FindCenter(vertices);

            return RotationPoints(vertices, center, axis, degrees);
        }

        // Масштабирование относительно начала координат
        public List<Point> ScalePoints(List<Point> vertices, float alpha, float beta, float gamma)
        {
            return TransformPoints(vertices, ScaleMatrix(alpha, beta, gamma));
        }

        // Масштабирование относительно заданной точки
        public List<Point> ScalePoints(List<Point> vertices, Point point, float alpha, float beta, float gamma)
        {
            double[,] m1 = TranslationMatrix(
                -point.X, -point.Y, -point.Z);

            double[,] m2 = ScaleMatrix(alpha, beta, gamma);

            double[,] m3 = TranslationMatrix(
                point.X, point.Y, point.Z);

            double[,] matrix = MultMatrix(m1, m2);
            matrix = MultMatrix(matrix, m3);

            return TransformPoints(vertices, matrix);
        }

        // Масштабирование относительно центра многогранника
        public List<Point> DilatationCenterPoints(List<Point> vertices, float alpha, float beta, float gamma)
        {
            Point center = FindCenter(vertices);

            return ScalePoints(vertices, center, alpha, beta, gamma);
        }

        // Отражение относительно выбранной координатной плоскости: "XY", "XZ" или "YZ"
        public List<Point> ReflectionPoints(List<Point> vertices, string plane)
        {
            return TransformPoints(vertices, ReflectionMatrix(plane));
        }

        // Поворот вокруг произвольной прямой, заданной двумя точками p1 и p2
        public List<Point> RotationAroundLinePoints(List<Point> vertices, Point p1, Point p2,float degrees)
        {
            // Прямая проходит через p1, а не через начало координат: переносим p1 в начало, вращаем, возвращаем.
            double[,] m1 = TranslationMatrix(-p1.X, -p1.Y, -p1.Z);
            double[,] m2 = RotationLMatrix(p1, p2, degrees);
            double[,] m3 = TranslationMatrix(p1.X, p1.Y, p1.Z);

            double[,] matrix = MultMatrix(m1, m2);
            matrix = MultMatrix(matrix, m3);

            return TransformPoints(vertices, matrix);
        }
    }
}
