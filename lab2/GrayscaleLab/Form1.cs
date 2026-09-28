using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace GrayscaleLab
{
    public partial class Form1 : Form
    {
        private Bitmap originalBitmap;
        private Bitmap grayNtsc;   // Y' = 0.299R + 0.587G + 0.114B
        private Bitmap grayHdtv;   // Y' = 0.2126R + 0.7152G + 0.0722B
        private Bitmap diffImage;

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
                ProcessImage();
            }
        }

        private void ProcessImage()
        {
            if (originalBitmap == null) return;

            grayNtsc?.Dispose();
            grayHdtv?.Dispose();
            diffImage?.Dispose();

            grayNtsc = ConvertToGrayscale(originalBitmap, 0.299, 0.587, 0.114);
            grayHdtv = ConvertToGrayscale(originalBitmap, 0.2126, 0.7152, 0.0722);
            diffImage = ComputeDifference(grayNtsc, grayHdtv);

            pictureBoxGray1.Image = grayNtsc;
            pictureBoxGray2.Image = grayHdtv;
            pictureBoxDiff.Image = diffImage;

            int[] hist1 = ComputeHistogram(grayNtsc);
            int[] hist2 = ComputeHistogram(grayHdtv);

            pictureBoxHist1.Image = DrawHistogram(hist1, "YUV/NTSC");
            pictureBoxHist2.Image = DrawHistogram(hist2, "HDTV");
        }

        // Перевод в градации серого с заданными коэффициентами
        private Bitmap ConvertToGrayscale(Bitmap source, double kr, double kg, double kb)
        {
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color c = source.GetPixel(x, y);
                    int gray = (int)Math.Round(kr * c.R + kg * c.G + kb * c.B);
                    gray = Math.Max(0, Math.Min(255, gray));
                    result.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            return result;
        }

        // Разность двух полутоновых изображений (по модулю)
        private Bitmap ComputeDifference(Bitmap img1, Bitmap img2)
        {
            Bitmap result = new Bitmap(img1.Width, img1.Height, PixelFormat.Format24bppRgb);

            for (int y = 0; y < img1.Height; y++)
            {
                for (int x = 0; x < img1.Width; x++)
                {
                    int g1 = img1.GetPixel(x, y).R;
                    int g2 = img2.GetPixel(x, y).R;
                    int diff = Math.Abs(g1 - g2);
                    result.SetPixel(x, y, Color.FromArgb(diff, diff, diff));
                }
            }
            return result;
        }

        // Подсчёт гистограммы интенсивности
        private int[] ComputeHistogram(Bitmap gray)
        {
            int[] hist = new int[256];
            for (int y = 0; y < gray.Height; y++)
                for (int x = 0; x < gray.Width; x++)
                    hist[gray.GetPixel(x, y).R]++;
            return hist;
        }

        // Отрисовка гистограммы в отдельный Bitmap
        private Bitmap DrawHistogram(int[] hist, string title)
        {
            int width = 512, height = 220;
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                int max = hist.Max();
                if (max == 0) max = 1;

                float barWidth = width / 256f;
                using (Pen pen = new Pen(Color.Black))
                {
                    for (int i = 0; i < 256; i++)
                    {
                        float barHeight = (float)hist[i] / max * (height - 25);
                        g.DrawLine(pen, i * barWidth, height, i * barWidth, height - barHeight);
                    }
                }
                g.DrawString(title, this.Font, Brushes.Black, 5, 5);
            }
            return bmp;
        }
    }
}