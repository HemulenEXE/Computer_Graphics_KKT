using System.Windows.Forms;

namespace GrayscaleLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Panel topPanel;
        private Button btnOpen;
        private TableLayoutPanel mainTable;

        private GroupBox gbOriginal, gbGray1, gbGray2, gbDiff, gbHist1, gbHist2;
        private PictureBox pictureBoxOriginal, pictureBoxGray1, pictureBoxGray2,
                            pictureBoxDiff, pictureBoxHist1, pictureBoxHist2;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.topPanel = new Panel();
            this.btnOpen = new Button();
            this.mainTable = new TableLayoutPanel();

            this.gbOriginal = new GroupBox();
            this.gbGray1 = new GroupBox();
            this.gbGray2 = new GroupBox();
            this.gbDiff = new GroupBox();
            this.gbHist1 = new GroupBox();
            this.gbHist2 = new GroupBox();

            this.pictureBoxOriginal = new PictureBox();
            this.pictureBoxGray1 = new PictureBox();
            this.pictureBoxGray2 = new PictureBox();
            this.pictureBoxDiff = new PictureBox();
            this.pictureBoxHist1 = new PictureBox();
            this.pictureBoxHist2 = new PictureBox();

            // topPanel
            this.topPanel.Dock = DockStyle.Top;
            this.topPanel.Height = 45;
            this.topPanel.Controls.Add(this.btnOpen);

            // btnOpen
            this.btnOpen.Text = "Загрузить изображение";
            this.btnOpen.Location = new System.Drawing.Point(10, 8);
            this.btnOpen.Size = new System.Drawing.Size(200, 30);
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);

            // mainTable
            this.mainTable.Dock = DockStyle.Fill;
            this.mainTable.ColumnCount = 3;
            this.mainTable.RowCount = 2;
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            this.mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            this.mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            SetupGroup(gbOriginal, pictureBoxOriginal, "Оригинал");
            SetupGroup(gbGray1, pictureBoxGray1, "Оттенки серого (YUV/NTSC)");
            SetupGroup(gbGray2, pictureBoxGray2, "Оттенки серого (HDTV)");
            SetupGroup(gbDiff, pictureBoxDiff, "Разность изображений");
            SetupGroup(gbHist1, pictureBoxHist1, "Гистограмма 1");
            SetupGroup(gbHist2, pictureBoxHist2, "Гистограмма 2");

            this.mainTable.Controls.Add(this.gbOriginal, 0, 0);
            this.mainTable.Controls.Add(this.gbGray1, 1, 0);
            this.mainTable.Controls.Add(this.gbGray2, 2, 0);
            this.mainTable.Controls.Add(this.gbDiff, 0, 1);
            this.mainTable.Controls.Add(this.gbHist1, 1, 1);
            this.mainTable.Controls.Add(this.gbHist2, 2, 1);

            // Form1
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.mainTable);
            this.Controls.Add(this.topPanel);
            this.Text = "Лабораторная — перевод в оттенки серого";
        }

        private void SetupGroup(GroupBox gb, PictureBox pb, string title)
        {
            gb.Text = title;
            gb.Dock = DockStyle.Fill;
            gb.Margin = new Padding(5);

            pb.Dock = DockStyle.Fill;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.BackColor = System.Drawing.Color.WhiteSmoke;

            gb.Controls.Add(pb);
        }
    }
}