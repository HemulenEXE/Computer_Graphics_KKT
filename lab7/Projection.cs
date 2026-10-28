using System;

namespace PolyhedraLab
{
    /// <summary>Тип проекции.</summary>
    public enum ProjectionKind
    {
        Axonometric,
        Perspective
    }

    /// <summary>
    /// Матрицы 4x4 для проецирования. Соглашение то же, что в MatrixOperations:
    /// точка — строка (x y z 1), преобразование — умножение справа: p' = p * M.
    /// Все проекции тоже сделаны матрицами, как требует задание.
    /// </summary>
    public static class Projection
    {
        public static double[,] Identity()
        {
            double[,] m = new double[4, 4];
            for (int i = 0; i < 4; i++)
                m[i, i] = 1;
            return m;
        }

        /// <summary>Поворот вокруг оси OX (та же матрица, что в MatrixOperations).</summary>
        public static double[,] RotationX(double degrees)
        {
            double r = degrees * Math.PI / 180.0;
            double c = Math.Cos(r), s = Math.Sin(r);

            double[,] m = Identity();
            m[1, 1] = c; m[1, 2] = s;
            m[2, 1] = -s; m[2, 2] = c;
            return m;
        }

        /// <summary>Поворот вокруг оси OY (та же матрица, что в MatrixOperations).</summary>
        public static double[,] RotationY(double degrees)
        {
            double r = degrees * Math.PI / 180.0;
            double c = Math.Cos(r), s = Math.Sin(r);

            double[,] m = Identity();
            m[0, 0] = c; m[0, 2] = -s;
            m[2, 0] = s; m[2, 2] = c;
            return m;
        }

        public static double[,] Multiply(double[,] a, double[,] b)
        {
            double[,] r = new double[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < 4; k++)
                        sum += a[i, k] * b[k, j];
                    r[i, j] = sum;
                }
            return r;
        }

        /// <summary>
        /// Матрица вида: поворачивает сцену, чтобы смотреть на неё «под углом».
        /// Сначала поворот вокруг OY на yaw, потом вокруг OX на pitch.
        /// После неё зритель находится на оси +Z и смотрит в сторону −Z, ось Y направлена вверх.
        /// </summary>
        public static double[,] View(double yawDegrees, double pitchDegrees)
        {
            return Multiply(RotationY(yawDegrees), RotationX(pitchDegrees));
        }

        /// <summary>
        /// Аксонометрия (ортогональная проекция на плоскость XY): координата Z отбрасывается.
        /// Вид «из угла» задаётся матрицей View. При yaw = 45° и pitch = 35,264° получается изометрия.
        /// </summary>
        public static double[,] Orthographic()
        {
            double[,] m = Identity();
            m[2, 2] = 0;
            return m;
        }

        /// <summary>
        /// Перспектива: центр проекции в точке (0, 0, d), плоскость проекции z = 0.
        /// В однородных координатах w' = 1 − z/d, после деления на w' удалённые точки становятся меньше.
        /// </summary>
        public static double[,] Perspective(double distance)
        {
            double[,] m = Identity();
            m[2, 2] = 0;
            m[2, 3] = -1.0 / distance;
            return m;
        }

        /// <summary>Умножение строки (x y z 1) на матрицу 4x4. Возвращает (x', y', z', w').</summary>
        public static double[] Transform(double[,] m, double x, double y, double z)
        {
            double[] result = new double[4];
            for (int j = 0; j < 4; j++)
                result[j] = x * m[0, j] + y * m[1, j] + z * m[2, j] + m[3, j];
            return result;
        }
    }
}
