using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace AdsSessionTest
{
    public class SymbolsTreeViewModel
    {
        public SymbolsTreeViewModel(IEnumerable<ISymbol> symbols)
        {
            _symbols = new ReadOnlyCollection<SymbolViewModel>(
               (from symbol in symbols
                select new SymbolViewModel(symbol))
                .ToList<SymbolViewModel>());

            _searchCommand = new SearchSymbolsCommand(this);
        }

        ReadOnlyCollection<SymbolViewModel> _symbols = null;

        /// <summary>
        /// </summary>
        public ReadOnlyCollection<SymbolViewModel> Symbols
        {
            get { return _symbols; }
        }

        readonly ICommand _searchCommand;
        /// <summary>
        /// Returns the command used to execute a search in the family tree.
        /// </summary>
        public ICommand SearchCommand
        {
            get { return _searchCommand; }
        }

        IEnumerator<SymbolViewModel> _matchingSymbols;
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
                _matchingSymbols = null;
            }
        }

        void PerformSearch()
        {
            if (_matchingSymbols == null || !_matchingSymbols.MoveNext())
                this.VerifyMatchingSymbolsEnumerator();

            var symbol = _matchingSymbols.Current;

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
            var matches = this.FindMatches(_searchText, _symbols);
            _matchingSymbols = matches.GetEnumerator();

            if (!_matchingSymbols.MoveNext())
            {
                MessageBox.Show("No matching names were found.","Try Again",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }

        IEnumerable<SymbolViewModel> FindMatches(string searchText, IEnumerable<SymbolViewModel> symbols)
        {
            foreach (SymbolViewModel symbol in symbols)
            {

                if (symbol.NameContainsText(searchText))
                    yield return symbol;


                if (symbol.SubSymbols != null && symbol.SubSymbols.Count > 0)
                {
                    foreach (SymbolViewModel match in this.FindMatches(searchText, symbol.SubSymbols))
                    {
                        yield return match;
                    }
                }
            }
        }

        public class SearchSymbolsCommand : ICommand
        {
            readonly SymbolsTreeViewModel _symbolsTree;

            public SearchSymbolsCommand(SymbolsTreeViewModel symbolsTree)
            {
                _symbolsTree = symbolsTree;
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
                _symbolsTree.PerformSearch();
            }
        }
    }

    public class SymbolViewModel : INotifyPropertyChanged
    {
        public SymbolViewModel(ISymbol symbol)
            : this(symbol,null)
        {
        }

        private SymbolViewModel(ISymbol symbol, SymbolViewModel parent)
        {
            _symbol = (IAdsSymbol)symbol;
            _parent = parent;

            if (_symbol is IStructInstance)
            {
                IStructInstance si = (IStructInstance)_symbol;

                _subSymbols = new ReadOnlyCollection<SymbolViewModel>(
                    (from member in si.MemberInstances
                     select new SymbolViewModel(member, this))
                     .ToList<SymbolViewModel>());
            }
            else if (_symbol is IArrayInstance)
            {
                IArrayInstance ai = (IArrayInstance)_symbol;

                _subSymbols = new ReadOnlyCollection<SymbolViewModel>(
                    (from element in ai.Elements
                     select new SymbolViewModel(element, this))
                     .ToList<SymbolViewModel>());
            }
            else if (_symbol is IPointerInstance)
            {
                IPointerInstance pi = (IPointerInstance)_symbol;

                _subSymbols = new ReadOnlyCollection<SymbolViewModel>(
                    new SymbolViewModel[] { new SymbolViewModel(pi.Reference, this) }
                   );
            }
            //else if (_symbol is IReferenceInstance)
            //{
            //    IReferenceInstance ri = (IReferenceInstance)_symbol;
            //    _subSymbols = new ReadOnlyCollection<SymbolViewModel>(
            //        new SymbolViewModel[] { new SymbolViewModel(ri.Reference, this) }
            //       );
            //}
        }


        IAdsSymbol _symbol = null;


        SymbolViewModel _parent = null;

        public SymbolViewModel Parent
        {
            get { return _parent; }
        }

        ReadOnlyCollection<SymbolViewModel> _subSymbols = null;

        public ReadOnlyCollection<SymbolViewModel> SubSymbols
        {
            get { return _subSymbols; }
        }

        public string InstanceName
        {
            get { return _symbol.InstanceName; }
        }

        public string InstancePath
        {
            get { return _symbol.InstancePath; }
        }

        public string TypeName
        {
            get { return _symbol.TypeName; }
        }

        public DataTypeCategory Category
        {
            get { return _symbol.Category; }
        }

        public string Comment
        {
            get
            {
                return _symbol.Comment;
            }
        }

        public int Level
        {
            get { return 0; }
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

        public string Offset
        {
            get
            {
                IStructInstance si = _symbol.Parent as IStructInstance;

                if (si != null)
                {
                    IStructType st = si.DataType as IStructType;

                    if (st != null)
                    {
                        IMember member = st.AllMembers[_symbol.InstanceName];
                        return member.Offset.ToString();
                    }
                }
                
                //Hack
                //IMember member = _symbol.Parent._symbol.InstanceName as IMember;
                return string.Empty;
            }
        }

        public int Size
        {
            get { return _symbol.Size; }
        }
        public uint IndexGroup
        {
            get { return _symbol.IndexGroup; }
        }
        public uint IndexOffset
        {
            get { return _symbol.IndexOffset; }
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
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.InstanceName))
                return false;

            return this.InstanceName.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }
    }

    // Exposes attached behaviors that can be
    /// applied to TreeViewItem objects.
    /// </summary>
    public static class TreeViewItemBehavior
    {
        #region IsBroughtIntoViewWhenSelected

        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
        }

        public static void SetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty = DependencyProperty.RegisterAttached("IsBroughtIntoViewWhenSelected",typeof(bool),typeof(TreeViewItemBehavior),new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified.  Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if (!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}
