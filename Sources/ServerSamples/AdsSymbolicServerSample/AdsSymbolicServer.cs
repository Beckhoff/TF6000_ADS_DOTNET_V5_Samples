using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using TwinCAT.Ams;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;
using TwinCAT.Ads.Server.TypeSystem;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using System.Text.RegularExpressions;
using System.Reflection;

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
        /// Dictionary containing the Values of the Symbols (Symbol --> Value)
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


        bool _toggleValues = false;
        TimeSpan _toggleValuesTime = TimeSpan.FromMilliseconds(1000);

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

            _toggleValues = true; // Simulates a value change
        }

        IDisposable _changeValueObserver = null;

        /// <summary>
        /// Handler function when the SymbolicTestServer gets connected.
        /// </summary>
        protected override void OnConnected()
        {
            this.AddSymbols()
                .AddNotificationTrigger();

            // An Observable.Interval is used to simulate changed values 
            IObservable<long> changeValueTrigger = Observable.Interval(_toggleValuesTime, Scheduler.Default);
            _changeValueObserver = changeValueTrigger.Subscribe(toggleValues);
            base.OnConnected();
        }

        protected override bool OnDisconnect()
        {
            if (_changeValueObserver != null)
                _changeValueObserver.Dispose();

            return base.OnDisconnect();
        }

        private static bool s_toggledBoolValue = false;
        /// <summary>
        /// Toggle values as simulation for a changing process image.
        /// </summary>
        /// <param name="count">The count.</param>
        private void toggleValues(long count)
        {

            if (_toggleValues)
            {
                s_toggledBoolValue = !s_toggledBoolValue;
                SetValue("Globals.bool1", s_toggledBoolValue);
            }
        }

        /// <summary>
        /// Creates an Notification trigger for the Notifications base tick.
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
            PrimitiveType dtBool = new PrimitiveType("BOOL", typeof(bool)); // 1-Byte size
            PrimitiveType dtInt = new PrimitiveType("INT", typeof(short)); // 2-Byte size
            PrimitiveType dtDInt = new PrimitiveType("DINT", typeof(int)); // 4-Byte size

            // Create an TwinCAT specific Type PCCH (for testing purposes)
            // Which is used for interop to C++ TCOM Modules
            // It defines a pointer to an UTF8 String, where the Length of the data is  specified
            // With a LengthIs parameter
            PrimitiveType dtByte = new PrimitiveType("BYTE", typeof(byte)); // Necessary for the PCCH Type (which is in fact an Alias to POINTER TO BYTE)!
            PCCHType dtPCCH = PCCHType.Create();
            
            // StringType (fixed size)
            StringType dtString = new StringType(80, Encoding.Unicode);

            // Struct Type
            // Associated to this struct Type, we use a corresponding 'ValueType' within our ServerValues
            // to hold the StructInstance value. This class/value is used to marshal/unmarshal struct types.
            // See the C# class 'MyStruct' at the bottom of this example. 
            StructType dtStruct = new StructType("MYSTRUCT", null)
                // Add Members
                .AddAligned(new Member("name",dtString))
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

            // ... Create the StructType itself and bind the Members/Methods
            StructType dtStructRpc = new StructType("MYRPCSTRUCT");
            dtStructRpc
                 // Add Members (Fields)
                .AddAligned(new Member("name", dtString))
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

            // Create the Array of Struct Type itself
            ArrayType dtArray = new ArrayType(dtInt, dims);

            // Create an Array of STRUCT Type
            // Create the Dimensions
            IDimensionCollection dims2 = new DimensionCollection()
                // Add Dimension
                .AddDimension(new Dimension(0, 10));

            // Create the Complex Array Type itself
            ArrayType dtComplexArray = new ArrayType(dtStruct, dims2);

            // Create an Enumeration Type
            // Define the Enum Fields/Values
            EnumValueCollection<int> enumValues = new EnumValueCollection<int>()
                // Add Enumeration Fields/Entry
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
                .AddType(dtComplexArray)
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
                .AddSymbol("Globals.myComplexArray", dtComplexArray, globals)
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
                .AddSymbol("Main.myComplexArray", dtComplexArray, general)
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
            _symbolValues.Add(this.Symbols["Globals.myStruct1"], new MyStruct("Globals.myStruct1",true,42,99));
            _symbolValues.Add(this.Symbols["Globals.myArray1"], new short[4, 2]); ;
            _symbolValues.Add(this.Symbols["Globals.myEnum1"], "Yellow");
            _symbolValues.Add(this.Symbols["Globals.myAlias1"], "Red");
            _symbolValues.Add(this.Symbols["Globals.pointer1"], 0);
            _symbolValues.Add(this.Symbols["Globals.reference1"], 0);
            _symbolValues.Add(this.Symbols["Globals.rpcInvoke1"], new MyStruct("Globals.rpcInvoke1",false, 555, 666));
            _symbolValues.Add(this.Symbols["Main.bool1"], true);
            _symbolValues.Add(this.Symbols["Main.int1"], (short)42);
            _symbolValues.Add(this.Symbols["Main.dint1"], 42);
            _symbolValues.Add(this.Symbols["Main.string1"], "Hello world!");
            _symbolValues.Add(this.Symbols["Main.myStruct1"], new MyStruct("Main.myStruct1",true, 42, 99));
            _symbolValues.Add(this.Symbols["Main.myArray1"], new short[4, 2]);
            _symbolValues.Add(this.Symbols["Main.myEnum1"], "Yellow");
            _symbolValues.Add(this.Symbols["Main.myAlias1"], "Red");
            _symbolValues.Add(this.Symbols["Main.pointer1"], 0);
            _symbolValues.Add(this.Symbols["Main.reference1"], 0);
            _symbolValues.Add(this.Symbols["Main.rpcInvoke1"], new MyStruct("Main.rpcInvoke1",false,555,666));


            // Adding Values for Complex Arrays
            MyStruct[] complexArrayValueGlobal = new MyStruct[10];

            for (int i = 0; i < 10; i++)
            {
                complexArrayValueGlobal[i] = new MyStruct($"Globals.myComplexArray[i]", true, (short)i, i);
            }

            MyStruct[] complexArrayValueMain = new MyStruct[10];

            for (int i = 0; i < 10; i++)
            {
                complexArrayValueMain[i] = new MyStruct($"Main.myComplexArray[i]", true, (short)i, i);
            }

            _symbolValues.Add(this.Symbols["Globals.myComplexArray"], complexArrayValueGlobal);
            _symbolValues.Add(this.Symbols["Main.myComplexArray"], complexArrayValueMain);

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
        //protected override Task<ResultReadDeviceState> OnReadDeviceStateAsync(CancellationToken cancel)
        protected override Task<ResultReadDeviceState> OnReadDeviceStateAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
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
            // Everything below here is dependant on how the values are stored
            // Here we use a very simplistic Dictionary and have to parse the SubElements
            // Use here your own strategy !!!
            object value;
            AdsErrorCode ret = OnGetValue(symbol, out value);

            if (value != null)
            {
                int bytes = 0;
                if (_symbolMarshaler.TryMarshal(symbol, value, span, out bytes))
                {
                    ret = AdsErrorCode.NoError;
                }
                else
                {
                    ret = AdsErrorCode.DeviceInvalidSize;
                }

            }
            return ret;
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
            AdsErrorCode errorCode = SetValue(symbol, value);
            return errorCode;
        }

        /// <summary>
        /// Handler function to store a new Symbol value within internal caches of the <see cref="T:TwinCAT.Ads.Server.AdsSymbolicServer" />.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="value">The value.</param>
        protected override AdsErrorCode OnSetValue(ISymbol symbol, object value, out bool valueChanged)
        {
            AdsErrorCode ret = AdsErrorCode.DeviceSymbolNotFound; // Haven't found the value
            valueChanged = false;

            // Everything below here is dependant on how the values are stored
            // Here we use a very simplistic Dictionary and have to parse the SubElements
            // Use here your own strategy !!!

            if (!_symbolValues.ContainsKey(symbol)) // Try to get the value directly from dictionary
            {
                // Check for a complex value that is stored as a blob in the value dictionary
                // (Struct or Array in this sample)

                string[] split = splitAccessPath(symbol.InstancePath);
                ISymbol rootSymbol = getValueRoot(symbol); // Getting the symbol key that stores our value as complex object

                if (rootSymbol != null)
                {
                    object rootValue = _symbolValues[rootSymbol];
                    string[] rootSplit = rootSymbol.InstancePath.Split('.');

                    Span<string> delta = split.AsSpan(rootSplit.Length); // Calculate the relative splits
                    valueChanged = SetSubValue(delta, rootSymbol, rootValue,value); // Set the Value relative to the rootValue
                    ret = AdsErrorCode.NoError;
                }
            }
            else
            {
                // Value is in the SymbolValues dictionary and we can simply exchange the full value

                object oldValue = _symbolValues[symbol];

                // Be sure to have an appropriate Equals overload for Equality check!
                // It is necessary to do an Value Equals (not reference equals to work properly)
                if (!oldValue.Equals(value))
                {
                    _symbolValues[symbol] = value;
                    valueChanged = true;
                }
                ret = AdsErrorCode.NoError;
            }


            return ret;
        }

        /// <summary>
        /// Handler function to determine the (stored) value of the symbol from the internal caches of the <see cref="T:TwinCAT.Ads.Server.AdsSymbolicServer" />.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>System.Object.</returns>
        protected override AdsErrorCode OnGetValue(ISymbol symbol, out object value)
        {
            // Everything below here is dependant on how the values are stored
            // Here we use a very simplistic Dictionary and have to parse the SubElements
            // Use here your own strategy !!!

            AdsErrorCode ret = AdsErrorCode.DeviceSymbolNotFound; // Haven't found the value
            value = null;

            // Everything below here is dependant on how the values are stored
            // Here we use a very simplistic Dictionary and have to parse the SubElements
            // Use here your own strategy !!!

            if (!_symbolValues.TryGetValue(symbol, out value)) // Try to get the value directly from dictionary
            {
                // Check for a complex value that is stored as a blob in the value dictionary
                // (Struct or Array in this sample)

                string[] split = splitAccessPath(symbol.InstancePath);
                ISymbol rootSymbol = getValueRoot(symbol); // Getting the symbol key that stores our value as complex object

                if (rootSymbol != null)
                {
                    object rootValue = _symbolValues[rootSymbol];
                    string[] rootSplit = rootSymbol.InstancePath.Split('.');

                    Span<string> delta = split.AsSpan(rootSplit.Length); // Calculate the relative splits
                    value = GetSubValue(delta, rootSymbol, rootValue); // Get the Value relative to the rootValue
                    ret = AdsErrorCode.NoError;
                }
            }
            else
            {
                ret = AdsErrorCode.NoError;
            }
            return ret;
        }

        /// <summary>
        /// Splits the access path to a string array
        /// </summary>
        /// <param name="instancePath">The instance path.</param>
        /// <returns>System.String[].</returns>
        string[] splitAccessPath(string instancePath)
        {
            List<string> list = new List<string>();
            string[] strings = instancePath.Split('.');

            foreach (string s in strings)
            {
                Match match = s_regInstanceAccess.Match(s);

                if (match.Success)
                {
                    string fieldName = match.Groups["name"].Value;
                    string arrayIndex = null;
                    bool isArrayIndex = false;

                    list.Add(fieldName);

                    arrayIndex = match.Groups["jaggedArray"].Value;

                    if (!string.IsNullOrEmpty(arrayIndex))
                    {
                        isArrayIndex = true;

                        var dimGroup = match.Groups["subArray"];
                        var dimensions = match.Groups["subArray"].Captures;

                        foreach (Capture dim in dimensions)
                            list.Add(dim.Value);

                        var boundCaptures = match.Groups["index"].Captures;
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Gets a SubValue from the rootValue object (resolves the relative access path)
        /// </summary>
        /// <param name="delta">Splitted access path relative to the rootvalue symbol</param>
        /// <param name="rootSymbol">The root value symbol (Symbol that belongs to the value stored in the value dictionary)</param>
        /// <param name="rootValue">The root value.</param>
        /// <returns>System.Object.</returns>
        object GetSubValue(Span<string> delta, ISymbol rootSymbol, object rootValue)
        {
            object actValue = rootValue;
            foreach (string member in delta)
            {
                bool isArrayIndex = member.StartsWith("[");

                if (isArrayIndex)
                {
                    Match match = s_regJaggedArray.Match(member);

                    if (match.Success)
                    {
                        Group indicesGroup = match.Groups["jaggedArray"];
                        //int jagCount = indicesGroup.Captures.Count;

                        var jagCaptures = match.Groups["subArray"].Captures;
                        int jagCount = jagCaptures.Count;

                        foreach(Capture dimCapture in jagCaptures)
                        {
                            Match matchDim = s_regSubArray.Match(dimCapture.Value);

                            if (matchDim.Success)
                            {
                                var boundCaptures = matchDim.Groups["index"].Captures;
                                int dimensions = boundCaptures.Count;

                                int[] indices = new int[dimensions];

                                for (int i=0; i<dimensions; i++)
                                {
                                    indices[i] = int.Parse(boundCaptures[i].Value);
                                }

                                Array array = (Array)rootValue;
                                GetArrayBounds(array, out int[] lowerBounds, out int[] upperBounds);

                                actValue = array.GetValue(indices);
                            }
                        }
                    }
                }
                else
                {
                    string fieldName = member;
                    // We have a struct here, we access it via Reflection in this example (might not the most efficent)
                    FieldInfo fieldInfo = rootValue.GetType().GetField(member);
                    actValue = fieldInfo.GetValue(rootValue);
                }
            }
            return actValue;
        }

        /// <summary>
        /// Gets a SubValue from the rootValue object (resolves the relative access path)
        /// </summary>
        /// <param name="delta">Splitted access path relative to the rootvalue symbol</param>
        /// <param name="rootSymbol">The root value symbol (Symbol that belongs to the value stored in the value dictionary)</param>
        /// <param name="rootValue">The root value.</param>
        /// <returns>System.Object.</returns>
        bool SetSubValue(Span<string> delta, ISymbol rootSymbol, object rootValue, object value)
        {
            object? oldValue = null;
            bool changed = false;

            object actValue = rootValue;
            bool last = false;
            
            for(int m=0; m<delta.Length; m++)
            {
                string member = delta[m];
                last = m == delta.Length - 1;

                bool isArrayIndex = member.StartsWith("[");

                if (isArrayIndex)
                {
                    Match match = s_regJaggedArray.Match(member);

                    if (match.Success)
                    {
                        Group indicesGroup = match.Groups["jaggedArray"];
                        //int jagCount = indicesGroup.Captures.Count;

                        var jagCaptures = match.Groups["subArray"].Captures;
                        int jagCount = jagCaptures.Count;

                        foreach (Capture dimCapture in jagCaptures)
                        {
                            Match matchDim = s_regSubArray.Match(dimCapture.Value);

                            if (matchDim.Success)
                            {
                                var boundCaptures = matchDim.Groups["index"].Captures;
                                int dimensions = boundCaptures.Count;

                                int[] indices = new int[dimensions];

                                for (int i = 0; i < dimensions; i++)
                                {
                                    indices[i] = int.Parse(boundCaptures[i].Value);
                                }

                                Array array = (Array)rootValue;
                                GetArrayBounds(array, out int[] lowerBounds, out int[] upperBounds);

                                if (last)
                                {
                                    oldValue = array.GetValue(indices);

                                    if (!oldValue.Equals(value))
                                    {
                                        array.SetValue(value, indices);
                                        changed = true;
                                    }
                                }
                                else
                                {
                                    actValue = array.GetValue(indices);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string fieldName = member;
                    // We have a struct here, we access it via Reflection in this example (might not the most efficent)
                    FieldInfo fieldInfo = rootValue.GetType().GetField(member);

                    if (last)
                    {
                        oldValue = fieldInfo.GetValue(rootValue);

                        if (!oldValue.Equals(value))
                        {
                            fieldInfo.SetValue(rootValue, value);
                            changed = true;
                        }
                    }
                    else
                    {
                        actValue = fieldInfo.GetValue(rootValue);
                    }
                }
            }
            return changed;
        }

        /// <summary>
        /// Gets the array bounds of an array value
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="lowerBounds">The lower bounds.</param>
        /// <param name="lengths">The lengths.</param>
        private static void GetArrayBounds(Array array, out int[] lowerBounds, out int[] lengths)
        {
            int rank = array.GetType().GetArrayRank();

            lengths = new int[rank];
            lowerBounds = new int[rank];

            for (int r = 0; r < rank; r++)
            {
                lengths[r] = array.GetLength(r);
                lowerBounds[r] = array.GetLowerBound(r);
            }
        }

        /// <summary>
        /// Pattern for parsing a symbol name
        /// </summary>
        const string patternInstanceName = @"(?<name>[a-z0-9_]+)";
        /// <summary>
        /// Pattern for parsing a single array index
        /// </summary>
        const string patternIndex = @$"(?<index>-?\d+)";
        /// <summary>
        /// Pattern for parsing the element indices of a single (Sub)Array
        /// </summary>
        const string patternSubArray = @$"(?<subArray>\[{patternIndex}(?:,{patternIndex}*)\])";
        /// <summary>
        /// Pattern for parsing jagged array indices
        /// </summary>
        const string patternJaggedArray = @$"(?<jaggedArray>{patternSubArray}*)";

        /// <summary>
        /// Regex pattern for Parsing InstancePath/InstanceName
        /// </summary>
        const string patternInstanceAccess = @$"{patternInstanceName}{patternJaggedArray}$";

        private static Regex s_regSubArray = new Regex(patternSubArray, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        /// <summary>
        /// Regular expression for parsing a full Array index specification for a jagged array
        /// </summary>
        /// <remarks>e.g '[0,1,2][3,4,5]' (two jagged arrays)</remarks>
        private static Regex s_regJaggedArray = new Regex(patternJaggedArray, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        
        /// <summary>
        /// Regex for parsing InstanceAccess path
        /// </summary>
        /// <remarks>
        /// e.g 'myStruct1.a' or 'myArray1[0,1]
        /// </remarks>
        private static Regex s_regInstanceAccess = new Regex(patternInstanceAccess, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Gets the root symbol that is stored in the value dictionary
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>ISymbol.</returns>
        private ISymbol getValueRoot(ISymbol symbol)
        {
            string[] split = symbol.InstancePath.Split('.');
            return getValueRoot(split);
        }

        private ISymbol getValueRoot(string[] split)
        {
            for (int i = split.Length - 1; i >= 0; i--)
            {
                Match match = s_regInstanceAccess.Match(split[i]);

                if (match.Success)
                {
                    string instanceName = match.Groups["name"].Value;
                    string[] temp = new string[i+1];

                    for (int j = 0; j < i; j++)
                    {
                        temp[j] = split[j];
                    }
                    temp[i] = instanceName;

                    string instancePath = string.Join(".", temp);

                    ISymbol symbol =_symbolValues.Keys.FirstOrDefault(s => s.InstancePath.Equals(instancePath, StringComparison.OrdinalIgnoreCase));

                    if (symbol != null)
                        return symbol;
                }
            }
            return null;
        }

        


        /// <summary>
        /// Handler function called when an RpcInvoke occurs.
        /// </summary>
        /// <param name="rpcInstance">The structure instance.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <param name="returnValue">The return value.</param>
        /// <returns>AdsErrorCode.</returns>
        /// <remarks>Overwrite this method to react on RpcInvokes within your custom <see cref="AdsSymbolicServer" />.
        /// The default implementation returns <see cref="AdsErrorCode.DeviceServiceNotSupported" />.</remarks>
        protected override AdsErrorCode OnRpcInvoke(IInterfaceInstance rpcInstance, IRpcMethod method, object[] parameterValues, out object returnValue)
        {
            // Here we implement or handler for the RPC Call.

            object val;
            // Select the right RpcStructInstance and get its value object.
            if (_symbolValues.TryGetValue(rpcInstance, out val))
            {
                MyStruct myStructValue = val as MyStruct;

                if (myStructValue != null)
                {
                    // For demo simplification, we choose the Method simply by name.
                    // This could be done in a more generic way, e.g with Reflection or whatever custom infrastructure.
                    switch (method.Name)
                    {
                        case "Method1":
                        {
                            returnValue = myStructValue.Method1((short)parameterValues[0], (short)parameterValues[1]);
                            return AdsErrorCode.NoError;
                        }
                        case "Method2":
                        {
                            returnValue = myStructValue.Method2((short)parameterValues[0], out var out1);
                            parameterValues[1] = out1;
                            return AdsErrorCode.NoError;
                        }
                        case "Method3":
                        {
                            returnValue = myStructValue.Method3((short)parameterValues[0], (string)parameterValues[1]);
                            return AdsErrorCode.NoError;
                        }
                        case "Method4":
                        {
                            returnValue = myStructValue.Method4((short)parameterValues[0], out var out1);
                            parameterValues[1] = out1;
                            return AdsErrorCode.NoError;
                        }
                        case "Method5":
                        {
                            returnValue = myStructValue.Method5((short)parameterValues[0], out var out1);
                            parameterValues[1] = out1;
                            return AdsErrorCode.NoError;
                        }
                        default:
                            returnValue = null;
                            return AdsErrorCode.DeviceServiceNotSupported;
                    }
                }
                else
                {
                    returnValue = null;
                    return AdsErrorCode.DeviceServiceNotSupported;
                }
            }
            else
            {
                returnValue = null;
                return AdsErrorCode.DeviceServiceNotSupported;
            }
        }
    }

    // Necessary helper struct to use the .NET Default Interop marshaler which is used by the
    // SymbolicAnyTypeMarshaler to Marshal/Unmarshal Struct values.
    // The Layout must exactly map the 'dtMyStruct' DataType definition, so that
    // the .NET Interop default marshaler is able to 'blit' the value in its own data buffers.
    // MyStruct als implements the RpcInvoke Methods in this example.
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1)]
    public class MyStruct
    {
        // Parameterless Constructor needed to deserialize instances from Network
        public MyStruct()
        {
        }

        // Constructor
        public MyStruct(string name, bool a, short b, int c)
        {
            this.name = name;
            this.a = a;
            this.b = b;
            this.c = c;
        }

        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string name;
        [FieldOffset(162)]
        [MarshalAs(UnmanagedType.U1)] // Boolean is Marshaled as UnmanagedType.I4 otherwise
        public bool a;
        [FieldOffset(163)]
        // [MarshalAs(UnmanagedType.I2)] (Default)
        public short b;
        [FieldOffset(165)]
        // [MarshalAs(UnmanagedType.I4)] (Default)
        public int c;


        // Definition of the RpcMethods

        public short Method1(short i1, short i2)
        {
            // Just return the addition of both inputs
            return (short)(i1 + i2);
        }
        public short Method2(short i1, out short i2)
        {
            i2 = (short)(i1 + 1);
            return (short)(i1 + 2);
        }

        public string Method3(short len, string str)
        {   //str should have len always
            return str;
        }
        public string Method4(short len, out string str)
        {
            str = "Method4Method4Method4Method4Method4Method4Method4Method4";
            // Will only return len bytes!
            return str;
        }
        public string Method5(short len, out byte[] pcch)
        {
            string str = "Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5Method5";
            pcch = Encoding.UTF8.GetBytes(str);
            return Encoding.UTF8.GetString(pcch, 0, len);
        }

        public override string ToString()
        {
            // Dumping the Structs Value
            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(",", $"Name:{name}",$"a:{a}", $"b:{b}",$"c:{c}");
            return sb.ToString();
        }
    }
}
