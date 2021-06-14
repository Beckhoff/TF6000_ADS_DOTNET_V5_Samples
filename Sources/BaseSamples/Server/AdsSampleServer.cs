using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;

namespace TestServer
{
    #region SERVER_CODE_SAMPLE
    /*
     * Extend the AdsServer class to implement your own ADS server.
     */
    public class AdsSampleServer : AdsServer
    {
        /// <summary>
        /// Fixed ADS Port (to be changed ...)
        /// </summary>
        const ushort ADS_PORT = 42;

        /// <summary>
        /// Fixed Name for the ADS Port (change this ...)
        /// </summary>
        const string ADS_PORT_NAME = "AdsSampleServer_Port42";

        /// <summary>
        /// Some simple data / ProcessImage
        /// </summary>
        private byte[] _dataBuffer = {1, 2, 3, 4};

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

        /* Instanstiate an ADS server with a fix ADS port assigned by the ADS router.
        */

        public AdsSampleServer()
        : this(null)
        {
        }

        public AdsSampleServer(ILogger logger) : base(ADS_PORT, ADS_PORT_NAME)
        {
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

        /// <summary>
        /// AdsServer Version
        /// </summary>
        static AdsVersion s_version =  new AdsVersion(0,0,1);

        /* Overwrite the indication methods of the TcAdsServer class for the services your ADS server
         * provides. They are called upon incoming requests. All indications that are not overwritten in
         * this class return the ADS DeviceServiceNotSupported error code to the requester.
         */

        /// <summary>
        /// Called when an ADS Read Device Info indication is received by your <see cref="AdsSampleServer"/>.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read Device Info indications.
        /// The default implementation replies with an <see AdsErrorCode.DeviceServiceNotSupported></see> error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's / requester's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="OnReadDeviceInfoIndicationAsync"/> operation. The <see cref="Task{T}"/> parameter contains the <see cref="AdsErrorCode"/> as
        /// <see cref="Task{Task}.Result"/>.
        /// </returns>
        protected override Task<AdsErrorCode> ReadDeviceInfoIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            LogTrace("ReadDeviceINfoIndication(Address:{0}, ID: {1})", sender, invokeId);
            
            // Send a response to the requester
            return ReadDeviceInfoResponseAsync(sender,   // requester's AMS address     
                invokeId,                               // invoke id provided by requester
                AdsErrorCode.NoError,                   // ADS error code
                "C#_TestServer",                        // name of this server
                s_version,                              // version of this server
                cancel);                                // Cancellation Token
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

            LogTrace("WriteIndicationAsync(Address:{0}, ID:{1}, IG:{2}, IO:{3}, Length:{4})", sender, invokeId, indexGroup, indexOffset, writeData.Length);

            switch (indexGroup) /* use index group (and offset) to distinguish between the services
                                    of this server */
            {
                case 0x10000:
                    if (writeData.Length == 4)
                    {
                        writeData.CopyTo(_dataBuffer);
                    }
                    else
                    {
                        adsError = AdsErrorCode.DeviceInvalidParam;
                    }

                    break;
                case 0x20000: /* used for the PLC Sample */
                    if (writeData.Length == 4)
                    {
                        uint value = BinaryPrimitives.ReadUInt32LittleEndian(writeData.Span);
                        LogInformation(String.Format("PLC Counter: {0}", value));
                    }

                    break;

                default: /* other services are not supported */
                    adsError = AdsErrorCode.DeviceServiceNotSupported;
                    break;
            }

            // Send a response to the requester

            return WriteResponseAsync(sender, // requester's AMS address   
                invokeId, // invoke id provided by requester
                adsError, // ADS error code
                cancel);
        }

        /// <summary>
        /// Called when an ADS Read indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="indexGroup">The index group of the requested ADS service</param>
        /// <param name="indexOffset">The index offset of the requested ADS service</param>
        /// <param name="readLength">The number of bytes to be read</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="ReadIndicationAsync"/> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> ReadIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            LogTrace("ReadIndicationAsync(Address:{0}, ID:{1}, IG:{2}, IO:{3}, Length:{4})", sender, invokeId, indexGroup, indexOffset, readLength);

            /* Distinguish between services like in AdsWriteInd */

            // Send a response to the requester
            return ReadResponseAsync(sender, // requester's AMS address
                invokeId,                   // invoke id provided by requester
                AdsErrorCode.NoError,       // ADS error code
                _dataBuffer.AsMemory(),     // data buffer
                cancel);
        }

        /// <summary>
        /// Called when an ADS Read State indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read State indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="ReadDeviceStateIndicationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> ReadDeviceStateIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            LogTrace("ReadIndicationAsync(Address:{0}, ID:{1}, IG:{2}, IO:{3}, Length:{4})", sender, invokeId);

            return ReadDeviceStateResponseAsync(sender, // requester's AMS address
                invokeId,                               // invoke id provided by requester
                AdsErrorCode.NoError,                   // ADS error code
                _adsState,                              // ADS state
                _deviceState,                           // device state
                cancel);
        }

        /// <summary>
        /// Called when an ADS Write Control indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Write Control indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="adsState">The requested new ADS state of this ADS device</param>
        /// <param name="deviceState">The requested new device state of this ADS device</param>
        /// <param name="cbLength">The length in bytes of the additional data buffer</param>
        /// <param name="data">An additional data buffer of cbLength bytes</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="WriteControlIndicationAsync"/> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> WriteControlIndicationAsync(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            LogTrace("WriteControlIndication(Address:{0}, ID:{1}, AdsState:{2}, DeviceState:{3}, Length:{4})", sender, invokeId, adsState, deviceState, data.Length);

            // Set requested ADS and device status

            _adsState = adsState;
            _deviceState = deviceState;

            // Send a response to the requester

            return WriteControlResponseAsync(sender,    // requester's AMS address
                invokeId,                               // invoke id provided by requester
                AdsErrorCode.NoError,                   // ADS error code
                cancel);
        }

        /// <summary>
        /// Called when an ADS Add Device Notification indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Add Device Notification indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="indexGroup">The index group of the requested ADS service</param>
        /// <param name="indexOffset">The index offset of the requested ADS service</param>
        /// <param name="dataLength">Number of bytes to be transmitted</param>
        /// <param name="settings">The Notification settings.</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'AddDeviceNotificationIndication' operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> AddDeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int dataLength, NotificationSettings settings, CancellationToken cancel)
        {
            LogTrace("AddDeviceNotificationIndication(Address:{0}, ID:{1}, IG:{2}, IO:{3}, Length:{4})", sender, invokeId, indexGroup, indexOffset, dataLength);

            /* Create a new notification entry an store it in the notification table */
            NotificationRequestEntry notEntry = new NotificationRequestEntry(sender, indexGroup, indexOffset, dataLength, settings);

            _notificationTable.AddOrUpdate(_currentNotificationHandle, notEntry,(key,value) => notEntry);
            _currentNotificationHandle++;

            // Send a response to the requester
            return AddDeviceNotificationResponseAsync(sender, invokeId, AdsErrorCode.NoError, _currentNotificationHandle++, cancel);
        }

        /// <summary>
        /// Called when an ADS Delete Device Notification indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Delete Device Notification indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="hNotification">The notification handle to be deleted</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="DeleteDeviceNotificationIndicationAsync"/> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> DeleteDeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint hNotification, CancellationToken cancel)
        {
            LogTrace("DeleteDeviceNotification(Address:{0}, ID:{1}, Handle:{2})", sender, invokeId, hNotification);

            AdsErrorCode errorCode = AdsErrorCode.NoError;

            /* check if the requested notification handle is still in the notification table */
            if (_notificationTable.ContainsKey(hNotification))
            {
                NotificationRequestEntry entry = null;
                _notificationTable.TryRemove(hNotification, out entry);
            }
            else // notification handle is not in the notification table -> return an error code
                // to the requester
            {
                errorCode = AdsErrorCode.DeviceNotifyHandleInvalid;
            }

            // Send a response to the requester
            return DeleteDeviceNotificationResponseAsync(sender, invokeId, errorCode, cancel);
        }

        /// <summary>
        /// Called when an ADS Device Notification indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Device Notification indications.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="numStampHeaders">The number of ADS Stamp Headers contained in stampHeaders</param>
        /// <param name="stampHeaders">The array of received ADS Stamp Headers.</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="DeviceNotificationIndicationAsync(AmsAddress, uint, uint, NotificationSamplesStamp[], CancellationToken)"/> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        /// <exclude/>
        protected override Task<AdsErrorCode> DeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint numStampHeaders, NotificationSamplesStamp[] stampHeaders, CancellationToken cancel)
        {
            LogTrace("DeviceNotificationIndication(Address:{0}, ID:{1}, NumStampHeaders:{2})", sender, invokeId, numStampHeaders);
            LogInformation("Received Device Notification Request");

            /*
             * Call notification handlers.
             */
            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Read Write indication is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read Write indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invokeId provided by the sender</param>
        /// <param name="indexGroup">The index group of the requested ADS service</param>
        /// <param name="indexOffset">The index offset of the requested ADS service</param>
        /// <param name="readLength">Number of bytes to be read</param>
        /// <param name="writeData">The data to be written</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'ReadWriteIndication' operation. The <see cref="Task{T}"/> parameter contains the <see cref="AdsErrorCode"/> as
        /// <see cref="Task{Task}.Result"/>.
        /// </returns>
        protected async override Task<AdsErrorCode> ReadWriteIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            LogTrace("ReadWriteIndication(Address:{0}, ID:{1}, IG:{2}, IO:{3}, ReadLen:{4}, WriteLen:{5})", sender, invokeId, indexGroup, indexOffset, readLength,writeData.Length);

            /* Distinguish between services like in AdsWriteInd */
            // Send a response to the requester

            AdsErrorCode errorCode = AdsErrorCode.None;

            if (readLength == 4 && writeData.Length == 4)
            {
                errorCode = await ReadWriteResponseAsync(sender, // requester's AMS address
                    invokeId, // invoke id provided by requester
                    AdsErrorCode.NoError, // ADS error code
                    _dataBuffer.AsMemory(), cancel).ConfigureAwait(false);
                    writeData.CopyTo(_dataBuffer.AsMemory(0, 4));
            }
            else
            {
                errorCode = await ReadWriteResponseAsync(sender, // requester's AMS address
                    invokeId, // invoke id provided by requester
                    AdsErrorCode.DeviceInvalidSize, // ADS error code
                    Memory<byte>.Empty, cancel).ConfigureAwait(false);
            }

            return errorCode;
        }

        /* Overwrite the  confirmation methods of the TcAdsServer class for the requests your ADS server
         * sends. They are called upon incoming responses. These sample implementations only add a log message
         * to the sample form.
        */

        /// <summary>
        /// Called when an ADS Read State confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read State confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="adsState">The ADS state of the sender</param>
        /// <param name="deviceState">The device state of the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="ReadDeviceStateConfirmationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> ReadDeviceStateConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState, CancellationToken cancel)
        {
            LogTrace("ReadDeviceStateConfirmation(Address:{0}, ID:{1}, Result:{2}, AdsState:{3}, DeviceSTate:{4})", sender, invokeId, result, adsState, deviceState);
            LogInformation("Received Read State Confirmation");
            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Read confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="cbLength">The number of read bytes</param>
        /// <param name="data">The read data buffer</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="ReadConfirmationAsync"/> operation. The <see cref="Task{T}"/> parameter contains the <see cref="AdsErrorCode"/> as
        /// <see cref="Task{Task}.Result"/>.
        /// </returns>
        protected override Task<AdsErrorCode> ReadConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)
        {
            LogTrace("ReadConfirmation(Address:{0}, ID:{1}, IG:{2}, Result:{3}, Length:{4})", sender, invokeId, result, readData.Length);
            LogInformation("Received Read Confirmation");

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Write confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Write confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="WriteConfirmationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> WriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        {
            LogTrace("WriteConfirmation(Address:{0}, ID:{1}, Result:{2})", sender, invokeId, result);
            LogInformation("Received Write Confirmation");

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Read Device Info confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read Device Info confirmations.
        /// </remarks>
        /// <param name="target">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="name">The sender's name</param>
        /// <param name="version">The sender's version</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="ReadDeviceInfoConfirmationAsync(AmsAddress, uint, AdsErrorCode, string, AdsVersion, CancellationToken)" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> ReadDeviceInfoConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version, CancellationToken cancel)
        {
            LogTrace("ReadDeviceInfoConfirmation(Address:{0}, ID:{1}, Result:{2}, Name:{3}, Version:{4})", sender, invokeId, result, name, version);
            LogInformation("Received Read Device Info Confirmation");

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Write Control confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Write Control confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="WriteControlConfirmationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> WriteControlConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        {
            LogTrace("WriteControlConfirmation(Address:{0}, ID:{1}, Result:{2})", sender, invokeId, result);
            LogInformation("Received Write Control Confirmation");

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Add Device Notification confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Add Device Notification confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="notificationHandle">The notification handle provided by the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="AddDeviceNotificationConfirmationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> AddDeviceNotificationConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, uint notificationHandle, CancellationToken cancel)
        {
            //_serverLogger.ServerNotificationHandle = notificationHandle;

            LogTrace("AddDeviceNotificationConfirmation(Address:{0}, ID:{1}, Result:{2}, Handle:{3})", sender, invokeId, result,notificationHandle);
            LogInformation("Received Add Device Notification Confirmation. Notification handle: " + notificationHandle);

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Delete Device Notification confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Delete Device Notification confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="DeleteDeviceNotificationConfirmationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> DeleteDeviceNotificationConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        {
            LogTrace("DeleteDeviceNotificationConfirmation(Address:{0}, ID:{1}, Result:{2})", sender, invokeId, result);
            LogInformation("Received Delete Device Notification Confirmation");

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        /// <summary>
        /// Called when an ADS Read Write confirmation is received.
        /// </summary>
        /// <remarks>
        /// Overwrite this method in derived classes to react on ADS Read Write confirmations.
        /// </remarks>
        /// <param name="sender">The sender's AMS address</param>
        /// <param name="invokeId">The invoke id provided by this server during the corresponding request</param>
        /// <param name="result">The ADS error code provided by the sender</param>
        /// <param name="cbLength">The  number of read bytes</param>
        /// <param name="data">The read data buffer</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="ReadWriteConfirmationAsync" /> operation. The <see cref="Task{T}" /> parameter contains the <see cref="AdsErrorCode" /> as
        /// <see cref="Task{Task}.Result" />.</returns>
        protected override Task<AdsErrorCode> ReadWriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)
        {
            LogTrace("AddDeviceNotificationIndication(Address:{0}, ID:{1}, Result:{2}, Length:{3})", sender, invokeId, result, readData.Length);
            LogInformation("Received Read Write Confirmation");
            return Task.FromResult(AdsErrorCode.Succeeded);
        }
    }

    /// <summary>
    /// AdsSampleServer Notification request entry
    /// </summary>
    internal class NotificationRequestEntry
    {
        private AmsAddress _rAddr;      // the AmsNetId of the requester
        private uint _indexGroup;       // the requested index group
        private uint _indexOffset;      // the requested index offset
        private int _cbLength;         // the number of bytes to send
        NotificationSettings _settings; // the notification settings

        internal NotificationRequestEntry(AmsAddress rAddr,
                                          uint indexGroup,
                                          uint indexOffset,
                                          int cbLength,
                                          NotificationSettings settings)
        {
            _rAddr = rAddr;
            _indexGroup = indexGroup;
            _indexOffset = indexOffset;
            _cbLength = cbLength;
            _settings = settings;
        }
    }
    #endregion
}