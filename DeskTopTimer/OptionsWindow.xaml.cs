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

namespace DeskTopTimer
{
    /// <summary>
    /// FontSelectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsWindow : MahApps.Metro.Controls.MetroWindow 
    {
        MainWorkSpace? viewModel = null;
        public OptionsWindow()
        {
            InitializeComponent();
            DataContextChanged += OptionsWindow_DataContextChanged;
        }

        private void OptionsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = e.NewValue as MainWorkSpace;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //viewModel.ShouldOpenMakeFontPack = true;
        }
    }
}
