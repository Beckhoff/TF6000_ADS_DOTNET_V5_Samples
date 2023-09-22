using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
using TwinCAT.Ads.Reactive;
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

            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromMilliseconds(200);
            _uiTimer.Tick += _uiTimer_Tick;

            tBDefaultResurrectionTime.Text = ((int)SessionSettings.DefaultResurrectionTime.TotalSeconds).ToString();
            tBDefaultTimeout.Text = ((int)SessionSettings.DefaultCommunicationTimeout.TotalSeconds).ToString();

            EnableDisableControls();
            UpdateUI();
            tbNetId.TextChanged += tbNetId_TextChanged;
            tbPort.TextChanged += tbPort_TextChanged;

            _ticksObserver = new TimerObservable((int)sldFreq.Value);
            lblFreq.Content = ((int)sldFreq.Value).ToString();
        }

        Dictionary<string, AdsErrorCode> _errorCodesDict = new Dictionary<string, AdsErrorCode>();

        protected override void OnClosed(EventArgs e)
        {
            OnDisconnect();
            base.OnClosed(e);
        }

        /// <summary>
        /// Timer Updating the UI
        /// </summary>
        DispatcherTimer _uiTimer = null;

        /// <summary>
        /// Observable creating the Ticks in this application for GetState.
        /// </summary>
        TimerObservable _ticksObserver = null;
        
        IDisposable _ticksSubscription = null;
        IDisposable _deviceStateSubscription = null;

        /// <summary>
        /// Cached ReadDeviceState result
        /// </summary>
        ResultReadDeviceState _resultState;

        /// <summary>
        /// The session
        /// </summary>
        AdsSession _session = null;
        /// <summary>
        /// The connection
        /// </summary>
        AdsConnection _connection = null;

        CancellationTokenSource _connectCancel;
        /// <summary>
        /// The frames per second subscription
        /// </summary>
        IDisposable _framesSubscription = null;

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
                UpdateUI();
            }
        }

        private void OnConnect()
        {
            _connectCancel = new CancellationTokenSource();

            AmsNetId netId = AmsNetId.Parse(tbNetId.Text);
            int port = int.Parse(tbPort.Text);

            int communicationTimeout = int.Parse(tBDefaultTimeout.Text) * 1000;
            int resurrectionTime = int.Parse(tBDefaultResurrectionTime.Text);

            SessionSettings settings = SessionSettings.Default;
            settings.ResurrectionTime = TimeSpan.FromSeconds((double)resurrectionTime);

            _session = new AdsSession(netId, port, settings);
            _session.ConnectionStateChanged += _session_ConnectionStateChanged;

            _connection = (AdsConnection)_session.Connect();
            _connection.ConnectionStateChanged += _connection_ConnectionStatusChanged;
            _connection.RouterStateChanged += _connection_RouterStateChanged;

            try
            {
                _connection.RegisterAdsStateChangedAsync(_connection_AdsStateChanged, CancellationToken.None);
                _connection.AdsSymbolVersionChanged += _connection_AdsSymbolVersionChanged;
            }
            catch (Exception)
            {
                //Debug.Fail("Can happen when Target doesn't support AdsState ",ex.Message);
            }
            _uiTimer.Start();

            // Same interval as timer
            _framesSubscription = _connection.PollCyclesPerSecond(TimeSpan.FromSeconds(1), Scheduler.Default)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((c) => { 
                    lblFramesPerSec.Content = c.RequestsPerSecond.ToString("0.00");
                    lblErrorsPerSec.Content = c.ErrorsPerSecond.ToString("0.00");
                    lblSuccededPerSec.Content = c.SucceedsPerSecond.ToString("0.00");
                });
            
            _ticksSubscription = _ticksObserver.Subscribe();
            _ticksObserver.Start();
            _deviceStateSubscription = _connection.PollDeviceStateAsync(_ticksObserver, CancellationToken.None).Subscribe(r => _resultState = r);
        }

        private void OnDisconnect()
        {
            AmsAddress oldConnection = null;

            if (_ticksObserver != null)
            {
                _ticksObserver.Stop();
            }

            if (_ticksSubscription != null)
            { 
                _ticksSubscription.Dispose();
                _ticksSubscription = null;
            }

            if (_deviceStateSubscription != null)
            {
                _deviceStateSubscription.Dispose();
                _deviceStateSubscription = null;
            }

            if (_framesSubscription != null)
            {
                _framesSubscription.Dispose();
                _framesSubscription = null;
            }

            if (_uiTimer != null)
                _uiTimer.Stop();

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
            _resultState = null;
        }

        private void _connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
                EnableDisableControls();
                AddMessage($"Router: RouterStateChanged changed to '{e.State}'!");
            });
        }

        private void _uiTimer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void _session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
                EnableDisableControls();
                AddMessage($"Session: ConnectionStateChanged {e.OldState} --> {e.NewState} Reason: {e.Reason}");
            });
        }

        private void _connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
                EnableDisableControls();
                AddMessage($"Connection: ConnectionStateChanged {e.OldState} --> {e.NewState} Reason: {e.Reason}");
            });
        }

        private void _connection_AdsSymbolVersionChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
                AddMessage("Symbol version has changed");
            });
        }

        private void _connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
                AddMessage(string.Format("AdsState changed to '{0}", e.State.AdsState));
            });
        }

        private void UpdateUI()
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
                    //StateInfo info = new StateInfo();
                    //AdsErrorCode errorCode = _connection.TryReadState(out info);
                    //tBAdsState.Text = info.AdsState.ToString();
                    AdsState state = AdsState.Invalid;

                    if (_resultState != null)
                    {
                        state = _resultState.State.AdsState;
                    }

                    tBAdsState.Text = state.ToString();

                    switch (state)
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

        private void sldFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_ticksObserver != null)
                _ticksObserver.Interval = (int)e.NewValue;

            if (lblFreq != null)
                lblFreq.Content = (int)e.NewValue;
        }
    }

    /// <summary>
    /// Class TimerObservable. Implementation of a Timer Observable, where the Interval can be changed on the fly.
    /// Implements the <see cref="System.IObservable{System.Reactive.Unit}" />
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="System.IObservable{System.Reactive.Unit}" />
    /// <seealso cref="IDisposable" />
    public sealed class TimerObservable : IObservable<Unit>, IDisposable
    {
        private readonly HashSet<IObserver<Unit>> _observers = new();

        Timer _timer;
        int _interval = 200;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerObservable"/> class.
        /// </summary>
        /// <param name="interval">The interval.</param>
        public TimerObservable(int interval)
        {
            _interval = interval;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _timer = new Timer(Elapsed, null, 0, _interval);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            if (_timer != null)
                _timer.Dispose();
            _timer = null;
        }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        public int Interval
        {
            get { return _interval; }

            set 
            {
                _interval = value;
                if (_timer != null)
                {
                    _timer.Change(0, _interval);
                }
            }
        }

        /// <summary>
        /// Timer elapsed handler
        /// </summary>
        /// <param name="stateInfo">The state information.</param>
        private void Elapsed(Object stateInfo)
        {
            foreach(var observer in _observers)
            {
                observer.OnNext(Unit.Default);
            }
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            // Check whether observer is already registered. If not, add it.
            _observers.Add(observer);
            return new Unsubscriber<Unit>(_observers, observer);
        }

        /// <summary>
        /// Completes the Observable
        /// </summary>
        public void Complete()
        {
            foreach (IObserver<Unit> observer in _observers)
            {
                observer.OnCompleted();
            }
            _observers.Clear();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Stop();
                }
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Unsubscription object for the TimerObservable
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <typeparam name="Unit">The type of the unit.</typeparam>
    /// <seealso cref="IDisposable" />
    internal sealed class Unsubscriber<Unit> : IDisposable
    {
        private readonly ISet<IObserver<Unit>> _observers;
        private readonly IObserver<Unit> _observer;

        internal Unsubscriber(
            ISet<IObserver<Unit>> observers,
            IObserver<Unit> observer) => (_observers, _observer) = (observers, observer);

        public void Dispose() => _observers.Remove(_observer);
    }
}
