using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _1_v
{
    public partial class Form1 : Form
    {
        private Bitmap originalBitmap;
        private Bitmap resultBitmap;

        private Color borderColor;
        private Point startPoint;
        private List<Point> boundaryPoints;

        // Направления по схеме со слайда:
        // 3 2 1
        // 4 X 0
        // 5 6 7
        private static readonly int[] dx = { 1, 1, 0, -1, -1, -1, 0, 1 };
        private static readonly int[] dy = { 0, -1, -1, -1, 0, 1, 1, 1 };

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Изображения|*.bmp;*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                originalBitmap?.Dispose();
                using (var img = Image.FromFile(ofd.FileName))
                {
                    originalBitmap = new Bitmap(img);
                }

                pictureBoxOriginal.Image = originalBitmap;
                pictureBoxResult.Image = null;
                lblStatus.Text = "Кликните на пиксель границы на левом изображении.";
            }
        }

        private void pictureBoxOriginal_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;

            Point imgPoint = ControlToImageCoordinates(pictureBoxOriginal, e.Location);
            if (imgPoint.X < 0)
            {
                lblStatus.Text = "Клик мимо изображения, попробуйте ещё раз.";
                return;
            }

            borderColor = originalBitmap.GetPixel(imgPoint.X, imgPoint.Y);
            startPoint = imgPoint;

            TraceAndDraw();
        }

        // Пересчёт координат клика на PictureBox (SizeMode = Zoom) в координаты пикселя исходного Bitmap
        private Point ControlToImageCoordinates(PictureBox pb, Point controlPoint)
        {
            if (pb.Image == null) return new Point(-1, -1);

            float imgWidth = pb.Image.Width;
            float imgHeight = pb.Image.Height;
            float pbWidth = pb.ClientSize.Width;
            float pbHeight = pb.ClientSize.Height;

            float imageAspect = imgWidth / imgHeight;
            float pbAspect = pbWidth / pbHeight;

            float drawWidth, drawHeight, offsetX, offsetY;

            if (imageAspect > pbAspect)
            {
                drawWidth = pbWidth;
                drawHeight = pbWidth / imageAspect;
                offsetX = 0;
                offsetY = (pbHeight - drawHeight) / 2f;
            }
            else
            {
                drawHeight = pbHeight;
                drawWidth = pbHeight * imageAspect;
                offsetX = (pbWidth - drawWidth) / 2f;
                offsetY = 0;
            }

            float relX = (controlPoint.X - offsetX) / drawWidth;
            float relY = (controlPoint.Y - offsetY) / drawHeight;

            if (relX < 0 || relX > 1 || relY < 0 || relY > 1)
                return new Point(-1, -1);

            int imgX = (int)(relX * imgWidth);
            int imgY = (int)(relY * imgHeight);

            imgX = Math.Max(0, Math.Min((int)imgWidth - 1, imgX));
            imgY = Math.Max(0, Math.Min((int)imgHeight - 1, imgY));

            return new Point(imgX, imgY);
        }

        private void TraceAndDraw()
        {
            boundaryPoints = TraceBoundary(originalBitmap, borderColor, startPoint);

            resultBitmap?.Dispose();
            resultBitmap = new Bitmap(originalBitmap);

            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                foreach (var pt in boundaryPoints)
                {
                    resultBitmap.SetPixel(pt.X, pt.Y, Color.Red);
                }

                // отметим стартовую точку зелёным кружком, чтобы её было видно
                g.DrawEllipse(new Pen(Color.Lime, 1), startPoint.X - 4, startPoint.Y - 4, 8, 8);
            }

            pictureBoxResult.Image = resultBitmap;

            lblStatus.Text = $"Старт: ({startPoint.X},{startPoint.Y}), цвет: R={borderColor.R} G={borderColor.G} B={borderColor.B}. " +
                              $"Найдено точек границы: {boundaryPoints.Count}";
        }

        // Проверка, является ли пиксель (x,y) "граничным" (с допуском по цвету)
        private bool IsBorderColor(Bitmap bmp, int x, int y, Color target, int tolerance)
        {
            if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height) return false;
            Color c = bmp.GetPixel(x, y);
            return Math.Abs(c.R - target.R) <= tolerance &&
                   Math.Abs(c.G - target.G) <= tolerance &&
                   Math.Abs(c.B - target.B) <= tolerance;
        }

        // Сам алгоритм обхода границы (Moore-neighbor tracing)
        private List<Point> TraceBoundary(Bitmap bmp, Color borderColor, Point start, int tolerance = 30)
        {
            List<Point> result = new List<Point>();

            if (!IsBorderColor(bmp, start.X, start.Y, borderColor, tolerance))
                return result; // выбранная точка не граничного цвета

            result.Add(start);

            Point current = start;
            int arrivalDir = 0;      // фиктивное направление "прихода" для первого шага
            int maxSteps = bmp.Width * bmp.Height * 2; // защита от зацикливания

            for (int steps = 0; steps < maxSteps; steps++)
            {
                // 90° по часовой стрелке от направления прихода = -2 (mod 8)
                int searchStart = (arrivalDir - 2 + 8) % 8;
                bool found = false;

                // ищем против часовой стрелки (+1, +2, ...) до 8 направлений
                for (int i = 0; i < 8; i++)
                {
                    int dir = (searchStart + i) % 8;
                    int nx = current.X + dx[dir];
                    int ny = current.Y + dy[dir];

                    if (IsBorderColor(bmp, nx, ny, borderColor, tolerance))
                    {
                        current = new Point(nx, ny);
                        arrivalDir = dir;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break; // изолированный пиксель без соседей

                if (current == start)
                    break; // контур замкнулся — вернулись в начало

                result.Add(current);
            }

            return result;
        }
    }
}