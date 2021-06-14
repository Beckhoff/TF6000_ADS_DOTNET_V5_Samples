namespace TwinCATAds_Sample08
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
            this._routerLabelTitle = new System.Windows.Forms.Label();
            this._plcLabelTitle = new System.Windows.Forms.Label();
            this._routerLabelValue = new System.Windows.Forms.Label();
            this._plcLabelValue = new System.Windows.Forms.Label();
            this._exitButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _routerLabelTitle
            // 
            this._routerLabelTitle.AutoSize = true;
            this._routerLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._routerLabelTitle.Location = new System.Drawing.Point(13, 31);
            this._routerLabelTitle.Name = "_routerLabelTitle";
            this._routerLabelTitle.Size = new System.Drawing.Size(165, 29);
            this._routerLabelTitle.TabIndex = 0;
            this._routerLabelTitle.Text = "Router State:";
            // 
            // _plcLabelTitle
            // 
            this._plcLabelTitle.AutoSize = true;
            this._plcLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._plcLabelTitle.Location = new System.Drawing.Point(13, 93);
            this._plcLabelTitle.Name = "_plcLabelTitle";
            this._plcLabelTitle.Size = new System.Drawing.Size(136, 29);
            this._plcLabelTitle.TabIndex = 1;
            this._plcLabelTitle.Text = "PLC State:";
            // 
            // _routerLabelValue
            // 
            this._routerLabelValue.AutoSize = true;
            this._routerLabelValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._routerLabelValue.Location = new System.Drawing.Point(212, 31);
            this._routerLabelValue.Name = "_routerLabelValue";
            this._routerLabelValue.Size = new System.Drawing.Size(0, 29);
            this._routerLabelValue.TabIndex = 2;
            // 
            // _plcLabelValue
            // 
            this._plcLabelValue.AutoSize = true;
            this._plcLabelValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._plcLabelValue.Location = new System.Drawing.Point(212, 93);
            this._plcLabelValue.Name = "_plcLabelValue";
            this._plcLabelValue.Size = new System.Drawing.Size(0, 29);
            this._plcLabelValue.TabIndex = 3;
            // 
            // _exitButton
            // 
            this._exitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._exitButton.Location = new System.Drawing.Point(217, 162);
            this._exitButton.Name = "_exitButton";
            this._exitButton.Size = new System.Drawing.Size(75, 23);
            this._exitButton.TabIndex = 4;
            this._exitButton.Text = "Exit";
            this._exitButton.UseVisualStyleBackColor = true;
            this._exitButton.Click += new System.EventHandler(this._exitButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 197);
            this.Controls.Add(this._exitButton);
            this.Controls.Add(this._plcLabelValue);
            this.Controls.Add(this._routerLabelValue);
            this.Controls.Add(this._plcLabelTitle);
            this.Controls.Add(this._routerLabelTitle);
            this.Name = "Form1";
            this.Text = "Sample23";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _routerLabelTitle;
		private System.Windows.Forms.Label _plcLabelTitle;
		private System.Windows.Forms.Label _routerLabelValue;
		private System.Windows.Forms.Label _plcLabelValue;
		private System.Windows.Forms.Button _exitButton;

	}
}

