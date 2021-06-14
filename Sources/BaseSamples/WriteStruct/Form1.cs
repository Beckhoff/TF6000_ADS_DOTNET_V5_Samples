using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT.Ads;

namespace Sample13
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.TextBox tbInt;
		private System.Windows.Forms.TextBox tbDint;
		private System.Windows.Forms.TextBox tbByte;
		private System.Windows.Forms.TextBox tbLReal;
		private System.Windows.Forms.TextBox tbReal;

		private System.ComponentModel.Container components = null;

		private uint hVar;
		private AdsClient tcClient;

		public Form1()
		{
			InitializeComponent();			
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.tbInt = new System.Windows.Forms.TextBox();
            this.tbDint = new System.Windows.Forms.TextBox();
            this.tbByte = new System.Windows.Forms.TextBox();
            this.tbLReal = new System.Windows.Forms.TextBox();
            this.tbReal = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnWrite = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbInt
            // 
            this.tbInt.Location = new System.Drawing.Point(86, 37);
            this.tbInt.Name = "tbInt";
            this.tbInt.Size = new System.Drawing.Size(120, 22);
            this.tbInt.TabIndex = 0;
            this.tbInt.Text = "1000";
            // 
            // tbDint
            // 
            this.tbDint.Location = new System.Drawing.Point(86, 74);
            this.tbDint.Name = "tbDint";
            this.tbDint.Size = new System.Drawing.Size(120, 22);
            this.tbDint.TabIndex = 1;
            this.tbDint.Text = "10000";
            // 
            // tbByte
            // 
            this.tbByte.Location = new System.Drawing.Point(86, 111);
            this.tbByte.Name = "tbByte";
            this.tbByte.Size = new System.Drawing.Size(120, 22);
            this.tbByte.TabIndex = 2;
            this.tbByte.Text = "100";
            // 
            // tbLReal
            // 
            this.tbLReal.Location = new System.Drawing.Point(86, 148);
            this.tbLReal.Name = "tbLReal";
            this.tbLReal.Size = new System.Drawing.Size(120, 22);
            this.tbLReal.TabIndex = 3;
            this.tbLReal.Text = "3,145";
            // 
            // tbReal
            // 
            this.tbReal.Location = new System.Drawing.Point(86, 185);
            this.tbReal.Name = "tbReal";
            this.tbReal.Size = new System.Drawing.Size(120, 22);
            this.tbReal.TabIndex = 4;
            this.tbReal.Text = "3,14";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbByte);
            this.groupBox1.Controls.Add(this.tbDint);
            this.groupBox1.Controls.Add(this.tbInt);
            this.groupBox1.Controls.Add(this.tbLReal);
            this.groupBox1.Controls.Add(this.tbReal);
            this.groupBox1.Location = new System.Drawing.Point(19, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(231, 231);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PLCStruct";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(10, 185);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 26);
            this.label5.TabIndex = 9;
            this.label5.Text = "realVal :";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(10, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 26);
            this.label4.TabIndex = 8;
            this.label4.Text = "lrealVal :";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(10, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 26);
            this.label3.TabIndex = 7;
            this.label3.Text = "byteVal :";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 26);
            this.label2.TabIndex = 6;
            this.label2.Text = "dintVal :";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 26);
            this.label1.TabIndex = 5;
            this.label1.Text = "intVal :";
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(19, 249);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(231, 28);
            this.btnWrite.TabIndex = 6;
            this.btnWrite.Text = "Write";
            this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(266, 285);
            this.Controls.Add(this.btnWrite);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Sample13";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
        #region CODE_SAMPLE
		private void Form1_Load(object sender, System.EventArgs e)
		{
			try
			{
                tcClient = new AdsClient();
                tcClient.Connect(851);	
				hVar = tcClient.CreateVariableHandle("MAIN.PLCStruct");
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void btnWrite_Click(object sender, System.EventArgs e)
		{
			MemoryStream dataStream = new MemoryStream(32);
			BinaryWriter binWrite = new BinaryWriter(dataStream);

			dataStream.Position = 0;
			try
			{
				// Adjust datastream.position for 8 byte-alignment

                binWrite.Write(short.Parse(tbInt.Text));
                dataStream.Position = 4;
				binWrite.Write(int.Parse(tbDint.Text));
                dataStream.Position = 8;
				binWrite.Write(byte.Parse(tbByte.Text));
                dataStream.Position = 16;
				binWrite.Write(double.Parse(tbLReal.Text));
                dataStream.Position = 24;
				binWrite.Write(float.Parse(tbReal.Text));

				tcClient.Write(hVar,dataStream.GetBuffer().AsMemory());
			}
			catch( Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//Resourcen wieder freigeben
			try
			{
				tcClient.DeleteVariableHandle(hVar);					
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
			tcClient.Dispose();	
		}
        #endregion
    }
}
