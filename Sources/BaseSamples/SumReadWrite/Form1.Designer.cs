namespace _22_SumReadWrite
{
    partial class Form1
    {
        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.tbDint = new System.Windows.Forms.TextBox();
            this.tbBool = new System.Windows.Forms.TextBox();
            this.tbUint = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tbDint2 = new System.Windows.Forms.TextBox();
            this.tbBool2 = new System.Windows.Forms.TextBox();
            this.tbUint2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(104, 100);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 27);
            this.button1.TabIndex = 0;
            this.button1.Text = "Read";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbDint
            // 
            this.tbDint.Location = new System.Drawing.Point(89, 40);
            this.tbDint.Name = "tbDint";
            this.tbDint.Size = new System.Drawing.Size(120, 22);
            this.tbDint.TabIndex = 2;
            // 
            // tbBool
            // 
            this.tbBool.Location = new System.Drawing.Point(89, 70);
            this.tbBool.Name = "tbBool";
            this.tbBool.Size = new System.Drawing.Size(120, 22);
            this.tbBool.TabIndex = 3;
            // 
            // tbUint
            // 
            this.tbUint.Location = new System.Drawing.Point(89, 10);
            this.tbUint.Name = "tbUint";
            this.tbUint.Size = new System.Drawing.Size(120, 22);
            this.tbUint.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "uintValue:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(14, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "dintValue:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(14, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "boolValue:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(230, 100);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 27);
            this.button2.TabIndex = 7;
            this.button2.Text = "Write";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tbDint2
            // 
            this.tbDint2.Location = new System.Drawing.Point(216, 40);
            this.tbDint2.Name = "tbDint2";
            this.tbDint2.Size = new System.Drawing.Size(120, 22);
            this.tbDint2.TabIndex = 12;
            // 
            // tbBool2
            // 
            this.tbBool2.Location = new System.Drawing.Point(216, 70);
            this.tbBool2.Name = "tbBool2";
            this.tbBool2.Size = new System.Drawing.Size(120, 22);
            this.tbBool2.TabIndex = 13;
            // 
            // tbUint2
            // 
            this.tbUint2.Location = new System.Drawing.Point(216, 10);
            this.tbUint2.Name = "tbUint2";
            this.tbUint2.Size = new System.Drawing.Size(120, 22);
            this.tbUint2.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(333, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "*please note: you have to fill in all of the three fields";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 153);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(279, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "on the right to write Values to the Variables";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(323, 95);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 17);
            this.label6.TabIndex = 16;
            this.label6.Text = "*";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(350, 179);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbBool2);
            this.Controls.Add(this.tbDint2);
            this.Controls.Add(this.tbUint2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbBool);
            this.Controls.Add(this.tbDint);
            this.Controls.Add(this.tbUint);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "ADS communication of VS .NET";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox tbDint;
        private System.Windows.Forms.TextBox tbBool;
        private System.Windows.Forms.TextBox tbUint;
        private System.Windows.Forms.TextBox tbDint2;
        private System.Windows.Forms.TextBox tbBool2;
        private System.Windows.Forms.TextBox tbUint2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}