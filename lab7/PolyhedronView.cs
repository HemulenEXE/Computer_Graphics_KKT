#nullable disable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PolyhedraLab
{
    /// <summary>
    /// Поле, в котором рисуется многогранник. Сам ничего не считает:
    /// берёт готовые экранные многоугольники у Projector и закрашивает их.
    /// </summary>
    public class PolyhedronView : Control
    {
        /// <summary>Рисуемый многогранник (может быть null).</summary>
        public Polyhedron Model { get; set; }

        public ViewSettings Settings { get; private set; }

        public bool ShowAxes { get; set; }
        public bool ShowVertexLabels { get; set; }

        /// <summary>Прямая для поворота (две точки A и B). Рисуется пунктиром, если ShowLine = true.</summary>
        public bool ShowLine { get; set; }
        public Point LineA { get; set; }
        public Point LineB { get; set; }

        /// <summary>Вызывается, когда масштаб изменён колёсиком мыши.</summary>
        public event EventHandler ZoomChanged;

        public PolyhedronView()
        {
            Settings = new ViewSettings();
            ShowAxes = true;
            LineA = new Point(0, 0, 0);
            LineB = new Point(1, 1, 1);
            BackColor = Color.White;

            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Render(e.Graphics, ClientSize);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();                       // чтобы колёсико мыши приходило в это поле
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            double factor = Math.Pow(1.1, e.Delta / 120.0);
            Settings.Zoom = Math.Max(10.0, Math.Min(500.0, Settings.Zoom * factor));
            Invalidate();

            if (ZoomChanged != null)
                ZoomChanged(this, EventArgs.Empty);
        }

        /// <summary>Рисует всю сцену. Вынесено отдельно, чтобы можно было рисовать и не в окне (например, в картинку).</summary>
        public void Render(Graphics g, Size size)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            PointF origin = new PointF(size.Width / 2f, size.Height / 2f);

            if (ShowAxes)
                DrawAxes(g, origin);

            if (Model != null)
            {
                ProjectedPolyhedron projected = Projector.Project(Model, Settings, origin);
                DrawFaces(g, projected);

                if (ShowVertexLabels)
                    DrawVertexLabels(g, projected);
            }

            if (ShowLine)
                DrawRotationLine(g, origin);

            DrawInfo(g, size);
        }

        // ------------------------------------------------------------------

        private void DrawFaces(Graphics g, ProjectedPolyhedron projected)
        {
            Color baseColor = Color.SteelBlue;
            bool solid = Settings.HideBackFaces;     // с отсечением — непрозрачные грани, без — полупрозрачные

            using (Pen outline = new Pen(Color.FromArgb(40, 40, 70), 1.8f))
            {
                outline.LineJoin = LineJoin.Round;

                foreach (ProjectedFace face in projected.Faces)
                {
                    int r = Clamp255(baseColor.R * face.Brightness);
                    int gr = Clamp255(baseColor.G * face.Brightness);
                    int b = Clamp255(baseColor.B * face.Brightness);
                    int alpha = solid ? 255 : 90;

                    using (Brush fill = new SolidBrush(Color.FromArgb(alpha, r, gr, b)))
                    {
                        g.FillPolygon(fill, face.Points);
                    }
                    g.DrawPolygon(outline, face.Points);
                }
            }
        }

        private static int Clamp255(double value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (int)value;
        }

        private void DrawVertexLabels(Graphics g, ProjectedPolyhedron projected)
        {
            using (Font font = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                for (int i = 0; i < projected.Vertices.Length; i++)
                {
                    PointF? p = projected.Vertices[i];
                    if (!p.HasValue) continue;

                    g.FillEllipse(Brushes.Black, p.Value.X - 3, p.Value.Y - 3, 6, 6);
                    g.DrawString("V" + i, font, Brushes.Black, p.Value.X + 4, p.Value.Y - 16);
                }
            }
        }

        private void DrawAxes(Graphics g, PointF origin)
        {
            const double length = 3.0;

            DrawAxis(g, origin, length, 1, 0, 0, Color.Firebrick, "X");
            DrawAxis(g, origin, length, 0, 1, 0, Color.ForestGreen, "Y");
            DrawAxis(g, origin, length, 0, 0, 1, Color.RoyalBlue, "Z");
        }

        private void DrawAxis(Graphics g, PointF origin, double length, double dx, double dy, double dz, Color color, string name)
        {
            PointF? zero = Projector.ProjectPoint(0, 0, 0, Settings, origin);
            PointF? plus = Projector.ProjectPoint(dx * length, dy * length, dz * length, Settings, origin);
            PointF? minus = Projector.ProjectPoint(-dx * length, -dy * length, -dz * length, Settings, origin);

            using (Pen solid = new Pen(color, 1.6f))
            using (Pen faint = new Pen(Color.FromArgb(110, color), 1.2f))
            using (Brush text = new SolidBrush(color))
            using (Font font = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                faint.DashStyle = DashStyle.Dash;

                if (zero.HasValue && minus.HasValue)
                    g.DrawLine(faint, minus.Value, zero.Value);

                if (zero.HasValue && plus.HasValue)
                {
                    g.DrawLine(solid, zero.Value, plus.Value);
                    g.DrawString(name, font, text, plus.Value.X + 3, plus.Value.Y - 8);
                }
            }
        }

        private void DrawRotationLine(Graphics g, PointF origin)
        {
            double dx = LineB.X - LineA.X;
            double dy = LineB.Y - LineA.Y;
            double dz = LineB.Z - LineA.Z;
            double len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (len < 1e-9) return;
            dx /= len; dy /= len; dz /= len;

            const double extent = 4.0;      // насколько продолжить прямую в обе стороны, в единицах длины

            PointF? p1 = Projector.ProjectPoint(LineA.X - dx * extent, LineA.Y - dy * extent, LineA.Z - dz * extent, Settings, origin);
            PointF? p2 = Projector.ProjectPoint(LineA.X + dx * extent, LineA.Y + dy * extent, LineA.Z + dz * extent, Settings, origin);
            PointF? a = Projector.ProjectPoint(LineA.X, LineA.Y, LineA.Z, Settings, origin);
            PointF? b = Projector.ProjectPoint(LineB.X, LineB.Y, LineB.Z, Settings, origin);

            using (Pen pen = new Pen(Color.MediumVioletRed, 2f))
            using (Brush brush = new SolidBrush(Color.MediumVioletRed))
            using (Font font = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                pen.DashStyle = DashStyle.Dash;

                if (p1.HasValue && p2.HasValue)
                    g.DrawLine(pen, p1.Value, p2.Value);

                if (a.HasValue)
                {
                    g.FillEllipse(brush, a.Value.X - 4, a.Value.Y - 4, 8, 8);
                    g.DrawString("A", font, brush, a.Value.X + 5, a.Value.Y - 18);
                }
                if (b.HasValue)
                {
                    g.FillEllipse(brush, b.Value.X - 4, b.Value.Y - 4, 8, 8);
                    g.DrawString("B", font, brush, b.Value.X + 5, b.Value.Y - 18);
                }
            }
        }

        private void DrawInfo(Graphics g, Size size)
        {
            string kind = Settings.Kind == ProjectionKind.Perspective
                ? "перспективная (d = " + Settings.Distance.ToString("0.##") + ")"
                : "аксонометрическая";

            string text = "Проекция: " + kind
                + ";  вид: Y " + Settings.Yaw.ToString("0.##") + "°, X " + Settings.Pitch.ToString("0.##") + "°"
                + ";  масштаб " + Settings.Zoom.ToString("0") + " пикс/ед.";

            using (Font font = new Font("Segoe UI", 9f))
            {
                g.DrawString(text, font, Brushes.DimGray, 8, size.Height - 22);
            }
        }
    }
}
