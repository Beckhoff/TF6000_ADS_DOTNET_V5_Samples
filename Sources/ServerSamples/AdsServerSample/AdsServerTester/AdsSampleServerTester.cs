using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestServer;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;

/* Sample Windows.Forms application that instantiates ans connects an AdsSampleServer. 
 */

public partial class AdsSampleServerTester : Form, /*IServerLogger,*/ ILogger
{
    private AdsSampleServer _server;
    private uint _serverNotificationHandle = 0;

    //private delegate void LoggerAppender(String message);
    //private event LoggerAppender _loggerAppender;

    public AdsSampleServerTester()
    {
        InitializeComponent();

        // Create a new AdsSampleServer instance listening on Ads port 27000.

        _server = new AdsSampleServer(27000, "AdsSampleServer", this);
        _server.ServerConnectionStateChanged+=ConnectionStatusChanged;
        //_loggerAppender = new LoggerAppender(AppendLoggerListDelegate);
    }

    private void ConnectionStatusChanged(object sender, ServerConnectionStateChangedEventArgs args)
    {
        if (args.State == ServerConnectionState.Connected)
            AppendLoggerList("Server is connected to port" + _server.ServerAddress.Port);
        else if (args.State == ServerConnectionState.Disconnected)
            AppendLoggerList("Server is disconnected");
    }

    void AdsSampleServerTester_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            // Disconnect the server from the local ADS router.
            if (_server.IsConnected)
                _server.Disconnect();
        }
        catch
        {
            Debug.Fail("");
            // Do nothing if disconnect fails while closing the application
        }

    }

    [STAThread]
    static void Main()
    {
        Application.Run(new AdsSampleServerTester());
    }

    /* Event handlers for buttons.
     */

    private async void _buttonConnect_Click(object sender, EventArgs e)
    {
        try
        {
            /* Connect the server to the local ADS router. Now the server is ready to 
             * answer requests.
             */

            if (this.AsyncMode)
            {
                await _server.ConnectServerAndWaitAsync(CancellationToken.None);
            }
            else
            {
                uint port = _server.ConnectServer();
            }
            //Task task = _server.ConnectAsync();

            
            //AppendLoggerList("Server is connected to port " + _server.Address.Port);
            //await task;
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format("Connect failed ({0})",ex.Message));
        }
    }

    private void _buttonDisconnect_Click(object sender, EventArgs e)
    {
        try
        {
            /* Disconnect the server from the local ADS router.
             */
            _server.Disconnect();
            //AppendLoggerList("Server is disconnected");
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format("Disconnect failed ({0})!)",ex.Message));
        }
    }

    private void ThrowOnError(AdsErrorCode result)
    {
        if (result != AdsErrorCode.NoError)
        {
            throw new AdsException(string.Format("Failed with Error: {0}", result.ToString()));
        }
    }

    /* The following button event handlers send ADS requests to this server. The responses are
     * handled by the confirmation methods in the AdsSampleServer class.
     * The invokeId is always set to 0 in this sample. Use the invokeId in your server implemtation
     * to match requests an confirmations.
     */
    #region Request Button Handlers 

    private async void _ReadDevInfoButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadDeviceInfoRequestAsync(
                _server.ServerAddress, // receiver address
                cancel);
            ThrowOnError(result);
        }
        catch (Exception ex)
        {
            AppendLoggerList(string.Format("Read Device Info call failed ({0}).", ex.Message));
        }
    }

    private async void _readButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadRequestAsync(
                   _server.ServerAddress, // receiver address
                   0x10000,         // index group
                   0,               // index offset
                   4,              // number of bytes to read
                   cancel);
            ThrowOnError(result);
        }
        catch (Exception ex)
        {

            AppendLoggerList(string.Format("Read call failed ({0}).", ex.Message));
        }
    }

    private async void _writeButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerWriteRequestAsync(
                _server.ServerAddress, // receiver address
                0x10000,         // index group
                0,               // index offset
                new byte[] { },  // data
                cancel);           

            ThrowOnError(result);

        }
        catch (Exception ex)
        {

            AppendLoggerList(string.Format("Write call failed ({0}).", ex.Message));
        }
    }

    private async void _readStateButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadStateRequestAsync(
                _server.ServerAddress, // receiver address
                cancel);

            ThrowOnError(result);

        }
        catch (Exception ex)
        {

            AppendLoggerList(string.Format("Read State call failed ({0}).", ex.Message));
        }
    }

    private async void _writeContolButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerWriteControlRequestAsync(
                _server.ServerAddress, // receiver address
                AdsState.Idle,  // new ADS state
                3,              // new device state
                new byte[] { },   // additional data buffer
                cancel);          

            ThrowOnError(result);
        }
        catch (Exception ex)
        {
            AppendLoggerList(string.Format("Write Control call failed ({0}).", ex.Message));
        }
    }

    CancellationToken cancel = CancellationToken.None;

    private async void _addNotButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerAddDeviceNotificationRequestAsync(
                _server.ServerAddress, // receiver address
                (uint)AdsReservedIndexGroup.DeviceData,             // index group
                (uint)AdsReservedIndexOffset.DeviceDataAdsState,    // index offset
                4,               // number of bytes to be sent
                new NotificationSettings(
                    AdsTransMode.OnChange, // transmission mode
                    1000,           // maximum delay
                    1000),          // cycle time
                cancel);

            ThrowOnError(result);
        }
        catch (Exception ex)
        {

            AppendLoggerList(string.Format("Add Device Notification call failed ({0}).", ex.Message));
        }
    }

    private async void _delNotButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerDeleteDeviceNotificationRequestAsync(
                _server.ServerAddress,            // receiver address
                _serverNotificationHandle,         // notification handle to be deleted
                cancel);
            ThrowOnError(result);

        }
        catch (Exception ex)
        {

            AppendLoggerList(string.Format("Add Device Notification call failed ({0}).", ex.Message));
        }
    }

    private async void _readWriteButton_Click(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadWriteRequestAsync(
                _server.ServerAddress,    // receiver address
                0x10000,            // index group
                0,                  // index offset
                4,                  // number of bytes to read
                new byte[] { },     // write data buffer
                cancel);            // 
            ThrowOnError(result);
        }
        catch (Exception ex)
        {

            AppendLoggerList(string.Format("Read Write call failed ({0}).", ex.Message));
        }
    }

    #endregion


    public uint ServerNotificationHandle
    {
        get { return _serverNotificationHandle; }
        set { _serverNotificationHandle = value; }
    }

    #region Helper Methods

    //void IServerLogger.AppendLoggerList(string str)
    //{
    //    this.AppendLoggerList(str);
    //}

    //void IServerLogger.OnAdsWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> data)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadInd(AmsAddress rAddr, uint invokeId, uint indexOffset, int cbLength)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadStateInd(AmsAddress rAddr, uint invokeId)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsWriteControlInd(AmsAddress rAddr, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsAddDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings settings)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsDelDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint hNotification)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadStateCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsDelDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
    //{
    //    //throw new NotImplementedException();
    //}

    //void IServerLogger.OnAdsReadDeviceInfoInd(AmsAddress rAddr, uint invokeId)
    //{
    //    //throw new NotImplementedException();
    //}

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);
        AppendLoggerList(message);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) => default;

    public void AppendLoggerList(string logMessage)
    {

        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => _loggerListbox.Items.Add(logMessage)));
        }
        else
        {
            this._loggerListbox.Items.Add(logMessage);
        }
    }

    public bool AsyncMode
    {
        get { return cbAsync.Checked; }
    }

    //uint IServerLogger.ServerNotificationHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    

    //private void AppendLoggerListDelegate(string logMessage)
    //{
    //    AppendLoggerList(logMessage);
    //}

    #endregion

}
