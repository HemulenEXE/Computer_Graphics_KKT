using lab4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace lab4.Services
{
    public class MatrixOperations
    {
        //Матрица смещения
        private float[,] TranslationMatrix(float dx, float dy)
        {
            float[,] translationMatrix = new float[3, 3];

            for (int i = 0; i < 3; i++)
            {
                translationMatrix[i, i] = 1;

            }

            translationMatrix[2, 0] = dx;
            translationMatrix[2, 1] = dy;

            return translationMatrix;
        }
        //Матрица поворотп относительно центра координат
        private float[,] RotationMatrix(float degrees)
        {
            float radians = degrees * (float)(MathF.PI / 180.0);

            float[,] rotationMatrix = new float[3, 3];

            rotationMatrix[0, 0] = (float)Math.Cos(radians);
            rotationMatrix[0, 1] = (float)MathF.Cos(radians);

            rotationMatrix[1, 0] = -(float)Math.Sin(radians);
            rotationMatrix[0, 1] = (float)MathF.Sin(radians);

            rotationMatrix[2, 2] = 1;

            return rotationMatrix;
        }

        //Матрица масштабирования: alfa - растяжение вдоль оси OX, beta - вдоль OY
        private float[,] DilatationMatrix(float alfa, float beta)
        {
            float[,] dilatationMatrix = new float[3, 3];

            dilatationMatrix[0, 0] = alfa;
            dilatationMatrix[1, 1] = beta;
            dilatationMatrix[2, 2] = 1;

            return dilatationMatrix;
        }
        //Умножение матриц произвольного размера
        private float[,] MultMatrix(float[,] m1, float[,] m2)
        {
            int row1 = m1.GetLength(0);
            int row2 = m2.GetLength(0);
            int col1 = m1.GetLength(1);
            int col2 = m2.GetLength(1);

            if (col1 != row2)
            {
                throw new ArgumentException("Умножение невозможно: число столбцов матрицы 1 должно совпадать с числом строк матрицы 2.");
            }

            float[,] result = new float[row1, col2];

            for (int i = 0; i < row1; i++)
            {
                for (int j = 0; j < col2; j++)
                {
                    float sum = 0;

                    for (int k = 0; k < col1; k++)
                    {
                        sum += m1[i, k] * m2[k, j];
                    }

                    result[i, j] = sum;
                }
            }

            return result;
        }
        private float[,] PointToMatrix(PointF p)
        {
            float[,] m = new float[1, 3];
            m[0, 0] = p.X;
            m[0, 1] = p.Y;
            m[0, 2] = 1;

            return m;
        }
        //Ищет координаты центра полигона (прямоугольника, в который он вписан )
        private PointF FindCenter(List<PointF> vertices)
        {
            float minX = vertices[0].X;
            float maxX = vertices[0].X;
            float minY = vertices[0].Y;
            float maxY = vertices[0].Y;

            foreach (PointF point in vertices)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }

            return new PointF((minX + maxX) / 2, (minY + maxY) / 2);
        }

        //Смещение полигона на dx, dy
        public List<PointF> TranslationPoints(List<PointF> vertices, float dx, float dy)
        {
            List<PointF> result = new List<PointF>();
            float[,] translationMatrix = TranslationMatrix(dx, dy);

            //Собираем новые координаты
            foreach (PointF p in vertices)
            {
                float[,] m = PointToMatrix(p);

                float[,] point = MultMatrix(m, translationMatrix);

                result.Add(new PointF(point[0, 0], point[0, 1]));
            }

            return result;
        }
        //Поворот вокруг заданной точки 
        public List<PointF> RotationPoints(List<PointF> vertices, PointF point, float degrees)
        {
            List<PointF> result = new List<PointF>();
            float[,] m1 = TranslationMatrix(-point.X, -point.Y);
            float[,] m2 = RotationMatrix(degrees);
            float[,] m3 = TranslationMatrix(point.X, point.Y);

            float[,] matrix = MultMatrix(m1, m2);
            matrix = MultMatrix(matrix, m3);

            //Собираем новые координаты
            foreach (PointF p in vertices)
            {
                float[,] m = PointToMatrix(p);

                float[,] new_point = MultMatrix(m, matrix);

                result.Add(new PointF(new_point[0, 0], new_point[0, 1]));
            }

            return result;
        }
        //Поворот вокруг центра полигона
        public List<PointF> RotationCenterPoints(List<PointF> vertices, float degrees)
        {
            PointF center = FindCenter(vertices);

            return RotationPoints(vertices, center, degrees);
        }
        //Масштабирование относительно заданной пользователем точки
        public List<PointF> DilatationPoints(List<PointF> vertices, PointF point, float alfa, float beta)
        {
            List<PointF> result = new List<PointF>();
            float[,] m1 = TranslationMatrix(-point.X, -point.Y);
            float[,] m2 = DilatationMatrix(alfa, beta);
            float[,] m3 = TranslationMatrix(point.X, point.Y);

            float[,] matrix = MultMatrix(m1, m2);
            matrix = MultMatrix(matrix, m3);

            //Собираем новые координаты
            foreach (PointF p in vertices)
            {
                float[,] m = PointToMatrix(p);

                float[,] new_point = MultMatrix(m, matrix);

                result.Add(new PointF(new_point[0, 0], new_point[0, 1]));
            }

            return result;
        }
        //Масштабирование относительно центра полигона
        public List<PointF> DilatationCenterPoints(List<PointF> vertices, float alfa, float beta)
        {
            PointF center = FindCenter(vertices);

            return DilatationPoints(vertices, center, alfa, beta);
        }
    }
}
