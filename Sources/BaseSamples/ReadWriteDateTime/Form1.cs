using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using TwinCAT.Ads;
using TwinCAT.PlcOpen;
using TwinCAT.TypeSystem;
using System.Threading;
using System.Threading.Tasks;

namespace S11_ReadWriteDateTime
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.Button btnRead;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox2;
		private AdsClient adsClient;
		private uint[] varHandles;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

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

        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.btnWrite = new System.Windows.Forms.Button();
			this.btnRead = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 21);
			this.label1.TabIndex = 7;
			this.label1.Text = "MAIN.Time1:";
			// 
			// btnWrite
			// 
			this.btnWrite.Location = new System.Drawing.Point(240, 48);
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.Size = new System.Drawing.Size(72, 24);
			this.btnWrite.TabIndex = 6;
			this.btnWrite.Text = "Write";
			this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
			// 
			// btnRead
			// 
			this.btnRead.Location = new System.Drawing.Point(240, 16);
			this.btnRead.Name = "btnRead";
			this.btnRead.Size = new System.Drawing.Size(72, 24);
			this.btnRead.TabIndex = 5;
			this.btnRead.Text = "Read";
			this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(80, 16);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(144, 20);
			this.textBox1.TabIndex = 4;
			this.textBox1.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 21);
			this.label2.TabIndex = 9;
			this.label2.Text = "MAIN.Date1:";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(80, 48);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(144, 20);
			this.textBox2.TabIndex = 8;
			this.textBox2.Text = "";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(320, 85);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnWrite);
			this.Controls.Add(this.btnRead);
			this.Controls.Add(this.textBox1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
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
				varHandles = new uint[2];
				varHandles[0] = adsClient.CreateVariableHandle("MAIN.Time1");
				varHandles[1] = adsClient.CreateVariableHandle("MAIN.Date1");
			}
			catch( Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				adsClient.Dispose();
			}
			catch( Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void btnRead_Click(object sender, System.EventArgs e)
		{
			try
			{
                byte[] buffer = new byte[4];
				//AdsStream adsStream = new AdsStream(4);
				adsClient.Read(varHandles[0], buffer.AsMemory());

                TIME plcTime = null;
                PrimitiveTypeMarshaler.Default.Unmarshal(buffer.AsSpan(), out plcTime);
				textBox1.Text = plcTime.ToString();
				
				adsClient.Read(varHandles[1], buffer.AsMemory());
                DATE plcDate = null;
                PrimitiveTypeMarshaler.Default.Unmarshal(buffer.AsSpan(), out plcDate);
                textBox2.Text = plcDate.ToString();
            }
            catch (Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}

		private void btnWrite_Click(object sender, System.EventArgs e)
		{
			try
			{
                byte[] buffer = new byte[4];
                TimeSpan span = TimeSpan.Parse(textBox1.Text);
                TIME plcTime = new TIME(span);
                PrimitiveTypeMarshaler.Default.Marshal(plcTime, buffer.AsSpan());

				
                adsClient.Write(varHandles[0], buffer.AsMemory());

                DateTime dateTime = DateTime.Parse(textBox2.Text);
                DATE plcDate = new DATE(dateTime);
                PrimitiveTypeMarshaler.Default.Marshal(plcDate, buffer.AsSpan());

				adsClient.Write(varHandles[1], buffer.AsMemory());
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}


        private void ReadWritePlcOpenTypes()
        {
            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local plc

                uint handleTime = 0;
                uint handleLTime = 0;
                uint handleDate = 0;

                try
                {
                    handleTime = client.CreateVariableHandle("MAIN.time"); // TIME
                    handleLTime = client.CreateVariableHandle("MAIN.lTime"); // LTIME
                    handleDate = client.CreateVariableHandle("MAIN.date"); // DATE

                    byte[] readBuffer = new byte[LTIME.MarshalSize]; // Largest PlcOpen Type is 8 Bytes
                    byte[] writeBuffer = new byte[LTIME.MarshalSize];

                    // Reading raw value TIME
                    client.Read(handleTime, readBuffer.AsMemory(0,TIME.MarshalSize));

                    // Unmarshalling
                    TIME plcTime = null;
                    PrimitiveTypeMarshaler.Default.Unmarshal(readBuffer.AsSpan(0, TIME.MarshalSize), out plcTime);
                    TimeSpan time = plcTime.Time;

                    // Writing raw value TIME
                    PrimitiveTypeMarshaler.Default.Marshal(time, writeBuffer.AsSpan());
                    client.Write(handleTime, writeBuffer.AsMemory(0,TIME.MarshalSize));

                    // Reading raw value LTIME
                    client.Read(handleLTime, readBuffer.AsMemory(0, LTIME.MarshalSize));

                    // Unmarshalling
                    LTIME plcLTime = null;
                    PrimitiveTypeMarshaler.Default.Unmarshal(readBuffer.AsSpan(0, LTIME.MarshalSize), out plcLTime);
                    TimeSpan lTime = plcLTime.Time;

                    // Writing raw value LTIME
                    PrimitiveTypeMarshaler.Default.Marshal(lTime, writeBuffer.AsSpan());
                    client.Write(handleLTime, writeBuffer.AsMemory(0, LTIME.MarshalSize));

                    // Reading raw value DATE
                    DATE plcDate = null;
                    client.Read(handleDate, readBuffer.AsMemory(0, DATE.MarshalSize));

                    // Unmarshalling
                    PrimitiveTypeMarshaler.Default.Unmarshal(readBuffer.AsSpan(0, DATE.MarshalSize), out plcDate);
                    DateTimeOffset dateTime = plcDate.Date;

                    // Writeing raw value DATE
                    PrimitiveTypeMarshaler.Default.Marshal(plcDate, writeBuffer.AsSpan());
                    client.Write(handleDate, writeBuffer.AsMemory(0, DATE.MarshalSize));
                }
                finally
                {
                    client.DeleteVariableHandle(handleLTime);
                    client.DeleteVariableHandle(handleTime);
                    client.DeleteVariableHandle(handleDate);
                }
            }
        }

        private async Task ReadWritePlcOpenTypesAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local plc

                ResultHandle resultHandleTime = await client.CreateVariableHandleAsync("MAIN.time", cancel); // TIME
                ResultHandle resultHandleLTime = await client.CreateVariableHandleAsync("MAIN.lTime", cancel); // LTIME
                ResultHandle resultHandleDate = await client.CreateVariableHandleAsync("MAIN.date", cancel); // DATE

                if (resultHandleTime.Succeeded && resultHandleLTime.Succeeded && resultHandleDate.Succeeded)
                {
                    try
                    {
                        byte[] readBuffer = new byte[LTIME.MarshalSize]; // Largest PlcOpen Type is 8 Bytes
                        byte[] writeBuffer = new byte[LTIME.MarshalSize];

                        // Reading raw value TIME
                        await client.ReadAsync(resultHandleTime.Handle, readBuffer.AsMemory(0, TIME.MarshalSize), cancel);

                        // Unmarshalling
                        TIME plcTime = null;
                        PrimitiveTypeMarshaler.Default.Unmarshal(readBuffer.AsSpan(0, TIME.MarshalSize), out plcTime);
                        TimeSpan time = plcTime.Time;

                        // Writing raw value TIME
                        PrimitiveTypeMarshaler.Default.Marshal(time, writeBuffer.AsSpan());
                        await client.WriteAsync(resultHandleTime.Handle, writeBuffer.AsMemory(0, TIME.MarshalSize), cancel);

                        // Reading raw value LTIME
                        await client.ReadAsync(resultHandleLTime.Handle, readBuffer.AsMemory(0, LTIME.MarshalSize), cancel);

                        // Unmarshalling
                        LTIME plcLTime = null;
                        PrimitiveTypeMarshaler.Default.Unmarshal(readBuffer.AsSpan(0, LTIME.MarshalSize), out plcLTime);
                        TimeSpan lTime = plcLTime.Time;

                        // Writing raw value LTIME
                        PrimitiveTypeMarshaler.Default.Marshal(lTime, writeBuffer.AsSpan());
                        await client.WriteAsync(resultHandleLTime.Handle, writeBuffer.AsMemory(0, LTIME.MarshalSize), cancel);

                        // Reading raw value DATE
                        DATE plcDate = null;
                        await client.ReadAsync(resultHandleDate.Handle, readBuffer.AsMemory(0, DATE.MarshalSize), cancel);

                        // Unmarshalling
                        PrimitiveTypeMarshaler.Default.Unmarshal(readBuffer.AsSpan(0, DATE.MarshalSize), out plcDate);
                        DateTimeOffset dateTime = plcDate.Date;

                        // Writeing raw value DATE
                        PrimitiveTypeMarshaler.Default.Marshal(plcDate, writeBuffer.AsSpan());
                        await client.WriteAsync(resultHandleDate.Handle, writeBuffer.AsMemory(0, DATE.MarshalSize), cancel);
                    }
                    finally
                    {
                        ResultAds r1 = await client.DeleteVariableHandleAsync(resultHandleLTime.Handle, cancel);
                        ResultAds r2 = await client.DeleteVariableHandleAsync(resultHandleTime.Handle, cancel);
                        ResultAds r3 = await client.DeleteVariableHandleAsync(resultHandleDate.Handle, cancel);
                    }
                }
            }
        }

        private void ReadWritePlcOpenTypesAny()
        {
            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local plc

                uint handleTime = 0;
                uint handleDate = 0;
                uint handleLTime = 0;

                try
                {
                    handleTime = client.CreateVariableHandle("MAIN.time"); // TIME
                    handleDate = client.CreateVariableHandle("MAIN.date"); // DATE
                    handleLTime = client.CreateVariableHandle("MAIN.lTime"); // LTIME

                    TIME time = (TIME)client.ReadAny(handleTime, typeof(TIME)); // TIME
                    TimeSpan timeSpan = time.Time;
                    client.WriteAny(handleTime, time);

                    DATE date = (DATE)client.ReadAny(handleDate, typeof(DATE)); // DATE
                    DateTimeOffset dateTime = date.Date;
                    client.WriteAny(handleDate, date);

                    LTIME ltime = (LTIME)client.ReadAny(handleLTime, typeof(LTIME)); // LTIME
                    TimeSpan lTimeSpan = ltime.Time;
                    client.WriteAny(handleLTime, ltime);
                }
                finally
                {
                    client.DeleteVariableHandle(handleTime);
                    client.DeleteVariableHandle(handleDate);
                    client.DeleteVariableHandle(handleLTime);
                }
            }
        }

        private async Task ReadWritePlcOpenTypesAnyAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                client.Connect(AmsNetId.Local, 851); // Connect to local plc

                ResultHandle resultHandleTime = await client.CreateVariableHandleAsync("MAIN.time",cancel); // TIME
                ResultHandle resultHandleDate = await client.CreateVariableHandleAsync("MAIN.date", cancel); // DATE
                ResultHandle resultHandleLTime = await client.CreateVariableHandleAsync("MAIN.lTime", cancel); // LTIME

                if (resultHandleTime.Succeeded && resultHandleDate.Succeeded && resultHandleLTime.Succeeded)
                {
                    try
                    {
                        ResultAnyValue resultTime = await client.ReadAnyAsync(resultHandleTime.Handle, typeof(TIME), cancel); // TIME

                        TIME time = (TIME)resultTime.Value;
                        TimeSpan timeSpan = time.Time;

                        await client.WriteAnyAsync(resultHandleTime.Handle, time, cancel);

                        ResultAnyValue resultData = await client.ReadAnyAsync(resultHandleDate.Handle, typeof(DATE), cancel); // DATE
                        DATE date = (DATE)resultTime.Value;
                        DateTimeOffset dateTime = date.Date;

                        await client.WriteAnyAsync(resultHandleDate.Handle, date, cancel);

                        ResultAnyValue resultLTime = await client.ReadAnyAsync(resultHandleLTime.Handle, typeof(LTIME), cancel); // LTIME
                        LTIME lTime = (LTIME)resultTime.Value;
                        TimeSpan lTimeSpan = lTime.Time;

                        await client.WriteAnyAsync(resultHandleLTime.Handle, lTime, cancel);
                    }
                    finally
                    {
                        ResultAds r1 = await client.DeleteVariableHandleAsync(resultHandleTime.Handle,cancel);
                        ResultAds r2 = await client.DeleteVariableHandleAsync(resultHandleDate.Handle,cancel);
                        ResultAds r3 = await client.DeleteVariableHandleAsync(resultHandleLTime.Handle,cancel);
                    }
                }
            }
        }
    }
}
