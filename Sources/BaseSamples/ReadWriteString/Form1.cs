using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT.Ads;
using System.Text;
using TwinCAT.TypeSystem;
using System.Threading;
using System.Threading.Tasks;

namespace S10_ReadWriteString
{
	public class Form1 : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button btnRead;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.Label label1;
		private AdsClient adsClient;
		private uint varHandle;

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

        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnRead = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(86, 18);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(183, 22);
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
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(288, 55);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(86, 28);
            this.btnWrite.TabIndex = 2;
            this.btnWrite.Text = "Write";
            this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "MAIN.text:";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(377, 89);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnWrite);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Sample10";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

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
				varHandle = adsClient.CreateVariableHandle("MAIN.text");
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

		private void btnRead_Click(object sender, System.EventArgs e)
		{
			try
			{
                byte[] buffer = new byte[30];
				adsClient.Read(varHandle, buffer.AsMemory());
                PrimitiveTypeMarshaler converter = PrimitiveTypeMarshaler.Default;

                string value = null;
                converter.Unmarshal(buffer.AsSpan(), out value);
                textBox1.Text = value;
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
                byte[] buffer = new byte[30];
                PrimitiveTypeMarshaler.Default.Marshal(textBox1.Text, buffer);
				adsClient.Write(varHandle, buffer);
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

        private void CodeSampleReadString()
        {
            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local port 851 (PLC)
                uint handle = client.CreateVariableHandle("MAIN.string"); // Symbol "string" in MAIN defined as string

                try
                {
                    // Read ANSI String string[80]
                    int byteSize = 81; // Size of 80 ANSI chars + /0 (STRING[80])
                    PrimitiveTypeMarshaler converter = new PrimitiveTypeMarshaler(StringMarshaler.DefaultEncoding);
                    byte[] buffer = new byte[byteSize];

                    int readBytes = client.Read(handle, buffer.AsMemory());

                    string value = null;
                    converter.Unmarshal<string>(buffer.AsSpan(), out value);

                    // Write ANSI String string[80]
                    byte[] writeBuffer = new byte[byteSize];
                    value = "Changed";
                    converter.Marshal(value, writeBuffer);
                    client.Write(handle, writeBuffer);
                }
                finally
                {
                    client.DeleteVariableHandle(handle);
                }
            }
        }

        private async Task CodeSampleReadStringAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local port 851 (PLC)

                ResultHandle resultHandle = await client.CreateVariableHandleAsync("MAIN.string",cancel); // Symbol "string" in MAIN defined as string

                if (resultHandle.Succeeded)
                {
                    try
                    {
                        // Read ANSI String string[80]
                        int byteSize = 81; // Size of 80 ANSI chars + /0 (STRING[80])
                        PrimitiveTypeMarshaler converter = new PrimitiveTypeMarshaler(StringMarshaler.DefaultEncoding);
                        byte[] buffer = new byte[byteSize];

                        ResultRead resultRead = await client.ReadAsync(resultHandle.Handle, buffer.AsMemory(), cancel);

                        if (resultRead.Succeeded)
                        {
                            string value = null;
                            converter.Unmarshal<string>(buffer.AsSpan(), out value);

                            byte[] writeBuffer = new byte[byteSize];
                            // Write ANSI String string[80]
                            value = "Changed";
                            converter.Marshal(value, writeBuffer);
                            ResultWrite resultWrite = await client.WriteAsync(resultHandle.Handle, writeBuffer, cancel);
                        }
                    }
                    finally
                    {
                        ResultAds r1 = await client.DeleteVariableHandleAsync(resultHandle.Handle, cancel);
                    }
                }
            }
        }

        private void CodeSampleReadWString()
        {
            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local port 851 (PLC)
                uint handle = client.CreateVariableHandle("MAIN.wstring"); // Symbol "wstring" defined in MAIN as WSTRING

                try
                {
                    // Read UNICODE String wstring[80]

                    PrimitiveTypeMarshaler converter = new PrimitiveTypeMarshaler(Encoding.Unicode);
                    int byteSize = converter.MarshalSize(80); // Size of 80 UNICODE chars + /0 (WSTRING[80]) (162)
                    byte[] readBuffer = new byte[byteSize];

                    int readBytes = client.Read(handle, readBuffer);

                    string value = null;
                    converter.Unmarshal(readBuffer.AsSpan(), out value);

                    // Write Unicode String string[80]
                    value = "Changed";
                    byte[] writeBuffer = new byte[byteSize];
                    converter.Marshal(value, writeBuffer.AsSpan());

                    client.Write(handle, writeBuffer.AsMemory());
                }
                finally
                {
                    client.DeleteVariableHandle(handle);
                }
            }
        }

        private async Task CodeSampleReadWStringAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local port 851 (PLC)
                ResultHandle resultHandle = await client.CreateVariableHandleAsync("MAIN.wstring",cancel); // Symbol "wstring" defined in MAIN as WSTRING

                if (resultHandle.Succeeded)
                {
                    try
                    {
                        // Read UNICODE String wstring[80]
                        PrimitiveTypeMarshaler converter = new PrimitiveTypeMarshaler(Encoding.Unicode);
                        int byteSize = converter.MarshalSize(80); // Size of 80 UNICODE chars + /0 (WSTRING[80]) (162)
                        byte[] readBuffer = new byte[byteSize];

                        ResultRead resultRead = await client.ReadAsync(resultHandle.Handle, readBuffer, cancel);

                        if (resultRead.Succeeded)
                        {
                            string value = null;
                            converter.Unmarshal(readBuffer.AsSpan(), out value);

                            // Write Unicode String string[80]
                            value = "Changed";
                            byte[] writeBuffer = new byte[byteSize];
                            converter.Marshal(value, writeBuffer.AsSpan());

                            ResultWrite resultWrite = await client.WriteAsync(resultHandle.Handle, writeBuffer.AsMemory(), cancel);
                        }
                    }
                    finally
                    {
                        ResultAds r1 = await client.DeleteVariableHandleAsync(resultHandle.Handle, cancel);
                    }
                }
            }
        }

        private void CodeSampleStringAny()
        {
            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local port 851 (PLC)

                uint stringHandle = client.CreateVariableHandle("MAIN.string"); // Symbol "string" defined in MAIN as STRING
                uint wStringHandle = client.CreateVariableHandle("MAIN.wstring"); // Symbol "string" defined in MAIN as WSTRING

                try
                {
                    string str = client.ReadAnyString(stringHandle, 80, StringMarshaler.DefaultEncoding);
                    string wStr = client.ReadAnyString(wStringHandle, 80, Encoding.Unicode);

                    string changedValue = "Changed";

                    // Attention, take care that the memory of the string in the process image is not exceeded!
                    client.WriteAnyString(stringHandle, changedValue, 80, StringMarshaler.DefaultEncoding);
                    client.WriteAnyString(wStringHandle, changedValue, 80, Encoding.Unicode);
                }
                finally
                {
                    client.DeleteVariableHandle(stringHandle);
                    client.DeleteVariableHandle(wStringHandle);
                }
            }
        }

        private async Task CodeSampleStringAnyAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local port 851 (PLC)

                ResultHandle resultHandleStr = await client.CreateVariableHandleAsync("MAIN.string",cancel); // Symbol "string" defined in MAIN as STRING
                ResultHandle resultHandleWStr = await client.CreateVariableHandleAsync("MAIN.wstring",cancel); // Symbol "string" defined in MAIN as WSTRING

                if (resultHandleStr.Succeeded && resultHandleWStr.Succeeded)
                {
                    try
                    {
                        ResultAnyValue resultReadStr = await client.ReadAnyStringAsync(resultHandleStr.Handle, 80, StringMarshaler.DefaultEncoding, cancel);
                        ResultAnyValue resultReadWStr = await client.ReadAnyStringAsync(resultHandleWStr.Handle, 80, Encoding.Unicode, cancel);

                        string changedValue = "Changed";

                        // Attention, take care that the memory of the string in the process image is not exceeded!
                        ResultWrite resultWriteStr = await client.WriteAnyStringAsync(resultHandleStr.Handle, changedValue, 80, StringMarshaler.DefaultEncoding, cancel);
                        ResultWrite resultWriteWStr = await client.WriteAnyStringAsync(resultHandleWStr.Handle, changedValue, 80, Encoding.Unicode, cancel);
                    }
                    finally
                    {
                        ResultAds r1 = await client.DeleteVariableHandleAsync(resultHandleStr.Handle, cancel);
                        ResultAds r2 = await client.DeleteVariableHandleAsync(resultHandleWStr.Handle, cancel);
                    }
                }
            }
        }
    }
}
