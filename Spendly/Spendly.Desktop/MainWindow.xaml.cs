using System.Windows;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
