using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is ReportsViewModel vm)
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(ReportsViewModel.IsAddFormOpen) && vm.IsAddFormOpen)
                        Dispatcher.BeginInvoke(() => DescriptionField.Focus(), DispatcherPriority.Input);
                };
        };
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not ReportsViewModel vm) return;
        if (e.Key == Key.Enter)  vm.AddTransactionCommand.Execute(null);
        if (e.Key == Key.Escape) vm.ToggleAddFormCommand.Execute(null);
    }
}
