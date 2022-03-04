namespace _30_ADS.NET_MethodCall
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbValueA = new System.Windows.Forms.TextBox();
            this.tbValueB = new System.Windows.Forms.TextBox();
            this.tbSumAB = new System.Windows.Forms.TextBox();
            this.btnMethodCall = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbValueA
            // 
            this.tbValueA.Location = new System.Drawing.Point(13, 21);
            this.tbValueA.Name = "tbValueA";
            this.tbValueA.Size = new System.Drawing.Size(40, 20);
            this.tbValueA.TabIndex = 0;
            this.tbValueA.Text = "23";
            // 
            // tbValueB
            // 
            this.tbValueB.Location = new System.Drawing.Point(78, 21);
            this.tbValueB.Name = "tbValueB";
            this.tbValueB.Size = new System.Drawing.Size(40, 20);
            this.tbValueB.TabIndex = 0;
            this.tbValueB.Text = "19";
            // 
            // tbSumAB
            // 
            this.tbSumAB.Location = new System.Drawing.Point(165, 21);
            this.tbSumAB.Name = "tbSumAB";
            this.tbSumAB.Size = new System.Drawing.Size(40, 20);
            this.tbSumAB.TabIndex = 0;
            // 
            // btnMethodCall
            // 
            this.btnMethodCall.Location = new System.Drawing.Point(124, 19);
            this.btnMethodCall.Name = "btnMethodCall";
            this.btnMethodCall.Size = new System.Drawing.Size(35, 22);
            this.btnMethodCall.TabIndex = 1;
            this.btnMethodCall.Text = "=";
            this.btnMethodCall.UseVisualStyleBackColor = true;
            this.btnMethodCall.Click += new System.EventHandler(this.btnMethodCall_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(59, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "+";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 62);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnMethodCall);
            this.Controls.Add(this.tbSumAB);
            this.Controls.Add(this.tbValueB);
            this.Controls.Add(this.tbValueA);
            this.Name = "Form1";
            this.Text = "ADS Method call";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbValueA;
        private System.Windows.Forms.TextBox tbValueB;
        private System.Windows.Forms.TextBox tbSumAB;
        private System.Windows.Forms.Button btnMethodCall;
        private System.Windows.Forms.Label label3;
    }
}

