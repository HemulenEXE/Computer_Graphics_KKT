using FastBitmapSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace _1b_Bresenham
{
    public partial class Form1 : Form
    {
        string img_dir = @"..\..\Images";
        Bitmap bitmap;
        Bitmap texture;
        Color source;
        int startX;
        int startY;
        Point start;
        Point end;
        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(
                pictureBox1.Width,
                pictureBox1.Height);
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }

            pictureBox1.Image = bitmap;

            pictureBox1.MouseClick += new MouseEventHandler(pictureBox1_MouseClick);
            pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
            pictureBox1.MouseUp += new MouseEventHandler(pictureBox1_MouseUp);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) start = e.Location;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                end = e.Location;
                Bresenham(start, end);
                pictureBox1.Invalidate();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.GetFullPath(Path.Combine(Application.StartupPath, img_dir));
                dialog.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    texture = new Bitmap(dialog.FileName);
                }
            }
        }
        private int getCoordinate(int cur, int start, int size)
        {
            int res = (cur - start) % size;

            if (res < 0)
                res += size;

            return res;
        }
        private void FillBitmap(int curX, int curY)
        {
            int leftBound = curX;
            int rightBound = curX;

            using (var fastBitmap = new FastBitmap(bitmap))
            {
                //Находим границы заливки серии 
                while (leftBound > 0 && fastBitmap[leftBound, curY] == source)
                {
                    leftBound--;
                }
                while (rightBound < fastBitmap.Width && fastBitmap[rightBound, curY] == source)
                {
                    rightBound++;
                }

                for (var x = leftBound + 1; x < rightBound; x++)
                {
                    //Вычисление нужных координат на текстуре
                    int textureX =  getCoordinate(x, startX, texture.Width);//x % texture.Width;
                    int textureY =  getCoordinate(curY, startY, texture.Height);//curY % texture.Height;

                    Color texturePixel = texture.GetPixel(textureX, textureY);

                    fastBitmap[x, curY] = texturePixel;
                }
            }
            if (curY - 1 >= 0)
            {
                FindNextSeries(leftBound + 1, rightBound, curY - 1);                  
            }
            if (curY + 1 < bitmap.Height)
            {
                FindNextSeries(leftBound + 1, rightBound, curY + 1);
            }
            
        }
        //Находит следующую серию
        private void FindNextSeries(int leftX, int rightX, int curY)
        {
            int x = leftX;

            while (x < rightX)
            {
                // пропускаем всё, что не является исходным цветом
                while (x < rightX && bitmap.GetPixel(x, curY) != source)
                {
                    x++;
                }

                if (x >= rightX)
                    return;

                // нашли начало новой серии
                int newX = x;

                // пропускаем всю найденную серию
                while (x < rightX && bitmap.GetPixel(x, curY) == source)
                {
                    x++;
                }

                FillBitmap(newX, curY);
            }
        }
        //Для заливки используем правую кнопку мыши!
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                return;

            if (source == null)
            {
                MessageBox.Show("Вначале выберете изображение!");
                return;
            }

            // координаты клика в PictureBox
            startX = e.X;
            startY = e.Y;

            // Получаем цвет пикселя;
            source = bitmap.GetPixel(startX, startY);

            FillBitmap(startX, startY);

            pictureBox1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }

            pictureBox1.Image = bitmap;
        }
        //Отрисовывает линии, если x0 > x1 
        private void PlotLineHight(int x0, int y0, int x1, int y1)
        {
            int dx = (x1 - x0);
            int dy = (y1 - y0);
            int xi = 1;

            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }

            int D = (2 * dx) - dy;
            int x = x0;

            using (var fastBitmap = new FastBitmap(bitmap))
            {
                for (int y = y0; y <= y1; y++)
                {
                    fastBitmap[x, y] = Color.Black;

                    if (D > 0)
                    {
                        x = x + xi;
                        D = D + (2 * (dx - dy));
                    }
                    else D = D + 2 * dx;
                }
            }
        }
        //Отрисовывает линии, если y0 > y1 
        private void PlotLineLow(int x0, int y0, int x1, int y1)
        {
            int dx = (x1 - x0);
            int dy = (y1 - y0);
            int yi = 1;

            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }

            int D = (2 * dy) - dx;
            int y = y0;

            using (var fastBitmap = new FastBitmap(bitmap))
            {
                for (int x = x0; x <= x1; x++)
                {
                    fastBitmap[x, y] = Color.Black;

                    if (D > 0)
                    {
                        y = y + yi;
                        D = D + (2 * (dy - dx));
                    }
                    else D = D + 2 * dy;
                }
            }
        }
        private void Bresenham(Point start, Point end)
        {
            if (Math.Abs(end.Y - start.Y) < Math.Abs(end.X - start.X))
            {
                if (start.X > end.X)
                    PlotLineLow(end.X, end.Y, start.X, start.Y);
                else
                    PlotLineLow(start.X, start.Y, end.X, end.Y);
            }
            else 
            { 
                if (start.Y > end.Y)
                    PlotLineHight(end.X, end.Y, start.X, start.Y);
                else
                    PlotLineHight(start.X, start.Y, end.X, end.Y);

            }
        }
    }
}