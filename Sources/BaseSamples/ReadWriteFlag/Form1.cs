using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using System.Linq;
using TwinCAT;
using System.Threading.Tasks;
using System.Threading;
using TwinCAT.ValueAccess;
using System.Text.RegularExpressions;

namespace Sample01
{
	public class Form1 : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button btnRead;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Label label1;
		private AdsClient adsClient;

		public Form1()
		{				
			InitializeComponent();
		}

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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnRead = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(117, 18);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(152, 22);
            this.textBox1.TabIndex = 0;
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(288, 18);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(86, 28);
            this.btnRead.TabIndex = 1;
            this.btnRead.Text = "Read";
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(288, 55);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(86, 28);
            this.btnReset.TabIndex = 2;
            this.btnReset.Text = "Reset";
            this.btnReset.Click += new System.EventHandler(this.btnWrite_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "MAIN.nCounter:";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(384, 91);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Sample01";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			try
			{
				adsClient = new AdsClient();
				adsClient.Connect(851);
			}
			catch( Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			adsClient.Dispose();
		}		

    #region CODE_SAMPLE

		private void btnRead_Click(object sender, System.EventArgs e)
		{
			try
			{
                //Specify IndexGroup, IndexOffset and read PLCVar 
                uint iFlag = (uint)adsClient.ReadAny(0x4020, 0x0, typeof(UInt32));
                textBox1.Text = iFlag.ToString();
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void btnWrite_Click(object sender, System.EventArgs e)
		{
			try
			{
                //Specify IndexGroup, IndexOffset and write PLCVar, then read the new Value
                UInt32 iFlag = 0;
                adsClient.WriteAny(0x4020, 0x0, iFlag);
                iFlag = (uint)adsClient.ReadAny(0x4020, 0x0, typeof(UInt32));
                textBox1.Text = iFlag.ToString();
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}
        #endregion

        public void ReadIndexGroupIndexOffset()
        {
            #region CODE_SAMPLE_READWRITE_IGIO

            using (AdsClient client = new AdsClient())
            {
                UInt32 valueToRead = 0;
                UInt32 valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Write an UINT32 Value
                byte[] writeData = new byte[sizeof(uint)];
                MemoryStream writeStream = new MemoryStream(writeData);
                BinaryWriter writer = new BinaryWriter(writeStream);
                writer.Write(valueToWrite);
                client.Write(0x4020, 0x0, writeData);

                // Read an UINT32 Value
                byte[] readData = new byte[sizeof(uint)];
                int readBytes = client.Read(0x4020, 0x0, readData);

                MemoryStream readStream = new MemoryStream(readData);
                readStream.Position = 0;
                BinaryReader reader = new BinaryReader(readStream);
                valueToRead = reader.ReadUInt32();
            }
            #endregion

            #region CODE_SAMPLE_READWRITE_BYHANDLE

            using (AdsClient client = new AdsClient())
            {
                uint varHandle = 0;
                client.Connect(AmsNetId.Local, 851);
                try
                {
                    UInt16 valueToRead = 0;
                    UInt16 valueToWrite = 42;

                    // Create the Variable Handle
                    varHandle = client.CreateVariableHandle("MAIN.testVar"); //Test Var is defined as PLC INT

                    // Write an UINT16 Value
                    byte[] writeData = new byte[sizeof(ushort)];

                    MemoryStream writeStream = new MemoryStream(writeData);
                    BinaryWriter writer = new BinaryWriter(writeStream);
                    writer.Write(valueToWrite); // Marshal the Value
                    client.Write(varHandle, writeData.AsMemory());

                    // Read an UINT16 Value
                    byte[] readData = new byte[sizeof(ushort)];

                    MemoryStream readStream = new MemoryStream(readData);
                    client.Read(varHandle, readData.AsMemory());
                    BinaryReader reader = new BinaryReader(readStream);
                    valueToRead = reader.ReadUInt16(); // Unmarshal the Value
                }
                finally
                {
                    // Unregister VarHandle after Use
                    client.DeleteVariableHandle(varHandle);
                }
            }
            #endregion

        }

        public void ReadAny()
        {
            #region CODE_SAMPLE_READWRITE_ANY

            using (AdsClient client = new AdsClient())
            {
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);
                adsClient.WriteAny(0x4020, 0x0, valueToWrite);
                valueToRead = (uint)adsClient.ReadAny(0x4020, 0x0, typeof(uint));
            }
            #endregion


            #region CODE_SAMPLE_BYHANDLE

            using (AdsClient client = new AdsClient())
            {
                uint varHandle = 0;
                client.Connect(AmsNetId.Local, 851);
                try
                {
                    uint valueToRead = 0;
                    uint valueToWrite = 42;

                    varHandle = client.CreateVariableHandle("MAIN.nCounter");
                    adsClient.WriteAny(varHandle, valueToWrite);
                    valueToRead = (uint)adsClient.ReadAny(varHandle, typeof(uint));
                }
                finally
                {
                    // Unregister VarHandle after Use
                    client.DeleteVariableHandle(varHandle);
                }
            }
            #endregion

            #region CODE_SAMPLE_BYPATH

            using (AdsClient client = new AdsClient())
            {
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);
                client.WriteValue("MAIN.nCounter", valueToWrite);
                valueToRead = (uint)client.ReadValue("MAIN.nCounter", typeof(uint));
            }
            #endregion

            #region CODE_SAMPLE_SYMBOL

            using (AdsClient client = new AdsClient())
            {
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                IAdsSymbol symbol = adsClient.ReadSymbol("MAIN.nCounter");
                adsClient.WriteValue(symbol, valueToWrite);
                valueToRead = (uint)adsClient.ReadValue(symbol);
            }
            #endregion

            #region CODE_SAMPLE_SYMBOLBROWSER

            using (AdsClient client = new AdsClient())
            {
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Load all Symbols + DataTypes
                ISymbolLoader loader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                Symbol symbol = (Symbol)loader.Symbols["MAIN.nCounter"];

                // Works for ALL Primitive 'ANY TYPES' Symbols
                symbol.WriteValue(valueToWrite); 
                valueToRead = (uint)symbol.ReadValue();

                // Simple filtering of Symbols
                Regex filterExpression = new Regex(pattern: @"^MAIN.*"); // Everything that starts with "MAIN"

                // FilterFunction that filters for the InstancePath
                Func<ISymbol, bool> filter = s => filterExpression.IsMatch(s.InstancePath);
                SymbolIterator iterator = new SymbolIterator(symbols: loader.Symbols, recurse: true, predicate: filter);

                foreach (ISymbol filteredSymbol in iterator)
                {
                    Console.WriteLine(filteredSymbol.InstancePath);
                }
            }
            #endregion

            #region CODE_SAMPLE_DYNAMIC

            using (AdsClient client = new AdsClient())
            {
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Load all Symbols + DataTypes
                // Primitive Parts will be automatically resolved to .NET Primitive types.
                IDynamicSymbolLoader loader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                dynamic symbols = loader.SymbolsDynamic;
                dynamic main = symbols.Main;

                // Use typed object to use InfoTips
                DynamicSymbol nCounter = main.nCounter;

                // or to be fullDynamic 
                //dynamic nCounter = main.nCounter;

                // Works for ALL sorts of types (not restricted to ANY_TYPE basing primitive types)
                nCounter.WriteValue(valueToWrite);
                valueToRead = (uint)nCounter.ReadValue();
            }
            #endregion

            #region CODE_SAMPLE_DYNAMIC_COMPLEX

            using (AdsClient client = new AdsClient())
            {
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Load all Symbols + DataTypes
                // Primitive Parts will be automatically resolved to .NET Primitive types.
                IDynamicSymbolLoader loader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                dynamic symbols = loader.SymbolsDynamic;
                dynamic main = symbols.Main;

                // Use typed object to use InfoTips
                DynamicSymbol nCounter = main.nCounter; // UDINT

                // or to be fullDynamic 
                //dynamic nCounter = main.nCounter;

                // Works for ALL sorts of types (not restricted to ANY_TYPE basing primitive types)
                valueToRead = (uint)nCounter.ReadValue();
                // or
                var varValue = nCounter.ReadValue();
                // or
                dynamic dynValue = nCounter.ReadValue();

                // Same for writing
                nCounter.WriteValue(valueToWrite);

                // Or Notifications / Events
                nCounter.ValueChanged += new EventHandler<ValueChangedEventArgs>(NCounter_ValueChanged);

                //Reading complexTypes e.g. Struct

                DynamicSymbol myStructSymbol = main.plcStruct; // Dynamically created
                dynamic myStructVal = myStructSymbol.ReadValue(); // Takes an ADS Snapshot of the value

                dynamic int1Val = myStructVal.int1; // Value to an INT (short)
                dynamic valueNestedStruct = myStructVal.nestedStruct; //value to another complex type (here a nested Struct)

                myStructSymbol.ValueChanged += new EventHandler<ValueChangedEventArgs>(MyStructSymbol_ValueChanged);
                //wait for notifications for 5 seconds
                Thread.Sleep(5000);
            }
            #endregion
        }

        public async Task ReadIndexGroupIndexOffsetAsync()
        {
            #region CODE_SAMPLE_READWRITE_IGIO_ASYNC
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                UInt32 valueToRead = 0;
                UInt32 valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                byte[] writeData = new byte[sizeof(uint)];

                // Write an UINT32 Value
                MemoryStream writeStream = new MemoryStream(writeData);
                BinaryWriter writer = new BinaryWriter(writeStream);
                writer.Write(valueToWrite);
                ResultWrite resultWrite = await client.WriteAsync(0x4020, 0x0, writeData.AsMemory(),cancel);

                // Read an UINT32 Value
                byte[] readData = new byte[sizeof(uint)];
                ResultRead resultRead = await client.ReadAsync(0x4020, 0x0, readData.AsMemory(),cancel);

                MemoryStream readStream = new MemoryStream(readData);
                BinaryReader reader = new BinaryReader(readStream);
                valueToRead = reader.ReadUInt32();
            }
            #endregion

            #region CODE_SAMPLE_READWRITE_BYHANDLE_ASYNC

            CancellationToken cancelT = CancellationToken.None;
            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851);

                ushort valueToRead = 0; // System.UInt16
                ushort valueToWrite = 42; // System.UInt16

                // Create the Variable Handle
                ResultHandle resultHandle = await client.CreateVariableHandleAsync("MAIN.testVar", cancel); //Test Var is defined as PLC INT

                if (resultHandle.Succeeded)
                {
                    uint varHandle = 0;

                    try
                    {
                        // Write an UINT16 Value
                        byte[] writeData = new byte[sizeof(ushort)];

                        MemoryStream writeStream = new MemoryStream(writeData);
                        BinaryWriter writer = new BinaryWriter(writeStream);
                        writer.Write(valueToWrite); // Marshal the Value
                        ResultWrite resultWrite = await client.WriteAsync(varHandle, writeData.AsMemory(), cancelT);

                        bool succeeded = resultWrite.Succeeded;

                        // Read an UINT16 Value
                        byte[] readData = new byte[sizeof(ushort)];
                        ResultRead resultRead = await client.ReadAsync(varHandle, readData.AsMemory(), cancel);

                        if (resultRead.Succeeded)
                        {
                            MemoryStream readStream = new MemoryStream(readData);
                            BinaryReader reader = new BinaryReader(readStream);
                            valueToRead = reader.ReadUInt16(); // Unmarshal the Value
                        }
                    }
                    finally
                    {
                        // Unregister VarHandle after Use
                        ResultAds result = await client.DeleteVariableHandleAsync(varHandle, cancel);
                    }
                }
            }
            #endregion

        }

        public async Task ReadAnyAsync()
        {
            #region CODE_SAMPLE_READWRITE_ANY_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;

                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);
                ResultWrite resultWrite = await client.WriteAnyAsync(0x4020, 0x0, valueToWrite,cancel);
                bool succeeded = resultWrite.Succeeded;

                ResultValue<uint> resultRead = await client.ReadAnyAsync<uint>(0x4020, 0x0, cancel);

                if(resultRead.Succeeded)
                {
                    valueToRead = (uint)resultRead.Value;
                }
            }
            #endregion

            #region CODE_SAMPLE_BYHANDLE_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;
                uint varHandle = 0;
                client.Connect(AmsNetId.Local, 851);

                uint valueToRead = 0;
                uint valueToWrite = 42;

                ResultHandle resultHandle = await client.CreateVariableHandleAsync("MAIN.nCounter", cancel);
                varHandle = resultHandle.Handle;

                if (resultHandle.Succeeded)
                {
                    try
                    {
                        ResultWrite resultWrite = await client.WriteAnyAsync(varHandle, valueToWrite, cancel);
                        ResultValue<uint> resultRead = await client.ReadAnyAsync<uint>(varHandle, cancel);

                        if (resultRead.Succeeded)
                            valueToRead = resultRead.Value;
                    }
                    finally
                    {
                        // Unregister VarHandle after Use
                        ResultAds result = await client.DeleteVariableHandleAsync(varHandle,cancel);
                    }
                }
            }
            #endregion

            #region CODE_SAMPLE_BYPATH_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;
                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                ResultWrite resultWrite = await client.WriteValueAsync("MAIN.nCounter", valueToWrite, cancel);
                ResultValue<uint> resultRead = await client.ReadValueAsync<uint>("MAIN.nCounter",cancel);
                
                if (resultRead.Succeeded)
                    valueToRead = resultRead.Value;
            }
            #endregion

            #region CODE_SAMPLE_SYMBOL_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;

                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                ResultValue<IAdsSymbol> resultSymbol = await client.ReadSymbolAsync("MAIN.nCounter",cancel);

                if (resultSymbol.Succeeded)
                {
                    ResultWrite resultWrite = await client.WriteValueAsync(resultSymbol.Value, valueToWrite, cancel);
                    bool succeeded = resultWrite.Succeeded;

                    ResultValue<uint> resultValue = await client.ReadValueAsync<uint>(resultSymbol.Value,cancel);

                    if (resultValue.Succeeded)
                        valueToRead = resultValue.Value;
                }
            }
            #endregion

            #region CODE_SAMPLE_SYMBOLBROWSER_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;

                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Load all Symbols + DataTypes
                ISymbolLoader loader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);

                ResultSymbols resultSymbols  = await loader.GetSymbolsAsync(cancel);

                if (resultSymbols.Succeeded)
                {
                    Symbol symbol = (Symbol)resultSymbols.Symbols["MAIN.nCounter"];

                    // Works for ALL Primitive 'ANY TYPES' Symbols
                    ResultWriteAccess resultWrite = await symbol.WriteValueAsync(valueToWrite, cancel);
                    ResultReadValueAccess resultRead = await symbol.ReadValueAsync(cancel);

                    if (resultRead.Succeeded)
                        valueToRead = (uint)resultRead.Value;

                    // Simple filtering of Symbols
                    Regex filterExpression = new Regex(pattern: @"^MAIN.*"); // Everything that starts with "MAIN"

                    // FilterFunction that filters for the InstancePath
                    Func<ISymbol, bool> filter = s => filterExpression.IsMatch(s.InstancePath);
                    SymbolIterator iterator = new SymbolIterator(symbols: resultSymbols.Symbols, recurse: true, predicate: filter);

                    foreach (ISymbol filteredSymbol in iterator)
                    {
                        Console.WriteLine(filteredSymbol.InstancePath);
                    }
                }
            }
            #endregion

            #region CODE_SAMPLE_DYNAMIC_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;

                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Load all Symbols + DataTypes
                // Primitive Parts will be automatically resolved to .NET Primitive types.
                IDynamicSymbolLoader loader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                //dynamic symbols = loader.SymbolsDynamic;
                ResultDynamicSymbols resultSymbols = await loader.GetDynamicSymbolsAsync(cancel);
                if (resultSymbols.Succeeded)
                {
                    dynamic symbols = resultSymbols.Symbols; // Symbols collection with 'dynamic' access.
                    dynamic main = symbols.Main; // Get the 'dynamic' main FB as Property

                    // Use typed object to use InfoTips instead of 'dynamic'
                    DynamicSymbol nCounter = main.nCounter;
                    // or to be fullDynamic 
                    dynamic nCounter2 = main.nCounter;

                    // Works for ALL sorts of types (not restricted to ANY_TYPE basing primitive types)
                    ResultWriteAccess resultWrite = await nCounter.WriteValueAsync(valueToWrite,cancel);
                    ResultReadValueAccess resultRead = await nCounter.ReadValueAsync(cancel);

                    if (resultRead.Succeeded)
                    {
                        // Because the PLC value is defined as UDINT (32-Bit)
                        // We get back an already Marshalled UInt32 here ...
                        valueToRead = (uint)resultRead.Value;
                    }
                }
            }
            #endregion

            #region CODE_SAMPLE_DYNAMIC_COMPLEX_ASYNC

            using (AdsClient client = new AdsClient())
            {
                CancellationToken cancel = CancellationToken.None;

                uint valueToRead = 0;
                uint valueToWrite = 42;

                client.Connect(AmsNetId.Local, 851);

                // Load all Symbols + DataTypes
                // Primitive Parts will be automatically resolved to .NET Primitive types.
                IDynamicSymbolLoader loader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                //dynamic symbols = loader.SymbolsDynamic;
                ResultDynamicSymbols resultSymbols = await loader.GetDynamicSymbolsAsync(cancel);

                if (resultSymbols.Succeeded)
                {
                    dynamic symbols = resultSymbols.Symbols;
                    dynamic main = symbols.Main;

                    // Use typed object to use InfoTips
                    DynamicSymbol nCounter = main.nCounter; // UDINT

                    // or to be fullDynamic  
                    dynamic nCounter2 = main.nCounter;

                    // Works for ALL sorts of types (not restricted to ANY_TYPE basing primitive types)
                    ResultReadValueAccess resultRead = await nCounter.ReadValueAsync(cancel);

                    if (resultRead.Succeeded)
                    {
                        valueToRead = (uint)resultRead.Value;

                        // or
                        var varValue = resultRead.Value;
                        // or
                        dynamic dynValue = resultRead.Value;
                    }
                    // Same for writing
                    ResultWriteAccess resultWrite = await nCounter.WriteValueAsync(valueToWrite,cancel);

                    // Or Notifications / Events (typed dynamically)
                    nCounter.ValueChanged += NCounter_ValueChanged;

                    //Reading complexTypes e.g. Struct

                    DynamicSymbol myStructSymbol = main.plcStruct; // Dynamically created
                    ResultReadValueAccess resultRead2 = await myStructSymbol.ReadValueAsync(cancel); // Takes an ADS Snapshot of the value

                    if (resultRead2.Succeeded)
                    {
                        dynamic myStructVal = resultRead2.Value;

                        dynamic int1Val = myStructVal.int1; // Value to an INT (short)
                        dynamic valueNestedStruct = myStructVal.nestedStruct; //value to another complex type (here a nested Struct)
                    }
                    myStructSymbol.ValueChanged += MyStructSymbol_ValueChanged;
                    //wait 5 seconds to get some events 
                    Thread.Sleep(5000);
                }
            }
            #endregion
        }

        #region CODE_SAMPLE_DYNAMIC_COMPLEX2
        private void NCounter_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var uintVal = e.Value;
        }

        private void MyStructSymbol_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            dynamic structValue = e.Value; // Snapshot of the whole Struct and all its contents
        }
        #endregion
    }
}
