using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class BudgetView : UserControl
{
    public BudgetView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is BudgetViewModel vm)
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(BudgetViewModel.IsAddFormOpen) && vm.IsAddFormOpen)
                        Dispatcher.BeginInvoke(() => BudgetNameField.Focus(), DispatcherPriority.Input);
                };
        };
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not BudgetViewModel vm) return;
        if (e.Key == Key.Enter)  vm.AddBudgetCommand.Execute(null);
        if (e.Key == Key.Escape) vm.ToggleAddFormCommand.Execute(null);
    }
}
