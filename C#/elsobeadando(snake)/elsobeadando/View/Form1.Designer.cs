namespace elsobeadando
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - 
        /// do not modify the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            labelTime = new Label();
            timerTime = new System.Windows.Forms.Timer(components);
            labelScore = new Label();
            buttonPause = new Button();
            SuspendLayout();
            // 
            // labelTime
            // 
            labelTime.AutoSize = true;
            labelTime.Font = new Font("Segoe UI", 12F);
            labelTime.Location = new Point(0, 0);
            labelTime.Name = "labelTime";
            labelTime.Size = new Size(87, 28);
            labelTime.TabIndex = 0;
            labelTime.Text = "Time: 0 s";
            // 
            // timerTime
            // 
            timerTime.Interval = 1000;
            timerTime.Tick += timerTime_Tick;
            // 
            // labelScore
            // 
            labelScore.AutoSize = true;
            labelScore.Font = new Font("Segoe UI", 12F);
            labelScore.Location = new Point(100, 0);
            labelScore.Name = "labelScore";
            labelScore.Size = new Size(81, 28);
            labelScore.TabIndex = 1;
            labelScore.Text = "Score: 0";
            labelScore.Click += labelScore_Click;
            // 
            // buttonPause
            // 
            buttonPause.Font = new Font("Segoe UI", 10F);
            buttonPause.ImageAlign = ContentAlignment.TopCenter;
            buttonPause.Location = new Point(200, 0);
            buttonPause.Name = "buttonPause";
            buttonPause.Size = new Size(94, 30);
            buttonPause.TabIndex = 2;
            buttonPause.Text = "Pause";
            buttonPause.UseVisualStyleBackColor = true;
            buttonPause.Click += buttonPause_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(788, 399);
            Controls.Add(buttonPause);
            Controls.Add(labelScore);
            Controls.Add(labelTime);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Snake";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelTime;
        private System.Windows.Forms.Timer timerTime;
        private Label labelScore;
        private Button buttonPause;
    }
}
