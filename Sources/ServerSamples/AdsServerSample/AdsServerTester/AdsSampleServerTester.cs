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

public partial class AdsSampleServerTester : Form, ILogger
{
    private AdsSampleServer _server;
    private uint _serverNotificationHandle = 0;

    public AdsSampleServerTester()
    {
        InitializeComponent();

        // Create a new AdsSampleServer instance listening on Ads port 32768.
        // User Area for AmsPorts is >= 0x8000
        _server = new AdsSampleServer(0x8000, "AdsSampleServer", this);
        _server.ServerConnectionStateChanged+=ConnectionStatusChanged;
        enableDisableControls();
    }

    protected void enableDisableControls()
    {
        this.Text = $"AdsServerTester '{_server.ServerAddress}'";

        _buttonConnect.Enabled = !_server.IsConnected;
        _buttonDisconnect.Enabled = _server.IsConnected;

        _addNotButton.Enabled = _server.IsConnected;
        _delNotButton.Enabled = _server.IsConnected;
        _readButton.Enabled = _server.IsConnected;
        _ReadDevInfoButton.Enabled = _server.IsConnected;
        _readStateButton.Enabled = _server.IsConnected;
        _readWriteButton.Enabled = _server.IsConnected;
        _writeButton.Enabled = _server.IsConnected;
        _writeControlButton.Enabled = _server.IsConnected;
    }


    private void ConnectionStatusChanged(object sender, ServerConnectionStateChangedEventArgs args)
    {
        if (args.State == ServerConnectionState.Connected)
            AppendLogMessage("Server is connected to port" + _server.ServerAddress.Port);
        else if (args.State == ServerConnectionState.Disconnected)
            AppendLogMessage("Server is disconnected");

        enableDisableControls();
    }

    void AdsSampleServerTester_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            // Disconnect the server from the local ADS router.
            if (_server.IsConnected)
                _server.Disconnect();
        }
        catch (Exception)
        {
            //Debug.Fail("");
            // Do nothing if disconnect fails while closing the application
        }

    }

    [STAThread]
    static void Main()
    {
        Application.Run(new AdsSampleServerTester());
    }


    /// <summary>
    /// CancellationTokenSource to disconnect the Server and stopping Requests.
    /// </summary>
    CancellationTokenSource _cancelSource = null;

    /* Event handlers for buttons.
     */

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        try
        {
            _cancelSource = new CancellationTokenSource();

            /* Connect the server to the local ADS router. Now the server is ready to 
             * answer requests.
             */
            await _server.ConnectServerAndWaitAsync(_cancelSource.Token);
            _cancelSource = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format("Connect failed ({0})",ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private void OnDisconnectClicked(object sender, EventArgs e)
    {
        try
        {
            /* Disconnect the server from the local ADS router.
             */
            _cancelSource.Cancel();
            //TODO: Actually the Disconnect should not be necessary.
            //But it seems that the Cancel doesn't close the connection propertly.
            _server.Disconnect();
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format("Disconnect failed ({0})!)",ex.Message));
        }
        finally
        {
            enableDisableControls();
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
     * The invokeId is always set to 0 in this sample. Use the invokeId in your server implementation
     * to match requests and confirmations.
     */
    #region Request Button Handlers 

    private async void OnReadDevInfoClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadDeviceInfoRequestAsync(
                _server.ServerAddress,  // receiver address
                _cancelSource.Token);   // cancellation token
            ThrowOnError(result);
        }
        catch (Exception ex)
        {
            AppendLogMessage(string.Format("Read Device Info call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnReadClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadRequestAsync(
                   _server.ServerAddress,   // receiver address
                   0x10000,                 // index group
                   0,                       // index offset
                   4,                       // number of bytes to read
                   _cancelSource.Token);    // cancellation token
            ThrowOnError(result);
        }
        catch (Exception ex)
        {

            AppendLogMessage(string.Format("Read call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnWriteClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerWriteRequestAsync(
                _server.ServerAddress,  // receiver address
                0x10000,                // index group
                0,                      // index offset
                new byte[] { },         // data
                _cancelSource.Token);   // cancellation token

            ThrowOnError(result);

        }
        catch (Exception ex)
        {

            AppendLogMessage(string.Format("Write call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnReadStateClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadStateRequestAsync(
                _server.ServerAddress,  // receiver address
                _cancelSource.Token);   // cancellation token

            ThrowOnError(result);

        }
        catch (Exception ex)
        {
            AppendLogMessage(string.Format("Read State call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnWriteControlClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerWriteControlRequestAsync(
                _server.ServerAddress,  // receiver address
                AdsState.Idle,          // new ADS state
                3,                      // new device state
                new byte[] { },         // additional data buffer
                _cancelSource.Token);   // cancellation token

            ThrowOnError(result);
        }
        catch (Exception ex)
        {
            AppendLogMessage(string.Format("Write Control call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnAddNotificationClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerAddDeviceNotificationRequestAsync(
                _server.ServerAddress,      // receiver address
                (uint)AdsReservedIndexGroup.DeviceData,             // index group
                (uint)AdsReservedIndexOffset.DeviceDataAdsState,    // index offset
                4,                          // number of bytes to be sent
                new NotificationSettings(
                    AdsTransMode.OnChange,  // transmission mode
                    1000,                   // maximum delay
                    1000),                  // cycle time
                    _cancelSource.Token);   // cancellation token

            ThrowOnError(result);
        }
        catch (Exception ex)
        {

            AppendLogMessage(string.Format("Add Device Notification call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnDeleteNotificationClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerDeleteDeviceNotificationRequestAsync(
                _server.ServerAddress,      // receiver address
                _serverNotificationHandle,  // notification handle to be deleted
                _cancelSource.Token);       // cancellation token
            ThrowOnError(result);

        }
        catch (Exception ex)
        {
            AppendLogMessage(string.Format("Add Device Notification call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    private async void OnReadWriteClicked(object sender, EventArgs e)
    {
        try
        {
            AdsErrorCode result = await _server.TriggerReadWriteRequestAsync(
                _server.ServerAddress,  // receiver address
                0x10000,                // index group
                0,                      // index offset
                4,                      // number of bytes to read
                new byte[] { },         // write data buffer
                _cancelSource.Token);   // cancellation token
            ThrowOnError(result);
        }
        catch (Exception ex)
        {

            AppendLogMessage(string.Format("Read Write call failed ({0}).", ex.Message));
        }
        finally
        {
            enableDisableControls();
        }
    }

    #endregion


    public uint ServerNotificationHandle
    {
        get { return _serverNotificationHandle; }
        set { _serverNotificationHandle = value; }
    }

    #region Helper Methods

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);
        AppendLogMessage(message);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) => default;

    public void AppendLogMessage(string logMessage)
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

    //public bool AsyncMode
    //{
    //    get { return cbAsync.Checked; }
    //}

    #endregion
}
