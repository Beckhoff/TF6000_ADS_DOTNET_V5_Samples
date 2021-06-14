using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using TwinCAT.Ads;
using System.IO;

namespace Sample12
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnRead;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		private uint hVar;
		private System.Windows.Forms.ListBox lbArray;
		private AdsClient _client;
	
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
            this.lbArray = new System.Windows.Forms.ListBox();
            this.btnRead = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbArray
            // 
            this.lbArray.ItemHeight = 16;
            this.lbArray.Location = new System.Drawing.Point(19, 9);
            this.lbArray.Name = "lbArray";
            this.lbArray.Size = new System.Drawing.Size(173, 244);
            this.lbArray.TabIndex = 0;
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(19, 277);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(173, 26);
            this.btnRead.TabIndex = 1;
            this.btnRead.Text = "Read";
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(212, 322);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.lbArray);
            this.Name = "Form1";
            this.Text = "Sample12";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
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
			_client = new AdsClient();
			_client.Connect(851);
			
			try
			{
				hVar = _client.CreateVariableHandle("MAIN.PLCArray");
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}
		
		private void btnRead_Click(object sender, System.EventArgs e)
		{
			try
			{
                byte[] readBuffer = new byte[100*2];

				//Array komplett auslesen			
				_client.Read(hVar,readBuffer.AsMemory());

                MemoryStream dataStream = new MemoryStream(readBuffer);
                BinaryReader binRead = new BinaryReader(dataStream);

                lbArray.Items.Clear();
				dataStream.Position = 0;			
				for(int i=0; i<100; i++)
				{
					lbArray.Items.Add(binRead.ReadInt16().ToString());
				}
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//Resourcen wieder freigeben
			try
			{
				_client.DeleteVariableHandle(hVar);					
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
			_client.Dispose();				
		}
        #endregion
    }
}
