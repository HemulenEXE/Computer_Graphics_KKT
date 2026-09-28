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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using FastBitmapSpace;
using System.Windows.Forms.DataVisualization.Charting;

namespace _2th_point
{
    public partial class Form1 : Form
    {
        string img_dir = @"..\..\Images";

        Bitmap bitmap;
        Bitmap red_bitmap;
        Bitmap green_bitmap;
        Bitmap blue_bitmap;
        public Form1()
        {
            InitializeComponent();
            string name = @"ФРУКТЫ.jpg";
            string fullPath = Path.GetFullPath(Path.Combine(Application.StartupPath, img_dir, name));

            ChangeImage(fullPath);
            ExtractionRGB();
        }
        private void ChangeImage(string path)
        {
            pictureBox1.Image = Image.FromFile(path);
            bitmap = new Bitmap(path);
            red_bitmap = new Bitmap(bitmap);
            green_bitmap = new Bitmap(bitmap);
            blue_bitmap = new Bitmap(bitmap);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.GetFullPath(Path.Combine(Application.StartupPath, img_dir));
                dialog.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ChangeImage(dialog.FileName);
                    ExtractionRGB();
                }
            }
        }
        private void PrintHist()
        {
            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();
            chart1.Titles.Clear();
            chart2.Titles.Clear();
            chart3.Titles.Clear();

            chart1.Titles.Add("Распределение яркости в R");
            chart2.Titles.Add("Распределение яркости в G");
            chart3.Titles.Add("Распределение яркости в B");

            // Создание серии данных для гистограммы
            Series red_series = new Series();
            Series green_series = new Series();
            Series blue_series = new Series();

            red_series.ChartType = SeriesChartType.Column; // Тип — столбчатая диаграмма (гистограмма)
            green_series.ChartType = SeriesChartType.Column;
            blue_series.ChartType = SeriesChartType.Column;

            var red_hist = new int[256];
            var green_hist = new int[256];
            var blue_hist = new int[256];

            red_bitmap.ForEach(color => {
                red_hist[color.R]++;
            });
            green_bitmap.ForEach(color => {
                green_hist[color.G]++;
            });
            blue_bitmap.ForEach(color => {
                blue_hist[color.B]++;
            });

            red_series.Points.DataBindY(red_hist);
            green_series.Points.DataBindY(green_hist);
            blue_series.Points.DataBindY(blue_hist);

            chart1.ChartAreas[0].AxisY.Maximum = red_hist.Max();
            chart2.ChartAreas[0].AxisY.Maximum = green_hist.Max();
            chart3.ChartAreas[0].AxisY.Maximum = blue_hist.Max();

            chart1.Series.Add(red_series);
            chart2.Series.Add(green_series);
            chart3.Series.Add(blue_series);

        }
        private void ExtractionRGB()
        {
            var red = new FastBitmap(red_bitmap);
            var green = new FastBitmap(green_bitmap);
            var blue = new FastBitmap(blue_bitmap);


            using (var fastBitmap = new FastBitmap(bitmap))
            {
                for (var x = 0; x < fastBitmap.Width; x++)
                    for (var y = 0; y < fastBitmap.Height; y++)
                    {
                        var color = fastBitmap[x, y];

                        red[x, y] = Color.FromArgb(color.R, 0, 0);
                        green[x, y] = Color.FromArgb(0, color.G, 0);
                        blue[x, y] = Color.FromArgb(0, 0, color.B);
                    }
               
            }
            red.Dispose();
            green.Dispose();
            blue.Dispose();

            pictureBox1.Image = bitmap;
            pictureBox2.Image = red_bitmap;
            pictureBox3.Image = green_bitmap;
            pictureBox4.Image = blue_bitmap;

            PrintHist();
        }

    }
}
