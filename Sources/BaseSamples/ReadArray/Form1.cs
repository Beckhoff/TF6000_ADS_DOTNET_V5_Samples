using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using TwinCAT.Ads;
using System.IO;
using TwinCAT;
using TwinCAT.TypeSystem;
using TwinCAT.Ads.TypeSystem;

namespace _12_ReadArray
{
	#region CODE_SAMPLE
	public partial class Form1 : Form
	{
		AdsClient tcClient = null;
		ISymbolLoader loader = null;
		string arrayVar = "";
		 //Update the AMSNetId for your target
		AmsNetId targetAmsNetId = AmsNetId.Local;
		 public Form1()
        {
            InitializeComponent();
        }
		private void Form1_Load(object sender, System.EventArgs e)
		{
			//Create a new instance of class AdsClient
			tcClient = new AdsClient();

			//Connect to target PLC - Port 851
			tcClient.Connect("169.254.139.155.1.1", 851);
			
			try
			{
				//Load symbols form target system
				SymbolLoaderSettings settings = new SymbolLoaderSettings(SymbolsLoadMode.DynamicTree);
				loader = SymbolLoaderFactory.Create(tcClient, settings);
				arrayVar = "MAIN.PLCArray";
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
				//Get Array values
				DynamicSymbol arrayRead = (DynamicSymbol)loader.Symbols[arrayVar]; 
				short[] readBuffer = (short[])arrayRead.ReadValue();

				foreach (short i in readBuffer)
				{
					lbArray.Items.Add(i.ToString());
				}
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				// Dispose the Client during Form Cleanup
				tcClient.Dispose();

				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
        #endregion
	}
	
    
}
