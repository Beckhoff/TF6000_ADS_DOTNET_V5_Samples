using System;
using System.Collections;
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

        //private IServerLogger _serverLogger;
        private ILogger _logger;

        /* Instanstiate an ADS server with a fix ADS port assigned by the ADS router.
        */
        //public AdsSampleServer(ushort port, string portName, ILogger logger) : base(port, portName, logger)
        //{
        //    _serverLogger = new ServerLogger(logger)
        //    //_serverLogger = logger;
        //}

        public AdsSampleServer(ushort port, string portName, ILoggerFactory loggerFactory) : base(port, portName, null)
        {
            _logger = loggerFactory.CreateLogger(portName);
            //_serverLogger = new ServerLogger(logger);
        }

        /*
         * Instanstiate an ADS server with an unfixed ADS port assigned by the ADS router.
         */
        public AdsSampleServer(string portName, ILoggerFactory loggerFactory) : base(portName, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AdsSampleServer>();
            //_serverLogger = new ServerLogger(logger);

            // custom intialization  
            //_logger = gui;
            //base.SetLogger(gui);
            //base.logger = logger;
        }

        protected override void OnConnected()
        {
            _logger.LogInformation("Server '{Server}', Address: {Address} connected!",nameof(AdsSampleServer),this.ServerAddress);
        }

        /* Overwrite the indication methods of the AdsServer class for the services your ADS server
         * provides. They are called upon incoming requests. All indications that are not overwritten in
         * this class return the ADS DeviceServiceNotSupported error code to the requestor.
         * This server replys on: ReadDeviceInfo, Read, Write and ReadState requests. 
         */

        protected override Task<ResultWrite> OnWriteAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            ResultWrite result = ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported);

            switch (indexGroup) /* use index group (and offset) to distinguish between the services
                                    of this server */
            {
                case 0x10000:
                    if (writeData.Length == 4)
                    {
                        writeData.CopyTo(_dataBuffer.AsMemory(0, 4));
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidParam);
                    }
                    break;
                case 0x20000: /* used for the PLC Sample */
                    if (writeData.Length == 4)
                    {
                        uint value = BinaryPrimitives.ReadUInt32LittleEndian(writeData.Span.Slice(0, 4));
                        _logger.LogInformation("PLC Counter: {Count}", value);
                        result = ResultWrite.CreateSuccess();
                    }
                    break;

                default: /* other services are not supported */
                    result = ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported);
                    break;
            }
            return Task.FromResult(result);
        }

        
        protected override Task<ResultReadBytes> OnReadAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            /* Distinguish between services like in AdsWriteInd */

            ResultReadBytes result = ResultReadBytes.CreateSuccess(_dataBuffer.AsMemory());
            return Task.FromResult(result);
        }

        protected override Task<ResultReadDeviceState> OnReadDeviceStateAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            ResultReadDeviceState result = ResultReadDeviceState.CreateSuccess(new StateInfo(_localAdsState,_localDeviceState));
            return Task.FromResult(result);
        }

        protected override Task<ResultAds> OnWriteControlAsync(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            // Set requested ADS and device status
            _localAdsState = adsState;
            _localDeviceState = deviceState;

            // Send a response to the requester
            ResultAds result = ResultAds.CreateSuccess();
            return Task.FromResult(result);
        }

        protected override Task<ResultHandle> OnAddDeviceNotificationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int dataLength, AmsAddress receiver, NotificationSettings settings, CancellationToken cancel)
        {
            /* Create a new notifcation entry an store it in the notification table */
            NotificationRequestEntry notEntry = new NotificationRequestEntry(receiver, indexGroup, indexOffset, dataLength, settings);
            _notificationTable.Add(_currentNotificationHandle, notEntry);

            ResultHandle result = ResultHandle.CreateSuccess(_currentNotificationHandle++);
            return Task.FromResult(result);
        }

        protected override Task<ResultAds> OnDeleteDeviceNotificationAsync(AmsAddress sender, uint invokeId, uint hNotification, CancellationToken cancel)
        {
            ResultAds result = ResultAds.CreateSuccess();

            /* check if the requested notification handle is still in the notification table */
            if (_notificationTable.Contains(hNotification))
            {
                _notificationTable.Remove(hNotification); // remove the notification handle from
                // the notification table
            }
            else // notification handle is not in the notofication table -> return an error code
                 // to the requestor
            {
                result = ResultAds.CreateError(AdsErrorCode.DeviceNotifyHandleInvalid);
            }
            return Task.FromResult(result);
        }

        protected override Task<ResultAds> OnDeviceNotificationAsync(AmsAddress sender, NotificationSamplesStamp[] stampHeaders, CancellationToken cancel)
        {
            /*
             * Call notification handlers.
             */
            return Task.FromResult(ResultAds.CreateSuccess());
        }

        protected override Task<ResultReadWriteBytes> OnReadWriteAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            ResultReadWriteBytes result = ResultReadWriteBytes.CreateError(AdsErrorCode.DeviceServiceNotSupported);

            /* Distinguish between services like in AdsWriteInd */
            // Send a response to the requestor

            if (readLength == 4 && writeData.Length == 4)
            {
                result = ResultReadWriteBytes.CreateSuccess(_dataBuffer.AsMemory());
                writeData.CopyTo(_dataBuffer.AsMemory(0, 4));
            }
            else
            {
                result = ResultReadWriteBytes.CreateError(AdsErrorCode.DeviceInvalidSize);
            }

            return Task.FromResult(result);
        }


        /* Overwritten indication methods of the AdsServer class for logging incoming request indications.
         * They are called upon incoming requests. These sample implemetations only add a log message
         * to the sample form.
         * 
         * In common, it is not necessary to overload the virtual Confirmation methods!
        */

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete
        protected override Task<AdsErrorCode> WriteIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            _logger.LogDebug($"WriteInd(Address: {sender}, InvokeId: {invokeId}, IG: {indexGroup}, IO: {indexOffset}, Length: {writeData.Length})");
            return base.WriteIndicationAsync(sender, invokeId, indexGroup, indexOffset, writeData, cancel);
        }

        protected override Task<AdsErrorCode> ReadIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            _logger.LogDebug($"ReadInd(Address: {sender}, InvokeId: {invokeId}, IO: {indexOffset}, Length: {readLength})");
            return base.ReadIndicationAsync(sender, invokeId, indexGroup, indexOffset, readLength, cancel);
        }

        protected override Task<AdsErrorCode> ReadDeviceInfoIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            _logger.LogDebug($"ReadDeviceInfoInd(Address: {sender}, InvokeId: {invokeId})");
            return base.ReadDeviceInfoIndicationAsync(sender, invokeId, cancel);
        }

        protected override Task<AdsErrorCode> ReadDeviceStateIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            _logger.LogDebug($"ReadStateInd(Address: {sender}, InvokeId: {invokeId})");
            return base.ReadDeviceStateIndicationAsync(sender, invokeId, cancel);
        }

        protected override Task<AdsErrorCode> WriteControlIndicationAsync(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            _logger.LogDebug($"WriteControlInd(Address: {sender}, InvokeId: {invokeId}, AdsState: {adsState}, DeviceState: {deviceState}, Length: {data.Length})");
            return base.WriteControlIndicationAsync(sender, invokeId, adsState, deviceState, data, cancel);
        }

        protected override Task<AdsErrorCode> AddDeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int length, NotificationSettings settings, CancellationToken cancel)
        {
            _logger.LogDebug($"AddDeviceNotificationInd(Address: {sender}, InvokeId: {invokeId}, IG: {indexGroup}, IO: {indexOffset}, Len: {length})");
            return base.AddDeviceNotificationIndicationAsync(sender, invokeId, indexGroup, indexOffset, length, settings, cancel);
        }

        protected override Task<AdsErrorCode> DeleteDeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint hNotification, CancellationToken cancel)
        {
            _logger.LogDebug($"DelDeviceNotificationInd(Address: {sender}, InvokeId: {invokeId}, Handle: {hNotification})");
            return base.DeleteDeviceNotificationIndicationAsync(sender, invokeId, hNotification, cancel);
        }

        protected override Task<AdsErrorCode> DeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint numStampHeaders, NotificationSamplesStamp[] stampHeaders, CancellationToken cancel)

        {
            _logger.LogDebug($"DeviceNotificationInd(Address: {sender}, InvokeId: {invokeId}, Headers: {numStampHeaders})");
            _logger.LogInformation("Received Device Notification Request");

            return base.DeviceNotificationIndicationAsync(sender, invokeId, numStampHeaders, stampHeaders, cancel);
        }

        protected override Task<AdsErrorCode> ReadWriteIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            _logger.LogDebug($"ReadWriteInd(Address: {sender}, InvokeId: {invokeId}, IG: {indexGroup}, IO: {indexOffset}, ReadLen: {readLength}, WriteLen: {writeData.Length})");
            return base.ReadWriteIndicationAsync(sender, invokeId, indexGroup, indexOffset, readLength, writeData, cancel);
        }
#pragma warning restore CS0672 // Member overrides obsolete member
#pragma warning restore CS0618 // Type or member is obsolete

        /* Overwritten confirmation methods of the AdsServer class for the requests your ADS server
         * sends. They are called upon incoming responses. These sample implemetations only add a log message
         * to the sample form.
         * 
         * In common, it is not necessary to overload the virtual Confirmation methods!
         */
        protected override Task<AdsErrorCode> OnReadDeviceStateConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState, CancellationToken cancel)
        {
            _logger.LogDebug($"ReadStateCon(Address: {sender}, InvokeId: {invokeId}, Result: {result}, State: {adsState}, DeviceState: {deviceState})");
            _logger.LogInformation("Received Read State Confirmation");

            return base.OnReadDeviceStateConfirmationAsync(sender, invokeId, result, adsState, deviceState, cancel);
        }

        protected override Task<AdsErrorCode> OnReadConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)

        {
            _logger.LogDebug($"ReadCon(Address: {sender}, InvokeId: {invokeId}, Result: {result}, cbLength: {readData.Length}");
            _logger.LogInformation("Received Read Confirmation");

            return base.OnReadConfirmationAsync(sender, invokeId, result, readData, cancel);
        }

        protected override Task<AdsErrorCode> OnWriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)
        {
            _logger.LogDebug($"WriteCon(Address: {sender}, InvokeId: {invokeId}, Result: {result})");
            _logger.LogInformation("Received Write Confirmation");

            return base.OnWriteConfirmationAsync(sender, invokeId, result, cancel);
        }

        protected override Task<AdsErrorCode> OnReadDeviceInfoConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version, CancellationToken cancel)
        {
            _logger.LogDebug($"ReadDeviceInfoCon(Address: {sender}, InvokeId: {invokeId}, Result: {result}, Name: {name}, Version: {version})");
            _logger.LogInformation("Received Read Device Info Confirmation");

            return base.OnReadDeviceInfoConfirmationAsync(sender, invokeId, result, name, version, cancel);
        }

        protected override Task<AdsErrorCode> OnWriteControlConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)

        {
            _logger.LogDebug($"ReadDeviceInfoCon(Address: {sender}, InvokeId: {invokeId}, Result: {result})");
            _logger.LogInformation("Received Write Control Confirmation");

            return base.OnWriteControlConfirmationAsync(sender, invokeId, result, cancel);
        }

        protected override Task<AdsErrorCode> OnAddDeviceNotificationConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle, CancellationToken cancel)

        {
            _logger.LogDebug($"AddDeviceNotificationCon(Address: {rAddr}, InvokeId: {invokeId}, Result: {result}, Handle: {notificationHandle}");
            _logger.LogInformation("Received Add Device Notification Confirmation. Notification handle: " + notificationHandle);

            return base.OnAddDeviceNotificationConfirmationAsync(rAddr, invokeId, result, notificationHandle, cancel);
        }

        protected override Task<AdsErrorCode> OnDeleteDeviceNotificationConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, CancellationToken cancel)

        {
            _logger.LogDebug($"DelDeviceNotificationCon(Address: {sender}, InvokeId: {invokeId}, Result: {result})");
            _logger.LogInformation("Received Delete Device Notification Confirmation");

            return base.OnDeleteDeviceNotificationConfirmationAsync(sender, invokeId, result, cancel);
        }

        protected override Task<AdsErrorCode> OnReadWriteConfirmationAsync(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData, CancellationToken cancel)

        {
            _logger.LogDebug($"ReadWriteConfirmation(Address: {sender}, InvokeId: {invokeId}, Result: {result}, Length: {readData.Length})");
            _logger.LogInformation("Received Read Write Confirmation");

            return base.OnReadWriteConfirmationAsync(sender, invokeId, result, readData, cancel);
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