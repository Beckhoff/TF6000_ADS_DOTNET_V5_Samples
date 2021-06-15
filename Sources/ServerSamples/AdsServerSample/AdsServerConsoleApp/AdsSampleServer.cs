using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TwinCAT.Ads.Server;
using TwinCAT.Ads;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Binary;

namespace TestServer
{
    public interface IServerLogger
    {
        void AppendLoggerList(string str);
        uint ServerNotificationHandle { get; set; }

        void OnAdsWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData);
        void OnAdsReadInd(AmsAddress rAddr, uint invokeId, uint indexOffset, int cbLength);
        void OnAdsReadStateInd(AmsAddress rAddr, uint invokeId);
        void OnAdsWriteControlInd(AmsAddress rAddr, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data);
        void OnAdsAddDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings notificationSettings);
        void OnAdsDelDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint hNotification);
        void OnAdsDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders);
        void OnAdsReadWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData);
        void OnAdsReadStateCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState);
        void OnAdsReadCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData);
        void OnAdsWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result);
        void OnAdsReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, string name, AdsVersion version);
        void OnAdsReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result);
        void OnAdsAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle);
        void OnAdsDelDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result);
        void OnAdsReadWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData);
        void OnAdsReadDeviceInfoInd(AmsAddress rAddr, uint invokeId);

        ILogger Logger { get; }
    }

    public class ServerLoggerBase : IServerLogger
    {
        ILogger _logger = null;

        public ILogger Logger
        {
            get { return _logger; }
        }

        protected ServerLoggerBase(ILogger logger)
        {
            _logger = logger;
        }

        public virtual uint ServerNotificationHandle
        {
            get { return 0; }
            set { }
        }

        public virtual void AppendLoggerList(string str)
        {
        }

        public virtual void OnAdsAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle)
        {
        }

        public virtual void OnAdsAddDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings settings)
        {
        }

        public virtual void OnAdsDelDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
        }

        public virtual void OnAdsDelDeviceNotificationInd(AmsAddress sender, uint invokeId, uint hNotification)
        {
        }

        public virtual void OnAdsDeviceNotificationInd(AmsAddress sender, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders)
        {
        }

        public virtual void OnAdsReadCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
        }

        public virtual void OnAdsReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
        {
        }

        public virtual void OnAdsReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
        }

        public virtual void OnAdsReadDeviceInfoInd(AmsAddress sender, uint invokeId)
        {
        }

        public virtual void OnAdsReadInd(AmsAddress sender, uint invokeId, uint indexOffset, int cbLength)
        {
        }

        public virtual void OnAdsReadStateCon(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
        {
        }

        public virtual void OnAdsReadStateInd(AmsAddress sender, uint invokeId)
        {
        }

        public virtual void OnAdsReadWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
        }

        public virtual void OnAdsReadWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData)
        {
        }

        public virtual void OnAdsWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
        }

        public virtual void OnAdsWriteControlInd(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data)
        {
        }

        public virtual void OnAdsWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData)
        {
        }
    }

    public class ServerLogger : ServerLoggerBase
    {
        //ILogger _logger;

        public ServerLogger(ILogger logger) : base(logger)
        {
            //_logger = logger;
        }


        public override void OnAdsAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle)
        {
            Logger.LogDebug(string.Format("AddDeviceNotificationCon(Address: {0}, InvokeId: {1}, Result: {2}, Handle: {3}"), rAddr, invokeId, result, notificationHandle);
        }

        public override void OnAdsAddDeviceNotificationInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings settings)
        {
            Logger.LogDebug(string.Format("AddDeviceNotificationInd(Address: {0}, InvokeId: {1}, IG: {2}, IO: {3}, Len: {4})", sender, invokeId, indexGroup, indexOffset, cbLength));
        }

        public override void OnAdsDelDeviceNotificationCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
            Logger.LogDebug(string.Format("DelDeviceNotificationCon(Address: {0}, InvokeId: {1}, Result: {2})", sender, invokeId, result));
        }

        public override void OnAdsDelDeviceNotificationInd(AmsAddress sender, uint invokeId, uint hNotification)
        {
            Logger.LogDebug(string.Format("DelDeviceNotificationInd(Address: {0}, InvokeId: {1}, Handle: {2})", sender, invokeId, hNotification));
        }

        public override void OnAdsDeviceNotificationInd(AmsAddress sender, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders)
        {
            Logger.LogDebug(string.Format("DeviceNotificationInd(Address: {0}, InvokeId: {1}, Headers: {2})", sender, invokeId, numStapHeaders));
        }

        public override void OnAdsReadCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
            Logger.LogDebug(string.Format("ReadCon(Address: {0}, InvokeId: {1}, Result: {2}, cbLength: {3}", sender, invokeId, result, readData.Length));
        }

        public override void OnAdsReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
        {
            Logger.LogDebug(string.Format("ReadDeviceInfoCon(Address: {0}, InvokeId: {1}, Result: {2}, Name: {3}, Version: {4})", sender, invokeId, result, name, version));
        }

        public override void OnAdsReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
            Logger.LogDebug(string.Format("ReadDeviceInfoCon(Address: {0}, InvokeId: {1}, Result: {2})", sender, invokeId, result));
        }

        public override void OnAdsReadDeviceInfoInd(AmsAddress sender, uint invokeId)
        {
            Logger.LogDebug(string.Format("ReadDeviceInfoInd(Address: {0}, InvokeId: {1})", sender, invokeId));
        }

        public override void OnAdsReadInd(AmsAddress sender, uint invokeId, uint indexOffset, int cbLength)
        {
            Logger.LogDebug(string.Format("ReadInd(Address: {0}, InvokeId: {1}, Result: {2}, IO: {3}, Length: {4})", sender, invokeId, indexOffset, cbLength));
        }

        public override void OnAdsReadStateCon(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
        {
            Logger.LogDebug(string.Format("ReadStateCon(Address: {0}, InvokeId: {1}, Result: {2}, State: {3}, DeviceState: {4})", sender, invokeId, result, adsState, deviceState));
        }

        public override void OnAdsReadStateInd(AmsAddress sender, uint invokeId)
        {
            Logger.LogDebug(string.Format("ReadStateInd(Address: {0}, InvokeId: {1})", sender, invokeId));
        }

        public override void OnAdsReadWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
            Logger.LogDebug(string.Format("ReadWriteConfirmation(Address: {0}, InvokeId: {1}, Result: {2}, Length: {3})", sender, invokeId, result, readData.Length));
        }

        public override void OnAdsReadWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData)
        {
            Logger.LogDebug(string.Format("ReadWriteInd(Address: {0}, InvokeId: {1}, IG: {2}, IO: {3}, ReadLen: {4}, WriteLen: {5})", sender, invokeId, indexGroup, indexOffset, cbReadLength, writeData.Length));
        }

        public override void OnAdsWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
            Logger.LogDebug(string.Format("WriteCon(Address: {0}, InvokeId: {1}, Result: {2})", sender, invokeId, result));
        }

        public override void OnAdsWriteControlInd(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data)
        {
            Logger.LogDebug(string.Format("WriteControlInd(Address: {0}, InvokeId: {1}, AdsState: {2}, DeviceState: {3}, Length: {4})", sender, invokeId, adsState, deviceState, data.Length));
        }

        public override void OnAdsWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData)
        {
            Logger.LogDebug(string.Format("WriteInd(Address: {0}, InvokeId: {1}, IG: {2}, IO: {3}, Length: {4})", sender, invokeId, indexGroup, indexOffset, writeData.Length));
        }
    }

    /*
     * Extend the TcAdsServer class to implement your own ADS server.
     */
    public class AdsSampleServer : AdsServer
    {
        private byte[] _dataBuffer = {1, 2, 3, 4};
        private AdsState _localAdsState = AdsState.Config;
        private ushort _localDeviceState = 0;
        private Hashtable _notificationTable = new Hashtable();
        private uint _currentNotificationHandle = 0;

        private IServerLogger _serverLogger;

        /* Instanstiate an ADS server with a fix ADS port assigned by the ADS router.
        */
        public AdsSampleServer(ushort port, string portName, IServerLogger logger) : base(port, portName, logger.Logger)
        {
            _serverLogger = logger;
        }

        public AdsSampleServer(ushort port, string portName, ILogger logger) : base(port, portName, logger)
        {
            _serverLogger = new ServerLogger(logger);
        }

        /*
         * Instanstiate an ADS server with an unfixed ADS port assigned by the ADS router.
         */
        public AdsSampleServer(string portName, ILogger logger) : base(portName, logger)
        {
            _serverLogger = new ServerLogger(logger);

            // custom intialization  
            //_logger = gui;
            //base.SetLogger(gui);
            //base.logger = logger;
        }

        protected override void OnConnected()
        {
            _serverLogger.Logger.LogInformation($"Server '{this.GetType()}', Address: {base.ServerAddress} connected!");
        }


        /* Overwrite the indication methods of the TcAdsServer class for the services your ADS server
         * provides. They are called upon incoming requests. All indications that are not overwritten in
         * this class return the ADS DeviceServiceNotSupported error code to the requestor.
         * This server replys on: ReadDeviceInfo, Read, Write and ReadState requests. 
         */
        protected override Task<AdsErrorCode> ReadDeviceInfoIndicationAsync(AmsAddress rAddr, uint invokeId, CancellationToken cancel)
        {
            AdsVersion version = new AdsVersion(1, 0, 111);

            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadDeviceInfoInd(rAddr, invokeId);
            }
            // Send a response to the requester

            return ReadDeviceInfoResponseAsync(rAddr, // requestor's AMS address     
                invokeId, // invoke id provided by requestor
                AdsErrorCode.NoError, // ADS error code
                "C#_TestServer", // name of this server
                version, // version of this server
                cancel);
        }

        /// <summary>
        /// Called when an ADS Write indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Write indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="indexGroup">The index group of the requested ADS service</param>
        /// <param name="indexOffset">The index offset of the requested ADS service</param>
        /// <param name="writeData">The data to be written</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="WriteIndicationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> WriteIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            AdsErrorCode adsError = AdsErrorCode.NoError;

            if (_serverLogger != null)
            {
                _serverLogger.OnAdsWriteInd(sender, invokeId, indexGroup, indexOffset, writeData);
            }

            switch (indexGroup) /* use index group (and offset) to distinguish between the services
                                    of this server */
            {
                case 0x10000:
                    if (writeData.Length == 4)
                    {
                        writeData.CopyTo(_dataBuffer.AsMemory(0,4));
                    }
                    else
                    {
                        adsError = AdsErrorCode.DeviceInvalidParam;
                    }

                    break;
                case 0x20000: /* used for the PLC Sample */
                    if (writeData.Length == 4)
                    {
                        //BinaryReader binReader = new BinaryReader(new MemoryStream(writeData.ToArray));
                        uint value = BinaryPrimitives.ReadUInt32LittleEndian(writeData.Span.Slice(0, 4));

                        if (_serverLogger != null)
                        {
                            _serverLogger.AppendLoggerList(String.Format("PLC Counter: {0}", value));
                        }
                    }

                    break;

                default: /* other services are not supported */
                    adsError = AdsErrorCode.DeviceServiceNotSupported;
                    break;
            }

            // Send a response to the requester

            return WriteResponseAsync(sender, // requestor's AMS address   
                invokeId, // invoke id provided by requestor
                adsError, // ADS error code
                cancel);
        }

        protected override Task<AdsErrorCode> ReadIndicationAsync(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadInd(rAddr, invokeId, indexOffset, readLength);
            }
            /* Distinguish between services like in AdsWriteInd */

            // Send a response to the requester
            return ReadResponseAsync(rAddr, // requester's AMS address
                invokeId, // invoke id provided by requestor
                AdsErrorCode.NoError, // ADS error code
                _dataBuffer.AsMemory(), // data buffer
                cancel);
        }

        protected override Task<AdsErrorCode> ReadDeviceStateIndicationAsync(AmsAddress rAddr, uint invokeId, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadStateInd(rAddr, invokeId);
            }

            return ReadDeviceStateResponseAsync(rAddr, // requestor's AMS address
                invokeId, // invoke id provided by requestor
                AdsErrorCode.NoError, // ADS error code
                _localAdsState, // ADS state
                _localDeviceState, // device state
                cancel);
        }

        protected override Task<AdsErrorCode> WriteControlIndicationAsync(AmsAddress rAddr, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsWriteControlInd(rAddr, invokeId, adsState, deviceState, data);
            }

            // Set requested ADS and device status

            _localAdsState = adsState;
            _localDeviceState = deviceState;

            // Send a response to the requester

            return WriteControlResponseAsync(rAddr, // requester's AMS address
                invokeId, // invoke id provided by requester
                AdsErrorCode.NoError, // ADS error code
                cancel);
        }

        protected override Task<AdsErrorCode> AddDeviceNotificationIndicationAsync(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int length, NotificationSettings settings, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsAddDeviceNotificationInd(rAddr, invokeId, indexGroup, indexOffset, length, settings);
            }


            /* Create a new notifcation entry an store it in the notification table */
            NotificationRequestEntry notEntry = new NotificationRequestEntry(rAddr, indexGroup, indexOffset, length, settings);
            _notificationTable.Add(_currentNotificationHandle, notEntry);

            // Send a response to the requestor
            return AddDeviceNotificationResponseAsync(rAddr, invokeId, AdsErrorCode.NoError, _currentNotificationHandle++, cancel);
        }

        protected override Task<AdsErrorCode> DeleteDeviceNotificationIndicationAsync(AmsAddress rAddr, uint invokeId, uint hNotification, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsDelDeviceNotificationInd(rAddr, invokeId, hNotification);
            }

            AdsErrorCode errorCode = AdsErrorCode.NoError;

            /* check if the requested notification handle is still in the notification table */
            if (_notificationTable.Contains(hNotification))
            {
                _notificationTable.Remove(hNotification); // remove the notification handle from
                // the notification table
            }
            else // notification handle is not in the notofication table -> return an error code
                // to the requestor
            {
                errorCode = AdsErrorCode.DeviceNotifyHandleInvalid;
            }

            // Send a response to the requestor

            return DeleteDeviceNotificationResponseAsync(rAddr, invokeId, errorCode, cancel);
        }

        protected override Task<AdsErrorCode> DeviceNotificationIndicationAsync(AmsAddress address, uint invokeId, uint numStampHeaders, NotificationSamplesStamp[] stampHeaders, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsDeviceNotificationInd(address, invokeId, numStampHeaders, stampHeaders);
                _serverLogger.AppendLoggerList("Received Device Notification Request");
            }

            /*
             * Call notification handlers.
             */
            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected async override Task<AdsErrorCode> ReadWriteIndicationAsync(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte> writeData, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadWriteInd(rAddr, invokeId, indexGroup, indexOffset, readLength, writeData);
            }

            /* Distinguish between services like in AdsWriteInd */
            // Send a response to the requestor

            AdsErrorCode errorCode = AdsErrorCode.None;

            if (readLength == 4 && writeData.Length == 4)
            {
                errorCode = await ReadWriteResponseAsync(rAddr, // requester's AMS address
                invokeId, // invoke id provided by requester
                AdsErrorCode.NoError, // ADS error code
                _dataBuffer.AsMemory(), cancel).ConfigureAwait(false);
                writeData.CopyTo(_dataBuffer.AsMemory(0, 4));
            }
            else
            {
                errorCode = await ReadWriteResponseAsync(rAddr, // requestor's AMS address
                    invokeId, // invoke id provided by requestor
                    AdsErrorCode.DeviceInvalidSize, // ADS error code
                    Memory<byte>.Empty, cancel).ConfigureAwait(false);
            }

            return errorCode;
        }

        /* Overwrite the  confirmation methods of the TcAdsServer class for the requestts your ADS server
         * sends. They are called upon incoming responses. These sample implemetations only add a log message
         * to the sample form.
         */
        protected override Task<AdsErrorCode> ReadDeviceStateConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadStateCon(rAddr, invokeId, result, adsState, deviceState);
                _serverLogger.AppendLoggerList("Received Read State Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> ReadConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadCon(sender, invokeId, result, readData);
                _serverLogger.AppendLoggerList("Received Read Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> WriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsWriteCon(sender, invokeId, result);
                _serverLogger.AppendLoggerList("Received Write Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> ReadDeviceInfoConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadDeviceInfoCon(sender, invokeId, result, name, version);
                _serverLogger.AppendLoggerList("Received Read Device Info Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> WriteControlConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadDeviceInfoCon(rAddr, invokeId, result);
                _serverLogger.AppendLoggerList("Received Write Control Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> AddDeviceNotificationConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle, CancellationToken cancel)

        {
            _serverLogger.ServerNotificationHandle = notificationHandle;

            if (_serverLogger != null)
            {
                _serverLogger.OnAdsAddDeviceNotificationCon(rAddr, invokeId, result, notificationHandle);
                _serverLogger.AppendLoggerList("Received Add Device Notification Confirmation. Notification handle: " + notificationHandle);
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> DeleteDeviceNotificationConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsDelDeviceNotificationCon(rAddr, invokeId, result);
                _serverLogger.AppendLoggerList("Received Delete Device Notification Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> ReadWriteConfirmationAsync(AmsAddress address, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.OnAdsReadWriteCon(address, invokeId, result, readData);
                _serverLogger.AppendLoggerList("Received Read Write Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }
    }
}