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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace fractals
{
    public partial class Form1 : Form
    {
        private string _systems_dir = @"..\..\LSystems";
        private string _atom;
        private double _rotation;
        private double _diraction;
        private Dictionary<char, string> _rules = new Dictionary<char, string>();
        double maxX, maxY, minX, minY; //Размер фрактала
        private int maxBranchDepth; //Макс. глубина дерева
        //Толщина и цвет для отрисовки
        private const float MaxLineWidth = 8f;
        private const float MinLineWidth = 0.5f;
        private readonly Color trunkColor = Color.FromArgb(65, 37, 31);
        private readonly Color branchColor = Color.ForestGreen;
        Random rand = new Random();
        public Form1()
        {
            InitializeComponent();

            pictureBox1.Paint += PictureBox1_Paint;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.GetFullPath(Path.Combine(Application.StartupPath, _systems_dir));
                dialog.Filter = "Текстовые файлы (*.txt)|*.txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        //Считываем файл
                        using (StreamReader reader = new StreamReader(dialog.FileName))
                        {
                            string line = reader.ReadLine();

                            var values = line.Split(' ');
                            _atom = values[0];
                            _rotation = double.Parse(values[1]) * (Math.PI / 180); //храним сразу в радианах
                            _diraction = double.Parse(values[2]) * (Math.PI / 180); //тоже в радианах 

                            _rules.Clear();
                            while ((line = reader.ReadLine()) != null)
                            {
                                _rules[line[0]] = line.Substring(3);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    pictureBox1.Invalidate();
                }
            }
        }
        //Получение последовательности команд для отрисовки
        string GetSequence()
        {
            int depth = (int)numericUpDown1.Value;

            StringBuilder sb = new StringBuilder();
            string res = _atom;


            for (int i = 0; i < depth; i++)
            {
                foreach (char c in res)
                {
                    if (_rules.ContainsKey(c))
                    {
                        sb.Append(_rules[c]);
                    }
                    else
                        sb.Append(c);
                }

                res = sb.ToString();
                sb.Clear();
            }

            return res;
        }
        //Состояния для скобочной l-системы
        private struct State
        {
            public PointF p;
            public double r;
            public int depth; //глубина ыетыления для дерева

            public State(PointF curPoint, double curRotation, int branchDepth)
            {
                p = curPoint;
                r = curRotation;
                depth = branchDepth;
            }
        }
    
        private struct Segment
        {
            public PointF Start;
            public PointF End;
            public int Depth;

            public Segment(PointF start, PointF end, int depth)
            {
                Start = start;
                End = end;
                Depth = depth;
            }
        }


        //Расчёт точек
        private List<Segment> CalculateFractal()
        {
            List<Segment> fractal = new List<Segment>();
            string seq = GetSequence();

            float lineLength = 1f;
            PointF curPoint = new PointF(0, 0);
            double curRotation = _diraction;

            Stack<State> states = new Stack<State>();

            maxX = maxY = minX = minY = 0;

            //глубина ветви и максимальная глубина
            int branchDepth = 0;
            maxBranchDepth = 0;

            foreach (char c in seq)
            {
                double x, y;

                if (c == 'F')
                {
                    PointF start = curPoint;

                    x = curPoint.X + lineLength * Math.Cos(curRotation);
                    y = curPoint.Y + lineLength * Math.Sin(curRotation);

                    curPoint = new PointF((float)x, (float)y);

                    fractal.Add(new Segment(start, curPoint, branchDepth));

                    // Обновляем границы фрактала
                    maxX = Math.Max(maxX, curPoint.X);
                    maxY = Math.Max(maxY, curPoint.Y);
                    minX = Math.Min(minX, curPoint.X);
                    minY = Math.Min(minY, curPoint.Y);
                }
                else if (c == '-')
                {
                    curRotation -= _rotation;
                }
                else if (c == '+')
                {
                    curRotation += _rotation;
                }
                else if (c == '[')
                {
                    states.Push(new State(curPoint, curRotation, branchDepth));

                    branchDepth++;

                    maxBranchDepth = Math.Max(maxBranchDepth, branchDepth);
                }
                else if (c == ']')
                {
                    State st = states.Pop();

                    curPoint = st.p;
                    curRotation = st.r;
                    branchDepth = st.depth;
                }
                else if (c == '@')
                {
                    //обработка случайного поворота
                    curRotation += (rand.NextDouble() - 1);
                }
            }

            return fractal;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        { 
            if (string.IsNullOrEmpty(_atom))
                return;

            List<Segment> fractal = CalculateFractal();

            if (fractal.Count == 0)
                return;

            double fractalWidth = maxX - minX;
            double fractalHeight = maxY - minY;

            double margin = 10;

            double Width = pictureBox1.Width - 2 * margin;
            double Height = pictureBox1.Height - 2 * margin;

            double scale = Math.Min( Width / fractalWidth, Height / fractalHeight); //коэф. масштабирования

            //Центрирование
            double offsetX = (pictureBox1.Width - fractalWidth * scale) / 2.0;

            double offsetY = (pictureBox1.Height - fractalHeight * scale) / 2.0;


            bool isTree = checkBox1.Checked;

            if (isTree)
                printTree(fractal, offsetX, offsetY, scale, e.Graphics);
            else
            {
                foreach (Segment segment in fractal)
                {
                    PointF start = new PointF(
                    (float)(offsetX + (segment.Start.X - minX) * scale),
                    (float)(offsetY + (maxY - segment.Start.Y) * scale));

                    PointF end = new PointF(
                        (float)(offsetX + (segment.End.X - minX) * scale),
                        (float)(offsetY + (maxY - segment.End.Y) * scale));

                    using (Pen p = new Pen(Color.Black, 2))
                    {
                        e.Graphics.DrawLine(p, start, end);
                    }
                } 
            }
        }
        private void printTree(List<Segment> fractal, double offsetX, double offsetY, double scale, Graphics g)
        {
            foreach (Segment segment in fractal)
            {
                float t = (float)segment.Depth / maxBranchDepth;

                // Толщина уменьшается от основания к концам ветвей
                float lineWidth = MaxLineWidth + (MinLineWidth - MaxLineWidth) * (float)Math.Pow(t, 0.5);

                // Цвет постепенно меняется от коричневого к зелёному
                Color lineColor = InterpolateColor(trunkColor, branchColor, t);

                PointF start = new PointF(
                    (float)(offsetX + (segment.Start.X - minX) * scale),
                    (float)(offsetY + (maxY - segment.Start.Y) * scale));

                PointF end = new PointF(
                    (float)(offsetX + (segment.End.X - minX) * scale),
                    (float)(offsetY + (maxY - segment.End.Y) * scale));

                using (Pen pen = new Pen(lineColor, lineWidth))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    g.DrawLine(pen, start, end);
                }
            }

        }

        //плавно смешивает два цвета по коэффициенту t
        private Color InterpolateColor(Color start, Color end, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));

            int r = (int)(start.R + (end.R - start.R) * t);
            int g = (int)(start.G + (end.G - start.G) * t);
            int b = (int)(start.B + (end.B - start.B) * t);

            return Color.FromArgb(r, g, b);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
