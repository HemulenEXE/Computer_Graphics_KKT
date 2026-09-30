namespace _1_v
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Panel topPanel;
        private Button btnOpen;
        private Label lblStatus;
        private TableLayoutPanel mainTable;
        private GroupBox gbOriginal, gbResult;
        private PictureBox pictureBoxOriginal, pictureBoxResult;

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
            this.lblStatus = new Label();
            this.mainTable = new TableLayoutPanel();
            this.gbOriginal = new GroupBox();
            this.gbResult = new GroupBox();
            this.pictureBoxOriginal = new PictureBox();
            this.pictureBoxResult = new PictureBox();

            // topPanel
            this.topPanel.Dock = DockStyle.Top;
            this.topPanel.Height = 70;

            // btnOpen
            this.btnOpen.Text = "Загрузить изображение";
            this.btnOpen.Location = new System.Drawing.Point(10, 8);
            this.btnOpen.Size = new System.Drawing.Size(200, 30);
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);

            // lblStatus
            this.lblStatus.Text = "Загрузите изображение, затем кликните на пиксель границы (цвет клика станет цветом границы, точка — стартовой).";
            this.lblStatus.Location = new System.Drawing.Point(10, 42);
            this.lblStatus.Size = new System.Drawing.Size(1000, 25);
            this.lblStatus.AutoEllipsis = true;

            this.topPanel.Controls.Add(this.btnOpen);
            this.topPanel.Controls.Add(this.lblStatus);

            // mainTable
            this.mainTable.Dock = DockStyle.Fill;
            this.mainTable.ColumnCount = 2;
            this.mainTable.RowCount = 1;
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // gbOriginal
            this.gbOriginal.Text = "Оригинал (кликните на границу)";
            this.gbOriginal.Dock = DockStyle.Fill;
            this.gbOriginal.Margin = new Padding(5);

            this.pictureBoxOriginal.Dock = DockStyle.Fill;
            this.pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBoxOriginal.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pictureBoxOriginal.Cursor = Cursors.Cross;
            this.pictureBoxOriginal.MouseClick += new MouseEventHandler(this.pictureBoxOriginal_MouseClick);
            this.gbOriginal.Controls.Add(this.pictureBoxOriginal);

            // gbResult
            this.gbResult.Text = "Найденная граница (красным)";
            this.gbResult.Dock = DockStyle.Fill;
            this.gbResult.Margin = new Padding(5);

            this.pictureBoxResult.Dock = DockStyle.Fill;
            this.pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBoxResult.BackColor = System.Drawing.Color.WhiteSmoke;
            this.gbResult.Controls.Add(this.pictureBoxResult);

            this.mainTable.Controls.Add(this.gbOriginal, 0, 0);
            this.mainTable.Controls.Add(this.gbResult, 1, 0);

            // Form1
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.mainTable);
            this.Controls.Add(this.topPanel);
            this.Text = "Лабораторная — обход границы связной области";
        }
    }
}