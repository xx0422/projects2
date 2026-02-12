using System;
using System.Windows.Forms;

namespace elsobeadando
{
    partial class LevelSelectForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonLvl1 = new Button();
            buttonLvl2 = new Button();
            buttonLvl3 = new Button();
            textBox = new TextBox();
            SuspendLayout();
            // 
            // buttonLvl1
            // 
            buttonLvl1.Location = new Point(285, 104);
            buttonLvl1.Name = "buttonLvl1";
            buttonLvl1.Size = new Size(190, 59);
            buttonLvl1.TabIndex = 0;
            buttonLvl1.Text = "Stage 1";
            buttonLvl1.UseVisualStyleBackColor = true;
            buttonLvl1.Click += buttonLvl1_Click;
            // 
            // buttonLvl2
            // 
            buttonLvl2.Location = new Point(285, 229);
            buttonLvl2.Name = "buttonLvl2";
            buttonLvl2.Size = new Size(190, 59);
            buttonLvl2.TabIndex = 1;
            buttonLvl2.Text = "Stage 2";
            buttonLvl2.UseVisualStyleBackColor = true;
            buttonLvl2.Click += buttonLvl2_Click;
            // 
            // buttonLvl3
            // 
            buttonLvl3.Location = new Point(285, 346);
            buttonLvl3.Name = "buttonLvl3";
            buttonLvl3.Size = new Size(190, 59);
            buttonLvl3.TabIndex = 2;
            buttonLvl3.Text = "Stage 3";
            buttonLvl3.UseVisualStyleBackColor = true;
            buttonLvl3.Click += buttonLvl3_Click;
            // 
            // textBox
            // 
            textBox.Location = new Point(307, 27);
            textBox.Name = "textBox";
            textBox.ReadOnly = true;
            textBox.Size = new Size(150, 27);
            textBox.TabIndex = 3;
            textBox.Text = "Kérlek válassz pályát!";
            // 
            // LevelSelectForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox);
            Controls.Add(buttonLvl3);
            Controls.Add(buttonLvl2);
            Controls.Add(buttonLvl1);
            Name = "LevelSelectForm";
            Text = "LevelelectForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonLvl1;
        private Button buttonLvl2;
        private Button buttonLvl3;
        private TextBox textBox;
    }
}