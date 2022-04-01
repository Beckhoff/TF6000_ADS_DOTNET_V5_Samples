using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT.Ads;
using System.Threading;
using TwinCAT.TypeSystem;
using System.Text;
using System.Buffers.Binary;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using TwinCAT;
using System.Collections.Generic;

namespace S03_EventReading
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbInt;
        private System.Windows.Forms.TextBox tbDint;
        private System.Windows.Forms.TextBox tbSint;
        private System.Windows.Forms.TextBox tbLreal;
        private System.Windows.Forms.TextBox tbReal;
        private System.Windows.Forms.TextBox tbString;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private AdsClient _client;
        private uint[] hConnect;
        //private AdsStream dataStream;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbBool;
        //private AdsBinaryReader binRead;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbInt = new System.Windows.Forms.TextBox();
            this.tbDint = new System.Windows.Forms.TextBox();
            this.tbSint = new System.Windows.Forms.TextBox();
            this.tbLreal = new System.Windows.Forms.TextBox();
            this.tbReal = new System.Windows.Forms.TextBox();
            this.tbString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tbBool = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbInt
            // 
            this.tbInt.Location = new System.Drawing.Point(104, 48);
            this.tbInt.Name = "tbInt";
            this.tbInt.Size = new System.Drawing.Size(306, 20);
            this.tbInt.TabIndex = 0;
            // 
            // tbDint
            // 
            this.tbDint.Location = new System.Drawing.Point(104, 80);
            this.tbDint.Name = "tbDint";
            this.tbDint.Size = new System.Drawing.Size(306, 20);
            this.tbDint.TabIndex = 1;
            // 
            // tbSint
            // 
            this.tbSint.Location = new System.Drawing.Point(104, 112);
            this.tbSint.Name = "tbSint";
            this.tbSint.Size = new System.Drawing.Size(306, 20);
            this.tbSint.TabIndex = 2;
            // 
            // tbLreal
            // 
            this.tbLreal.Location = new System.Drawing.Point(104, 144);
            this.tbLreal.Name = "tbLreal";
            this.tbLreal.Size = new System.Drawing.Size(306, 20);
            this.tbLreal.TabIndex = 3;
            // 
            // tbReal
            // 
            this.tbReal.Location = new System.Drawing.Point(104, 176);
            this.tbReal.Name = "tbReal";
            this.tbReal.Size = new System.Drawing.Size(306, 20);
            this.tbReal.TabIndex = 4;
            // 
            // tbString
            // 
            this.tbString.Location = new System.Drawing.Point(104, 208);
            this.tbString.Name = "tbString";
            this.tbString.Size = new System.Drawing.Size(306, 20);
            this.tbString.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "MAIN.intVal :";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 23);
            this.label2.TabIndex = 7;
            this.label2.Text = "MAIN.dintVal :";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 23);
            this.label3.TabIndex = 8;
            this.label3.Text = "MAIN.sintVal :";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 23);
            this.label4.TabIndex = 9;
            this.label4.Text = "MAIN.lrealVal :";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 23);
            this.label5.TabIndex = 10;
            this.label5.Text = "MAIN.realVal :";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(8, 208);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 23);
            this.label6.TabIndex = 11;
            this.label6.Text = "MAIN.stringVal :";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 144);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 23);
            this.label7.TabIndex = 9;
            this.label7.Text = "label4";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(8, 112);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 23);
            this.label8.TabIndex = 8;
            this.label8.Text = "label3";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(8, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 23);
            this.label9.TabIndex = 6;
            this.label9.Text = "label1";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(8, 80);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 23);
            this.label10.TabIndex = 7;
            this.label10.Text = "label2";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(8, 16);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(88, 23);
            this.label11.TabIndex = 13;
            this.label11.Text = "MAIN.boolVal :";
            // 
            // tbBool
            // 
            this.tbBool.Location = new System.Drawing.Point(104, 16);
            this.tbBool.Name = "tbBool";
            this.tbBool.Size = new System.Drawing.Size(306, 20);
            this.tbBool.TabIndex = 12;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(422, 275);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbBool);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbString);
            this.Controls.Add(this.tbReal);
            this.Controls.Add(this.tbLreal);
            this.Controls.Add(this.tbSint);
            this.Controls.Add(this.tbDint);
            this.Controls.Add(this.tbInt);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }

        private void RegisterNotifications()
        {
            using (AdsClient client = new AdsClient())
            {
                // Add the Notification event handler
                client. AdsNotification += Client_AdsNotification;

                // Connect to target
                client.Connect(AmsNetId.Local, 851);
                uint notificationHandle = 0;

                try
                {
                    // Notification to a DINT Type (UINT32)
                    // Check for change every 200 ms

                    int size = sizeof(UInt32);
                    //byte[] notificationBuffer = new byte[sizeof(UInt32)];
                    
                    notificationHandle = client.AddDeviceNotification("MAIN.nCounter", size, new NotificationSettings(AdsTransMode.OnChange, 200, 0), null);
                    Thread.Sleep(5000); // Sleep the main thread to get some (asynchronous Notifications)
                }
                finally
                {
                    // Unregister the Event / Handle
                    client.DeleteDeviceNotification(notificationHandle);
                    client.AdsNotification -= Client_AdsNotification;
                }
            }
        }

        private void Client_AdsNotification(object sender, AdsNotificationEventArgs e)
        {
            // Or here we know about UDINT type --> can be marshalled as UINT32
            uint nCounter = BinaryPrimitives.ReadUInt32LittleEndian(e.Data.Span);

            // If Synchronization is needed (e.g. in Windows.Forms or WPF applications)
            // we could synchronize via SynchronizationContext into the UI Thread

            /*SynchronizationContext syncContext = SynchronizationContext.Current;
              _context.Post(status => someLabel.Text = nCounter.ToString(), null); // Non-blocking post */
        }

        private async Task RegisterNotificationsAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                // Add the Notification event handler
                client.AdsNotification += Client_AdsNotification2;

                // Connect to target
                client.Connect(AmsNetId.Local, 851);
                uint notificationHandle = 0;

                // Notification to a DINT Type (UINT32)
                // Check for change every 200 ms

                int size = sizeof(UInt32);

                ResultHandle result = await client.AddDeviceNotificationAsync("MAIN.nCounter", size, new NotificationSettings(AdsTransMode.OnChange, 200, 0), null, cancel);

                if (result.Succeeded)
                {
                    notificationHandle = result.Handle;
                    await Task.Delay(5000); // Wait asynchronously without blocking the UI Thread.
                                            // Unregister the Event / Handle
                    ResultAds result2 = await client.DeleteDeviceNotificationAsync(notificationHandle, cancel);
                }
                client.AdsNotification -= Client_AdsNotification2;
            }        
        }

        private void Client_AdsNotification2(object sender, AdsNotificationEventArgs e)
        {
            // Or here we know about UDINT type --> can be marshalled as UINT32
            uint nCounter = BinaryPrimitives.ReadUInt32LittleEndian(e.Data.Span);

            // If Synchronization is needed (e.g. in Windows.Forms or WPF applications)
            // we could synchronize via SynchronizationContext into the UI Thread

            /*SynchronizationContext syncContext = SynchronizationContext.Current;
              _context.Post(status => someLabel.Text = nCounter.ToString(), null); // Non-blocking post */
        }

        private async Task RegisterSumNotificationsAsync()
        {
            CancellationToken cancel = CancellationToken.None;

            using (AdsClient client = new AdsClient())
            {
                // Add the Notification event handler
                client.AdsSumNotification += Client_SumNotification;

                // Connect to target
                client.Connect(AmsNetId.Local, 851);
                uint notificationHandle = 0;

                // Notification to a DINT Type (UINT32)
                // Check for change every 200 ms

                ResultHandle result = await client.AddDeviceNotificationAsync("MAIN.nCounter", sizeof(UInt32), new NotificationSettings(AdsTransMode.OnChange, 200, 0), null, cancel);

                if (result.Succeeded)
                {
                    notificationHandle = result.Handle;
                    await Task.Delay(5000); // Wait asynchronously without blocking the UI Thread.
                                            // Unregister the Event / Handle
                    ResultAds result2 = await client.DeleteDeviceNotificationAsync(notificationHandle, cancel);
                }
                client.AdsNotification -= Client_AdsNotification2;
            }
        }

        private void Client_SumNotification(object sender, AdsSumNotificationEventArgs e)
        {
            // Timestamp of the Notification List
            DateTimeOffset dateTime = e.TimeStamp;

            // List of Raw ADS Notifications
            IList<Notification> notifications = e.Notifications;

            foreach(Notification notification in notifications)
            {
                // Notifications can be handled more efficiently, because they occur togeterh
                // handler and can be transformed/synchronized in one step compared to AdsClient.AdsNotifcation events.
            }
        }


        private void SymbolValueChanged()
        {
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(AmsNetId.Local, 851);
                Symbol symbol = null;

                try
                {
                    ISymbolLoader loader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                    // DINT Type (UINT32)
                    symbol = (Symbol)loader.Symbols["MAIN.nCounter"];

                    // Set the Notification Settings of the Symbol if NotificationSettings.Default is not appropriate
                    // Check for change every 500 ms
                    symbol.NotificationSettings = new NotificationSettings(AdsTransMode.OnChange, 500, 0);

                    symbol.ValueChanged += Symbol_ValueChanged; // Registers the notification
                    Thread.Sleep(5000); // Sleep the main thread to get some (asynchronous Notifications)
                }
                finally
                {
                    // Unregister the Event and the underlying Handle
                    symbol.ValueChanged -= Symbol_ValueChanged; // Unregisters the notification
                }
            }
        }

        private void Symbol_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Symbol symbol = (Symbol)e.Symbol;

            // Object Value can be cast to int automatically, because it is an Primitive Value (DINT --> Int32).
            // The Symbol information is used internally to cast the value to its appropriate .NET Type.
            int iVal = (int)e.Value;

            // If Synchronization is needed (e.g. in Windows.Forms or WPF applications)
            // we could synchronize via SynchronizationContext into the UI Thread
            
            /*SynchronizationContext syncContext = SynchronizationContext.Current;
              _context.Post(status => someLabel.Text = iVal.ToString(), null); // Non-blocking post */
        }

        SynchronizationContext _context = null;

        private void Form1_Load(object sender, System.EventArgs e)
        {
            // Get the WindowsFormSynchronizationContext.
            _context = SynchronizationContext.Current;

            // Create AdsClient instance
            _client = new AdsClient(); // Don't forget to dispose when finish using

            // Connection to Port 851 on the local system
            _client.Connect(851);
            hConnect = new uint[7];

            try
            {
                hConnect[0] = _client.AddDeviceNotification("MAIN.boolVal", 1,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbBool);
                hConnect[1] = _client.AddDeviceNotification("MAIN.intVal", 2,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbInt);
                hConnect[2] = _client.AddDeviceNotification("MAIN.dintVal", 4,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbDint);
                hConnect[3] = _client.AddDeviceNotification("MAIN.sintVal", 1,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbSint);
                hConnect[4] = _client.AddDeviceNotification("MAIN.lrealVal", 8,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbLreal);
                hConnect[5] = _client.AddDeviceNotification("MAIN.realVal", 4,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbReal);
                hConnect[6] = _client.AddDeviceNotification("MAIN.stringVal", 13,
                    new NotificationSettings(AdsTransMode.OnChange, 100, 0), tbString);

                _client.AdsNotification += new EventHandler<AdsNotificationEventArgs>(OnNotification);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

		private void OnNotification(object sender, AdsNotificationEventArgs e)
		{
            // The Notification appears in Background Thread
            DateTimeOffset time = e.TimeStamp;
            ReadOnlyMemory<byte> memory = e.Data;
            
			string strValue = "";

            if (e.Handle == hConnect[0])
                strValue = BitConverter.ToBoolean(memory.ToArray(), 0).ToString();
            else if (e.Handle == hConnect[1])
                strValue = BitConverter.ToInt16(memory.ToArray(), 0).ToString();
            else if (e.Handle == hConnect[2])
                strValue = BitConverter.ToInt32(memory.ToArray(), 0).ToString();
            else if (e.Handle == hConnect[3])
            {
                byte[] data = memory.ToArray();
                strValue = ((sbyte)data[0]).ToString();
            }
            else if (e.Handle == hConnect[4])
                strValue = BitConverter.ToDouble(memory.ToArray(), 0).ToString();
            else if (e.Handle == hConnect[5])
                strValue = BitConverter.ToSingle(memory.ToArray(), 0).ToString();
            else if (e.Handle == hConnect[6])
            {
                //strValue = new String(binRead.ReadChars(13));
                PrimitiveTypeMarshaler converter = new PrimitiveTypeMarshaler(StringMarshaler.DefaultEncoding);
                converter.Unmarshal(memory.Span, out strValue);
            }

            // Determine the TextBox
            TextBox textBox = (TextBox)e.UserData;
            string text = string.Format("DateTime: {0},{1}ms; {2}", time, time.Millisecond, strValue);

            // Synchronization to UI Thread.
            this.Invoke(new Action(() => textBox.Text = text));
		}

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
                if (_client != null)
                {
                    // Removing Notifications
                    _client.AdsNotification -= new EventHandler<AdsNotificationEventArgs>(OnNotification);
                    // Disposing the Client.
                    _client.Dispose();
                    _client = null;
                }
			}
			catch(Exception err)
			{
				MessageBox.Show(err.Message);
			}			
		}
    }
}
