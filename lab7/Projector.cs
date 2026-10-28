using System;
using System.Collections.Generic;
using System.Drawing;

namespace PolyhedraLab
{
    /// <summary>Параметры просмотра (проекция, углы вида, расстояние до камеры, масштаб).</summary>
    public sealed class ViewSettings
    {
        public ProjectionKind Kind = ProjectionKind.Axonometric;

        /// <summary>Поворот вида вокруг оси OY, градусы.</summary>
        public double Yaw = 45.0;

        /// <summary>Поворот вида вокруг оси OX, градусы.</summary>
        public double Pitch = 35.264;

        /// <summary>Расстояние от плоскости проекции до камеры (для перспективы).</summary>
        public double Distance = 6.0;

        /// <summary>Сколько пикселей в одной единице длины.</summary>
        public double Zoom = 90.0;

        /// <summary>Не рисовать нелицевые (обращённые от зрителя) грани.</summary>
        public bool HideBackFaces = true;
    }

    /// <summary>Грань после проецирования: готовые экранные точки и вспомогательные данные.</summary>
    public sealed class ProjectedFace
    {
        public int Index;
        public PointF[] Points = new PointF[0];
        public double Brightness;   // 0..1, освещённость грани
        public bool IsFront;        // обращена ли грань к зрителю
        public double Depth;        // глубина центра грани (чем больше, тем ближе к зрителю)
    }

    public sealed class ProjectedPolyhedron
    {
        /// <summary>Экранные координаты вершин; null — вершина за камерой и не может быть показана.</summary>
        public PointF?[] Vertices = new PointF?[0];
        public List<ProjectedFace> Faces = new List<ProjectedFace>();
    }

    /// <summary>
    /// Превращает многогранник (вершины + грани) в набор экранных многоугольников.
    /// Рисования здесь нет — только вычисления, поэтому класс легко проверить отдельно.
    /// </summary>
    public static class Projector
    {
        // Точки, у которых w' меньше этого значения, считаются находящимися за камерой.
        private const double NearLimit = 0.05;

        // Направление на источник света в системе координат вида (y вверх, z к зрителю).
        private static readonly double[] LightDirection = Normalize(-0.4, 0.6, 1.0);

        private static double[] Normalize(double x, double y, double z)
        {
            double len = Math.Sqrt(x * x + y * y + z * z);
            return new double[] { x / len, y / len, z / len };
        }

        private static double[,] ProjectionMatrix(ViewSettings s)
        {
            if (s.Kind == ProjectionKind.Perspective)
                return Projection.Perspective(Math.Max(0.1, s.Distance));
            return Projection.Orthographic();
        }

        /// <summary>Проекция одной точки пространства (для осей, прямой и т.п.). null — точка за камерой.</summary>
        public static PointF? ProjectPoint(double x, double y, double z, ViewSettings s, PointF origin)
        {
            double[,] view = Projection.View(s.Yaw, s.Pitch);
            double[,] proj = ProjectionMatrix(s);
            double[] v = Projection.Transform(view, x, y, z);
            return ProjectViewSpace(v[0], v[1], v[2], proj, s, origin);
        }

        private static PointF? ProjectViewSpace(double vx, double vy, double vz, double[,] proj, ViewSettings s, PointF origin)
        {
            double[] h = Projection.Transform(proj, vx, vy, vz);
            double w = h[3];
            if (w < NearLimit)
                return null;

            double x = h[0] / w;     // перспективное деление
            double y = h[1] / w;

            // Экранная Y направлена вниз, поэтому знак меняем.
            return new PointF((float)(origin.X + x * s.Zoom), (float)(origin.Y - y * s.Zoom));
        }

        public static ProjectedPolyhedron Project(Polyhedron poly, ViewSettings s, PointF origin)
        {
            double[,] view = Projection.View(s.Yaw, s.Pitch);
            double[,] proj = ProjectionMatrix(s);
            bool perspective = s.Kind == ProjectionKind.Perspective;
            double d = Math.Max(0.1, s.Distance);

            int n = poly.Vertices.Count;
            double[][] vv = new double[n][];          // вершины в системе координат вида
            double cx = 0, cy = 0, cz = 0;            // центр многогранника в той же системе

            for (int i = 0; i < n; i++)
            {
                Point p = poly.Vertices[i];
                double[] t = Projection.Transform(view, p.X, p.Y, p.Z);
                vv[i] = t;
                cx += t[0]; cy += t[1]; cz += t[2];
            }
            cx /= n; cy /= n; cz /= n;

            ProjectedPolyhedron result = new ProjectedPolyhedron();
            result.Vertices = new PointF?[n];
            for (int i = 0; i < n; i++)
                result.Vertices[i] = ProjectViewSpace(vv[i][0], vv[i][1], vv[i][2], proj, s, origin);

            for (int f = 0; f < poly.Faces.Count; f++)
            {
                IReadOnlyList<int> idx = poly.Faces[f].VertexIndices;
                int k = idx.Count;

                PointF[] pts = new PointF[k];
                bool ok = true;
                for (int j = 0; j < k; j++)
                {
                    PointF? pf = result.Vertices[idx[j]];
                    if (!pf.HasValue) { ok = false; break; }
                    pts[j] = pf.Value;
                }
                if (!ok) continue;

                // Нормаль грани (метод Ньюэлла) и центр грани в системе вида.
                double nx = 0, ny = 0, nz = 0, fx = 0, fy = 0, fz = 0;
                for (int j = 0; j < k; j++)
                {
                    double[] a = vv[idx[j]];
                    double[] b = vv[idx[(j + 1) % k]];
                    nx += (a[1] - b[1]) * (a[2] + b[2]);
                    ny += (a[2] - b[2]) * (a[0] + b[0]);
                    nz += (a[0] - b[0]) * (a[1] + b[1]);
                    fx += a[0]; fy += a[1]; fz += a[2];
                }
                fx /= k; fy /= k; fz /= k;

                double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (len < 1e-12) continue;
                nx /= len; ny /= len; nz /= len;

                // Делаем нормаль «наружу» (от центра многогранника). Это не зависит от порядка обхода вершин,
                // поэтому остаётся верным и после отражения, когда порядок обхода «переворачивается».
                if (nx * (fx - cx) + ny * (fy - cy) + nz * (fz - cz) < 0)
                {
                    nx = -nx; ny = -ny; nz = -nz;
                }

                bool front;
                if (perspective)
                    front = nx * (0 - fx) + ny * (0 - fy) + nz * (d - fz) > 0;   // камера в точке (0, 0, d)
                else
                    front = nz > 1e-9;                                            // зритель смотрит вдоль −Z

                if (s.HideBackFaces && !front) continue;

                double light = nx * LightDirection[0] + ny * LightDirection[1] + nz * LightDirection[2];

                ProjectedFace face = new ProjectedFace();
                face.Index = f;
                face.Points = pts;
                face.IsFront = front;
                face.Depth = fz;
                face.Brightness = 0.35 + 0.65 * Math.Max(0.0, light);
                result.Faces.Add(face);
            }

            // Дальние грани рисуем первыми.
            result.Faces.Sort(delegate (ProjectedFace a, ProjectedFace b) { return a.Depth.CompareTo(b.Depth); });
            return result;
        }
    }
}
