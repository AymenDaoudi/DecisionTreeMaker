using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using DecisionTreeMaker.ViewModel;

namespace DecisionTreeMaker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void UIElement_OnDrop(object sender, DragEventArgs args)
        {
            var vm = ((MainViewModel)this.DataContext);
            if (vm.DropCsvFileCommand.CanExecute(args))
                vm.DropCsvFileCommand.Execute(args);
        }
    }
}
