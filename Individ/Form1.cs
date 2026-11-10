
using Individ.Algorithms;
using Individ.Controls;
using Individ.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Individ
{
    public partial class Form1 : Form
    {
        // Графическая область.
        private DrawingPanel drawingPanel;

        // Кнопки управления.
        private Button clearButton;
        private Button buildButton;

        // Информационные надписи.
        private Label pointsCountLabel;
        private Label hullCountLabel;
        private Label instructionLabel;

        public Form1()
        {
            InitializeComponent();

            InitializeInterface();
        }

        /// <summary>
        /// Создаёт и настраивает интерфейс приложения.
        /// </summary>
        private void InitializeInterface()
        {
            // Настройки главного окна.
            Text = "Построение выпуклой оболочки";
            Size = new Size(1100, 750);
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // Создаём основную таблицу интерфейса.
            TableLayoutPanel mainLayout = new TableLayoutPanel();

            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 2;

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 90)
            );

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100)
            );

            // Создаём верхнюю панель управления.
            Panel topPanel = new Panel();

            topPanel.Dock = DockStyle.Fill;
            topPanel.BackColor = Color.FromArgb(245, 245, 245);

            // Создаём кнопки.
            clearButton = new Button();

            clearButton.Text = "Очистить";
            clearButton.Size = new Size(120, 35);
            clearButton.Location = new Point(20, 15);

            buildButton = new Button();

            buildButton.Text = "Построить оболочку";
            buildButton.Size = new Size(170, 35);
            buildButton.Location = new Point(155, 15);

            // Создаём информационные надписи.
            pointsCountLabel = new Label();

            pointsCountLabel.Text = "Количество точек: 0";
            pointsCountLabel.AutoSize = true;
            pointsCountLabel.Location = new Point(350, 12);

            hullCountLabel = new Label();

            hullCountLabel.Text = "Вершин оболочки: 0";
            hullCountLabel.AutoSize = true;
            hullCountLabel.Location = new Point(350, 40);

            instructionLabel = new Label();

            instructionLabel.Text = "Нажмите мышью на рабочую область, чтобы добавить точку";
            instructionLabel.AutoSize = true;
            instructionLabel.Location = new Point(20, 60);
            instructionLabel.ForeColor = Color.DimGray;

            // Добавляем элементы на верхнюю панель.
            topPanel.Controls.Add(clearButton);
            topPanel.Controls.Add(buildButton);
            topPanel.Controls.Add(pointsCountLabel);
            topPanel.Controls.Add(hullCountLabel);
            topPanel.Controls.Add(instructionLabel);

            // Создаём графическую панель.
            drawingPanel = new DrawingPanel();

            drawingPanel.Dock = DockStyle.Fill;

            // Добавляем элементы в основную таблицу.
            mainLayout.Controls.Add(topPanel, 0, 0);
            mainLayout.Controls.Add(drawingPanel, 0, 1);

            // Подключаем таблицу к форме.
            Controls.Add(mainLayout);

            // Подключаем обработчики событий.
            drawingPanel.MouseClick += DrawingPanel_MouseClick;

            clearButton.Click += ClearButton_Click;

            buildButton.Click += BuildButton_Click;
        }

        /// <summary>
        /// Обрабатывает нажатие мыши на рабочей области.
        /// </summary>
        private void DrawingPanel_MouseClick(
            object sender,
            MouseEventArgs e)
        {
            // Добавляем новую точку по координатам клика.
            Point2D newPoint = new Point2D(e.X, e.Y);

            drawingPanel.Points.Add(newPoint);

            // Пересчитываем выпуклую оболочку.
            UpdateConvexHull();

            // Обновляем информацию.
            UpdateStatistics();
        }

        /// <summary>
        /// Пересчитывает выпуклую оболочку.
        /// </summary>
        private void UpdateConvexHull()
        {
            drawingPanel.Hull = ConvexHull.Build(
                drawingPanel.Points
            );

            // Запрашиваем перерисовку графической области.
            drawingPanel.Invalidate();
        }

        /// <summary>
        /// Обновляет статистику приложения.
        /// </summary>
        private void UpdateStatistics()
        {
            pointsCountLabel.Text =
                $"Количество точек: {drawingPanel.Points.Count}";

            hullCountLabel.Text =
                $"Вершин оболочки: {drawingPanel.Hull.Count}";
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки очистки.
        /// </summary>
        private void ClearButton_Click(
            object sender,
            EventArgs e)
        {
            // Удаляем все введённые точки.
            drawingPanel.Points.Clear();

            // Удаляем выпуклую оболочку.
            drawingPanel.Hull.Clear();

            // Обновляем изображение.
            drawingPanel.Invalidate();

            // Обновляем статистику.
            UpdateStatistics();
        }

        /// <summary>
        /// Обрабатывает ручной запуск построения оболочки.
        /// </summary>
        private void BuildButton_Click(
            object sender,
            EventArgs e)
        {
            UpdateConvexHull();

            UpdateStatistics();
        }
    }
}
