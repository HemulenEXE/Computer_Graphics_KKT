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
        SuspendLayout();

        // 
        // canvas
        // 
        canvas.BackColor = Color.White;
        canvas.BorderStyle = BorderStyle.FixedSingle;
        canvas.Location = new Point(12, 45);
        canvas.Name = "canvas";
        canvas.Size = new Size(1000, 650);
        canvas.TabIndex = 0;
        canvas.Paint += Canvas_Paint;
        canvas.MouseClick += Canvas_MouseClick;

        // 
        // clearButton
        // 
        clearButton.Location = new Point(12, 10);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(120, 29);
        clearButton.TabIndex = 1;
        clearButton.Text = "Очистить";
        clearButton.UseVisualStyleBackColor = true;
        clearButton.Click += ClearButton_Click;

        // 
        // helpLabel
        // 
        helpLabel.AutoSize = true;
        helpLabel.Location = new Point(150, 17);
        helpLabel.Name = "helpLabel";
        helpLabel.Size = new Size(450, 15);
        helpLabel.TabIndex = 2;
        helpLabel.Text = "ЛКМ — добавить вершину | ПКМ / Enter — завершить полигон";

        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1025, 710);
        Controls.Add(helpLabel);
        Controls.Add(clearButton);
        Controls.Add(canvas);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Работа с полигонами";
        Load += MainForm_Load;
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;

        ResumeLayout(false);
        PerformLayout();
    }
}