using System.Xml.Linq;

namespace lab4;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel canvas;
    private Button clearButton;
    private Label helpLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        canvas = new Panel();
        clearButton = new Button();
        helpLabel = new Label();
        dxInput = new NumericUpDown();
        label1 = new Label();
        dyInput = new NumericUpDown();
        label2 = new Label();
        label3 = new Label();
        angleInput = new NumericUpDown();
        label4 = new Label();
        scaleXInput = new NumericUpDown();
        scaleYInput = new NumericUpDown();
        label5 = new Label();
        label6 = new Label();
        pointXInput = new NumericUpDown();
        pointYInput = new NumericUpDown();
        label7 = new Label();
        label8 = new Label();
        label9 = new Label();
        label10 = new Label();
        label11 = new Label();
        comboBox1 = new ComboBox();
        ApplySettingsButton = new Button();
        ((System.ComponentModel.ISupportInitialize)dxInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dyInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)angleInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)scaleXInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)scaleYInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pointXInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pointYInput).BeginInit();
        SuspendLayout();
        // 
        // canvas
        // 
        canvas.BackColor = Color.White;
        canvas.BorderStyle = BorderStyle.FixedSingle;
        canvas.Location = new Point(14, 181);
        canvas.Margin = new Padding(3, 4, 3, 4);
        canvas.Name = "canvas";
        canvas.Size = new Size(1143, 745);
        canvas.TabIndex = 0;
        canvas.Paint += Canvas_Paint;
        canvas.MouseClick += Canvas_MouseClick;
        // 
        // clearButton
        // 
        clearButton.Location = new Point(14, 13);
        clearButton.Margin = new Padding(3, 4, 3, 4);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(137, 39);
        clearButton.TabIndex = 1;
        clearButton.Text = "Очистить";
        clearButton.UseVisualStyleBackColor = true;
        clearButton.Click += ClearButton_Click;
        // 
        // helpLabel
        // 
        helpLabel.AutoSize = true;
        helpLabel.Location = new Point(171, 23);
        helpLabel.Name = "helpLabel";
        helpLabel.Size = new Size(449, 20);
        helpLabel.TabIndex = 2;
        helpLabel.Text = "ЛКМ — добавить вершину | ПКМ / Enter — завершить полигон";
        // 
        // dxInput
        // 
        dxInput.Location = new Point(17, 109);
        dxInput.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
        dxInput.Name = "dxInput";
        dxInput.Size = new Size(77, 27);
        dxInput.TabIndex = 3;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(257, 111);
        label1.Name = "label1";
        label1.Size = new Size(83, 20);
        label1.TabIndex = 5;
        label1.Text = "Смещение";
        // 
        // dyInput
        // 
        dyInput.Location = new Point(131, 109);
        dyInput.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
        dyInput.Name = "dyInput";
        dyInput.Size = new Size(77, 27);
        dyInput.TabIndex = 6;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(100, 111);
        label2.Name = "label2";
        label2.Size = new Size(25, 20);
        label2.TabIndex = 7;
        label2.Text = "dx";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(214, 111);
        label3.Name = "label3";
        label3.Size = new Size(25, 20);
        label3.TabIndex = 8;
        label3.Text = "dy";
        // 
        // angleInput
        // 
        angleInput.Location = new Point(131, 142);
        angleInput.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
        angleInput.Name = "angleInput";
        angleInput.Size = new Size(77, 27);
        angleInput.TabIndex = 9;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(257, 142);
        label4.Name = "label4";
        label4.Size = new Size(146, 20);
        label4.TabIndex = 10;
        label4.Text = "Поворот в градусах";
        // 
        // scaleXInput
        // 
        scaleXInput.Location = new Point(494, 109);
        scaleXInput.Name = "scaleXInput";
        scaleXInput.Size = new Size(77, 27);
        scaleXInput.TabIndex = 11;
        // 
        // scaleYInput
        // 
        scaleYInput.Location = new Point(634, 109);
        scaleYInput.Name = "scaleYInput";
        scaleYInput.Size = new Size(77, 27);
        scaleYInput.TabIndex = 12;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(577, 111);
        label5.Name = "label5";
        label5.Size = new Size(51, 20);
        label5.TabIndex = 13;
        label5.Text = "scaleX";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(717, 111);
        label6.Name = "label6";
        label6.Size = new Size(50, 20);
        label6.TabIndex = 14;
        label6.Text = "scaleY";
        // 
        // pointXInput
        // 
        pointXInput.Location = new Point(494, 142);
        pointXInput.Name = "pointXInput";
        pointXInput.Size = new Size(77, 27);
        pointXInput.TabIndex = 15;
        // 
        // pointYInput
        // 
        pointYInput.Location = new Point(634, 142);
        pointYInput.Name = "pointYInput";
        pointYInput.Size = new Size(77, 27);
        pointYInput.TabIndex = 16;
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Location = new Point(577, 147);
        label7.Name = "label7";
        label7.Size = new Size(18, 20);
        label7.TabIndex = 17;
        label7.Text = "X";
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Location = new Point(717, 147);
        label8.Name = "label8";
        label8.Size = new Size(17, 20);
        label8.TabIndex = 18;
        label8.Text = "Y";
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.Location = new Point(785, 111);
        label9.Name = "label9";
        label9.Size = new Size(202, 20);
        label9.TabIndex = 19;
        label9.Text = "Масштабирование по осям";
        // 
        // label10
        // 
        label10.AutoSize = true;
        label10.Location = new Point(785, 144);
        label10.Name = "label10";
        label10.Size = new Size(131, 20);
        label10.TabIndex = 20;
        label10.Text = "Выбранная точка";
        // 
        // label11
        // 
        label11.AutoSize = true;
        label11.Location = new Point(12, 64);
        label11.Name = "label11";
        label11.Size = new Size(373, 20);
        label11.TabIndex = 21;
        label11.Text = "Аффинные преобразования выбранного полигона:";
        // 
        // comboBox1
        // 
        comboBox1.FormattingEnabled = true;
        comboBox1.Items.AddRange(new object[] { "Перенос (смещение)", "Поворот вокруг точки", "Поворот вокруг центра", "Масштабирование относительно  точки", "Масштабирование относительно центра" });
        comboBox1.Location = new Point(409, 64);
        comboBox1.Name = "comboBox1";
        comboBox1.Size = new Size(355, 28);
        comboBox1.TabIndex = 22;
        // 
        // ApplySettingsButton
        // 
        ApplySettingsButton.Location = new Point(782, 64);
        ApplySettingsButton.Margin = new Padding(3, 4, 3, 4);
        ApplySettingsButton.Name = "ApplySettingsButton";
        ApplySettingsButton.Size = new Size(137, 30);
        ApplySettingsButton.TabIndex = 23;
        ApplySettingsButton.Text = "Применить";
        ApplySettingsButton.UseVisualStyleBackColor = true;
        ApplySettingsButton.Click += ApplySettingsButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1171, 947);
        Controls.Add(ApplySettingsButton);
        Controls.Add(comboBox1);
        Controls.Add(label11);
        Controls.Add(label10);
        Controls.Add(label9);
        Controls.Add(label8);
        Controls.Add(label7);
        Controls.Add(pointYInput);
        Controls.Add(pointXInput);
        Controls.Add(label6);
        Controls.Add(label5);
        Controls.Add(scaleYInput);
        Controls.Add(scaleXInput);
        Controls.Add(label4);
        Controls.Add(angleInput);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(dyInput);
        Controls.Add(label1);
        Controls.Add(dxInput);
        Controls.Add(helpLabel);
        Controls.Add(clearButton);
        Controls.Add(canvas);
        KeyPreview = true;
        Margin = new Padding(3, 4, 3, 4);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Работа с полигонами";
        Load += MainForm_Load;
        KeyDown += MainForm_KeyDown;
        ((System.ComponentModel.ISupportInitialize)dxInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)dyInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)angleInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)scaleXInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)scaleYInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)pointXInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)pointYInput).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private NumericUpDown dxInput;
    private Label label1;
    private NumericUpDown dyInput;
    private Label label2;
    private Label label3;
    private NumericUpDown angleInput;
    private Label label4;
    private NumericUpDown scaleXInput;
    private NumericUpDown scaleYInput;
    private Label label5;
    private Label label6;
    private NumericUpDown pointXInput;
    private NumericUpDown pointYInput;
    private Label label7;
    private Label label8;
    private Label label9;
    private Label label10;
    private Label label11;
    private ComboBox comboBox1;
    private Button ApplySettingsButton;
}