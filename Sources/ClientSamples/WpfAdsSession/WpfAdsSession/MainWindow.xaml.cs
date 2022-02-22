using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TwinCAT;
using TwinCAT.Ads;
//using TwinCAT.SystemService;
//using TwinCAT.SystemService.UI;
using TwinCAT.TypeSystem;

namespace AdsSessionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string[] names = Enum.GetNames(typeof(AdsErrorCode));
            AdsErrorCode[] values = (AdsErrorCode[])Enum.GetValues(typeof(AdsErrorCode));

            for (int i = 0; i < names.Length; i++)
            {
                _errorCodesDict.Add(names[i], values[i]);
                cBError.Items.Add(names[i]);
            }

            cBError.SelectedItem = "ClientSyncTimeOut";

            tbNetId.Text = AmsNetId.Local.ToString();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += _timer_Tick;

            tBDefaultResurrectionTime.Text = ((int)SessionSettings.DefaultResurrectionTime.TotalSeconds).ToString();
            tBDefaultTimeout.Text = ((int)SessionSettings.DefaultCommunicationTimeout.TotalSeconds).ToString();

            EnableDisableControls();
            Update();
            tbNetId.TextChanged += tbNetId_TextChanged;
            tbPort.TextChanged += tbPort_TextChanged;

        }

        Dictionary<string, AdsErrorCode> _errorCodesDict = new Dictionary<string, AdsErrorCode>();

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            _timer = null;
            if (_connection != null)
            {
                IDisposable disp = _connection as IDisposable;
                
                if (disp != null)
                    disp.Dispose();
            }
            _connection = null;

            if (_session != null && !_session.Disposed)
            {
                _session.Dispose();
            }
            _session = null;

            //if (_systemService != null)
            //{
            //    _systemService.Dispose();
            //}
            //_systemService = null;
            base.OnClosed(e);
        }

        DispatcherTimer _timer = null;
        //SystemServiceClass _systemService = null;
        AdsSession _session = null;
        AdsConnection _connection = null;

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_connection == null)
                {
                    OnConnect();
                }
                else
                {
                    OnDisconnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                OnDisconnect();
            }
            finally
            {
                EnableDisableControls();
                Update();
            }
        }

        private void OnDisconnect()
        {
            AmsAddress oldConnection = null;

            if (_timer != null)
                _timer.Stop();

            if (_connection != null)
            {
                oldConnection = _connection.Address;
                //_connection.AdsStateChanged -= _connection_AdsStateChanged;
                //_connection.AdsSymbolVersionChanged -= _connection_AdsSymbolVersionChanged;
                //_connection.ConnectionStatusChanged -= _connection_ConnectionStatusChanged;
                if (!_connection.Disposed)
                    _connection.Dispose();
            }

            if (_session != null && !_session.Disposed)
            {
                _session.Dispose();
            }

            tBSynchronized.Text = "";
            _connection = null;
            _session = null;

            //if (oldConnection != null)
            //    AddMessage(string.Format("Disconnected from {0} Port: {1}", oldConnection.NetId, oldConnection.Port));
        }

        private void OnConnect()
        {
            //RouteTarget target = (RouteTarget)lbNetId.SelectedItem;
            AmsNetId netId = AmsNetId.Parse(tbNetId.Text);
            int port = int.Parse(tbPort.Text);

            int communicationTimeout = int.Parse(tBDefaultTimeout.Text) * 1000;
            int resurrectionTime = int.Parse(tBDefaultResurrectionTime.Text);

            //SessionSettings settings = new SessionSettings(true, communicationTimeout);
            SessionSettings settings = SessionSettings.Default;
            settings.ResurrectionTime = TimeSpan.FromSeconds((double)resurrectionTime);

            _session = new AdsSession(netId, port,settings);

#if NETFULL
#pragma warning disable CS0618 // Type or member is obsolete
            tBSynchronized.Text = settings.Synchronized.ToString();
#pragma warning restore CS0618 // Type or member is obsolete
#endif
            _session.ConnectionStateChanged += _session_ConnectionStateChanged;

            _connection = (AdsConnection)_session.Connect();
            _connection.ConnectionStateChanged += _connection_ConnectionStatusChanged;
            _connection.RouterStateChanged += _connection_RouterStateChanged;

            try
            {
                _connection.RegisterAdsStateChangedAsync(_connection_AdsStateChanged, CancellationToken.None);
                //_connection.AdsStateChanged += _connection_AdsStateChanged;
                _connection.AdsSymbolVersionChanged += _connection_AdsSymbolVersionChanged;
            }
            catch (Exception)
            {
                //Debug.Fail("Can happen when Target doesn't support AdsState ",ex.Message);
            }
            _timer.Start();
            //AddMessage(string.Format("Connected to {0} Port: {1}", netId, port));
        }

        private void _connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
                EnableDisableControls();
                AddMessage($"Router: RouterStateChanged changed to '{e.State}'!");
            });
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void _session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
                EnableDisableControls();
                AddMessage($"Session: ConnectionStateChanged {e.OldState} --> {e.NewState} Reason: {e.Reason}");
            });
        }

        private void _connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
                EnableDisableControls();
                AddMessage($"Connection: ConnectionStateChanged {e.OldState} --> {e.NewState} Reason: {e.Reason}");
            });
        }

        private void _connection_AdsSymbolVersionChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
                AddMessage("Symbol version has changed");
            });
        }

        private void _connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
                AddMessage(string.Format("AdsState changed to '{0}", e.State.AdsState));
            });
        }

        private void Update()
        {
            this.DataContext = null;
            this.DataContext = _session;

            SolidColorBrush adsStateBrush = new SolidColorBrush();
            SolidColorBrush connectionStateBrush = new SolidColorBrush();

            if (_connection != null)
            {
                tBConnectionState.Text = _connection.State.ToString();

                switch (_connection.State)
                {
                    case ConnectionState.None:
                        connectionStateBrush = new SolidColorBrush();
                        break;
                    case ConnectionState.Disconnected:
                        connectionStateBrush = new SolidColorBrush(Colors.LightGray);
                        break;
                    case ConnectionState.Connected:
                        connectionStateBrush = new SolidColorBrush(Colors.LightGreen);
                        break;
                    case ConnectionState.Lost:
                        connectionStateBrush = new SolidColorBrush(Colors.Yellow);
                        break;
                    default:
                        break;
                }


                if (_connection.IsConnected)
                {
                    StateInfo info = new StateInfo();
                    AdsErrorCode errorCode = _connection.TryReadState(out info);
                    tBAdsState.Text = info.AdsState.ToString();

                    switch(info.AdsState)
                    {
                        case AdsState.Run:
                            adsStateBrush = new SolidColorBrush(Colors.LightGreen);
                            break;
                        case AdsState.Stop:
                        case AdsState.Error:
                            adsStateBrush = new SolidColorBrush(Colors.Red);
                            break;
                        case AdsState.Config:
                            adsStateBrush = new SolidColorBrush(Colors.Blue);
                            break;
                    }
                }
            }
            else
            {
                tBConnectionState.Text = ConnectionState.None.ToString();
                tBAdsState.Text = AdsState.Invalid.ToString();
            }
            tBAdsState.Background = adsStateBrush;
            tBConnectionState.Background = connectionStateBrush;
        }

        private void EnableDisableControls()
        {
            if (_connection != null)
            {
                btnConnect.IsEnabled = true;
                btnConnect.Content = "Disconnect";
                btnInjectError.IsEnabled = !_connection.IsLost;
                btnSymbols.IsEnabled = !_connection.IsLost;
                btnResurrect.IsEnabled = _connection.IsLost;
                tbNetId.IsEnabled = false;
                tbPort.IsEnabled = false;
            }
            else
            {
                AmsNetId target = null;
                bool netIdParsed = AmsNetId.TryParse(tbNetId.Text, out target);
                int port = 0;
                bool portParsed = int.TryParse(tbPort.Text, out port);
                bool addressValid = netIdParsed && portParsed;

                btnConnect.IsEnabled = (addressValid);
                btnConnect.Content = "Connect";
                btnInjectError.IsEnabled = false;
                btnResurrect.IsEnabled = false;
                btnSymbols.IsEnabled = false;
                tbNetId.IsEnabled = true;
                tbPort.IsEnabled = true;
            }
        }

        private void lbNetId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableDisableControls();
        }

        private void btnAddRoute_Click(object sender, RoutedEventArgs e)
        {
            Debug.Fail("");
            //using (BroadcastSearchForm form = new BroadcastSearchForm())
            //{
            //    form.ShowDialog();
            //}
        }

        private void btnInjectError_Click(object sender, RoutedEventArgs e)
        {
            _connection.InjectError(_errorCode);
            AddMessage(string.Format("Injecting Error '{0}'", _errorCode));
        }

        private void btnResurrect_Click(object sender, RoutedEventArgs e)
        {
            AdsException ex;

            if (!_connection.TryResurrect(out ex))
            {
                AddMessage(string.Format("Resurrection Error '{0}'", ex.Message));
            }
            else
            {
                AddMessage("Resurrection succeeded!");
            }
        }

        private void AddMessage(string str)
        {
            tBMessages.AppendText("\r\n" + str);
            tBMessages.ScrollToEnd();
        }

        AdsErrorCode _errorCode = AdsErrorCode.ClientSyncTimeOut;

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _errorCode = (AdsErrorCode)Enum.Parse(typeof(AdsErrorCode),(string)cBError.SelectedItem);
        }

        private void btnSymbols_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IDataType> dataTypes = _session.SymbolServer.DataTypes;
            IEnumerable<ISymbol> symbols = _session.SymbolServer.Symbols;

            SymbolsWindow window = new SymbolsWindow();
            window.SetSymbols(symbols, dataTypes);
            window.ShowDialog();
        }

        private void tbNetId_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableDisableControls();
        }
        private void tbPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableDisableControls();
        }
    }
}
