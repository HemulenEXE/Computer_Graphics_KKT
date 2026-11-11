using System;
using System.Collections.Generic;
using System.Drawing;

namespace PolyhedraLab
{
    /// <summary>Параметры просмотра (проекция, углы вида, расстояние до камеры, масштаб).</summary>
    public sealed class ViewSettings
    {
        public ProjectionKind Kind = ProjectionKind.Axonometric;
        public Camera Camera { get; } = new Camera();

        /// <summary>Азимут камеры вокруг цели, градусы.</summary>
        public double Yaw = 45.0;

        /// <summary>Наклон камеры относительно плоскости XZ, градусы.</summary>
        public double Pitch = 35.264;

        /// <summary>Расстояние камеры до цели и фокусное расстояние перспективной проекции.</summary>
        public double Distance = 6.0;

        /// <summary>Сколько пикселей в одной единице длины.</summary>
        public double Zoom = 90.0;

        /// <summary>Не рисовать нелицевые (обращённые от зрителя) грани.</summary>
        public bool HideBackFaces = true;

        public ViewSettings()
        {
            UpdateCamera();
        }

        public void UpdateCamera()
        {
            Camera.Orbit(Camera.Target, Yaw, Pitch, Distance, Kind);
        }
    }

    /// <summary>Грань после проецирования: готовые экранные точки и вспомогательные данные.</summary>
    public sealed class ProjectedFace
    {
        public int Index;
        public PointF[] Points = new PointF[0];
        public double Brightness;   // 0..1, освещённость грани
        public bool IsFront;        // обращена ли грань к зрителю
        public double Depth;        // глубина центра грани (чем больше, тем ближе к зрителю)

        // Значение глубины для каждой вершины.
        public double[] DepthValues = new double[0];

        // Тип проекции, использованный при вычислении глубины.
        public bool IsPerspective;

        public PointF? NormalStart;
        public PointF? NormalEnd;
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

        /// <summary>Проекция одной точки пространства (для осей, прямой и т.п.). null — точка за камерой.</summary>
        public static PointF? ProjectPoint(double x, double y, double z, ViewSettings s, PointF origin)
        {
            s.UpdateCamera();
            double[,] view = s.Camera.ViewMatrix;
            double[,] proj = s.Camera.ProjectionMatrix;
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

        public static ProjectedPolyhedron Project(
            Polyhedron polyhedron,
            ViewSettings settings,
            PointF origin)
        {
            return Project(
                    polyhedron,
                    settings,
                    origin,
                    0, 0, 0);
        }
        public static ProjectedPolyhedron Project(
            Polyhedron poly,
            ViewSettings s,
            PointF origin,
            double offsetX,
            double offsetY,
            double offsetZ)
        {
            s.UpdateCamera();
            poly.UpdateFaceNormals();
            double[,] view = s.Camera.ViewMatrix;
            double[,] proj = s.Camera.ProjectionMatrix;
            bool perspective = s.Kind == ProjectionKind.Perspective;

            int n = poly.Vertices.Count;
            double[][] vv = new double[n][];          // вершины в системе координат вида

            for (int i = 0; i < n; i++)
            {
                Point p = poly.Vertices[i];
                double[] t = Projection.Transform(view,
                                                  p.X + offsetX,
                                                  p.Y + offsetY,
                                                  p.Z + offsetZ);
                vv[i] = t;
            }

            ProjectedPolyhedron result = new ProjectedPolyhedron();
            result.Vertices = new PointF?[n];
            for (int i = 0; i < n; i++)
                result.Vertices[i] = ProjectViewSpace(vv[i][0], vv[i][1], vv[i][2], proj, s, origin);

            for (int f = 0; f < poly.Faces.Count; f++)
            {
                Polygon polygon = poly.Faces[f];
                Point normal = polygon.Normal;
                if (normal.X == 0 && normal.Y == 0 && normal.Z == 0)
                    continue;

                Point center = polygon.Center;
                // Вектор ОТ наблюдателя К грани. В параллельной проекции общий
                // для всех граней, в перспективной зависит от положения грани.
                Point direction = perspective
                    ? new Point(center.X + offsetX - s.Camera.Position.X,
                                center.Y + offsetY - s.Camera.Position.Y,
                                center.Z + offsetZ - s.Camera.Position.Z)
                    : s.Camera.Direction;
                double directionLength = Math.Sqrt(direction.X * direction.X
                    + direction.Y * direction.Y + direction.Z * direction.Z);
                double dot = normal.X * direction.X + normal.Y * direction.Y + normal.Z * direction.Z;
                bool front = dot < -1e-9 * directionLength;

                // Отбрасываем нелицевую грань до передачи в Z-буфер.
                if (s.HideBackFaces && !front)
                    continue;

                IReadOnlyList<int> idx = polygon.VertexIndices;
                int k = idx.Count;

                PointF[] pts = new PointF[k];
                //Добавляем вычисление глубины для каждой вершины
                double[] depthValues = new double[k];

                bool ok = true;

                for (int j = 0; j < k; j++)
                {
                    int vertexIndex = idx[j];

                    PointF? pf = result.Vertices[vertexIndex];

                    if (!pf.HasValue)
                    {
                        ok = false;
                        break;
                    }

                    pts[j] = pf.Value;

                    double[] v = vv[vertexIndex];

                    if (perspective)
                    {
                        double cameraDistance = -v[2];

                        if (cameraDistance <= 0)
                        {
                            ok = false;
                            break;
                        }

                        // Обратная глубина. Линейно интерполируется в экранном пространстве.
                        depthValues[j] = 1.0 / cameraDistance;
                    }
                    else
                    {
                        // В параллельной проекции: большая координата Z означает более близкую точку.
                        // Поэтому используем -Z.
                        depthValues[j] = -v[2];
                    }
                }
                if (!ok) continue;

                // Нормаль поворачиваем в систему камеры без переноса.
                double nx = normal.X * view[0, 0] + normal.Y * view[1, 0] + normal.Z * view[2, 0];
                double ny = normal.X * view[0, 1] + normal.Y * view[1, 1] + normal.Z * view[2, 1];
                double nz = normal.X * view[0, 2] + normal.Y * view[1, 2] + normal.Z * view[2, 2];
                double[] fc = Projection.Transform(view, center.X + offsetX, center.Y + offsetY, center.Z + offsetZ);

                double light = nx * LightDirection[0] + ny * LightDirection[1] + nz * LightDirection[2];

                ProjectedFace face = new ProjectedFace();
                face.Index = f;
                face.Points = pts;
                face.IsFront = front;
                face.Depth = fc[2];
                face.DepthValues = depthValues;
                face.IsPerspective = perspective;
                face.Brightness = 0.35 + 0.65 * Math.Max(0.0, light);
                face.NormalStart = ProjectViewSpace(fc[0], fc[1], fc[2], proj, s, origin);
                face.NormalEnd = ProjectViewSpace(fc[0] + nx * 0.35, fc[1] + ny * 0.35, fc[2] + nz * 0.35, proj, s, origin);
                result.Faces.Add(face);
            }

            // Дальние грани рисуем первыми.
            result.Faces.Sort(delegate (ProjectedFace a, ProjectedFace b) { return a.Depth.CompareTo(b.Depth); });
            return result;
        }
    }
}
