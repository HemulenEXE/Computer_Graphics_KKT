using System;
using System.Drawing;
using System.Windows.Forms;

namespace WuLineLab
{
    public partial class Form1 : Form
    {
        private Bitmap canvasBitmap;
        private Point? firstPoint = null;
        private Point? secondPoint = null;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ResetCanvas();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ResetCanvas();
        }

        private void ResetCanvas()
        {
            canvasBitmap?.Dispose();
            canvasBitmap = new Bitmap(pictureBoxCanvas.ClientSize.Width, pictureBoxCanvas.ClientSize.Height);

            using (Graphics g = Graphics.FromImage(canvasBitmap))
                g.Clear(Color.White);

            pictureBoxCanvas.Image = canvasBitmap;
            pictureBoxZoom.Image = null;

            firstPoint = null;
            secondPoint = null;
            lblStatus.Text = "Кликните на холст: первый клик — начало отрезка, второй — конец.";
        }

        private void pictureBoxCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (canvasBitmap == null) return;

            if (firstPoint == null)
            {
                firstPoint = e.Location;
                lblStatus.Text = $"Начало отрезка: ({firstPoint.Value.X},{firstPoint.Value.Y}). Кликните конечную точку.";
                return;
            }

            secondPoint = e.Location;

            // перерисовываем холст с нуля, чтобы не накладывать линии друг на друга
            using (Graphics g = Graphics.FromImage(canvasBitmap))
                g.Clear(Color.White);

            DrawLineWu(canvasBitmap, firstPoint.Value.X, firstPoint.Value.Y,
                                     secondPoint.Value.X, secondPoint.Value.Y, Color.Black);

            pictureBoxCanvas.Image = canvasBitmap;
            ShowZoom(firstPoint.Value, secondPoint.Value);

            lblStatus.Text = $"Отрезок ({firstPoint.Value.X},{firstPoint.Value.Y}) — " +
                              $"({secondPoint.Value.X},{secondPoint.Value.Y}) нарисован алгоритмом Ву. " +
                              "Кликните ещё раз, чтобы начать новый отрезок.";

            // готовимся к следующему отрезку
            firstPoint = null;
            secondPoint = null;
        }

        
        private void DrawLineWu(Bitmap bmp, int x0, int y0, int x1, int y1, Color lineColor)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

           
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            double dx = x1 - x0;
            double dy = y1 - y0;
            double gradient = (dx == 0) ? 1.0 : dy / dx; // защита от деления на 0 (вертикальная линия после свопа невозможна, но на всякий случай)

           
            double xEnd = Round(x0);
            double yEnd = y0 + gradient * (xEnd - x0);
            double xGap = RFPart(x0 + 0.5);
            int xPixel1 = (int)xEnd;
            int yPixel1 = IPart(yEnd);

            Plot(bmp, xPixel1, yPixel1, RFPart(yEnd) * xGap, lineColor, steep);
            Plot(bmp, xPixel1, yPixel1 + 1, FPart(yEnd) * xGap, lineColor, steep);

            double yInter = yEnd + gradient; // y для первого промежуточного пикселя

           
            xEnd = Round(x1);
            double yEnd2 = y1 + gradient * (xEnd - x1);
            xGap = FPart(x1 + 0.5);
            int xPixel2 = (int)xEnd;
            int yPixel2 = IPart(yEnd2);

            Plot(bmp, xPixel2, yPixel2, RFPart(yEnd2) * xGap, lineColor, steep);
            Plot(bmp, xPixel2, yPixel2 + 1, FPart(yEnd2) * xGap, lineColor, steep);

            
            for (int x = xPixel1 + 1; x <= xPixel2 - 1; x++)
            {
                Plot(bmp, x, IPart(yInter), RFPart(yInter), lineColor, steep);
                Plot(bmp, x, IPart(yInter) + 1, FPart(yInter), lineColor, steep);
                yInter += gradient;
            }
        }

        
        private void Plot(Bitmap bmp, int x, int y, double intensity, Color lineColor, bool steep)
        {
            if (steep) Swap(ref x, ref y); // возвращаем координаты в исходную систему

            if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height) return;
            if (intensity <= 0) return;
            if (intensity > 1) intensity = 1;

            Color bg = bmp.GetPixel(x, y);

          
            int r = (int)(lineColor.R * intensity + bg.R * (1 - intensity));
            int g = (int)(lineColor.G * intensity + bg.G * (1 - intensity));
            int b = (int)(lineColor.B * intensity + bg.B * (1 - intensity));

            bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
        }

       
        private int IPart(double x) => (int)Math.Floor(x);          // целая часть (округление вниз)
        private double FPart(double x) => x - Math.Floor(x);        // дробная часть
        private double RFPart(double x) => 1.0 - FPart(x);          // "обратная" дробная часть (1 - дробная)
        private double Round(double x) => Math.Floor(x + 0.5);      // обычное округление

        private void Swap(ref int a, ref int b)
        {
            int t = a; a = b; b = t;
        }

        
        private void ShowZoom(Point p1, Point p2)
        {
            int midX = (p1.X + p2.X) / 2;
            int midY = (p1.Y + p2.Y) / 2;

            int cropSize = 20; // область 20x20 пикселей вокруг середины отрезка
            int startX = Math.Max(0, midX - cropSize / 2);
            int startY = Math.Max(0, midY - cropSize / 2);
            int w = Math.Min(cropSize, canvasBitmap.Width - startX);
            int h = Math.Min(cropSize, canvasBitmap.Height - startY);

            if (w <= 0 || h <= 0) return;

            Bitmap crop = canvasBitmap.Clone(new Rectangle(startX, startY, w, h), canvasBitmap.PixelFormat);

            
            Bitmap zoomed = new Bitmap(w * 15, h * 15);
            using (Graphics g = Graphics.FromImage(zoomed))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(crop, 0, 0, zoomed.Width, zoomed.Height);
            }

            pictureBoxZoom.Image = zoomed;
        }
    }
}