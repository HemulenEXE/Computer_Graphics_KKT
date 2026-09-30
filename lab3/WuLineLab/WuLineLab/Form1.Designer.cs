using System.Windows.Forms;

namespace WuLineLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Panel topPanel;
        private Button btnClear;
        private Label lblStatus;
        private TableLayoutPanel mainTable;
        private GroupBox gbCanvas, gbZoom;
        private PictureBox pictureBoxCanvas, pictureBoxZoom;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.topPanel = new Panel();
            this.btnClear = new Button();
            this.lblStatus = new Label();
            this.mainTable = new TableLayoutPanel();
            this.gbCanvas = new GroupBox();
            this.gbZoom = new GroupBox();
            this.pictureBoxCanvas = new PictureBox();
            this.pictureBoxZoom = new PictureBox();

            // topPanel
            this.topPanel.Dock = DockStyle.Top;
            this.topPanel.Height = 60;

            // btnClear
            this.btnClear.Text = "Очистить";
            this.btnClear.Location = new System.Drawing.Point(10, 8);
            this.btnClear.Size = new System.Drawing.Size(120, 30);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            // lblStatus
            this.lblStatus.Text = "Кликните на холст: первый клик — начало отрезка, второй — конец.";
            this.lblStatus.Location = new System.Drawing.Point(140, 15);
            this.lblStatus.Size = new System.Drawing.Size(900, 25);

            this.topPanel.Controls.Add(this.btnClear);
            this.topPanel.Controls.Add(this.lblStatus);

            // mainTable
            this.mainTable.Dock = DockStyle.Fill;
            this.mainTable.ColumnCount = 2;
            this.mainTable.RowCount = 1;
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));

            // gbCanvas
            this.gbCanvas.Text = "Холст (клик — точки отрезка)";
            this.gbCanvas.Dock = DockStyle.Fill;
            this.gbCanvas.Margin = new Padding(5);

            this.pictureBoxCanvas.Dock = DockStyle.Fill;
            this.pictureBoxCanvas.BackColor = System.Drawing.Color.White;
            this.pictureBoxCanvas.Cursor = Cursors.Cross;
            this.pictureBoxCanvas.MouseClick += new MouseEventHandler(this.pictureBoxCanvas_MouseClick);
            this.gbCanvas.Controls.Add(this.pictureBoxCanvas);

            // gbZoom
            this.gbZoom.Text = "Увеличенный фрагмент (видно сглаживание)";
            this.gbZoom.Dock = DockStyle.Fill;
            this.gbZoom.Margin = new Padding(5);

            this.pictureBoxZoom.Dock = DockStyle.Fill;
            this.pictureBoxZoom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pictureBoxZoom.SizeMode = PictureBoxSizeMode.Zoom;
            this.gbZoom.Controls.Add(this.pictureBoxZoom);

            this.mainTable.Controls.Add(this.gbCanvas, 0, 0);
            this.mainTable.Controls.Add(this.gbZoom, 1, 0);

            // Form1
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.mainTable);
            this.Controls.Add(this.topPanel);
            this.Text = "Лабораторная — рисование отрезка алгоритмом Ву";
        }
    }
}