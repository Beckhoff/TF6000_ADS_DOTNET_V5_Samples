using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TwinCAT.TypeSystem;

namespace AdsSessionTest
{
    public class SymbolTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {

                if (item is IStructInstance)
                {
                    return element.FindResource("StructTemplate") as HierarchicalDataTemplate;
                }
                else if (item is IArrayInstance)
                {
                    return element.FindResource("ArrayTemplate") as HierarchicalDataTemplate;
                }
                else if (item is IPointerInstance)
                {
                    return element.FindResource("PointerTemplate") as HierarchicalDataTemplate;
                }
                else if (item is IReferenceInstance)
                {
                    return element.FindResource("ReferenceTemplate") as HierarchicalDataTemplate;
                }
                else if (item is ISymbol)
                {
                    return element.FindResource("SymbolTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}
