using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using TwinCAT.Ads;
using TwinCAT.Ads.TcpRouter;

namespace TcpIpRouterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ILogger
    {
        private AmsTcpIpRouter _router;
        private CancellationTokenSource _cancel;
        private SynchronizationContext _ctx;

        AmsNetId _local = new AmsNetId("1.2.3.4.5.6");

        public MainWindow()
        {
            InitializeComponent();
            _ctx = SynchronizationContext.Current;

            //AmsNetId netId = new AmsNetId("1.2.3.4.5.6");
            //Debug.Fail("");
            //_cancel = new CancellationTokenSource();
            enableDisableControls();
        }

        private void _router_RouterStatusChanged(object sender, RouterStatusChangedEventArgs e)
        {
            _ctx.Post((state) =>
            {
                if (_router != null)
                    lblStatus.Content = _router.RouterStatus.ToString();
                else
                    lblStatus.Content = "";
                enableDisableControls();
            }
            ,null);
        }

        private void enableDisableControls()
        {
            bool validNetId = AmsNetId.TryParse(tbNetId.Text, out _local);
            btnStart.IsEnabled = validNetId;
            btnStop.IsEnabled = false;
            btnCancel.IsEnabled = _cancel != null;
            tbNetId.IsEnabled = true;

            if (_router != null)
            {
                switch (_router.RouterStatus)
                {
                    case RouterStatus.Initializing:
                    case RouterStatus.Stopping:
                        btnStart.IsEnabled = false;
                        btnStop.IsEnabled = false;
                        tbNetId.IsEnabled = false;
                        break;

                    case RouterStatus.Stopped:
                        btnStart.IsEnabled = validNetId;
                        btnStop.IsEnabled = false;
                        tbNetId.IsEnabled = true;
                        break;
                    case RouterStatus.Started:
                        btnStart.IsEnabled = false;
                        btnStop.IsEnabled = true;
                        tbNetId.IsEnabled = false;
                        break;

                    case RouterStatus.Starting:
                        btnStart.IsEnabled = false;
                        btnStop.IsEnabled = false;
                        tbNetId.IsEnabled = false;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _router = new AmsTcpIpRouter(_local, AmsTcpIpRouter.DEFAULT_TCP_PORT, null, AmsTcpIpRouter.DEFAULT_TCP_PORT, (IPNetwork)null,this);
            _router.RouterStatusChanged += _router_RouterStatusChanged;
            lblStatus.Content = _router.RouterStatus.ToString();

            btnStart.IsEnabled = false;
            btnCancel.IsEnabled = true;
            _cancel = new CancellationTokenSource();
            await _router.StartAsync(_cancel.Token);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _router.Stop();
            _router.RouterStatusChanged -= _router_RouterStatusChanged;
            _router = null;
            _cancel = null;
            enableDisableControls();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_cancel != null)
                _cancel.Cancel();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!((ILogger)(this)).IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);
            AppendLoggerList(message);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public void AppendLoggerList(string logMessage)
        {
            this.Dispatcher.BeginInvoke(new Action(() => lbLog.Items.Add(logMessage)));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.lbLog.Items.Clear();
        }

        private void tbNetId_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableDisableControls();
        }
    }
}
