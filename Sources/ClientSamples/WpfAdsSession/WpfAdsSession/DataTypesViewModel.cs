using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwinCAT.TypeSystem;

namespace AdsSessionTest
{

    public class DataTypesTreeViewModel
    {
        public DataTypesTreeViewModel(IEnumerable<IDataType> dataTypes)
        {
            _dataTypes = new ReadOnlyCollection<DataTypeViewModel>(
               (from dataType in dataTypes
                select new DataTypeViewModel(dataType))
                .ToList<DataTypeViewModel>());

            _searchCommand = new SearchDataTypesCommand(this);
        }

        ReadOnlyCollection<DataTypeViewModel> _dataTypes = null;

        /// <summary>
        /// </summary>
        public ReadOnlyCollection<DataTypeViewModel> DataTypes
        {
            get { return _dataTypes; }
        }

        readonly ICommand _searchCommand;
        /// <summary>
        /// Returns the command used to execute a search in the family tree.
        /// </summary>
        public ICommand SearchCommand
        {
            get { return _searchCommand; }
        }

        IEnumerator<DataTypeViewModelBase> _matchingItems;
        string _searchText = String.Empty;

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText)
                    return;

                _searchText = value;
                _matchingItems = null;
            }
        }

        void PerformSearch()
        {
            if (_matchingItems == null || !_matchingItems.MoveNext())
                this.VerifyMatchingSymbolsEnumerator();

            var symbol = _matchingItems.Current;

            if (symbol == null)
                return;

            // Ensure that this person is in view.
            if (symbol.Parent != null)
            {
                symbol.Parent.IsExpanded = true;
            }

            symbol.IsSelected = true;
        }

        void VerifyMatchingSymbolsEnumerator()
        {
            var matches = this.FindMatches(_searchText, _dataTypes);
            _matchingItems = matches.GetEnumerator();

            if (!_matchingItems.MoveNext())
            {
                MessageBox.Show("No matching names were found.","Try Again",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }

        IEnumerable<DataTypeViewModelBase> FindMatches(string searchText, IEnumerable<DataTypeViewModelBase> symbols)
        {
            foreach (DataTypeViewModelBase symbol in symbols)
            {

                if (symbol.NameContainsText(searchText))
                    yield return symbol;


                if (symbol.SubSymbols != null && symbol.SubSymbols.Count > 0)
                {
                    foreach (DataTypeViewModelBase match in this.FindMatches(searchText, symbol.SubSymbols))
                    {
                        yield return match;
                    }
                }
            }
        }

        public class SearchDataTypesCommand : ICommand
        {
            readonly DataTypesTreeViewModel _dataTypesTree;

            public SearchDataTypesCommand(DataTypesTreeViewModel dataTypesTree)
            {
                _dataTypesTree = dataTypesTree;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                // I intentionally left these empty because
                // this command never raises the event, and
                // not using the WeakEvent pattern here can
                // cause memory leaks.  WeakEvent pattern is
                // not simple to implement, so why bother.
                add { }
                remove { }
            }

            public void Execute(object parameter)
            {
                _dataTypesTree.PerformSearch();
            }
        }
    }

    public abstract class DataTypeViewModelBase : INotifyPropertyChanged
    {
        protected IDataType _dataType = null;

        protected DataTypeViewModelBase(IDataType type)
        {
            _dataType = type;
        }

        protected DataTypeViewModelBase(IDataType type, DataTypeViewModelBase parent)
        {
            //Debug.Assert(type != null);
            _dataType = type;
            _parent = parent;
        }

        protected DataTypeViewModelBase _parent = null;

        public DataTypeViewModelBase Parent
        {
            get { return _parent; }
        }

        protected ReadOnlyCollection<MemberViewModel> _members = null;

        public ReadOnlyCollection<MemberViewModel> SubSymbols
        {
            get { return _members; }
        }

        public abstract string Name { get; }

        public virtual string TypeName
        {
            get
            {
                if (_dataType != null)
                    return _dataType.Name;
                else
                    return "Not Resolved";
            }
        }

        public virtual DataTypeCategory Category
        {
            get
            {
                if (_dataType != null)
                    return _dataType.Category;
                else
                    return DataTypeCategory.None;
            }
        }

        public string BaseType
        {
            get
            {
                if (_dataType is IAliasType)
                {
                    return ((IAliasType)_dataType).BaseTypeName;
                }
                else if (_dataType is IArrayType)
                {
                    return ((IArrayType)_dataType).ElementType.Name;
                }
                else if (_dataType is IStructType)
                {
                    return ((IStructType)_dataType).BaseTypeName;
                }
                else if (_dataType is IReferenceType)
                {
                    IReferenceType rt = _dataType as IReferenceType;
                    if (rt != null && rt.ReferencedType != null)
                    {
                        return rt.ReferencedType.Name;
                    }
                }
                else if (_dataType is IPointerType)
                {
                    IPointerType rt = _dataType as IPointerType;
                    if (rt != null && rt.ReferencedType != null)
                    {
                        return rt.ReferencedType.Name;
                    }
                }
                else if (_dataType is IEnumType)
                {
                    return ((IEnumType)_dataType).BaseTypeName;
                }
                return string.Empty;
            }
        }

        public string Image
        {
            get
            {
                switch (this.Category)
                {
                    case DataTypeCategory.Enum:
                        return "Resources/Enum.ico";
                    case DataTypeCategory.Struct:
                    case DataTypeCategory.FunctionBlock:
                    case DataTypeCategory.Program:
                    case DataTypeCategory.Function:
                        return "Resources/Struct.ico";
                    case DataTypeCategory.Alias:
                        return "Resources/Alias.ico";
                    case DataTypeCategory.Array:
                        return "Resources/Array.ico";
                    case DataTypeCategory.String:
                        return "Resources/String.ico";


                    case DataTypeCategory.None:
                    case DataTypeCategory.Primitive:
                    case DataTypeCategory.SubRange:
                    case DataTypeCategory.Bitset:
                    case DataTypeCategory.Pointer:
                    case DataTypeCategory.Union:
                    case DataTypeCategory.Reference:
                    default:
                        return "Resources/DataType.ico";
                }
            }
        }

        public virtual int Size
        {
            get
            {
                if (_dataType != null)
                    return _dataType.Size;

                return 0;
            }
        }

        public virtual string Offset
        {
            get
            {
                return string.Empty;
            }
        }

        public string Comment
        {
            get
            {
                if (_dataType != null)
                    return _dataType.Comment;
                else
                    return string.Empty;
            }
        }

        bool _isExpanded;
        bool _isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
                return false;

            return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }

    public class DataTypeViewModel : DataTypeViewModelBase
    {
        public DataTypeViewModel(IDataType dataType)
            : this(dataType,null)
        {
        }

        protected DataTypeViewModel(IDataType dataType, DataTypeViewModel parent)
            : base(dataType)
        {
            if (_dataType is IStructType)
            {
                IStructType st = (IStructType)_dataType;

                _members = new ReadOnlyCollection<MemberViewModel>(
                    (from member in st.Members
                     select new MemberViewModel(member, this))
                     .ToList<MemberViewModel>());
            }
        }

        public override string Name
        {
            get { return _dataType.Name; }
        }
    }

    public class MemberViewModel : DataTypeViewModelBase
    {
        IMember _member = null;

        public MemberViewModel(IMember member, DataTypeViewModel parent)
            : base(member.DataType, parent)
        {
            if (member == null) throw new ArgumentNullException("member");

            //ATTENTION: Actualy member.DataType can be NULL if DataType is not resolved!
            _member = member;
        }

        public override string Name
        {
            get { return _member.InstanceName; }
        }

        public override string TypeName
        {
            get
            {
                return _member.TypeName;
            }
        }

        public override string Offset
        {
            get
            {
                return _member.Offset.ToString();
            }
        }

    }
}
