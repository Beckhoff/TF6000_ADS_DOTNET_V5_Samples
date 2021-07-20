using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;
using TwinCAT.Ads.Server.TypeSystem;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace AdsSymbolicServerSample
{
    public class SymbolicTestServer : AdsSymbolicServer
    {
        /// <summary>
        /// AmsPort of the SymbolicTestServer
        /// </summary>
        /// <remarks>
        /// User Server Ports must be >= 8000 !!!!
        /// </remarks>
        static ushort s_Port = 0x8000;

        /// <summary>
        /// Dictionary containing the Values of the Symbols (Symbol -- Value)
        /// </summary>
        Dictionary<ISymbol, object> _symbolValues = new Dictionary<ISymbol, object>();


        /// <summary>
        /// Gets the SymbolValue Dictionary of the <see cref="SymbolicTestServer"/>.
        /// </summary>
        /// <value>The symbol values.</value>
        public IDictionary<ISymbol, object> SymbolValues => _symbolValues;

        /// <summary>
        /// Symbolic Marshaler for Values
        /// </summary>
        SymbolicAnyTypeMarshaler _symbolMarshaler = new SymbolicAnyTypeMarshaler();

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicTestServer"/> class.
        /// </summary>
        public SymbolicTestServer()
            : base(s_Port, "SymbolicTestServer", null)
        {
        }

        IDisposable _changeValueObserver = null;

        /// <summary>
        /// Called when [connected].
        /// </summary>
        protected override void OnConnected()
        {
            OnCreateSymbols();
            OnCreateNotificationTrigger();

            IObservable<long> changeValueTrigger = Observable.Interval(TimeSpan.FromSeconds(1.0), Scheduler.Default);

            //Action<long> act = toggleValues;
            _changeValueObserver = changeValueTrigger.Subscribe(toggleValues);
            base.OnConnected();
        }

        protected override bool OnDisconnect()
        {
            if (_changeValueObserver != null)
                _changeValueObserver.Dispose();
            return base.OnDisconnect();
        }

        private void toggleValues(long count)
        {
            SetValue("Globals.int1", (short)count);
        }

        /// <summary>
        /// Creates an Notification tigger for the Notifications base tick.
        /// </summary>
        private void OnCreateNotificationTrigger()
        {
            base._notificationTrigger.Add(new BaseTickTrigger(TimeSpan.FromMilliseconds(100)));
        }


        private void OnCreateSymbols()
        {
            PrimitiveType dtBool = new PrimitiveType("BOOL", typeof(bool));
            PrimitiveType dtInt = new PrimitiveType("INT", typeof(short));
            PrimitiveType dtDInt = new PrimitiveType("DINT", typeof(int));
            StringType dtString = new StringType(80, Encoding.Unicode);

            AlignedMemberCollection members = new AlignedMemberCollection()
                .AddAligned(new Member("a", dtBool))
                .AddAligned(new Member("b", dtInt))
                .AddAligned(new Member("c", dtDInt));

            StructType dtStruct = new StructType("MYSTRUCT", null, members);

            IDimensionCollection dims = new DimensionCollection()
                .AddDimension(new Dimension(0, 4))
                .AddDimension(new Dimension(0, 2));

            ArrayType dtArray = new ArrayType(dtInt, dims);

            //ArrayIndexIterator iter = new ArrayIndexIterator(dtArray);

            //foreach(int[] indices in iter)
            //{
            //    string str = ArrayIndexConverter.IndicesToString(indices);
            //}

            EnumValueCollection<int> enumValues = new EnumValueCollection<int>()
                .AddValue("None", 0)
                .AddValue("Red", 1)
                .AddValue("Yellow", 2)
                .AddValue("Green", 3);

            EnumType<int> dtEnum = new EnumType<int>("MYENUM", dtDInt, enumValues);

            AliasType dtAlias = new AliasType("MYALIAS", dtEnum);
            PointerType dtPointer = new PointerType(dtInt, this.PlatformPointerSize);
            ReferenceType dtReference = new ReferenceType(dtInt, this.PlatformPointerSize);

            // Fluent
            base.symbolFactory
                .AddType(dtBool)
                .AddType(dtInt)
                .AddType(dtDInt)
                .AddType(dtString)
                .AddType(dtStruct)
                .AddType(dtArray)
                .AddType(dtEnum)
                .AddType(dtAlias)
                .AddType(dtPointer)
                .AddType(dtReference);

            //ISymbol globals = base.symbolFactory.CreateVirtualRoot("Globals", 0x100, 0);
            //ISymbol main = base.symbolFactory.CreateVirtualRoot("Main", 0x200, 0);

            //uint indexOffset = 0;
            //ISymbol created;

            DataArea general = new DataArea("General", 0x01, 0x1000, 0x1000);
            DataArea globals = new DataArea("Globals", 0x02, 0x1000, 0x1000);
            DataArea statics = new DataArea("Statics", 0x03, 0x1000, 0x1000);

            //Fluent
            base.symbolFactory
                .AddDataArea(general)
                .AddDataArea(globals)
                .AddDataArea(statics);

            base.symbolFactory
                .AddSymbol("Globals.bool1", dtBool, globals)
                .AddSymbol("Globals.int1", dtInt, globals)
                .AddSymbol("Globals.dint1", dtDInt, globals)
                .AddSymbol("Globals.string1", dtString, globals)
                .AddSymbol("Globals.myStruct1", dtStruct, globals)
                .AddSymbol("Globals.myArray1", dtArray, globals)
                .AddSymbol("Globals.myEnum1", dtEnum, globals)
                .AddSymbol("Globals.myAlias1", dtAlias, globals)
                .AddSymbol("Globals.pointer1", dtPointer, globals)
                .AddSymbol("Globals.reference1", dtReference, globals)
                .AddSymbol("Main.bool1", dtBool, general)
                .AddSymbol("Main.int1", dtInt, general)
                .AddSymbol("Main.dint1", dtDInt, general)
                .AddSymbol("Main.string1", dtString, general)
                .AddSymbol("Main.myStruct1", dtStruct, general)
                .AddSymbol("Main.myArray1", dtArray, general)
                .AddSymbol("Main.myEnum1", dtEnum, general)
                .AddSymbol("Main.myAlias1", dtAlias, general)
                .AddSymbol("Main.pointer1", dtPointer, general)
                .AddSymbol("Main.reference1", dtReference, general);

            _symbolValues.Add(this.Symbols["Globals.bool1"], true);
            _symbolValues.Add(this.Symbols["Globals.int1"], (short)42);
            _symbolValues.Add(this.Symbols["Globals.dint1"], 42);
            _symbolValues.Add(this.Symbols["Globals.string1"], "Hello world!");
            _symbolValues.Add(this.Symbols["Globals.myStruct1"], null);
            _symbolValues.Add(this.Symbols["Globals.myArray1"], new short[4, 2]); ;
            _symbolValues.Add(this.Symbols["Globals.myEnum1"], "Yellow");
            _symbolValues.Add(this.Symbols["Globals.myAlias1"], "Red");
            _symbolValues.Add(this.Symbols["Globals.pointer1"], 0);
            _symbolValues.Add(this.Symbols["Globals.reference1"], 0);
            _symbolValues.Add(this.Symbols["Main.bool1"], true);
            _symbolValues.Add(this.Symbols["Main.int1"], (short)42);
            _symbolValues.Add(this.Symbols["Main.dint1"], 42);
            _symbolValues.Add(this.Symbols["Main.string1"], "Hello world!");
            _symbolValues.Add(this.Symbols["Main.myStruct1"], null);
            _symbolValues.Add(this.Symbols["Main.myArray1"], new short[4, 2]);
            _symbolValues.Add(this.Symbols["Main.myEnum1"], "Yellow");
            _symbolValues.Add(this.Symbols["Main.myAlias1"], "Red");
            _symbolValues.Add(this.Symbols["Main.pointer1"], 0);
            _symbolValues.Add(this.Symbols["Main.reference1"], 0);
        }

        protected override Task<AdsErrorCode> ReadDeviceStateIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            return ReadDeviceStateResponseAsync(sender, invokeId, AdsErrorCode.Succeeded, AdsState.Run, 0, cancel);
        }

        protected override AdsErrorCode OnGetRawValue(ISymbol symbol, Span<byte> span)
        {
            object value;
            if (_symbolValues.TryGetValue(symbol, out value))
            {
                int bytes = 0;
                if (_symbolMarshaler.TryMarshal(symbol, value, span, out bytes))
                    return AdsErrorCode.NoError;
                else
                    return AdsErrorCode.DeviceInvalidSize;

            }
            else
                return AdsErrorCode.DeviceSymbolNotFound;
        }

        protected override AdsErrorCode OnWriteRawValue(ISymbol symbol, ReadOnlySpan<byte> span)
        {
            object value;
            _symbolMarshaler.Unmarshal(symbol, span, null, out value);
            SetValue(symbol, value);
            return AdsErrorCode.NoError;
        }

        protected override void OnSetValue(ISymbol symbol, object value)
        {
            _symbolValues[symbol] = value;
        }

        protected override object OnGetValue(ISymbol symbol)
        {
            return _symbolValues[symbol];
        }

        //private Task SendNotifications(CancellationToken cancel)
        //{
        //    IList<NotificationSettings> settings = _notificationTable.WeightedSettings;

        //    foreach(NotificationSettings s in settings)
        //    {
        //        AmsAddress[] notificationReceivers = _notificationTable.GetAddresses(s);

        //        foreach(AmsAddress a in notificationReceivers)
        //        {
        //            uint[] handles = _notificationTable.GetNotificationHandles(s, a);
        //            ISymbol[] symbols = _notificationTable.GetSymbols(a, s);
        //        }
        //    }

        //    //TODO:
        //    AmsAddress target = AmsAddress.Empty;
        //    uint invokeId = 42;

        //    uint handle = 0;
        //    byte[] data = new byte[4];

        //    NotificationSamplesStamp[] stamps = new NotificationSamplesStamp[1];
        //    NotificationDataSample sample = new NotificationDataSample(handle,data);

        //    return DeviceNotificationRequestAsync(target, invokeId, 1, stamps, cancel);
        //}
    }
}
