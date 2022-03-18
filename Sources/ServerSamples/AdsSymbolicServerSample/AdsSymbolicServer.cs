using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using TwinCAT.Ams;
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
        /// User Server Ports must be in between
        /// AmsPort.CUSTOMER_FIRST (25000) <= PORT <= AmsPort.CUSTOMER_LAST (25999)
        /// to not conflict with Beckhoff prereserved servers!
        /// </remarks>
        static ushort s_Port = 25000;

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
            /// AMS Router enpoint can be changed via envrionment variables which is
            /// benefitial in containerized setups where the AMS router is not listening
            /// at the default loopback address and port 
            IPAddress ipEndpoint;
            if( !IPAddress.TryParse(System.Environment.GetEnvironmentVariable("ENV_AmsConfiguration__LoopbackAddress"), out ipEndpoint))
            {
                ipEndpoint = IPAddress.Loopback;
            }

            int port;
            if( ! int.TryParse(System.Environment.GetEnvironmentVariable("ENV_AmsConfiguration__LoopbackPort"), out port))
            {
                port = 48898;
            }

            AmsConfiguration.RouterEndPoint = new IPEndPoint( ipEndpoint, port);
        }

        IDisposable _changeValueObserver = null;

        /// <summary>
        /// Called when [connected].
        /// </summary>
        protected override void OnConnected()
        {
            this.AddSymbols()
                .AddNotificationTrigger();

            // An Observable.Interval is used to simulate changed values (on a 1 Second base)
            IObservable<long> changeValueTrigger = Observable.Interval(TimeSpan.FromSeconds(1.0), Scheduler.Default);
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
        private SymbolicTestServer AddNotificationTrigger()
        {
            base.notificationTrigger.Add(new BaseTickTrigger(TimeSpan.FromMilliseconds(100)));
            return this;
        }

        /// <summary>
        /// Create the Symbolic information DataAreas, DataTypes and Symbols.
        /// </summary>
        private SymbolicTestServer AddSymbols()
        {
            // Create some Primitive types
            PrimitiveType dtBool = new PrimitiveType("BOOL", typeof(bool));
            PrimitiveType dtInt = new PrimitiveType("INT", typeof(short));
            PrimitiveType dtDInt = new PrimitiveType("DINT", typeof(int));

            // Create an TwinCAT specific Type PCCH (for testing purposes)
            // Which is used for interop to C++ TCOM Modules
            // It defines a pointer to an UTF8 String, where the Length of the data is  specified
            // With a LengthIs parameter
            PrimitiveType dtByte = new PrimitiveType("BYTE", typeof(byte)); // Necessary for the PCCH Type (which is in fact an Alias to POINTER TO BYTE)!
            PCCHType dtPCCH = PCCHType.Create();
            
            // StringType (fixed size)
            StringType dtString = new StringType(80, Encoding.Unicode);

            // Struct Type
            StructType dtStruct = new StructType("MYSTRUCT", null)
                // Add Members
                .AddAligned(new Member("a", dtBool))
                .AddAligned(new Member("b", dtInt))
                .AddAligned(new Member("c", dtDInt));

            // Create an RPC Invokable Type
            // Firstly the methods ...

            // INT Method1([in] INT i1, [in] i2)
            RpcMethod rpc1 = new RpcMethod("Method1")
                //Add Parameters
                .AddParameter("i1", dtInt, MethodParamFlags.In)
                .AddParameter("i2", dtInt, MethodParamFlags.In)
                //Set Return Type
                .SetReturnType(dtInt);

            // INT Method2([in] INT in1, [out] INT out1)
            RpcMethod rpc2 = new RpcMethod("Method2")
                .AddParameter("in1", dtInt, MethodParamFlags.In)
                .AddParameter("out1", dtInt, MethodParamFlags.Out)
                .SetReturnType(dtInt);

            // STRING[80] Method3([in] INT len, [in][LengthIs = 1] PCCH str)
            RpcMethod rpc3 = new RpcMethod("Method3")
                .AddParameter("len", dtInt, MethodParamFlags.In)
                // Referencing to parameter 'len' via LengthIs Attribute
                .AddParameter("str", dtPCCH, MethodParamFlags.In,1)
                .SetReturnType(dtString);

            // STRING[80] Method4([in] INT len, [out][LengthIs = 1] PCCH str)
            RpcMethod rpc4 = new RpcMethod("Method4")
                .AddParameter("len", dtInt, MethodParamFlags.In)
                .AddParameter("str", dtPCCH, MethodParamFlags.Out, 1)
                .SetReturnType(dtString);

            // STRING[80] Method5([in] INT len, [out][LengthIs = 1] PCCH str)
            RpcMethod rpc5 = new RpcMethod("Method5")
                .AddParameter("len", dtInt, MethodParamFlags.In)
                .AddParameter("str", dtPCCH, MethodParamFlags.Out, 1)
                .SetReturnType(dtString);

            // ... Create the RpcStructType itself and bind the Members/Methods
            RpcStructType dtStructRpc = new RpcStructType("MYRPCSTRUCT");
            dtStructRpc
                // Add Members (Fields)
                .AddAligned(new Member("a", dtBool))
                .AddAligned(new Member("b", dtInt))
                .AddAligned(new Member("c", dtDInt))
                // Add Methods
                .AddMethod(rpc1)
                .AddMethod(rpc2)
                .AddMethod(rpc3)
                .AddMethod(rpc4)
                .AddMethod(rpc5);

            // Create an Array Type
            // Create the Dimensions
            IDimensionCollection dims = new DimensionCollection()
                // Add Dimension
                .AddDimension(new Dimension(0, 4))
                .AddDimension(new Dimension(0, 2));

            // Create the Array Type itself
            ArrayType dtArray = new ArrayType(dtInt, dims);

            // Create an Enumeration Type
            // Define the Enum Fields/Values
            EnumValueCollection<int> enumValues = new EnumValueCollection<int>()
                // Add Enumeration Fields/Enry
                .AddValue("None", 0)
                .AddValue("Red", 1)
                .AddValue("Yellow", 2)
                .AddValue("Green", 3);

            // Create the EnumType
            EnumType<int> dtEnum = new EnumType<int>("MYENUM", dtDInt, enumValues);

            // Alias Type
            AliasType dtAlias = new AliasType("MYALIAS", dtEnum);
            
            //Pointer Type
            PointerType dtPointer = new PointerType(dtInt, this.PlatformPointerSize);
            
            // Reference Type
            ReferenceType dtReference = new ReferenceType(dtInt, this.PlatformPointerSize);

            // Add the Types to the internal SymbolFactory
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
                .AddType(dtReference)
                .AddType(dtStructRpc)
                .AddType(dtByte)
                .AddType(dtPCCH);

            // Define some ProcessImage DataAreas (virtual) used for IndexGroup/IndexOffset Alignements
            DataArea general = new DataArea("General", 0x01, 0x1000, 0x1000);
            DataArea globals = new DataArea("Globals", 0x02, 0x1000, 0x1000);
            DataArea statics = new DataArea("Statics", 0x03, 0x1000, 0x1000);

            // Add the DataAreas to the SymbolFactory
            base.symbolFactory
                .AddDataArea(general)
                .AddDataArea(globals)
                .AddDataArea(statics);

            // Create the Symbols (Symbol Instances)
            // With DataType and define its DataArea in the (virtual)ProcessImage
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
                .AddSymbol("Globals.rpcInvoke1", dtStructRpc,globals)
                .AddSymbol("Main.bool1", dtBool, general)
                .AddSymbol("Main.int1", dtInt, general)
                .AddSymbol("Main.dint1", dtDInt, general)
                .AddSymbol("Main.string1", dtString, general)
                .AddSymbol("Main.myStruct1", dtStruct, general)
                .AddSymbol("Main.myArray1", dtArray, general)
                .AddSymbol("Main.myEnum1", dtEnum, general)
                .AddSymbol("Main.myAlias1", dtAlias, general)
                .AddSymbol("Main.pointer1", dtPointer, general)
                .AddSymbol("Main.reference1", dtReference, general)
                .AddSymbol("Main.rpcInvoke1", dtStructRpc, general);

            // Here we set the initial values of or Symbol instances
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
            _symbolValues.Add(this.Symbols["Globals.rpcInvoke1"], 0);
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
            _symbolValues.Add(this.Symbols["Main.rpcInvoke1"], 0);

            return this;
        }

        /// <summary>
        /// Called when an ADS Read State indication is received.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous <see cref="M:TwinCAT.Ads.Server.AdsServer.ReadDeviceStateIndicationAsync(TwinCAT.Ads.AmsAddress,System.UInt32,System.Threading.CancellationToken)" /> operation. The <see cref="T:System.Threading.Tasks.Task`1" /> parameter contains the <see cref="T:TwinCAT.Ads.AdsErrorCode" /> as
        /// <see cref="P:System.Threading.Tasks.Task`1.Result" />.</returns>
        /// <remarks>Overwrite this method in derived classes to react on ADS Read State indications.
        /// The default implementation replies with an ADS ServiceNotSupported error code (0x701).</remarks>
        protected override Task<ResultReadDeviceState> OnReadDeviceStateAsync(CancellationToken cancel)
        {
            AdsState adsState = AdsState.Run;
            ushort deviceState = 0;
            StateInfo state = new StateInfo(adsState, deviceState);
            ResultReadDeviceState result = ResultReadDeviceState.CreateSuccess(state);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Handler function for Reading the internal value data in raw format.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="span">The span.</param>
        /// <returns>AdsErrorCode.</returns>
        /// <remarks>This method is called, when a Read request was received to read the symbols value.
        /// Implement this handler to Read and marshal the value data.</remarks>
        protected override AdsErrorCode OnReadRawValue(ISymbol symbol, Span<byte> span)
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

        /// <summary>
        /// Handler function for writing the symbol value data in raw format.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="span">The span.</param>
        /// <returns>AdsErrorCode.</returns>
        /// <remarks>This method is called, when a Write request was received to overwrite the symbols value.
        /// Implement this handler to overwrite the Ads Servers internal value data.</remarks>
        protected override AdsErrorCode OnWriteRawValue(ISymbol symbol, ReadOnlySpan<byte> span)
        {
            object value;
            _symbolMarshaler.Unmarshal(symbol, span, null, out value);
            SetValue(symbol, value);
            return AdsErrorCode.NoError;
        }

        /// <summary>
        /// Handler function to store a new Symbol value within internal caches of the <see cref="T:TwinCAT.Ads.Server.AdsSymbolicServer" />.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="value">The value.</param>
        protected override void OnSetValue(ISymbol symbol, object value)
        {
            _symbolValues[symbol] = value;
        }

        /// <summary>
        /// Handler function to determine the (stored) value of the symbol from the internal caches of the <see cref="T:TwinCAT.Ads.Server.AdsSymbolicServer" />.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>System.Object.</returns>
        protected override object OnGetValue(ISymbol symbol)
        {
            return _symbolValues[symbol];
        }

        /// <summary>
        /// Handler function called when an RpcInvoke occurs.
        /// </summary>
        /// <param name="structInstance">The structure instance.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <param name="returnValue">The return value.</param>
        /// <returns>AdsErrorCode.</returns>
        /// <remarks>Overwrite this method to react on RpcInvokes within your custom <see cref="AdsSymbolicServer" />.
        /// The default implementation returns <see cref="AdsErrorCode.DeviceServiceNotSupported" />.</remarks>
        protected override AdsErrorCode OnRpcInvoke(IRpcStructInstance structInstance, IRpcMethod method, object[] parameterValues, out object returnValue)
        {
            // Here we implement or handler for the RPC Call.

            // Switch for the different struct Instances
            // For keeping the code simple we just do it here via InstancePath string.
            switch (structInstance.InstancePath)
            {
                case "Globals.rpcInvoke1":
                case "Main.rpcInvoke1":
                    {
                        if (method.Name == "Method1")
                        {
                            // INT Method1([in] INT i1, [in] i2)
                            returnValue = (short)parameterValues[0] + (short)parameterValues[1];
                            return AdsErrorCode.NoError;
                        }
                        else if (method.Name == "Method2")
                        {
                            // INT Method2([in] INT in1, [out] INT out1)
                            parameterValues[1] = ((short)parameterValues[0]) + 1;
                            returnValue = ((short)parameterValues[0]) + 2;
                            return AdsErrorCode.NoError;
                        }
                        else if (method.Name == "Method3")
                        {
                            // STRING[80] Method3([in] INT len, [in][LengthIs = 1] PCCH str)
                            short len = (short)parameterValues[0];
                            returnValue = parameterValues[1];
                            return AdsErrorCode.NoError;
                        }
                        else if (method.Name == "Method4")
                        {
                            // STRING[80] Method4([in] INT len, [out][LengthIs = 1] PCCH str)
                            string str = "Method4Method4Method4Method4Method4Method4Method4Method4";
                            short len = (short)parameterValues[0];
                            parameterValues[1] = str;
                            returnValue = str;
                            return AdsErrorCode.NoError;
                        }
                        else if (method.Name == "Method5")
                        {
                            // STRING[80] Method5([in] INT len, [out][LengthIs = 1] PCCH str)
                            short len = (short)parameterValues[0];
                            string str = "Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5";
                            byte[] pcch = Encoding.UTF8.GetBytes(str);
                            parameterValues[1] = pcch;
                            returnValue = Encoding.UTF8.GetString(pcch, 0, len);
                            return AdsErrorCode.NoError;
                        }
                    }
                    break;
            }
            returnValue = null;
            return AdsErrorCode.DeviceServiceNotSupported;
        }
    }
}
