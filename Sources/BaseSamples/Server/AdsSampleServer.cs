using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;

namespace S60_Server
{
    /*
     * Extend the AdsServer class to implement your own ADS server.
     */
    public class AdsSampleServer : AdsServer
    {
        /// <summary>
        /// Fixed ADS Port (to be changed ...)
        /// </summary>
        /// <remarks>
        /// User Server Ports must be in between
        /// AmsPortRange.CUSTOMER_FIRST (25000) <= PORT <= AmsPort.CUSTOMER_LAST (25999)
        /// or
        /// AmsPortRange.CUSTOMERPRIVATE_FIRST (26000) <= PORT <= AmsPort.CUSTOMERPRIVATE_LAST (26999)
        /// to not conflict with Beckhoff prereserved servers!
        /// see https://infosys.beckhoff.com/content/1033/tc3_ads.net/9408352011.html?id=1801810347107555608
        /// </remarks>
        const ushort ADS_PORT = 26000;

        /// <summary>
        /// Fixed Name for the ADS Port (change this ...)
        /// </summary>
        const string ADS_PORT_NAME = "AdsSampleServer_Port25000";

        /// <summary>
        /// Some simple data / ProcessImage
        /// </summary>
        private byte[] _dataBuffer = { 1, 2, 3, 4 };

        /// <summary>
        /// Ads State
        /// </summary>
        private AdsState _adsState = AdsState.Config;
        /// <summary>
        /// Device State
        /// </summary>
        private ushort _deviceState = 0;

        /// <summary>
        /// Notification dictionary, thread safe
        /// </summary>
        private ConcurrentDictionary<uint, NotificationRequestEntry> _notificationTable = new ConcurrentDictionary<uint, NotificationRequestEntry>();

        /// <summary>
        /// Simple counter for different Notification handles here.
        /// </summary>
        private uint _currentNotificationHandle = 0;

        /// <summary>
        /// Logger
        /// </summary>
        private ILogger _logger;

        /* Instantiate an ADS server with a fix ADS port assigned by the ADS router.
        */

        public AdsSampleServer()
        : this(null)
        {
        }

        public AdsSampleServer(ILogger logger) : base(ADS_PORT, ADS_PORT_NAME)
        {
            base.serverVersion = new Version(0, 0, 1);
            _logger = logger;
        }

        /// <summary>
        /// Trace log message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        private void LogTrace(string message, params object[] args)
        {
            if (_logger != null)
                _logger.LogTrace(message, args);
        }

        /// <summary>
        /// Information Log
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        private void LogInformation(string message, params object[] args)
        {
            if (_logger != null)
                _logger.LogInformation(message, args);
        }

        /// <summary>
        /// Error log
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        private void LogError(string message, params object[] args)
        {
            if (_logger != null)
                _logger.LogError(message, args);
        }

        /* Overwrite the indication handlers of the AdsServer class for the services your ADS server
         * provides. They are called upon incoming requests. Indications that are not overwritten in
         * this class return the ADS DeviceServiceNotSupported error code to the requester.
         */

        /* Handler function for Write Indication */
        protected override Task<ResultWrite> OnWriteAsync(AmsAddress address, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            LogTrace($"OnWriteAsync(IG:{indexGroup}, IO:{indexOffset}, Length:{writeData.Length})");

            ResultWrite result;

            switch (indexGroup)     /* use index group (and offset) to distinguish between the services
                                    of this server */
            {
                case 0x10000:
                    if (writeData.Length == 4)
                    {
                        writeData.CopyTo(_dataBuffer);
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidSize);
                    }
                    break;

                case 0x20000: /* used for the PLC Sample */
                    if (writeData.Length == 4)
                    {
                        uint value = BinaryPrimitives.ReadUInt32LittleEndian(writeData.Span);
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidSize);
                    }

                    break;

                default: /* other services are not supported */
                    result = ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported);
                    break;
            }
            return Task.FromResult(result);
        }

        /* Handler function for Read Indication */
        protected override Task<ResultReadBytes> OnReadAsync(AmsAddress address, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            LogTrace($"OnReadAsync(IG:{indexGroup}, IO:{indexOffset}, Length:{readLength})");
            ResultReadBytes result;

            /* Distinguish between services like in OnWriteAsync */
            result = ResultReadBytes.CreateSuccess(_dataBuffer.AsMemory());

            // or Error with ErrorCode
            // result = ResultReadBytes.CreateError(AdsErrorCode.DeviceNotSupported);
            return Task.FromResult(result);
        }

        /* Handler function for ReadWrite Indication */
        protected override Task<ResultReadWriteBytes> OnReadWriteAsync(AmsAddress address, uint invokeId, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            LogTrace($"OnReadWriteAsync(IG:{indexGroup}, IO:{indexOffset}, ReadLen:{readLength}, WriteLen:{writeData.Length})");

            ResultReadWriteBytes result;

            /* Distinguish between services like in AdsWriteInd */

            if (readLength == 4 && writeData.Length == 4)
            {
                result = ResultReadWriteBytes.CreateSuccess(_dataBuffer.AsMemory(0, 4));
            }
            else
            {
                result = ResultReadWriteBytes.CreateError(AdsErrorCode.DeviceInvalidSize);
            }

            return Task.FromResult(result);
        }

        /* Handler function for ReadDeviceState Indication */
        protected override Task<ResultReadDeviceState> OnReadDeviceStateAsync(AmsAddress address, uint invokeId, CancellationToken cancel)
        {
            LogTrace("OnReadDeviceStateAsync()");

            StateInfo state = new StateInfo(_adsState, _deviceState);
            ResultReadDeviceState result = ResultReadDeviceState.CreateSuccess(state);
            return Task.FromResult(result);
        }

        /* Handler function for WriteControl Indication */
        protected override Task<ResultAds> OnWriteControlAsync(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            LogTrace($"OnWriteControlAsync(AdsState:{adsState}, DeviceState:{deviceState}, Length:{data.Length})");

            // Set requested ADS and device status

            _adsState = adsState;
            _deviceState = deviceState;

            ResultAds result = ResultAds.CreateSuccess();
            return Task.FromResult(result);
        }

        /* Handler function for AddDeviceNotification Indication */
        protected override Task<ResultHandle> OnAddDeviceNotificationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int dataLength, AmsAddress receiver, NotificationSettings settings, CancellationToken cancel)
        {
            LogTrace($"OnAddDeviceNotificationAsync(IG:{indexGroup}, IO:{indexOffset}, Length:{dataLength}");

            /* Create a new notification entry an store it in the notification table */
            NotificationRequestEntry notEntry = new NotificationRequestEntry(receiver, indexGroup, indexOffset, dataLength, settings);

            _notificationTable.AddOrUpdate(_currentNotificationHandle, notEntry, (key, value) => notEntry);
            ResultHandle result = ResultHandle.CreateSuccess(_currentNotificationHandle);

            _currentNotificationHandle++;
            return Task.FromResult(result);

        }

        /* Handler function for DeleteDeviceNotification Indication */
        protected override Task<ResultAds> OnDeleteDeviceNotificationAsync(AmsAddress sender, uint invokeId, uint hNotification, CancellationToken cancel)
        {
            LogTrace("OnDeleteDeviceNotificationAsync(Handle:{2})", hNotification);

            ResultAds result;

            /* check if the requested notification handle is still in the notification table */
            if (_notificationTable.ContainsKey(hNotification))
            {
                NotificationRequestEntry entry = null;
                _notificationTable.TryRemove(hNotification, out entry);
                result = ResultAds.CreateSuccess();
            }
            else // notification handle is not in the notification table -> return an error code
                 // to the requester
            {
                result = ResultAds.CreateError(AdsErrorCode.DeviceNotifyHandleInvalid);
            }

            return Task.FromResult(result);
        }

        /* Handler function for DeviceNotification Indication */
        protected override Task<ResultAds> OnDeviceNotificationAsync(AmsAddress sender, NotificationSamplesStamp[] stampHeaders, CancellationToken cancel)
        {
            LogTrace($"OnDeviceNotificationAsync(Address:{sender}, NumStampHeaders:{stampHeaders.Length})");
            LogInformation("Received Device Notification Request");

            /*
             * Call notification handlers.
             */
            return Task.FromResult(ResultAds.CreateSuccess());
        }
        
        /* Overwrite the  confirmation methods of the AdsServer class for the requests your ADS server
         * sends. They are called upon incoming responses. These sample implementations only add a log message
         * to the default implementation returning AdsErrorCode.Succeeded
         *
         * They are not necessary for implementing an AdsServer!
        */

        //protected override Task<AdsErrorCode> OnReadDeviceStateConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState, CancellationToken cancel)
        //{
        //    LogTrace("ReadDeviceStateConfirmation(Address:{0}, ID:{1}, Result:{2}, AdsState:{3}, DeviceState:{4})", sender, invokeId, result, adsState, deviceState);
        //    LogInformation("Received Read State Confirmation");
        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnReadConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)
        //{
        //    LogTrace("ReadConfirmation(Address:{0}, ID:{1}, IG:{2}, Result:{3}, Length:{4})", sender, invokeId, result, readData.Length);
        //    LogInformation("Received Read Confirmation");

        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnWriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        //{
        //    LogTrace("WriteConfirmation(Address:{0}, ID:{1}, Result:{2})", sender, invokeId, result);
        //    LogInformation("Received Write Confirmation");

        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnReadDeviceInfoConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version, CancellationToken cancel)
        //{
        //    LogTrace("ReadDeviceInfoConfirmation(Address:{0}, ID:{1}, Result:{2}, Name:{3}, Version:{4})", sender, invokeId, result, name, version);
        //    LogInformation("Received Read Device Info Confirmation");

        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnWriteControlConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        //{
        //    LogTrace("WriteControlConfirmation(Address:{0}, ID:{1}, Result:{2})", sender, invokeId, result);
        //    LogInformation("Received Write Control Confirmation");

        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnAddDeviceNotificationConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, uint notificationHandle, CancellationToken cancel)
        //{
        //    //_serverLogger.ServerNotificationHandle = notificationHandle;

        //    LogTrace("AddDeviceNotificationConfirmation(Address:{0}, ID:{1}, Result:{2}, Handle:{3})", sender, invokeId, result, notificationHandle);
        //    LogInformation("Received Add Device Notification Confirmation. Notification handle: " + notificationHandle);

        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnDeleteDeviceNotificationConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        //{
        //    LogTrace("DeleteDeviceNotificationConfirmation(Address:{0}, ID:{1}, Result:{2})", sender, invokeId, result);
        //    LogInformation("Received Delete Device Notification Confirmation");

        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}

        //protected override Task<AdsErrorCode> OnReadWriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)
        //{
        //    LogTrace("AddDeviceNotificationIndication(Address:{0}, ID:{1}, Result:{2}, Length:{3})", sender, invokeId, result, readData.Length);
        //    LogInformation("Received Read Write Confirmation");
        //    return Task.FromResult(AdsErrorCode.Succeeded);
        //}
    }

    /// <summary>
    /// AdsSampleServer Notification request entry
    /// </summary>
    internal class NotificationRequestEntry
    {
        private AmsAddress _sender;     // the AmsNetId of the requester
        private uint _indexGroup;       // the requested index group
        private uint _indexOffset;      // the requested index offset
        private int _length;            // the number of bytes to send
        NotificationSettings _settings; // the notification settings

        internal NotificationRequestEntry(AmsAddress sender,
                                          uint indexGroup,
                                          uint indexOffset,
                                          int dataLength,
                                          NotificationSettings settings)
        {
            _sender = sender;
            _indexGroup = indexGroup;
            _indexOffset = indexOffset;
            _length = dataLength;
            _settings = settings;
        }
    }
}