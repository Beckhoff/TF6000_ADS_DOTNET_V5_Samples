using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


#region CODE_SAMPLE
        private DispatcherTimer _timer = null;
        private AdsSession _session = null;
        //private AdsConnection _connection = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _session = new AdsSession(AmsNetId.Local, 10000);
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
            tbConnectionState.Text = e.NewState.ToString();
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
#endregion

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            StateInfo state = new StateInfo();
            state.AdsState = AdsState.Reconfig;
            AdsErrorCode error = _session.Connection.TryWriteControl(state);
            MessageBox.Show(error.ToString());
        }

        private void bntRun_Click(object sender, RoutedEventArgs e)
        {
            StateInfo state = new StateInfo();
            state.AdsState = AdsState.Start;
            AdsErrorCode error = _session.Connection.TryWriteControl(state);
            MessageBox.Show(error.ToString());
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            StateInfo state = new StateInfo();
            state.AdsState = AdsState.Reset;
            AdsErrorCode error = _session.Connection.TryWriteControl(state);
            MessageBox.Show(error.ToString());
        }
    }
}
