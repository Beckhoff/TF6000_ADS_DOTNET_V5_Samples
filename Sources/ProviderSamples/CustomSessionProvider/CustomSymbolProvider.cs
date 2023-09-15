using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.TypeSystem;
using TwinCAT.TypeSystem.Generic;
using TwinCAT.ValueAccess;

namespace Beckhoff.CustomSymbolProvider
{
    public class CustomSymbolInfo : ISymbolInfo
    {
        public CustomSymbolInfo(string name, string typeName)
            : this(name, null, typeName)
        {
        }

        public CustomSymbolInfo(string name, int[]? indices, string typeName)
        {
            this._path = name;
            this._type = typeName;

            if (indices != null)
            {
                for (int i = 0; i < indices.Length; i++)
                {
                    this._type += string.Format("[{0}]", indices[i]);
                }
            }
        }

        string _path;
        string _type;

        public string TypeName
        {
            get { return _type; }
        }

        public string Name
        {
            get { return _path; }
        }

        public string GetInstanceName()
        {
            return getInstanceName(_path);
        }

        public string GetInstancePath()
        {
            return getInstancePath(_path);
        }

        private string getInstanceName(string entryName)
        {
            string ret;
            int pos = entryName.LastIndexOf('.');

            if (pos >= 0)
                ret = entryName.Substring(pos + 1, entryName.Length - (pos + 1));
            else
                ret = entryName;

            return ret;
        }

        private string getInstancePath(string entryName)
        {
            string ret;
            if (entryName[0] == '.')
                ret = entryName.Substring(1);
            else
                ret = entryName;

            return ret;
        }
    }

    public class CustomSymbolFactory : SymbolFactoryBase
    {
        public CustomSymbolFactory()
            : base(false)
        {
        }

        protected override ISymbol OnCreateArrayElement(IArrayType arrayType, int[] currentIndex, ISymbol parent)
        {
            IArrayInstance arrayInstance = (IArrayInstance)parent;
            string indicesString = ArrayIndexConverter.IndicesToString(currentIndex);
            string instancePath = parent.InstancePath + indicesString;

            ISymbolInfo info = new CustomSymbolInfo(instancePath, arrayInstance.ElementType!.Name);
            ISymbol ret = this.CreateInstance(info, parent);
            return ret;
        }

        protected override IArrayInstance OnCreateArrayInstance(ISymbolInfo entry, IArrayType type, ISymbol? parent)
        {
            CustomSymbolInfo info = (CustomSymbolInfo)entry;
            IArrayInstance ret = new CustomArrayInstance(info.GetInstanceName(), info.GetInstancePath(), type, parent, base.FactoryServices!);
            return ret;
        }

        protected override ISymbol OnCreateFieldInstance(IField member, ISymbol parent)
        {
            string path = this.CombinePath(member, parent);
            ISymbol? ret = null;

            if (member.DataType!.Category == DataTypeCategory.Primitive)
            {
                ret = new CustomSymbol(member.InstanceName, path, member.DataType, parent, base.FactoryServices!);
            }
            else if (member.DataType.Category == DataTypeCategory.Struct)
            {
                ret = new CustomStructInstance(member.InstanceName, path, (IStructType)member.DataType, parent, base.FactoryServices!);
            }
            else if (member.DataType.Category == DataTypeCategory.Array)
            {
                ret = new CustomArrayInstance(member.InstanceName, path, (IArrayType)member.DataType, parent, base.FactoryServices!);
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }

        protected override IPointerInstance OnCreatePointerInstance(ISymbolInfo entry, IPointerType structType, ISymbol? parent)
        {
            throw new NotImplementedException();
        }

        protected override ISymbol OnCreateString(ISymbolInfo entry, IStringType stringType, ISymbol? parent)
        {
            CustomSymbolInfo info = (CustomSymbolInfo)entry;
            return new CustomStringInstance(info.GetInstanceName(), info.GetInstancePath(), stringType, parent, base.FactoryServices!);
        }

        protected override ISymbol OnCreatePrimitive(ISymbolInfo entry, IDataType? dataType, ISymbol? parent)
        {
            CustomSymbolInfo info = (CustomSymbolInfo)entry;
            return new CustomSymbol(info.GetInstanceName(), info.GetInstancePath(), dataType!, null, base.FactoryServices!);
        }

        protected override ISymbol? OnCreateReference(IPointerType type, ISymbol parent)
        {
            throw new NotImplementedException();
        }

        protected override IReferenceInstance OnCreateReferenceInstance(ISymbolInfo entry, IReferenceType referenceType, ISymbol? parent)
        {
            throw new NotImplementedException();
        }

        protected override IAliasInstance OnCreateAlias(ISymbolInfo entry, IAliasType aliasType, ISymbol? parent)
        {
            throw new NotImplementedException();
        }

        protected override IStructInstance OnCreateStruct(ISymbolInfo entry, IStructType structType, ISymbol? parent)
        {
            CustomSymbolInfo info = (CustomSymbolInfo)entry;
            return new CustomStructInstance(info.GetInstanceName(), info.GetInstancePath(), structType, parent, base.FactoryServices!);
        }

        protected override IInterfaceInstance OnCreateInterface(ISymbolInfo entry, IInterfaceType interfaceType, ISymbol? parent)
        {
            throw new NotImplementedException();
        }

        protected override IUnionInstance OnCreateUnion(ISymbolInfo entry, IUnionType structType, ISymbol? parent)
        {
            throw new NotImplementedException();
        }

        protected override ISymbol OnCreateVirtualStruct(string instanceName, string instancePath, ISymbol? parent)
        {
            return new CustomVirtualStructInstance(instanceName, instancePath, null, parent, (ISymbolFactoryServices)base.FactoryServices!);
        }
    }

    [DebuggerDisplay("Name = { _name }, Size = {_byteSize}, Category = {_category}")]
    public class CustomDataType : IDataType, IPrimitiveType, IBindable
    {
        static int s_id = 1;

        public CustomDataType(DataTypeCategory category, string name, int size, PrimitiveTypeFlags flags)
        {
            _name = name;
            _byteSize = size;
            _id = s_id++;
            _category = category;
            _typeFlags = flags;
        }

        PrimitiveTypeFlags _typeFlags;

        public PrimitiveTypeFlags PrimitiveFlags
        {
            get { return _typeFlags; }
        }

        public ITypeAttributeCollection Attributes
        {
            get { return TypeAttributeCollection.Empty; }
        }

        public int BitSize
        {
            get { return ByteSize * 8; }
        }


        int _byteSize = 0;

        public int ByteSize
        {
            get { return _byteSize; }
        }

        DataTypeCategory _category = DataTypeCategory.None;

        public DataTypeCategory Category
        {
            get { return _category; }
        }

        public string Comment
        {
            get { return string.Empty; }
        }

        public string FullName
        {
            get { return Name; }
        }


        int _id = 0;

        public int Id
        {
            get { return _id; }
        }

        public bool IsBitType
        {
            get { return false; }
        }

        public bool IsByteAligned
        {
            get { return true; }
        }

        public bool IsContainer
        {
            get { return PrimitiveTypeMarshaler.IsContainerType(_category); }
        }

        public bool IsPointer
        {
            get { return this._category == DataTypeCategory.Pointer; }
        }

        public bool IsPrimitive
        {
            get { return PrimitiveTypeMarshaler.IsPrimitiveType(this._category); }
        }

        public bool IsReference
        {
            get { return (this._category == DataTypeCategory.Reference); }
        }

        string _name;

        public string Name
        {
            get { return _name; }
        }

        public string Namespace
        {
            get { return string.Empty; }
        }

        public int Size
        {
            get { return _byteSize; }
        }

        public bool IsBound
        {
            get { return (_typeBinder != null); }
        }

        private IBinder? _typeBinder = null;

        public void Bind(IBinder binder)
        {
            _typeBinder = binder;
        }
    }

    public class CustomStringType : CustomDataType, IStringType
    {
        public CustomStringType(string name)
            : base(DataTypeCategory.String, name, -1, PrimitiveTypeFlags.System)
        {
        }

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public bool IsFixedLength
        {
            get { return false; }
        }

        public int Length
        {
            get
            {
                return -1; // this means dynamic length
            }
        }
    }

    public class CustomStructType : CustomDataType, IStructType
    {
        public CustomStructType(string name, int size, MemberCollection members)
            : base(DataTypeCategory.Struct, name, size, PrimitiveTypeFlags.None)
        {
            _members = members;
        }

        MemberCollection _members = MemberCollection.Empty;

        public IMemberCollection AllMembers
        {
            get { return _members.AsReadOnly(); }
        }

        public IDataType? BaseType
        {
            get { return null; }
        }

        public string BaseTypeName
        {
            get { return string.Empty; }
        }

        public bool HasRpcMethods
        {
            get { return false; }
        }

        public IMemberCollection Members
        {
            get { return _members.AsReadOnly(); }
        }

        public string[] InterfaceImplementationNames => Array.Empty<string>();

        public IInterfaceType[] InterfaceImplementations => Array.Empty<IInterfaceType>();
        public IRpcMethodCollection RpcMethods { get; }
    }

    [DebuggerDisplay("InstancePath = { instancePath }, Size = {Size}, Type = {TypeName}, Category = {Category}")]
    public class CustomInstance : IInstance, IBindable
    {
        /// <summary>
        /// Resolved DataType
        /// </summary>
        protected IDataType? type; // Resolved 

        /// <summary>
        /// Reference to the Type for the unresolved case!
        /// </summary>
        protected string typeName = string.Empty;

        /// <summary>
        /// Instance name
        /// </summary>
        protected string name = string.Empty;

        /// <summary>
        /// Full path to the instance
        /// </summary>
        protected string instancePath = string.Empty;

        //Use this constructor if you have the DataType as object!
        public CustomInstance(string name, string instancePath, IDataType? type)
        {
            this.name = name;
            this.instancePath = instancePath;
            this.type = type;
        }

        //Use this constructor if the Type cannot be resolved (yet)
        public CustomInstance(string name, string instancePath, string typeName)
        {
            this.name = name;
            this.instancePath = instancePath;
            type = null; // !!!
            this.typeName = typeName;
        }

        public DataTypeCategory Category
        {
            get
            {
                if (type != null)
                    return type.Category;
                else
                    return DataTypeCategory.Struct; // The virtual struct
            }
        }

        public int BitSize
        {
            get
            {
                if (type != null)
                    return type.BitSize;
                else
                    return 0;
            }
        }

        public int ByteSize
        {
            get
            {
                if (type != null)
                    return type.ByteSize;
                else
                    return 0;
            }
        }

        public string Comment
        {
            get { return string.Empty; }
        }

        public IDataType? DataType
        {
            get { return type; }
        }

        public string InstanceName
        {
            get { return name; }
        }

        public string InstancePath
        {
            get { return instancePath; }
        }

        public bool IsBitType
        {
            get { return false; }
        }

        public bool IsByteAligned
        {
            get { return false; }
        }

        public bool IsPointer
        {
            get
            {
                if (type != null)
                    return type.IsPointer;
                else
                    return false;
            }
        }

        public bool IsReference
        {
            get
            {
                if (type != null)
                    return type.IsReference;
                else
                    return false;
            }
        }

        public bool IsStatic
        {
            get { return false; }
        }

        public int Size
        {
            get
            {
                if (type != null)
                    return type.Size;
                else
                    return 0;
            }
        }

        public string TypeName
        {
            get
            {
                if (type != null)
                {
                    //Resolved case
                    return type.Name;
                }
                else
                    return typeName;
            }
        }

        public bool IsBound
        {
            get { return (_symbolBinder != null); }
        }

        private IBinder? _symbolBinder = null;

        public void Bind(IBinder binder)
        {
            _symbolBinder = binder;
        }


        protected TypeAttributeCollection attributes = new TypeAttributeCollection();
        public ITypeAttributeCollection Attributes
        {
            get { return attributes.AsReadOnly(); }
        }
    }

    [DebuggerDisplay("Name = {InstancePath}, Offset = {Offset}, Size = {Size}, Type = {TypeName}, Category = {DataType.Category}")]
    public class CustomMember : CustomInstance, IMember, IAlignmentSet
    {
        int _byteOffset = 0;
        CustomStructType _parentType;

        public CustomMember(string name, IDataType memberType, CustomStructType structType)
            : base(name, name, memberType)
        {
            _parentType = structType;
            //_byteOffset = offset;
        }

        public int BitOffset
        {
            get { return _byteOffset * 8; }
        }

        public int ByteOffset
        {
            get { return _byteOffset; }
        }

        public int Offset
        {
            get { return _byteOffset; }
        }

        public IDataType ParentType
        {
            get { return _parentType; }
        }

        public Encoding ValueEncoding { get => Encoding.Unicode; }

        public void SetOffset(int offset)
        {
            _byteOffset = offset;
        }
    }

    public class CustomSymbol
        : CustomInstance, IValueSymbol, IValueAccessorProvider, ISymbolInternal, ISymbolFactoryServicesProvider

    {
        //Use this constructor if the Type is resolved
        public CustomSymbol(string name, string instancePath, IDataType? type, ISymbol? parent, ISymbolFactoryServices services)
            : base(name, instancePath, type)
        {
            this._services = services;
            this._parent = parent;
            this.valueAccessor = (IAccessorValue)((ISymbolFactoryValueServices)_services).ValueAccessor;
        }

        //Use this constructor if the Type cannot be resolved (yet)
        public CustomSymbol(string name, string instancePath, string typeName, ISymbol? parent, ISymbolFactoryServices services)
            : base(name, instancePath, typeName)
        {
            this._services = services;
            this._parent = parent;
            this.valueAccessor = (IAccessorValue)((ISymbolFactoryValueServices)_services).ValueAccessor;
        }

        protected IAccessorValue valueAccessor;
        private ISymbolFactoryServices _services;

        /// <summary>
        /// Gets the factory services.
        /// </summary>
        /// <value>The factory services.</value>
        public ISymbolFactoryServices FactoryServices
        {
            get { return _services; }
        }

        //public ReadOnlyTypeAttributeCollection Attributes
        //{
        //    get { return attributes.AsReadOnly(); }
        //    get { return new TypeAttributeCollection().AsReadOnly(); }
        //}

        public virtual bool IsContainerType
        {
            get
            {
                if (this.DataType != null)
                    return this.DataType.IsContainer;
                else
                    return true; // This is the virtual Struct
            }
        }

        public bool IsPersistent
        {
            get { return false; }
        }

        public bool IsPrimitiveType
        {
            get
            {
                if (this.DataType != null)
                    return this.DataType.IsPrimitive;
                else
                    return false; // This is the Virtual struct.
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        ISymbol? _parent = null;

#pragma warning disable 0067
        public event EventHandler<ValueChangedEventArgs>? ValueChanged;
        public event EventHandler<RawValueChangedEventArgs>? RawValueChanged;
#pragma warning restore 0067

        /// <exclude />
        public ISymbol? Parent
        {
            get { return _parent; }
        }

        public bool IsRecursive
        {
            get { return false; }
        }

        #region IValueSymbol implementation

        public INotificationSettings? NotificationSettings
        {
            get { return null; }

            set { Debug.Fail("Not implemented!"); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:TwinCAT.TypeSystem.IValueSymbol" /> has a value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        /// <remarks>A VirtualSymbol does not support values, but in terms of the <see cref="T:TwinCAT.TypeSystem.IValueSymbol" /> definition, is a <see cref="T:TwinCAT.TypeSystem.IValueSymbol" /></remarks>
        public virtual bool HasValue
        {
            get { return true; }
        }

        public IAccessorRawValue? ValueAccessor
        {
            get { return valueAccessor; }
        }

        /// <exclude />
        public bool SubSymbolsCreated
        {
            get
            {
                ISymbolCollection<ISymbol>? coll = null;

                if (subSymbols != null && subSymbols.TryGetTarget(out coll))
                    return true;

                return false;
            }
        }

        public object ReadValue()
        {
            DateTimeOffset? readTime;
            return valueAccessor.ReadValue(this, out readTime);
        }

        public Task<ResultReadValueAccess> ReadValueAsync(CancellationToken cancel)
        {
            return valueAccessor.ReadValueAsync(this, cancel);
        }

        public void WriteValue(object value)
        {
            DateTimeOffset? writeTime;
            valueAccessor.WriteValue(this, value, out writeTime);
        }

        public Task<ResultWriteAccess> WriteValueAsync(object value, CancellationToken cancel)
        {
            return valueAccessor.WriteValueAsync(this, value, cancel);
        }

        public int TryReadValue(int timeout, out object? value)
        {
            DateTimeOffset? dateTime;
            return valueAccessor.TryReadValue(this, out value, out dateTime);
        }

        public int TryWriteValue(object value, int timeout)
        {
            DateTimeOffset? dateTime;
            return valueAccessor.TryWriteValue(this, value, out dateTime);
        }

        public byte[] ReadRawValue()
        {
            DateTimeOffset? readTime;
            byte[] bytes = new byte[this.ByteSize];
            int result = valueAccessor.TryReadRaw(this, bytes.AsMemory(), out readTime);
            return bytes;
        }

        public Task<ResultReadRawAccess> ReadRawValueAsync(CancellationToken cancel)
        {
            byte[] bytes = new byte[this.ByteSize];
            return valueAccessor.ReadRawAsync(this, bytes.AsMemory(), cancel);
        }

        public void WriteRawValue(byte[] value)
        {
            DateTimeOffset? writeTime;
            int result = valueAccessor.TryWriteValue(this, value, out writeTime);
        }

        public Task<ResultWriteAccess> WriteRawValueAsync(byte[] value, CancellationToken cancel)
        {
            return valueAccessor.WriteValueAsync(this, value.AsMemory(), cancel);
        }

        public void SetParent(ISymbol symbol)
        {
            _parent = symbol;
        }

        public ISymbolCollection<ISymbol> CreateSubSymbols(ISymbol parent)
        {
            ISymbolCollection<ISymbol>? ret = null;

            if (subSymbols == null)
            {
                ret = OnCreateSubSymbols(parent);
                subSymbols = new WeakReference<ISymbolCollection<ISymbol>>(OnCreateSubSymbols(parent));
            }

            //ret = (SymbolCollection) subSymbols.Target;
            subSymbols.TryGetTarget(out ret);

            if (ret == null)
            {
                ret = OnCreateSubSymbols(parent);
                //subSymbols.Target = ret;
                subSymbols.SetTarget(ret);
            }
            return ret;
        }

        /// <summary>
        /// Weak reference to SubSymbols
        /// </summary>
        protected WeakReference<ISymbolCollection<ISymbol>>? subSymbols = null;

        public ISymbolCollection<ISymbol> SubSymbolsInternal
        {
            get
            {
                ISymbolCollection<ISymbol>? ret = null;

                if (subSymbols == null)
                {
                    subSymbols = new WeakReference<ISymbolCollection<ISymbol>>(OnCreateSubSymbols(this));
                }

                subSymbols.TryGetTarget(out ret);
                //ret = (SymbolCollection) subSymbols.Target;

                if (ret == null)
                {
                    ret = OnCreateSubSymbols(this);
                    subSymbols.SetTarget(ret);
                }
                return ret;
            }
        }

        /// <summary>
        /// Creates the sub symbols collection.
        /// </summary>
        protected virtual ISymbolCollection<ISymbol> OnCreateSubSymbols(ISymbol parentSymbol)
        {
            SymbolCollection symbols = new SymbolCollection(InstanceCollectionMode.Names);
            return symbols;
        }

        public object ReadValue(int timeout)
        {
            return ReadValue();
        }

        public void WriteValue(object value, int timeout)
        {
            WriteValue(value);
        }

        public byte[] ReadRawValue(int timeout)
        {
            return ReadRawValue();
        }

        public void WriteRawValue(byte[] value, int timeout)
        {
            WriteRawValue(value);
        }

        public ResultReadValueAccess ReadValueAsResult()
        {
            throw new NotImplementedException();
        }

        public ResultWriteAccess WriteValueAsResult(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the SubSymbols of the <see cref="ISymbol" />
        /// </summary>
        /// <remarks>
        /// Used for Array, Struct, Pointer and Reference instances. Otherwise empty
        /// </remarks>
        public ISymbolCollection<ISymbol> SubSymbols
        {
            get { return new ReadOnlySymbolCollection(((ISymbolInternal)this).SubSymbolsInternal); }
        }


        protected SymbolAccessRights accessRights = SymbolAccessRights.All;
        /// <summary>
        /// Gets the access rights.
        /// </summary>
        /// <value>The access rights.</value>
        public SymbolAccessRights AccessRights
        {
            get { return accessRights; }
        }

        public IConnection? Connection
        {
            get
            {
                return ((IAccessorConnection)valueAccessor).Connection;
            }
        }

        public Encoding ValueEncoding => Encoding.Default;

        #endregion
    }

    public class CustomArrayInstance : CustomSymbol, IArrayInstance
    {
        public CustomArrayInstance(string name, string path, IArrayType type, ISymbol? parent, ISymbolFactoryServices services)
            : base(name, path, type, parent, services)
        {
        }

        public ISymbol this[int[] indices]
        {
            get { throw new NotImplementedException(); }
        }

        public IDimensionCollection Dimensions
        {
            get { return ((IArrayType)type!).Dimensions; }
        }

        public ISymbolCollection<ISymbol> Elements
        {
            get { return this.SubSymbols; }
        }

        public IDataType? ElementType
        {
            get { return ((IArrayType)type!).ElementType; }
        }

        public bool TryGetElement(int[] indices, [NotNullWhen(true)] out ISymbol? symbol)
        {
            CustomArrayType? dataType = (CustomArrayType?)this.DataType;

            if (dataType != null && ArrayIndexConverter.TryCheckIndices(indices, dataType))
            {
                int subIndex = ArrayIndexConverter.IndicesToSubIndex(indices, dataType);
                symbol = this.SubSymbols[subIndex];
                return true;
            }
            else
            {
                symbol = null;
                return false;
            }
        }

        public bool TryGetElement(IList<int[]> jaggedIndices, [NotNullWhen(true)] out ISymbol? symbol)
        {
            if (jaggedIndices == null) throw new ArgumentNullException(nameof(jaggedIndices));

            IArrayType? arrayType = (IArrayType?)this.DataType;

            if (arrayType == null) throw new CannotResolveDataTypeException(this);

            if (arrayType.JaggedLevel != jaggedIndices.Count) throw new ArgumentOutOfRangeException(nameof(jaggedIndices));

            bool ok = true;
            IArrayInstance actSubArray = this;
            ISymbol? actSymbol = null;
            symbol = null;

            // Resolve jagged arrays.

            for (int j = 0; j < jaggedIndices.Count; j++)
            {
                //ISymbol s;
                ok &= actSubArray!.TryGetElement(jaggedIndices[j], out actSymbol);

                if (!ok)
                    break;

                actSubArray = (IArrayInstance)actSymbol!;
            }

            if (ok)
            {
                symbol = actSymbol;
            }
            return (symbol != null);
        }

        protected override ISymbolCollection<ISymbol> OnCreateSubSymbols(ISymbol parentSymbol)
        {
            ISymbolCollection<ISymbol> symbols;
            IArrayType? arrayType = (IArrayType?)this.DataType;
            if (arrayType != null)
            {
                symbols = this.FactoryServices.SymbolFactory.CreateArrayElementInstances(this, arrayType);
            }
            else
            {
                symbols = new SymbolCollection(InstanceCollectionMode.Names); // Return empty collection (Virtual Struct???)
            }
            return symbols;
        }
    }

    public class CustomStructInstance : CustomSymbol, IStructInstance
    {
        public CustomStructInstance(string name, string path, IStructType? type, ISymbol? parent, ISymbolFactoryServices services)
            : base(name, path, type, parent, services)
        {
        }

        public bool HasRpcMethods
        {
            get { return false; }
        }

        public ISymbolCollection<ISymbol> MemberInstances
        {
            get { return this.SubSymbols; }
        }

        protected override ISymbolCollection<ISymbol> OnCreateSubSymbols(ISymbol parentSymbol)
        {
            ISymbolCollection<ISymbol> symbols;
            IStructType? structType = (IStructType?)this.DataType;

            if (structType != null)
            {
                symbols = this.FactoryServices.SymbolFactory.CreateFieldInstances(this, structType);
            }
            else
            {
                symbols = new SymbolCollection(InstanceCollectionMode.Names); // Return empty collection (Virtual Struct???)
            }
            return symbols;
        }

        public IRpcMethodCollection RpcMethods { get; }

        public object? InvokeRpcMethod(string methodName, object[]? inParameters)
        {
            throw new NotImplementedException();
        }

        public object? InvokeRpcMethod(string methodName, object[]? inParameters, out object[]? outParameters)
        {
            throw new NotImplementedException();
        }

        public int TryInvokeRpcMethod(string methodName, object[]? inParameters, out object? retValue)
        {
            throw new NotImplementedException();
        }

        public int TryInvokeRpcMethod(string methodName, object[]? inParameters, out object[]? outParameters, out object? retValue)
        {
            throw new NotImplementedException();
        }

        public Task<ResultRpcMethodAccess> InvokeRpcMethodAsync(string methodName, object[]? inParameters, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomStringInstance : CustomSymbol, IStringInstance
    {
        public CustomStringInstance(string name, string path, IStringType type, ISymbol? parent, ISymbolFactoryServices services)
            : base(name, path, type, parent, services)
        {
        }

        //public Encoding ValueEncoding
        //{
        //    get { return ((IStringType) DataType).Encoding; }
        //}

        public bool IsFixedLength
        {
            get
            {
                IStringType? stringType = (IStringType?)this.DataType;

                if (stringType != null)
                    return stringType.IsFixedLength;
                else
                    return true;
            }
        }
    }

    /// <exclude />
    public class CustomVirtualStructInstance : CustomStructInstance, IVirtualStructInstance
    {
        protected SymbolCollection memberInstances;


        public CustomVirtualStructInstance(string name, string path, IStructType? type, ISymbol? parent, ISymbolFactoryServices services)
            : base(name, path, type, parent, services)
        {
            memberInstances = new SymbolCollection();
        }


        /// <summary>
        /// Gets a value indicating whether this instance has a value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        public override bool HasValue
        {
            get { return false; }
        }

        public bool AddMember(ISymbol memberInstance, IVirtualStructInstance parent)
        {
            bool ret = true;
            memberInstances.Add(memberInstance);
            return ret;
        }

        protected override ISymbolCollection<ISymbol> OnCreateSubSymbols(ISymbol parentSymbol)
        {
            return memberInstances;
        }
    }

    public class CustomArrayType : CustomDataType, IArrayType
    {
        public CustomArrayType(string name, DimensionCollection dims, CustomDataType elementType)
            : base(DataTypeCategory.Array, name, dims.ElementCount * elementType.Size, PrimitiveTypeFlags.None)
        {
            _dims = dims;
            _elementType = elementType;
        }

        IDataType _elementType;
        DimensionCollection _dims;

        public IDimensionCollection Dimensions
        {
            get { return _dims.AsReadOnly(); }
        }

        public IDataType ElementType
        {
            get { return _elementType; }
        }

        public bool IsOversampled
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is jagged.
        /// </summary>
        /// <value><c>true</c> if this instance is jagged; otherwise, <c>false</c>.</value>
        public bool IsJagged
        {
            get { return (JaggedLevel > 1); }
        }

        /// <summary>
        /// Gets the jagged level (Non-Jagged Array have level 1)
        /// </summary>
        /// <value>The jagged level.</value>
        public int JaggedLevel
        {
            get
            {
                int level = 1;
                IArrayType act = this;

                while (act.ElementType != null && act.ElementType.Category == DataTypeCategory.Array)
                {
                    level++;
                    act = (IArrayType)act.ElementType;
                }
                return level;
            }
        }

        public string ElementTypeName { get => _elementType.Name; }
    }

    public class CustomSymbolLoader : ISymbolLoader, IDynamicSymbolLoader
    {
        private ISessionProvider _sessionProvider;

        public CustomSymbolLoader(ISession session, bool dynamicObjects, ISessionProvider sessionProvider)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (sessionProvider == null) throw new ArgumentNullException(nameof(sessionProvider));

            _sessionProvider = sessionProvider;
            _provider = new CustomSymbolProvider("Root", "System");

            ISymbolFactory _symbolFactory;
            IAccessorValue _valueAccessor;

            ValueCreationModes mode = ValueCreationModes.Default; // We want to resolve primitive values.
            //ValueCreationMode mode = ValueCreationMode.FullDynamics; // We don't want to resolve primitive values.

            IAccessorValueFactory valueFactory;

            if (dynamicObjects)
            {
                valueFactory = new DynamicValueFactory(mode);

                _symbolFactory = new TwinCAT.TypeSystem.DynamicSymbolFactory(new CustomSymbolFactory(), false);
                IAccessorValue innerValueAccessor = new CustomValueAccessor(valueFactory, session);

                _valueAccessor = new DynamicValueAccessor(innerValueAccessor, valueFactory, mode);
            }
            else
            {
                valueFactory = new ValueFactory((mode));
                _symbolFactory = new CustomSymbolFactory();
                _valueAccessor = new CustomValueAccessor(valueFactory, session);
            }

            IAccessorValueFactory2? valueFactory2 = valueFactory as IAccessorValueFactory2;

            if (valueFactory2 != null)
                valueFactory2.SetValueAccessor(_valueAccessor);

            CustomBinder _binder = new CustomBinder(_provider, _symbolFactory, true);

            _factoryServices = new SymbolFactoryValueServices(_binder, _symbolFactory, _valueAccessor, session);

            _symbolFactory.Initialize(_factoryServices);

            DataTypeCollection buildInTypes = CreateBuildInTypes();
            _binder.RegisterTypes(buildInTypes);

            ISymbolInfo info1 = new CustomSymbolInfo("MAIN.sym1", "BuildInBool");
            ISymbolInfo info2 = new CustomSymbolInfo("MAIN.sym2", "BuildInStruct");
            ISymbolInfo info3 = new CustomSymbolInfo("MAIN.sym3", "BuildInArray");

            ISymbolInfo yoda1 = new CustomSymbolInfo("Yoda.Quota1", "BuildInString");
            ISymbolInfo yoda2 = new CustomSymbolInfo("Yoda.Quota2", "BuildInString");
            ISymbolInfo yoda3 = new CustomSymbolInfo("Yoda.Quota3", "BuildInString");


            ISymbol symbol1 = _symbolFactory.CreateInstance(info1, null);
            ISymbol symbol2 = _symbolFactory.CreateInstance(info2, null);
            ISymbol symbol3 = _symbolFactory.CreateInstance(info3, null);

            ISymbol symbol4 = _symbolFactory.CreateInstance(yoda1, null);
            ISymbol symbol5 = _symbolFactory.CreateInstance(yoda2, null);
            ISymbol symbol6 = _symbolFactory.CreateInstance(yoda3, null);

            _binder.Bind((IHierarchicalSymbol)symbol1);
            _binder.Bind((IHierarchicalSymbol)symbol2);
            _binder.Bind((IHierarchicalSymbol)symbol3);
            _binder.Bind((IHierarchicalSymbol)symbol4);
            _binder.Bind((IHierarchicalSymbol)symbol5);
            _binder.Bind((IHierarchicalSymbol)symbol6);
        }

        //IBinder _binder = null;
        IInternalSymbolProvider _provider;
        //ISymbolFactory _symbolFactory = null;
        //IAccessorValue _valueAccessor = null;

        private ISymbolFactoryServices _factoryServices;

        public IDataTypeCollection BuildInTypes
        {
            get { return (IDataTypeCollection)_provider.Namespaces["System"].DataTypes; }
        }

        private DataTypeCollection CreateBuildInTypes()
        {
            DataTypeCollection buildInTypes = new DataTypeCollection();
            CustomDataType boolean = new CustomDataType(DataTypeCategory.Primitive, "BuildInBool", 1, PrimitiveTypeFlags.Bool);
            CustomDataType integer = new CustomDataType(DataTypeCategory.Primitive, "BuildInInt", 4, PrimitiveTypeFlags.Numeric);
            CustomStringType stringType = new CustomStringType("BuildInString"); //  1KB  long

            buildInTypes.Add(boolean);
            buildInTypes.Add(integer);
            buildInTypes.Add(stringType);

            AlignedMemberCollection structMembers = new AlignedMemberCollection();
            CustomStructType structType = new CustomStructType("BuildInStruct", 5, structMembers);

            CustomMember memberA = new CustomMember("a", boolean, structType);
            structMembers.Add(memberA);
            CustomMember memberB = new CustomMember("b", boolean, structType);
            structMembers.Add(memberB);
            CustomMember memberC = new CustomMember("c", boolean, structType);
            structMembers.Add(memberC);
            CustomMember memberD = new CustomMember("d", boolean, structType);
            structMembers.Add(memberD);
            CustomMember memberE = new CustomMember("e", boolean, structType);
            structMembers.Add(memberE);

            buildInTypes.Add(structType);

            DimensionCollection dims = new DimensionCollection();
            dims.Add(new Dimension(0, 5));
            dims.Add(new Dimension(0, 5));
            CustomArrayType arrayType = new CustomArrayType("BuildInArray", dims, integer);

            buildInTypes.Add(arrayType);

            return buildInTypes;
        }

        /// <summary>
        /// Gets the symbols asynchronously
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="ResultSymbols"/> parameter contains the data types
        /// (<see cref="ResultSymbols{T}.Symbols"/>) and the <see cref="ResultAds.ErrorCode"/> after execution.
        /// </returns>
        /// <seealso cref="Symbols"/>
        public Task<ResultSymbols> GetSymbolsAsync(CancellationToken cancel)
        {
            return new Task<ResultSymbols>(() => new ResultSymbols(AdsErrorCode.NoError, _provider.Symbols));
        }

        /// <summary>
        /// Gets the data types asynchronously.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="ResultDataTypes"/> parameter contains the data types
        /// (<see cref="ResultDataTypes.DataTypes"/>) and the <see cref="ResultAds.ErrorCode"/> after execution.
        /// </returns>
        /// <seealso cref="DataTypes"/>
        public Task<ResultDataTypes> GetDataTypesAsync(CancellationToken cancel)
        {
            return new Task<ResultDataTypes>(() => new ResultDataTypes(AdsErrorCode.NoError, _provider.DataTypes));
        }

        ///// <summary>
        ///// Gets the dynamic symbols asynchronously
        ///// </summary>
        ///// <param name="cancel">The cancellation token.</param>
        ///// <returns>Task&lt;ResultDynamicSymbols&gt;.</returns>
        //public Task<ResultDynamicSymbols> GetDynamicSymbolsAsync(CancellationToken cancel)
        //{
        //    return new Task<ResultDynamicSymbols>(() => new ResultDynamicSymbols(AdsErrorCode.NoError, this.SymbolsDynamic));
        //}

        ///// <summary>
        ///// Gets the symbols asynchronously
        ///// </summary>
        ///// <param name="forceReload">Reloads the information from the target device and bypasses the internal cache.</param>
        ///// <param name="cancel">The cancellation token.</param>
        ///// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="T:TwinCAT.TypeSystem.ResultSymbols" /> parameter contains the data types
        ///// (<see cref="P:TwinCAT.TypeSystem.ResultSymbols`1.Symbols" />) and the <see cref="P:TwinCAT.Ads.ResultAds.ErrorCode" /> after execution.</returns>
        ///// <seealso cref="P:TwinCAT.TypeSystem.ISymbolServer.Symbols" />
        //public Task<ResultSymbols> GetSymbolsAsync(CancellationToken cancel)
        //{
        //    return GetSymbolsAsync(cancel);
        //}

        /// <summary>
        /// Tries to geth the symbols from the device target.
        /// </summary>
        /// <param name="forceReload">Reloads the information from the target device and bypasses the internal cache.</param>
        /// <param name="symbols">The symbols.</param>
        /// <returns>AdsErrorCode.</returns>
        public AdsErrorCode TryGetSymbols(bool forceReload, out ISymbolCollection<ISymbol>? symbols)
        {
            return TryGetSymbols(out symbols);
        }

        /// <summary>
        /// Tries to geth the symbols from the device target.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        /// <returns>AdsErrorCode.</returns>
        public AdsErrorCode TryGetSymbols(out ISymbolCollection<ISymbol>? symbols)
        {
            symbols = this.Symbols;
            return AdsErrorCode.NoError;
        }

        ///// <summary>
        ///// Gets the data types asynchronously.
        ///// </summary>
        ///// <param name="cancel">The cancellation token.</param>
        ///// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="T:TwinCAT.TypeSystem.ResultDataTypes" /> parameter contains the data types
        ///// (<see cref="P:TwinCAT.TypeSystem.ResultDataTypes.DataTypes" />) and the <see cref="P:TwinCAT.Ads.ResultAds.ErrorCode" /> after execution.</returns>
        ///// <seealso cref="P:TwinCAT.TypeSystem.ISymbolServer.DataTypes" />
        //public Task<ResultDataTypes> GetDataTypesAsync(CancellationToken cancel)
        //{
        //    return Task.FromResult(new ResultDataTypes(AdsErrorCode.NoError, this.DataTypes));
        //}

        /// <summary>
        /// Tries to get the symbols from the device target.
        /// </summary>
        /// <param name="dataTypes">The data types.</param>
        /// <returns>AdsErrorCode.</returns>
        public AdsErrorCode TryGetDataTypes(out IDataTypeCollection<IDataType>? dataTypes)
        {
            dataTypes = this.DataTypes;
            return AdsErrorCode.NoError;
        }

        ///// <summary>
        ///// Tries to get the symbols from the device target.
        ///// </summary>
        ///// <param name="dataTypes">The data types.</param>
        ///// <returns>AdsErrorCode.</returns>
        //public AdsErrorCode TryGetDataTypes(out IDataTypeCollection<IDataType>? dataTypes)
        //{
        //    return TryGetDataTypes(false, out dataTypes);
        //}

        /// <summary>
        /// Gets the dynamic symbols asynchronously
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>Task&lt;ResultDynamicSymbols&gt;.</returns>
        public Task<ResultDynamicSymbols> GetDynamicSymbolsAsync(CancellationToken cancel)
        {
            return Task.FromResult(new ResultDynamicSymbols(AdsErrorCode.NoError, this.SymbolsDynamic));
        }

        public void ResetCachedSymbolicData()
        {
        }

        /// <summary>
        /// Gets the data types
        /// </summary>
        /// <remarks>
        /// This property reads the DataTypes synchronously, if the data is not available yet. For performance reasons, the asynchronous
        /// counterpart <see cref="GetDataTypesAsync(CancellationToken)"/> should be preferred for the first call.
        /// </remarks>
        /// <value>The data types.</value>
        /// <seealso cref="GetDataTypesAsync(CancellationToken)"/>
        public IDataTypeCollection<IDataType> DataTypes
        {
            get { return _provider.DataTypes; }
        }


        //NamespaceCollection _namespaces = new NamespaceCollection();
        public INamespaceCollection<IDataType> Namespaces
        {
            get { return _provider.Namespaces; }
        }

        public INamespace<IDataType>? RootNamespace
        {
            get { return _provider.RootNamespace; }
        }

        public string RootNamespaceName
        {
            get
            {
                //TODO:
                if (RootNamespace == null)
                    return string.Empty;
                else
                    return RootNamespace.Name;
            }
        }

        public ISymbolLoaderSettings Settings
        {
            get
            {
                //TODO:
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the symbols.
        /// </summary>
        /// <value>The symbols.</value>
        /// <remarks>This property reads the Symbol information synchronously, if the data is not available yet. For performance reasons, the asynchronous
        /// counterpart <see cref="GetSymbolsAsync(CancellationToken)" /> should be preferred for the first call.</remarks>
        /// <seealso cref="GetSymbolsAsync(CancellationToken)"/>
        public ISymbolCollection<ISymbol> Symbols
        {
            get { return _provider.Symbols; }
        }

        public IDynamicSymbolsCollection SymbolsDynamic
        {
            get
            {
                if (_provider.SymbolsInternal != null)
                {
                    return new DynamicSymbolsCollection(_provider.SymbolsInternal);
                }
                else
                {
                    return DynamicSymbolsCollection.Empty;
                }
            }
        }

        public Encoding DefaultValueEncoding
        {
            get
            {
                return this._provider.DefaultValueEncoding;
            }
        }
    }

    public class CustomSymbolProvider : IInternalSymbolProvider
    {
        public CustomSymbolProvider(string rootNamespace, string buildInNamespace)
        {
            _rootName = rootNamespace;
            _buildInName = buildInNamespace;

            Namespace root = new Namespace(rootNamespace);
            Namespace buildIn = new Namespace(buildInNamespace);

            _namespaces.Add(root);
            _namespaces.Add(buildIn);
        }

        string _rootName;
        string _buildInName;

        /// <summary>
        /// Gets the data types
        /// </summary>
        /// <remarks>
        /// This property reads the DataTypes synchronously, if the data is not available yet. For performance reasons, the asynchronous
        /// counterpart <see cref="GetDataTypesAsync(CancellationToken)"/> should be preferred for the first call.
        /// </remarks>
        /// <value>The data types.</value>
        /// <seealso cref="GetDataTypesAsync(CancellationToken)"/>
        public IDataTypeCollection<IDataType> DataTypes
        {
            get { return new ReadOnlyDataTypeCollection(_namespaces.AllTypesInternal); }
        }

        public IDataTypeCollection? DataTypesInternal
        {
            get { return (IDataTypeCollection)_namespaces.AllTypesInternal; }
        }

        NamespaceCollection _namespaces = new NamespaceCollection();

        public INamespaceCollection<IDataType> Namespaces
        {
            get { return _namespaces.AsReadOnly(); }
        }

        public INamespaceCollection NamespacesInternal
        {
            get { return (INamespaceCollection)_namespaces; }
        }

        public INamespace<IDataType> RootNamespace
        {
            get { return _namespaces[_rootName]; }
        }

        public string RootNamespaceName
        {
            get { return _rootName; }
        }

        SymbolCollection _symbols = new SymbolCollection(InstanceCollectionMode.PathHierarchy);

        /// <summary>
        /// Gets the symbols.
        /// </summary>
        /// <value>The symbols.</value>
        /// <remarks>This property reads the Symbol information synchronously, if the data is not available yet. For performance reasons, the asynchronous
        /// counterpart <see cref="GetSymbolsAsync(CancellationToken)" /> should be preferred for the first call.</remarks>
        /// <seealso cref="GetSymbolsAsync(CancellationToken)"/>
        public ISymbolCollection<ISymbol> Symbols
        {
            get { return _symbols.AsReadOnly(); }
        }

        public ISymbolCollection? SymbolsInternal
        {
            get { return _symbols; }
        }

        public Encoding DefaultValueEncoding => Encoding.Unicode;

        /// <summary>
        /// Gets the symbols asynchronously
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="T:TwinCAT.TypeSystem.ResultSymbols" /> parameter contains the data types
        /// (<see cref="P:TwinCAT.TypeSystem.ResultSymbols`1.Symbols" />) and the <see cref="P:TwinCAT.Ads.ResultAds.ErrorCode" /> after execution.</returns>
        /// <seealso cref="P:TwinCAT.TypeSystem.ISymbolServer.Symbols" />
        public Task<ResultSymbols> GetSymbolsAsync(CancellationToken cancel)
        {
            return Task.FromResult(ResultSymbols.CreateSuccess(_symbols.AsReadOnly()));
        }

        /// <summary>
        /// Tries to geth the symbols from the device target.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        /// <returns>AdsErrorCode.</returns>
        public AdsErrorCode TryGetSymbols(out ISymbolCollection<ISymbol>? symbols)
        {
            symbols = this.Symbols;
            return AdsErrorCode.NoError;
        }

        /// <summary>
        /// Gets the data types asynchronously.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="T:TwinCAT.TypeSystem.ResultDataTypes" /> parameter contains the data types
        /// (<see cref="P:TwinCAT.TypeSystem.ResultDataTypes.DataTypes" />) and the <see cref="P:TwinCAT.Ads.ResultAds.ErrorCode" /> after execution.</returns>
        /// <seealso cref="P:TwinCAT.TypeSystem.ISymbolServer.DataTypes" />
        public Task<ResultDataTypes> GetDataTypesAsync(CancellationToken cancel)
        {
            return Task.FromResult(ResultDataTypes.CreateSuccess(this.DataTypes));
        }

        /// <summary>
        /// Tries to get the symbols from the device target.
        /// </summary>
        /// <param name="dataTypes">The data types.</param>
        /// <returns>AdsErrorCode.</returns>
        public AdsErrorCode TryGetDataTypes(out IDataTypeCollection<IDataType>? dataTypes)
        {
            dataTypes = this.DataTypes;
            return AdsErrorCode.NoError;
        }

        /// <summary>
        /// Resets the cached symbolic data.
        /// </summary>
        public void ResetCachedSymbolicData()
        {
        }
    }

    public class CustomBinder : Binder
    {
        public CustomBinder(IInternalSymbolProvider provider, ISymbolFactory factory, bool useVirtualInstances)
            : base(provider, factory, useVirtualInstances)
        {
        }
    }


    public class CustomConnection : IConnection
    {
        string _address;
        bool _connected = false;
        private ISession _session;

        public CustomConnection(ISession session, string address)
        {
            _address = address;
            _connected = true; // Is connected initialy!!
            _session = session;
        }

        private static int s_id = 0;
        private int _id = ++s_id;

        public int Id
        {
            get { return _id; }
        }

        public ISession Session
        {
            get { return _session; }
        }

        public bool IsConnected
        {
            get { return _connected; }
        }

        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        public ConnectionState ConnectionState
        {
            get
            {
                if (IsConnected)
                {
                    return ConnectionState.Connected;
                }
                else
                {
                    return ConnectionState.Disconnected;
                }
            }
        }

        private int _timeout = 5000;

        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public Encoding DefaultValueEncoding => Encoding.Default;

        public void Close()
        {
            if (IsConnected)
            {
                OnConnectionStateChanged(ConnectionStateChangedReason.Closed, ConnectionState.Disconnected, ConnectionState.Connected);
            }
            _connected = false;
        }

        private void OnConnectionStateChanged(ConnectionStateChangedReason reason, ConnectionState newState, ConnectionState oldState)
        {
            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged(this, new ConnectionStateChangedEventArgs(reason, newState, oldState));
            }
        }

        public bool Connect()
        {
            if (!IsConnected)
            {
                _connected = true;
                return true;
            }
            return false;
        }

        public bool Disconnect()
        {
            if (IsConnected)
            {
                _connected = false;
                return true;
            }
            return false;
        }
    }

    public class CustomSymbolServer : ISymbolServer
    {
        ISessionProvider _sessionProvider;
        ISymbolLoader _loader;
        ISession _session;

        public CustomSymbolServer(CustomSession session, bool dynamicObjects, ISessionProvider sessionProvider)
        {
            _sessionProvider = sessionProvider;
            _session = session;
            _loader = new CustomSymbolLoader(session, dynamicObjects, sessionProvider);
        }

        /// <summary>
        /// Gets the data types
        /// </summary>
        /// <remarks>
        /// This property reads the DataTypes synchronously, if the data is not available yet. For performance reasons, the asynchronous
        /// counterpart <see cref="GetDataTypesAsync(CancellationToken)"/> should be preferred for the first call.
        /// </remarks>
        /// <value>The data types.</value>
        /// <seealso cref="GetDataTypesAsync(CancellationToken)"/>
        public IDataTypeCollection<IDataType> DataTypes
        {
            get
            {
                //if (_loader != null)
                return _loader.DataTypes;
                //else
                //    return null;
            }
        }

        /// <summary>
        /// Gets the symbols.
        /// </summary>
        /// <value>The symbols.</value>
        /// <remarks>This property reads the Symbol information synchronously, if the data is not available yet. For performance reasons, the asynchronous
        /// counterpart <see cref="GetSymbolsAsync(CancellationToken)" /> should be preferred for the first call.</remarks>
        /// <seealso cref="GetSymbolsAsync(CancellationToken)"/>
        public ISymbolCollection<ISymbol> Symbols
        {
            get
            {
                //if (_loader != null)
                return _loader.Symbols;
                //else
                //    return null;
            }
        }

        public Encoding DefaultValueEncoding => _loader.DefaultValueEncoding;

        /// <summary>
        /// Gets the data types asynchronously.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="ResultDataTypes"/> parameter contains the data types
        /// (<see cref="ResultDataTypes.DataTypes"/>) and the <see cref="ResultAds.ErrorCode"/> after execution.
        /// </returns>
        /// <seealso cref="DataTypes"/>
        public async Task<ResultDataTypes> GetDataTypesAsync(CancellationToken cancel)
        {
            if (_loader != null)
                return await _loader.GetDataTypesAsync(cancel).ConfigureAwait(false);
            else
                return new ResultDataTypes(AdsErrorCode.InternalError, null);

        }

        //public async Task<ResultDataTypes> GetDataTypesAsync(CancellationToken cancel)
        //{
        //    if (_loader != null)
        //        return await _loader.GetDataTypesAsync(, cancel).ConfigureAwait(false);
        //    else
        //        return new ResultDataTypes(AdsErrorCode.InternalError, null);
        //}

        /// <summary>
        /// Gets the symbols asynchronously
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous 'GetDataTypes' operation. The <see cref="ResultSymbols"/> parameter contains the data types
        /// (<see cref="ResultSymbols{T}.Symbols"/>) and the <see cref="ResultAds.ErrorCode"/> after execution.
        /// </returns>
        /// <seealso cref="Symbols"/>
        public async Task<ResultSymbols> GetSymbolsAsync(CancellationToken cancel)
        {
            if (_loader != null)
                return await _loader.GetSymbolsAsync(cancel).ConfigureAwait(false);
            else
                return new ResultSymbols(AdsErrorCode.InternalError, null);
        }

        public void ResetCachedSymbolicData()
        {
            throw new NotImplementedException();
        }

        //public async Task<ResultSymbols> GetSymbolsAsync(bool forceReload, CancellationToken cancel)
        //{
        //    if (_loader != null)
        //        return await _loader.GetSymbolsAsync(forceReload, cancel).ConfigureAwait(false);
        //    else
        //        return new ResultSymbols(AdsErrorCode.InternalError, null);
        //}

        //public AdsErrorCode TryGetDataTypes(bool forceReload, out IDataTypeCollection<IDataType>? dataTypes)
        //{
        //    throw new NotImplementedException();
        //}

        public AdsErrorCode TryGetDataTypes(out IDataTypeCollection<IDataType>? dataTypes)
        {
            if (_loader != null)
                return _loader.TryGetDataTypes(out dataTypes);
            else
            {
                dataTypes = null;
                return AdsErrorCode.InternalError;
            }
        }

        //public AdsErrorCode TryGetSymbols(bool forceReload, out ISymbolCollection<ISymbol>? symbols)
        //{
        //    throw new NotImplementedException();
        //}

        public AdsErrorCode TryGetSymbols(out ISymbolCollection<ISymbol>? symbols)
        {
            if (_loader != null)
                return _loader.TryGetSymbols(out symbols);
            else
            {
                symbols = null;
                return AdsErrorCode.InternalError;
            }
        }
    }

    public class CustomSession : Session, ISession, ISymbolServerProvider
    {
        string _address = string.Empty;
        private CustomSessionSettings _settings = CustomSessionSettings.Default;

        public CustomSession(string whateverAddress, CustomSessionSettings? settings)
            : base(CustomSessionProvider.Self)
        {
            _address = whateverAddress;

            if (settings != null)
            {
                _settings = settings;
            }
        }

        protected override string GetSessionName()
        {
            return string.Format("CustomSession({0}", _address);
        }

        protected override IConnection? OnConnect(bool reconnect)
        {
            if (!reconnect)
            {
                Connection = new CustomConnection(this, _address);
            }
            return base.OnConnect(reconnect);
        }

        protected override ISymbolServer OnCreateSymbolServer()
        {
            if (_settings.DynamicSymbols)
            {
                return new CustomSymbolServer(this, true, this.Provider);
            }
            else
            {
                return new CustomSymbolServer(this, false, this.Provider);
            }
        }

        protected override string OnGetAddress()
        {
            return _address;
        }
    }

    public class CustomSessionSettings : ISessionSettings
    {
        public CustomSessionSettings(bool dyn)
        {
            _dynamicSymbols = dyn;
        }

        private bool _dynamicSymbols = false;

        public bool DynamicSymbols
        {
            get { return _dynamicSymbols; }
        }

        public static CustomSessionSettings Default
        {
            get { return new CustomSessionSettings(false); }
        }
    }

    [Export(typeof(ISessionProvider))]
    [ExportMetadata("SessionProvider", "Custom")]
    public class CustomSessionProvider : SessionProvider<CustomSession, string, CustomSessionSettings>
    {
        public CustomSessionProvider()
            : base()
        {
        }

        public override string Name
        {
            get
            {
                return "Custom"; // e.g. ADS, MQTT or OPC
            }
        }

        /// <summary>
        /// Creates the Session with specified address and communication settings.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="settings">The communicationSettings.</param>
        /// <returns>S.</returns>
        public override CustomSession Create(string address, CustomSessionSettings? settings)
        {
            return new CustomSession(address, (CustomSessionSettings?)settings);
        }

        /// <summary>
        /// Gets the Singleton instance
        /// </summary>
        /// <value>The self.</value>
        /// <exclude/>
        public static CustomSessionProvider Self
        {
            get
            {
                if (s_self == null)
                {
                    s_self = new CustomSessionProvider();
                }
                return (CustomSessionProvider)s_self;
            }
        }
    }

    /// <summary>
    /// Class CustomValueAccessor.
    /// Implements the <see cref="TwinCAT.ValueAccess.ValueAccessor" />
    /// </summary>
    /// <seealso cref="TwinCAT.ValueAccess.ValueAccessor" />
    public class CustomValueAccessor : ValueAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomValueAccessor"/> class.
        /// </summary>
        /// <param name="factory">The value factory.</param>
        /// <param name="session">The session (if session based) or NULL</param>
        public CustomValueAccessor(IAccessorValueFactory factory, ISession session)
            : base(factory, session)
        {
            _converter = PrimitiveTypeMarshaler.Default;
        }

        private PrimitiveTypeMarshaler _converter;


        /// <summary>
        /// Reads an array element value as bytes.
        /// </summary>
        /// <param name="arrayInstance">The array instance.</param>
        /// <param name="indices">The indices specify which element to read.</param>
        /// <param name="destination">The destination / value memory.</param>
        /// <param name="timeStamp">The read time snapshot</param>
        /// <returns>Error code. 0 represents succeed.</returns>
        public override int TryReadArrayElementRaw(IArrayInstance arrayInstance, int[] indices, Memory<byte> source, out DateTimeOffset? timeStamp)
        {
            timeStamp = DateTimeOffset.Now;

            byte[] arrayValue = new byte[arrayInstance.ByteSize];

            IArrayType? arrayType = (IArrayType?)arrayInstance.DataType;
            IDataType? elementType = arrayType?.ElementType;

            if (arrayType != null)
            {
                int elements = arrayType.Dimensions.ElementCount;
                //int elementSize = arrayType.ElementType.ByteSize;
                int subIndex = ArrayIndexConverter.IndicesToSubIndex(indices, arrayType!);

                //Please fill with a concrete value.
                //In this demo, we return only the subIndex Value for Demo purposes!

                _converter.Marshal(subIndex, source.Span);

                return 0; // Ok
            }
            else
            {
                Debug.Fail("");
                return (int)AdsErrorCode.InternalError;
            }
        }

        /// <summary>
        /// Reads the raw memory value of an array element as asynchronous operation
        /// </summary>
        /// <param name="arrayInstance">The array instance.</param>
        /// <param name="indices">The indices of the element</param>
        /// <param name="destination">Memory location for the value.</param>
        /// <param name="cancel">The cancellation token.</param>
        /// <returns>An asynchronous task that represents the 'ReadArrayElementRaw' operation. The result of the operation is represented by the type <see cref="T:TwinCAT.ValueAccess.ResultReadRawAccess" />.</returns>
        public override Task<ResultReadRawAccess> ReadArrayElementRawAsync(IArrayInstance arrayInstance, int[] indices, Memory<byte> destination, CancellationToken cancel)
        {
            Func<ResultReadRawAccess> func = () =>
            {
                DateTimeOffset? time;
                int errorCode = TryReadArrayElementRaw(arrayInstance, indices, destination, out time);

                if (errorCode == 0)
                    return new ResultReadRawAccess(destination, errorCode, time!.Value, 0);
                else
                    return ResultReadRawAccess.Empty;
            };

            return Task.Run<ResultReadRawAccess>(func, cancel);
        }

        public override int TryReadRaw(ISymbol symbol, Memory<byte> va, out DateTimeOffset? utcReadTime)
        {
            //TODO: this method has a async copy
            utcReadTime = DateTimeOffset.Now;

            if (symbol.IsPrimitiveType)
            {
                object v = 42;

                if (symbol.Category == DataTypeCategory.String)
                {
                    Random random = new Random();
                    int index = random.Next(yodaQuotas.Length - 1);

                    Encoding.Unicode.GetBytes(yodaQuotas[index]).CopyTo(va);
                }
                else if (symbol.Size == 1)
                {
                    byte[] bytes = new byte[] { 42 };
                    bytes.CopyTo(va);
                }
                else if (symbol.Size == 2)
                {
                    BitConverter.GetBytes((short)42).CopyTo(va);
                }
                else if (symbol.Size == 4)
                {
                    BitConverter.GetBytes((int)42).CopyTo(va);
                }
                else if (symbol.Size == 8)
                {
                    BitConverter.GetBytes((long)42).CopyTo(va);
                }
            }
            else if (symbol.Category == DataTypeCategory.Struct)
            {
                List<byte[]> dynamicStructData = new List<byte[]>();

                IStructInstance instance = (IStructInstance)symbol;
                int sumBytes = 0;
                //byte[] bytes = null;

                foreach (IValueRawSymbol mi in instance.MemberInstances)
                {
                    byte[] b = mi.ReadRawValue();
                    dynamicStructData.Add(b);
                    sumBytes += mi.Size;
                }

                //if (sumBytes <= instance.Size)
                //{
                //    bytes = new byte[instance.Size];
                //}
                //else
                //{
                //    //Hack to support Virtual Structs!
                //    bytes = new byte[sumBytes];
                //}

                sumBytes = 0;
                for (int i = 0; i < dynamicStructData.Count; i++)
                {
                    byte[] sourceBytes = dynamicStructData[i];
                    sourceBytes.CopyTo(va.Slice(sumBytes));
                    //Array.Copy(sourceBytes, 0, value, sumBytes, sourceBytes.Length);
                    sumBytes += sourceBytes.Length;
                }
            }
            else if (symbol.Category == DataTypeCategory.Array)
            {
                IArrayInstance instance = (IArrayInstance)symbol;
                int elements = instance.Elements.Count;
                int elementSize = instance.ElementType!.Size;
                int currentIndex = 0;

                //value = new byte[instance.Size];

                ArrayIndexIterator iter = new ArrayIndexIterator((IArrayType)instance.DataType!);

                foreach (int[] indices in iter)
                {
                    DateTimeOffset? time;
                    TryReadArrayElementRaw(instance, indices, va.Slice(currentIndex, elementSize), out time);
                    currentIndex += elementSize;
                }
            }
            return 0;
        }

        public override Task<ResultReadRawAccess> ReadRawAsync(ISymbol symbolInstance, Memory<byte> value, CancellationToken cancel)
        {
            //ReadValueResult result = null;

            Func<ResultReadRawAccess> func = () =>
            {
                DateTimeOffset? time;
                int errorCode = TryReadRaw(symbolInstance, value, out time);

                if (errorCode == 0)
                    return new ResultReadRawAccess(value, errorCode, time!.Value, 0);
                else
                    return ResultReadRawAccess.Empty;
            };

            return Task.Run<ResultReadRawAccess>(func, cancel);
        }

        public override int TryWriteArrayElementRaw(IArrayInstance arrayInstance, int[] indices, ReadOnlyMemory<byte> source, out DateTimeOffset? utcWriteTime)
        {
            throw new NotImplementedException();
        }

        public override Task<ResultWriteAccess> WriteArrayElementRawAsync(IArrayInstance arrayInstance, int[] indices, ReadOnlyMemory<byte> source, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public override int TryWriteRaw(ISymbol symbolInstance, ReadOnlyMemory<byte> value, out DateTimeOffset? utcWriteTime)
        {
            throw new NotImplementedException();
        }

        public override Task<ResultWriteAccess> WriteRawAsync(ISymbol symbolInstance, ReadOnlyMemory<byte> value, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public override void WriteValue(ISymbol symbol, object value, out DateTimeOffset? utcWriteTime)
        {
            throw new NotImplementedException();
        }

        private string[] yodaQuotas = new string[]
        {
            "When nine hundred years old you reach, look as good you will not.",
            "Truly wonderful, the mind of a child is.",
            "That is why you fail.",
            "A Jedi uses the Force for knowledge and defense, never for attack.",
            "Adventure. Excitement. A Jedi craves not these things.",
            "Judge me by my size, do you?",
            "Fear is the path to the dark side…fear leads to anger…anger leads to hate…hate leads to suffering.",
            "Wars not make one great.",
            "Luminous beings are we…not this crude matter.",
            "Do. Or do not. There is no try."
        };
    }
}
