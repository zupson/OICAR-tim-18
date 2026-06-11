using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        if (e.Key == Key.Enter)
        {
            // Enter unutar DatePickera potvrđuje upisani datum — ne šalji formu.
            if (IsWithinDatePicker(e.OriginalSource as DependencyObject)) return;
            vm.AddTransactionCommand.Execute(null);
        }
        if (e.Key == Key.Escape) vm.ToggleAddFormCommand.Execute(null);
    }

    private static bool IsWithinDatePicker(DependencyObject? source)
    {
        while (source != null)
        {
            if (source is DatePicker) return true;
            source = source is Visual ? VisualTreeHelper.GetParent(source) : null;
        }
        return false;
    }
}
