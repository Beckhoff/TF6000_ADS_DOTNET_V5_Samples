using System;
using System.Windows.Forms;
using System.Buffers.Binary;
using System.Threading;
using TwinCAT.Ads;

namespace S23_TcStateChange
{
    public partial class Form1 : Form
	{
		private AdsClient	_tcClient = null;
		private uint _notificationHandle = 0;

		public Form1()
		{
			InitializeComponent();
		}

		SynchronizationContext _context = null;

		private void Form1_Load(object sender, EventArgs e)
		{
			try
			{
				_context = SynchronizationContext.Current;
				_tcClient = new AdsClient();

				/* connect the client to the local PLC */
				_tcClient.Connect(851);
				int size = 2;  // The AdsState enumeration base class is UInt16 (byte size 2)


				/* register callback to react on state changes of the local AMS Router */
				_tcClient.RouterStateChanged += new EventHandler<AmsRouterNotificationEventArgs>(OnRouterStateChanged);

				_notificationHandle = _tcClient.AddDeviceNotification(
											(uint)AdsReservedIndexGroup.DeviceData,	/* index group of the device state*/
											(uint)AdsReservedIndexOffset.DeviceDataAdsState, /*index offset of the device state */
											size,	/* Size of the NotificationData in bytes */
											new NotificationSettings(AdsTransMode.OnChange,	/* transfer mode: transmit ste on change */
											0,	/* transmit changes immediately */
											0),
											null);

				/* register callback to react on state changes of the local PLC */
				_tcClient.AdsNotification += new EventHandler<AdsNotificationEventArgs>(OnAdsNotification);

				/* A more simplified variant (with less control) to receive AdsState changes is:
				_tcClient.AdsStateChanged += ...
				*/
			}
			catch (AdsErrorException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/* callback function called on state changes of the PLC */
		void OnAdsNotification(object sender, AdsNotificationEventArgs e)
		{
			if (e.Handle == _notificationHandle)
			{
				BinaryPrimitives.ReadUInt16LittleEndian(e.Data.Span);
				AdsState plcState = (AdsState)BinaryPrimitives.ReadUInt16LittleEndian(e.Data.Span); /* Unmarshal received Data to AdsState object */

				_context.Post((s) => _plcLabelValue.Text = s.ToString(), plcState);
			}
		}

		/* callback function called on state changes of the local AMS Router */
		void OnRouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
		{
			_context.Post((s) => _routerLabelValue.Text = s.ToString(), e.State);
		}

		private void _exitButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				
                if(_notificationHandle != 0)
                    _tcClient.DeleteDeviceNotification(_notificationHandle);

                _tcClient.Dispose();
			}
			catch(AdsErrorException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}
