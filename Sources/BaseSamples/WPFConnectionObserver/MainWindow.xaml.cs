using System;
using System.Windows;
using System.Windows.Threading;
using TwinCAT;
using TwinCAT.Ads;

namespace WPFConnectionObserver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private DispatcherTimer _timer = null;
        private AdsSession _session = null;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Using the FastWriteThrough setting to detect recurring devices as fast as possible!
            // Otherwise we could stay in the 'Lost' connection state for 21 Seconds by default before
            // a reconnection is possible again.
            SessionSettings settings = SessionSettings.FastWriteThrough;
            _session = new AdsSession(AmsNetId.Local, 10000, settings);

            IConnection connection = _session.Connect();
            tbConnectionState.Text = connection.ConnectionState.ToString();
            _session.ConnectionStateChanged += _session_ConnectionStateChanged;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += TimerOnTick;

            _timer.Start();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _session.Dispose();
        }

        private void _session_ConnectionStateChanged(object sender, TwinCAT.ConnectionStateChangedEventArgs e)
        {
            // ConnectionStateChanged will be triggered by communication Invokes
            // or can be invoked by a router Notification. Therefore we must synchronize it into the UIThread!
            this.Dispatcher.Invoke(() => tbConnectionState.Text = e.NewState.ToString());
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            // The Timer Event will occur here in the UI thread because its an DispatcherTimer event!
            // An active ADS request will trigger Connection State periodically!
            StateInfo stateInfo;
            if (_session.Connection.TryReadState(out stateInfo) == AdsErrorCode.NoError)
            {
                tbAdsState.Text = stateInfo.AdsState.ToString();
            }
            else
            {
                tbAdsState.Text = "Invalid";
            }
        }

        private void bntRun_Click(object sender, RoutedEventArgs e)
        {
            StateInfo state = new StateInfo();
            state.AdsState = AdsState.Reset; // Restarting the SystemService (via Stop)
            AdsErrorCode error = _session.Connection.TryWriteControl(state);

            if (error != AdsErrorCode.NoError)
                MessageBox.Show(error.ToString());
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            StateInfo state = new StateInfo();
            state.AdsState = AdsState.Reconfig; // Go to config via Stop
            AdsErrorCode error = _session.Connection.TryWriteControl(state);

            if (error != AdsErrorCode.NoError)
                MessageBox.Show(error.ToString());
        }
    }
}
