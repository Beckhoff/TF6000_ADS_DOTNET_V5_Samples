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

namespace S12_ReadArray
{
	public partial class Form1 : Form
	{
		AdsClient _client = null;
		ISymbolLoader _symbolLoader = null;
		string _arrayVar = "";
		 
        //Update the AMSNetId for your target
		AmsNetId _targetAmsNetId = AmsNetId.Local;

		 public Form1()
        {
            InitializeComponent();
        }
		private void Form1_Load(object sender, System.EventArgs e)
		{
			//Create a new instance of class AdsClient
			_client = new AdsClient();

			//Connect to target PLC - Port 851
			_client.Connect(_targetAmsNetId, 851);
		}
		
		private void btnRead_Click(object sender, System.EventArgs e)
		{
			try
			{
				// 1. Alternative, Read the values by Symbol Loader
				// Using the Dynamic Tree automatically marshalls the values to .net types - even if they are complex types
				//Get Array values
				//Load symbols form target system
				SymbolLoaderSettings settings = new SymbolLoaderSettings(SymbolsLoadMode.DynamicTree);
				_symbolLoader = SymbolLoaderFactory.Create(_client, settings);
				_arrayVar = "MAIN.PLCArray";

				DynamicSymbol arrayRead = (DynamicSymbol)_symbolLoader.Symbols[_arrayVar]; 
				short[] readBuffer = (short[])arrayRead.ReadValue();

				foreach (short i in readBuffer)
				{
					lbArray.Items.Add(i.ToString());
				}

				// 2. Alternative, read the data without SymbolLoader in a raw buffer.
				//hVar = _client.CreateVariableHandle("MAIN.PLCArray"); // Create the variable handle by name
				//byte[] readBuffer = new byte[100 * 2];

				////Read complete array (200 bytes == 100 ushorts)
				//_client.Read(hVar, readBuffer.AsMemory());

				//MemoryStream dataStream = new MemoryStream(readBuffer);
				//BinaryReader binRead = new BinaryReader(dataStream);

				//lbArray.Items.Clear();
				//dataStream.Position = 0;
				//for (int i = 0; i < 100; i++)
				//{
				//	lbArray.Items.Add(binRead.ReadInt16().ToString());
				//}
				//_client.DeleteVariableHandle(hVar); // Free the handle after use
			}
			catch (Exception err)
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
				_client.Dispose();

				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
	}
}
