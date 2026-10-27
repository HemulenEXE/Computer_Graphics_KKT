using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lab7Bezier
{
    public struct BezierSegment
    {
        public PointF P0, P1, P2, P3;

        public BezierSegment(PointF p0, PointF p1, PointF p2, PointF p3)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3;
        }

        public PointF At(double t)
        {
            double u = 1.0 - t;
            double b0 = u * u * u;
            double b1 = 3.0 * u * u * t;
            double b2 = 3.0 * u * t * t;
            double b3 = t * t * t;

            return new PointF(
                (float)(b0 * P0.X + b1 * P1.X + b2 * P2.X + b3 * P3.X),
                (float)(b0 * P0.Y + b1 * P1.Y + b2 * P2.Y + b3 * P3.Y));
        }
    }

   
    public static class BezierSpline
    {
        /// <param name="pts">Точки, через которые проходит кривая (по порядку).</param>
        /// <param name="tension">Натяжение: 0 — ломаная, 1 — стандартная гладкая кривая (Catmull-Rom), больше 1 — «петлистее».</param>
        /// <param name="closed">Замкнуть кривую (последняя точка соединяется с первой).</param>
        public static List<BezierSegment> Build(IList<PointF> pts, double tension, bool closed)
        {
            List<BezierSegment> result = new List<BezierSegment>();
            int n = pts.Count;
            if (n < 2) return result;
            if (n < 3) closed = false;                  // замыкать две точки нет смысла

            int segCount = closed ? n : n - 1;

            for (int i = 0; i < segCount; i++)
            {
                int iStart = i;
                int iEnd = (i + 1) % n;

                PointF p0 = pts[iStart];
                PointF p3 = pts[iEnd];
                PointF prev = Neighbor(pts, iStart - 1, closed);   // точка перед началом сегмента
                PointF next = Neighbor(pts, iEnd + 1, closed);     // точка после конца сегмента

                // Касательные: U = tension·(P[i+1] − P[i−1]) / 2.
                PointF p1 = new PointF(
                    p0.X + (float)(tension * (p3.X - prev.X) / 6.0),
                    p0.Y + (float)(tension * (p3.Y - prev.Y) / 6.0));

                PointF p2 = new PointF(
                    p3.X - (float)(tension * (next.X - p0.X) / 6.0),
                    p3.Y - (float)(tension * (next.Y - p0.Y) / 6.0));

                result.Add(new BezierSegment(p0, p1, p2, p3));
            }

            return result;
        }

        private static PointF Neighbor(IList<PointF> pts, int index, bool closed)
        {
            int n = pts.Count;
            if (closed)
                return pts[((index % n) + n) % n];

            if (index < 0) index = 0;
            if (index > n - 1) index = n - 1;
            return pts[index];
        }
    }

    
    public class CanvasPanel : Panel
    {
        public CanvasPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }
    }

    public class BezierForm : Form
    {
        private const int PointRadius = 6;
        private const int HitRadius = 10;

        private readonly List<PointF> _points = new List<PointF>();
        private int _selected = -1;
        private bool _dragging = false;

        private CanvasPanel _canvas;
        private CheckBox _chkClosed;
        private CheckBox _chkHelpers;
        private TrackBar _tensionBar;
        private Label _tensionLabel;
        private Label _statusLabel;
        private Button _clearButton;

        public BezierForm()
        {
            Text = "Составная кубическая кривая Безье";
            ClientSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            BuildControls();

            // Стартовый пример, чтобы сразу было что смотреть. Кнопка «Очистить» его убирает.
            _points.Add(new PointF(150, 420));
            _points.Add(new PointF(320, 220));
            _points.Add(new PointF(520, 400));
            _points.Add(new PointF(700, 200));
            _points.Add(new PointF(850, 420));
            UpdateStatus();
        }

        private void BuildControls()
        {
            _canvas = new CanvasPanel();
            _canvas.Dock = DockStyle.Fill;
            _canvas.Paint += Canvas_Paint;
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.MouseDoubleClick += Canvas_MouseDoubleClick;
            _canvas.Cursor = Cursors.Cross;

            Label hint = new Label();
            hint.Dock = DockStyle.Top;
            hint.Height = 24;
            hint.TextAlign = ContentAlignment.MiddleLeft;
            hint.Padding = new Padding(8, 0, 0, 0);
            hint.BackColor = Color.WhiteSmoke;
            hint.Text = "Клик по пустому месту — добавить точку  |  Перетащить точку — переместить  |  "
                      + "Двойной клик / правая кнопка / Delete — удалить точку";

            FlowLayoutPanel top = new FlowLayoutPanel();
            top.Dock = DockStyle.Top;
            top.AutoSize = true;
            top.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            top.WrapContents = true;
            top.Padding = new Padding(4);

            _clearButton = new Button();
            _clearButton.Text = "Очистить";
            _clearButton.AutoSize = true;
            _clearButton.Click += delegate
            {
                _points.Clear();
                _selected = -1;
                _dragging = false;
                UpdateStatus();
                _canvas.Invalidate();
            };

            _chkClosed = new CheckBox();
            _chkClosed.Text = "Замкнуть кривую";
            _chkClosed.AutoSize = true;
            _chkClosed.Margin = new Padding(12, 8, 6, 0);
            _chkClosed.CheckedChanged += delegate { UpdateStatus(); _canvas.Invalidate(); };

            _chkHelpers = new CheckBox();
            _chkHelpers.Text = "Показывать опорные точки Безье";
            _chkHelpers.AutoSize = true;
            _chkHelpers.Checked = true;
            _chkHelpers.Margin = new Padding(6, 8, 6, 0);
            _chkHelpers.CheckedChanged += delegate { _canvas.Invalidate(); };

            Label tensionTitle = new Label();
            tensionTitle.Text = "Натяжение:";
            tensionTitle.AutoSize = true;
            tensionTitle.Margin = new Padding(12, 10, 0, 0);

            _tensionBar = new TrackBar();
            _tensionBar.Minimum = 0;
            _tensionBar.Maximum = 200;
            _tensionBar.Value = 100;
            _tensionBar.TickFrequency = 50;
            _tensionBar.Width = 160;
            _tensionBar.Scroll += delegate { UpdateTensionLabel(); _canvas.Invalidate(); };

            _tensionLabel = new Label();
            _tensionLabel.AutoSize = true;
            _tensionLabel.Margin = new Padding(0, 10, 12, 0);
            UpdateTensionLabel();

            _statusLabel = new Label();
            _statusLabel.AutoSize = true;
            _statusLabel.Margin = new Padding(6, 10, 0, 0);

            top.Controls.Add(_clearButton);
            top.Controls.Add(_chkClosed);
            top.Controls.Add(_chkHelpers);
            top.Controls.Add(tensionTitle);
            top.Controls.Add(_tensionBar);
            top.Controls.Add(_tensionLabel);
            top.Controls.Add(_statusLabel);

            // Порядок важен: последним добавленный Dock=Top окажется самым верхним.
            Controls.Add(_canvas);
            Controls.Add(hint);
            Controls.Add(top);
        }

        private double Tension
        {
            get { return _tensionBar.Value / 100.0; }
        }

        private void UpdateTensionLabel()
        {
            _tensionLabel.Text = Tension.ToString("0.00");
        }

        private void UpdateStatus()
        {
            int n = _points.Count;
            bool closed = _chkClosed != null && _chkClosed.Checked && n >= 3;
            int segments = n < 2 ? 0 : (closed ? n : n - 1);
            if (_statusLabel != null)
                _statusLabel.Text = "Точек: " + n + ",  сегментов Безье: " + segments;
        }

       
        private int HitTest(Point location)
        {
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                float dx = _points[i].X - location.X;
                float dy = _points[i].Y - location.Y;
                if (dx * dx + dy * dy <= HitRadius * HitRadius)
                    return i;
            }
            return -1;
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            int hit = HitTest(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                if (hit >= 0) RemovePoint(hit);
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (hit >= 0)
            {
               
                _selected = hit;
                _dragging = true;
            }
            else
            {
               
                _points.Add(e.Location);
                _selected = _points.Count - 1;
                _dragging = true;
                UpdateStatus();
            }

            _canvas.Invalidate();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging && _selected >= 0 && _selected < _points.Count)
            {
                float x = Math.Max(0, Math.Min(_canvas.ClientSize.Width - 1, e.X));
                float y = Math.Max(0, Math.Min(_canvas.ClientSize.Height - 1, e.Y));
                _points[_selected] = new PointF(x, y);
                _canvas.Invalidate();
                return;
            }

            _canvas.Cursor = HitTest(e.Location) >= 0 ? Cursors.Hand : Cursors.Cross;
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void Canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            int hit = HitTest(e.Location);
            if (hit >= 0) RemovePoint(hit);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                && _selected >= 0 && _selected < _points.Count)
            {
                RemovePoint(_selected);
                e.Handled = true;
            }
        }

        private void RemovePoint(int index)
        {
            _points.RemoveAt(index);
            _selected = -1;
            _dragging = false;
            UpdateStatus();
            _canvas.Invalidate();
        }

        // ------------------------------------------------------------------
        // Отрисовка
        // ------------------------------------------------------------------
        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool closed = _chkClosed.Checked && _points.Count >= 3;
            List<BezierSegment> segments = BezierSpline.Build(_points, Tension, closed);

            // 1) вспомогательные линии и опорные точки Безье (P1, P2 каждого сегмента)
            if (_chkHelpers.Checked)
            {
                using (Pen helperPen = new Pen(Color.Silver, 1))
                using (Brush helperBrush = new SolidBrush(Color.White))
                using (Pen helperDot = new Pen(Color.Gray, 1))
                {
                    helperPen.DashStyle = DashStyle.Dash;

                    foreach (BezierSegment s in segments)
                    {
                        g.DrawLine(helperPen, s.P0, s.P1);
                        g.DrawLine(helperPen, s.P2, s.P3);

                        g.FillRectangle(helperBrush, s.P1.X - 3, s.P1.Y - 3, 6, 6);
                        g.DrawRectangle(helperDot, s.P1.X - 3, s.P1.Y - 3, 6, 6);
                        g.FillRectangle(helperBrush, s.P2.X - 3, s.P2.Y - 3, 6, 6);
                        g.DrawRectangle(helperDot, s.P2.X - 3, s.P2.Y - 3, 6, 6);
                    }
                }
            }

            // 2) сама кривая: каждый сегмент разбиваем на небольшие отрезки
            using (Pen curvePen = new Pen(Color.RoyalBlue, 2.5f))
            {
                curvePen.LineJoin = LineJoin.Round;

                foreach (BezierSegment s in segments)
                {
                    double approxLength = Distance(s.P0, s.P1) + Distance(s.P1, s.P2) + Distance(s.P2, s.P3);
                    int steps = Math.Max(16, (int)(approxLength / 5.0));

                    PointF[] line = new PointF[steps + 1];
                    for (int k = 0; k <= steps; k++)
                        line[k] = s.At((double)k / steps);

                    g.DrawLines(curvePen, line);
                }
            }

            // 3) точки пользователя (поверх кривой)
            using (Font font = new Font("Segoe UI", 8f))
            {
                for (int i = 0; i < _points.Count; i++)
                {
                    PointF p = _points[i];
                    bool selected = (i == _selected);

                    using (Brush fill = new SolidBrush(selected ? Color.Red : Color.Black))
                    {
                        g.FillEllipse(fill, p.X - PointRadius, p.Y - PointRadius, PointRadius * 2, PointRadius * 2);
                    }

                    g.DrawString((i + 1).ToString(), font, Brushes.DimGray, p.X + PointRadius, p.Y - PointRadius - 12);
                }
            }
        }

        private static double Distance(PointF a, PointF b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}