//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using TwinCAT.TypeSystem;

//namespace AdsSessionTest
//{
//    public class DataTypeTemplateSelector : DataTemplateSelector
//    {
//        public override DataTemplate SelectTemplate(object item, DependencyObject container)
//        {
//            FrameworkElement element = container as FrameworkElement;

//            if (element != null && item != null)
//            {

//                if (item is IStructType)
//                {
//                    return element.FindResource("StructTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IArrayType)
//                {
//                    return element.FindResource("ArrayTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IAliasType)
//                {
//                    return element.FindResource("AliasTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IEnumType)
//                {
//                    return element.FindResource("EnumTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IMember)
//                {
//                    return element.FindResource("MemberTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IReferenceType)
//                {
//                    return element.FindResource("ReferenceTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IPointerType)
//                {
//                    return element.FindResource("PointerTemplate") as HierarchicalDataTemplate;
//                }
//                else if (item is IStringType)
//                {
//                    return element.FindResource("StringTemplate") as DataTemplate;
//                }
//                else if (item is IDataType)
//                {
//                    return element.FindResource("DataTypeTemplate") as DataTemplate;
//                }
//            }

//            return null;
//        }
//    }
//}
