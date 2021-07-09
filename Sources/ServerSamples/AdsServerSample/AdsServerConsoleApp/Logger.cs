using Microsoft.Extensions.Logging;
using System;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;

namespace TestServer
{
    public interface IServerLogger
    {
        void Log(string str);
        //uint ServerNotificationHandle { get; set; }

        void LogWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData);
        void LogReadInd(AmsAddress rAddr, uint invokeId, uint indexOffset, int cbLength);
        void LogReadStateInd(AmsAddress rAddr, uint invokeId);
        void LogWriteControlInd(AmsAddress rAddr, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data);
        void LogAddDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings notificationSettings);
        void LogDelDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint hNotification);
        void LogDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders);
        void LogReadWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData);
        void LogReadStateCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState);
        void LogReadCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData);
        void LogWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result);
        void LogReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, string name, AdsVersion version);
        void LogReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result);
        void LogAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle);
        void LogDelDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result);
        void LogReadWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData);
        void LogReadDeviceInfoInd(AmsAddress rAddr, uint invokeId);

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

        public virtual void Log(string str)
        {
        }

        public virtual void LogAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle)
        {
        }

        public virtual void LogAddDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings settings)
        {
        }

        public virtual void LogDelDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
        }

        public virtual void LogDelDeviceNotificationInd(AmsAddress sender, uint invokeId, uint hNotification)
        {
        }

        public virtual void LogDeviceNotificationInd(AmsAddress sender, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders)
        {
        }

        public virtual void LogReadCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
        }

        public virtual void LogReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
        {
        }

        public virtual void LogReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
        }

        public virtual void LogReadDeviceInfoInd(AmsAddress sender, uint invokeId)
        {
        }

        public virtual void LogReadInd(AmsAddress sender, uint invokeId, uint indexOffset, int cbLength)
        {
        }

        public virtual void LogReadStateCon(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
        {
        }

        public virtual void LogReadStateInd(AmsAddress sender, uint invokeId)
        {
        }

        public virtual void LogReadWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
        }

        public virtual void LogReadWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData)
        {
        }

        public virtual void LogWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
        }

        public virtual void LogWriteControlInd(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data)
        {
        }

        public virtual void LogWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData)
        {
        }
    }

    public class ServerLogger : ServerLoggerBase
    {
        public ServerLogger(ILogger logger) : base(logger)
        {
        }

        public override void LogAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle)
        {
            Logger.LogDebug($"AddDeviceNotificationCon(Address: {rAddr}, InvokeId: {invokeId}, Result: {result}, Handle: {notificationHandle}");
        }

        public override void LogAddDeviceNotificationInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, NotificationSettings settings)
        {
            Logger.LogDebug($"AddDeviceNotificationInd(Address: {sender}, InvokeId: {invokeId}, IG: {indexGroup}, IO: {indexOffset}, Len: {cbLength})");
        }

        public override void LogDelDeviceNotificationCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
            Logger.LogDebug($"DelDeviceNotificationCon(Address: {sender}, InvokeId: {invokeId}, Result: {result})");
        }

        public override void LogDelDeviceNotificationInd(AmsAddress sender, uint invokeId, uint hNotification)
        {
            Logger.LogDebug($"DelDeviceNotificationInd(Address: {sender}, InvokeId: {invokeId}, Handle: {hNotification})");
        }

        public override void LogDeviceNotificationInd(AmsAddress sender, uint invokeId, uint numStapHeaders, NotificationSamplesStamp[] stampHeaders)
        {
            Logger.LogDebug($"DeviceNotificationInd(Address: {sender}, InvokeId: {invokeId}, Headers: {numStapHeaders})");
        }

        public override void LogReadCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
            Logger.LogDebug($"ReadCon(Address: {sender}, InvokeId: {invokeId}, Result: {result}, cbLength: {readData.Length}");
        }

        public override void LogReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
        {
            Logger.LogDebug($"ReadDeviceInfoCon(Address: {sender}, InvokeId: {invokeId}, Result: {result}, Name: {name}, Version: {version})");
        }

        public override void LogReadDeviceInfoCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
            Logger.LogDebug($"ReadDeviceInfoCon(Address: {sender}, InvokeId: {invokeId}, Result: {result})");
        }

        public override void LogReadDeviceInfoInd(AmsAddress sender, uint invokeId)
        {
            Logger.LogDebug($"ReadDeviceInfoInd(Address: {sender}, InvokeId: {invokeId})");
        }

        public override void LogReadInd(AmsAddress sender, uint invokeId, uint indexOffset, int cbLength)
        {
            Logger.LogDebug($"ReadInd(Address: {sender}, InvokeId: {invokeId}, IO: {indexOffset}, Length: {cbLength})");
        }

        public override void LogReadStateCon(AmsAddress sender, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
        {
            Logger.LogDebug($"ReadStateCon(Address: {sender}, InvokeId: {invokeId}, Result: {result}, State: {adsState}, DeviceState: {deviceState})");
        }

        public override void LogReadStateInd(AmsAddress sender, uint invokeId)
        {
            Logger.LogDebug($"ReadStateInd(Address: {sender}, InvokeId: {invokeId})");
        }

        public override void LogReadWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> readData)
        {
            Logger.LogDebug($"ReadWriteConfirmation(Address: {sender}, InvokeId: {invokeId}, Result: {result}, Length: {readData.Length})");
        }

        public override void LogReadWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int cbReadLength, ReadOnlyMemory<byte> writeData)
        {
            Logger.LogDebug($"ReadWriteInd(Address: {sender}, InvokeId: {invokeId}, IG: {indexGroup}, IO: {indexOffset}, ReadLen: {cbReadLength}, WriteLen: {writeData.Length})");
        }

        public override void LogWriteCon(AmsAddress sender, uint invokeId, AdsErrorCode result)
        {
            Logger.LogDebug($"WriteCon(Address: {sender}, InvokeId: {invokeId}, Result: {result})");
        }

        public override void LogWriteControlInd(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data)
        {
            Logger.LogDebug($"WriteControlInd(Address: {sender}, InvokeId: {invokeId}, AdsState: {adsState}, DeviceState: {deviceState}, Length: {data.Length})");
        }

        public override void LogWriteInd(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData)
        {
            Logger.LogDebug($"WriteInd(Address: {sender}, InvokeId: {invokeId}, IG: {indexGroup}, IO: {indexOffset}, Length: {writeData.Length})");
        }
    }
}