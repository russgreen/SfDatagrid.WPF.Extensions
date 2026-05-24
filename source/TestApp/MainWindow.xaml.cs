using SfDatagrid.WPF.Extensions;
using Syncfusion.UI.Xaml.Grid;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestApp.Model;
using TestApp.ViewModel;

namespace TestApp;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.ordersGrid.UseCtrlDragFillSelectionController<OrderInfo>();

    }

    private void sfDataGridSheets_CurrentCellValidated(object sender, CurrentCellValidatedEventArgs e)
    {
        if (e.NewValue != e.OldValue)
        {
            Debug.WriteLine($"Cell value changed from '{e.OldValue}' to '{e.NewValue}'");
        }
    }


}