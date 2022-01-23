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
        //public AdsSampleServer(ushort port, string portName, ILogger logger) : base(port, portName, logger)
        //{
        //    _serverLogger = new ServerLogger(logger)
        //    //_serverLogger = logger;
        //}

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
                _serverLogger.LogReadDeviceInfoInd(rAddr, invokeId);
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
                _serverLogger.LogWriteInd(sender, invokeId, indexGroup, indexOffset, writeData);
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
                            _serverLogger.Log(String.Format("PLC Counter: {0}", value));
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
                _serverLogger.LogReadInd(rAddr, invokeId, indexOffset, readLength);
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
                _serverLogger.LogReadStateInd(rAddr, invokeId);
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
                _serverLogger.LogWriteControlInd(rAddr, invokeId, adsState, deviceState, data);
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
                _serverLogger.LogAddDeviceNotificationInd(rAddr, invokeId, indexGroup, indexOffset, length, settings);
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
                _serverLogger.LogDelDeviceNotificationInd(rAddr, invokeId, hNotification);
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
                _serverLogger.LogDeviceNotificationInd(address, invokeId, numStampHeaders, stampHeaders);
                _serverLogger.Log("Received Device Notification Request");
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
                _serverLogger.LogReadWriteInd(rAddr, invokeId, indexGroup, indexOffset, readLength, writeData);
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
                _serverLogger.LogReadStateCon(rAddr, invokeId, result, adsState, deviceState);
                _serverLogger.Log("Received Read State Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> ReadConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.LogReadCon(sender, invokeId, result, readData);
                _serverLogger.Log("Received Read Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> WriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.LogWriteCon(sender, invokeId, result);
                _serverLogger.Log("Received Write Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> ReadDeviceInfoConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version, CancellationToken cancel)
        {
            if (_serverLogger != null)
            {
                _serverLogger.LogReadDeviceInfoCon(sender, invokeId, result, name, version);
                _serverLogger.Log("Received Read Device Info Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> WriteControlConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.LogReadDeviceInfoCon(rAddr, invokeId, result);
                _serverLogger.Log("Received Write Control Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> AddDeviceNotificationConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle, CancellationToken cancel)

        {
            //_serverLogger.ServerNotificationHandle = notificationHandle;

            if (_serverLogger != null)
            {
                _serverLogger.LogAddDeviceNotificationCon(rAddr, invokeId, result, notificationHandle);
                _serverLogger.Log("Received Add Device Notification Confirmation. Notification handle: " + notificationHandle);
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> DeleteDeviceNotificationConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.LogDelDeviceNotificationCon(rAddr, invokeId, result);
                _serverLogger.Log("Received Delete Device Notification Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        protected override Task<AdsErrorCode> ReadWriteConfirmationAsync(AmsAddress address, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)

        {
            if (_serverLogger != null)
            {
                _serverLogger.LogReadWriteCon(address, invokeId, result, readData);
                _serverLogger.Log("Received Read Write Confirmation");
            }

            return Task.FromResult(AdsErrorCode.Succeeded);
        }

        uint invokeId = 0;

        public Task<AdsErrorCode> TriggerReadDeviceInfoRequestAsync(AmsAddress target, CancellationToken cancel)
        {
            return base.ReadDeviceInfoRequestAsync(target, invokeId++, cancel);
        }

        public Task<AdsErrorCode> TriggerReadRequestAsync(AmsAddress target, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            return ReadRequestAsync(target, invokeId++, indexGroup, indexOffset, readLength, cancel);
        }

        public Task<AdsErrorCode> TriggerWriteRequestAsync(AmsAddress target, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            return WriteRequestAsync(target, invokeId++, indexGroup, indexOffset, data, cancel);
        }

        public Task<AdsErrorCode> TriggerReadStateRequestAsync(AmsAddress target, CancellationToken cancel)
        {
            return ReadDeviceStateRequestAsync(target, invokeId++, cancel);
        }

        public Task<AdsErrorCode> TriggerWriteControlRequestAsync(AmsAddress target, AdsState state, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            return WriteControlRequestAsync(target, invokeId++, state, deviceState, data, cancel);
        }

        public Task<AdsErrorCode> TriggerAddDeviceNotificationRequestAsync(AmsAddress target, uint indexGroup, uint indexOffset, int dataLength, NotificationSettings settings, CancellationToken cancel)
        {
            return AddDeviceNotificationRequestAsync(target, invokeId++, indexGroup, indexOffset, dataLength, settings, cancel);
        }

        public Task<AdsErrorCode> TriggerDeleteDeviceNotificationRequestAsync(AmsAddress target, uint handle, CancellationToken cancel)
        {
            return DeleteDeviceNotificationRequestAsync(target, invokeId++, handle, cancel);
        }

        public Task<AdsErrorCode> TriggerReadWriteRequestAsync(AmsAddress target, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte>data, CancellationToken cancel)
        {
            return ReadWriteRequestAsync(target, invokeId++, indexGroup, indexOffset, readLength, data, cancel);
        }
    }
}