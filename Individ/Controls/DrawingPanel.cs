
using Individ.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Individ.Controls
{
    /// <summary>
    /// Графическая панель для отображения точек
    /// и построенной выпуклой оболочки.
    /// </summary>
    public class DrawingPanel : Panel
    {
        // Все точки, добавленные пользователем.
        public List<Point2D> Points { get; set; } =
            new List<Point2D>();

        // Вершины выпуклой оболочки.
        public List<Point2D> Hull { get; set; } =
            new List<Point2D>();

        // Размер одной ячейки координатной сетки.
        private const int GridSize = 25;

        public DrawingPanel()
        {
            // Включаем двойную буферизацию,
            // чтобы избежать мерцания при перерисовке.
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            // Настраиваем внешний вид панели.
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.FixedSingle;

            // Минимальный размер рабочей области.
            MinimumSize = new Size(300, 300);
        }

        /// <summary>
        /// Основной метод отрисовки графической области.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            // Сглаживание линий и окружностей.
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем координатную сетку.
            DrawGrid(graphics);

            // Рисуем выпуклую оболочку.
            DrawHull(graphics);

            // Рисуем все добавленные точки.
            DrawPoints(graphics);
        }

        /// <summary>
        /// Отрисовывает координатную сетку.
        /// </summary>
        private void DrawGrid(Graphics graphics)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(235, 235, 235), 1))
            {
                // Вертикальные линии.
                for (int x = 0; x < Width; x += GridSize)
                {
                    graphics.DrawLine(
                        gridPen,
                        x, 0,
                        x, Height
                    );
                }

                // Горизонтальные линии.
                for (int y = 0; y < Height; y += GridSize)
                {
                    graphics.DrawLine(
                        gridPen,
                        0, y,
                        Width, y
                    );
                }
            }

            // Выделяем центральные оси координат.
            using (Pen axisPen = new Pen(Color.LightGray, 1.5f))
            {
                graphics.DrawLine(
                    axisPen,
                    Width / 2, 0,
                    Width / 2, Height
                );

                graphics.DrawLine(
                    axisPen,
                    0, Height / 2,
                    Width, Height / 2
                );
            }
        }

        /// <summary>
        /// Отрисовывает выпуклую оболочку.
        /// </summary>
        private void DrawHull(Graphics graphics)
        {
            if (Hull == null || Hull.Count == 0)
                return;

            // Если есть только одна вершина,
            // рисовать многоугольник не нужно.
            if (Hull.Count == 1)
                return;

            // Преобразуем координаты точек
            // в формат, используемый Graphics.
            PointF[] polygon = new PointF[Hull.Count];

            for (int i = 0; i < Hull.Count; i++)
            {
                polygon[i] = new PointF(
                    (float)Hull[i].X,
                    (float)Hull[i].Y
                );
            }

            // Заливаем внутреннюю область оболочки.
            if (polygon.Length >= 3)
            {
                using (SolidBrush fillBrush =
                       new SolidBrush(Color.FromArgb(45, 30, 144, 255)))
                {
                    graphics.FillPolygon(fillBrush, polygon);
                }
            }

            // Рисуем границы оболочки.
            using (Pen hullPen = new Pen(Color.DodgerBlue, 2.5f))
            {
                hullPen.LineJoin = LineJoin.Round;

                if (polygon.Length >= 3)
                {
                    // Замкнутый многоугольник.
                    graphics.DrawPolygon(hullPen, polygon);
                }
                else
                {
                    // Если две точки — рисуем отрезок.
                    graphics.DrawLine(
                        hullPen,
                        polygon[0],
                        polygon[1]
                    );
                }
            }
        }

        /// <summary>
        /// Отрисовывает все точки пользователя.
        /// </summary>
        private void DrawPoints(Graphics graphics)
        {
            if (Points == null)
                return;

            foreach (Point2D point in Points)
            {
                // Проверяем, входит ли точка в оболочку.
                bool isHullPoint = IsHullPoint(point);

                // Вершины оболочки выделяем оранжевым.
                // Остальные точки отображаем красным.
                Color pointColor = isHullPoint
                    ? Color.OrangeRed
                    : Color.Crimson;

                using (SolidBrush brush = new SolidBrush(pointColor))
                {
                    const float radius = 5f;

                    graphics.FillEllipse(
                        brush,
                        (float)point.X - radius,
                        (float)point.Y - radius,
                        radius * 2,
                        radius * 2
                    );
                }

                // Обводка точки.
                using (Pen outlinePen = new Pen(Color.White, 1.5f))
                {
                    graphics.DrawEllipse(
                        outlinePen,
                        (float)point.X - 5,
                        (float)point.Y - 5,
                        10,
                        10
                    );
                }
            }
        }

        /// <summary>
        /// Проверяет, является ли точка вершиной оболочки.
        /// </summary>
        private bool IsHullPoint(Point2D point)
        {
            if (Hull == null)
                return false;

            foreach (Point2D hullPoint in Hull)
            {
                if (Math.Abs(hullPoint.X - point.X) < 0.000001 &&
                    Math.Abs(hullPoint.Y - point.Y) < 0.000001)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
