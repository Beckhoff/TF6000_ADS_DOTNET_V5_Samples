using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwinCAT.TypeSystem;

namespace AdsSessionTest
{
    /// <summary>
    /// Interaction logic for SymbolsControl.xaml
    /// </summary>
    public partial class DataTypesControl : UserControl
    {
        public DataTypesControl()
        {
            InitializeComponent();
        }

        public void SetSymbols(IEnumerable<ISymbol> symbols, IEnumerable<IDataType> dataTypes)
        {
            _model = new DataTypesTreeViewModel(dataTypes);
            base.DataContext = _model;
        }

        DataTypesTreeViewModel _model = null;

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _model.SearchCommand.Execute(null);
        }
    }
}
