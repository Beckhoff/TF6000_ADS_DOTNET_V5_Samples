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
using System.Windows.Shapes;
using TwinCAT.TypeSystem;

namespace AdsSessionTest
{
    /// <summary>
    /// Interaction logic for SymbolsWindow.xaml
    /// </summary>
    public partial class SymbolsWindow : Window
    {
        public SymbolsWindow()
        {
            InitializeComponent();
        }

        public void SetSymbols(IEnumerable<ISymbol> symbols, IEnumerable<IDataType> dataTypes)
        {
            symbolsControl.SetSymbols(symbols, dataTypes);
            dataTypesControl.SetSymbols(symbols, dataTypes);
        }
    }
}
