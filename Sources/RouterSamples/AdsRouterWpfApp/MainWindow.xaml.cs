using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TwinCAT.Ads;
using TwinCAT.Ads.Logging;
using TwinCAT.Ads.SystemService;
using TwinCAT.Ads.TcpRouter;
using TwinCAT.Router;

namespace TcpIpRouterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ILogger
    {
        /// <summary>
        /// Simple LoggerProvider class that simply binds an existing ILogger instance as ILoggerProvider
        /// Implements the <see cref="ILoggerProvider" />
        /// </summary>
        /// <seealso cref="ILoggerProvider" />
        private class MyLoggerProvider : ILoggerProvider
        {
            ILogger _logger;

            public MyLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return _logger;
            }

            public void Dispose()
            {
                _logger = null;
            }
        }

        /// <summary>
        /// The router
        /// </summary>
        private AmsTcpIpRouter _router;

        /// <summary>
        /// The router ADS Server (Port 1)
        /// </summary>
        private AdsRouterServer _routerServer;
        /// <summary>
        ///The System Service ADS Server (Port 10000)
        /// </summary>
        private SystemServiceServer _systemService;

        private CancellationTokenSource _cancel;
        private SynchronizationContext _ctx;

        /// <summary>
        /// The logger factory
        /// </summary>
        ILoggerFactory _loggerFactory;

        AmsNetId _local = new AmsNetId("1.2.3.4.5.6");

        public MainWindow()
        {
            AdsLoggerConfiguration config = new AdsLoggerConfiguration();
            config.LogLevel = LogLevel.Information;

            MyLoggerProvider loggerProvider = new MyLoggerProvider(this);
            loggerProvider.CreateLogger("test");

            _loggerFactory = LoggerFactory.Create(builder =>
                builder.AddProvider(loggerProvider)
                .SetMinimumLevel(LogLevel.Debug)
            );

            InitializeComponent();
            _ctx = SynchronizationContext.Current;
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
            , null);
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
            _router = new AmsTcpIpRouter(_local, AmsTcpIpRouter.DEFAULT_TCP_PORT, null, AmsTcpIpRouter.DEFAULT_TCP_PORT, (IPNetwork?)null, _loggerFactory);
            _router.RouterStatusChanged += _router_RouterStatusChanged;
            lblStatus.Content = _router.RouterStatus.ToString();

            btnStart.IsEnabled = false;
            btnCancel.IsEnabled = true;
            _cancel = new CancellationTokenSource();

            Task routerTask = _router.StartAsync(_cancel.Token);

            _routerServer = new AdsRouterServer(_router, _loggerFactory);
            _systemService = new SystemServiceServer(_router, _loggerFactory);

            Task<AdsErrorCode> routerServerTask = _routerServer.ConnectServerAndWaitAsync(_cancel.Token);
            Task<AdsErrorCode> systemServiceTask = _systemService.ConnectServerAndWaitAsync(_cancel.Token);

            // Wait for all Tasks to stop
            await Task.WhenAll(routerTask, routerServerTask, systemServiceTask);
            enableDisableControls();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_routerServer != null)
                _routerServer.Dispose();

            if (_systemService != null)
                _systemService.Dispose();

            if (_router != null)
            {
                _router.Stop();
                _router.RouterStatusChanged -= _router_RouterStatusChanged;
            }
            _cancel = null;
            _routerServer = null;
            _systemService = null;
            _router = null;
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
            if (logLevel >= LogLevel.Information)
            {
                return true;
            }
            else
                return false;
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
