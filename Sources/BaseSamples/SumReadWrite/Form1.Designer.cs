namespace S22_SumReadWrite
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
            this.btnRead = new System.Windows.Forms.Button();
            this.tbDint = new System.Windows.Forms.TextBox();
            this.tbBool = new System.Windows.Forms.TextBox();
            this.tbUint = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnWrite = new System.Windows.Forms.Button();
            this.tbDint2 = new System.Windows.Forms.TextBox();
            this.tbBool2 = new System.Windows.Forms.TextBox();
            this.tbUint2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(156, 160);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(135, 43);
            this.btnRead.TabIndex = 0;
            this.btnRead.Text = "Read";
            this.btnRead.Click += new System.EventHandler(this.readSymbolic_Click);
            // 
            // tbDint
            // 
            this.tbDint.Location = new System.Drawing.Point(134, 64);
            this.tbDint.Name = "tbDint";
            this.tbDint.Size = new System.Drawing.Size(180, 31);
            this.tbDint.TabIndex = 2;
            // 
            // tbBool
            // 
            this.tbBool.Location = new System.Drawing.Point(134, 112);
            this.tbBool.Name = "tbBool";
            this.tbBool.Size = new System.Drawing.Size(180, 31);
            this.tbBool.TabIndex = 3;
            // 
            // tbUint
            // 
            this.tbUint.Location = new System.Drawing.Point(134, 16);
            this.tbUint.Name = "tbUint";
            this.tbUint.Size = new System.Drawing.Size(180, 31);
            this.tbUint.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(21, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 37);
            this.label1.TabIndex = 4;
            this.label1.Text = "uintValue:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(21, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 37);
            this.label2.TabIndex = 5;
            this.label2.Text = "dintValue:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(21, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 37);
            this.label3.TabIndex = 6;
            this.label3.Text = "boolValue:";
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(345, 160);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(135, 43);
            this.btnWrite.TabIndex = 7;
            this.btnWrite.Text = "Write";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.writeSymbolic_Click);
            // 
            // tbDint2
            // 
            this.tbDint2.Location = new System.Drawing.Point(324, 64);
            this.tbDint2.Name = "tbDint2";
            this.tbDint2.Size = new System.Drawing.Size(180, 31);
            this.tbDint2.TabIndex = 12;
            // 
            // tbBool2
            // 
            this.tbBool2.Location = new System.Drawing.Point(324, 112);
            this.tbBool2.Name = "tbBool2";
            this.tbBool2.Size = new System.Drawing.Size(180, 31);
            this.tbBool2.TabIndex = 13;
            // 
            // tbUint2
            // 
            this.tbUint2.Location = new System.Drawing.Point(324, 16);
            this.tbUint2.Name = "tbUint2";
            this.tbUint2.Size = new System.Drawing.Size(180, 31);
            this.tbUint2.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 224);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(422, 25);
            this.label4.TabIndex = 14;
            this.label4.Text = "*please note: you have to fill in all of the three fields";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 251);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(352, 25);
            this.label5.TabIndex = 15;
            this.label5.Text = "on the right to write Values to the Variables";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(484, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 25);
            this.label6.TabIndex = 16;
            this.label6.Text = "*";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(9, 24);
            this.ClientSize = new System.Drawing.Size(539, 296);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbBool2);
            this.Controls.Add(this.tbDint2);
            this.Controls.Add(this.tbUint2);
            this.Controls.Add(this.btnWrite);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbBool);
            this.Controls.Add(this.tbDint);
            this.Controls.Add(this.tbUint);
            this.Controls.Add(this.btnRead);
            this.Name = "Form1";
            this.Text = "ADS communication of VS .NET";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnWrite;
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