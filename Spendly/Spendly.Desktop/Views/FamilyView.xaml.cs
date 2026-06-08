using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class FamilyView : UserControl
{
    public FamilyView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is FamilyViewModel vm)
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(FamilyViewModel.IsAddFormOpen) && vm.IsAddFormOpen)
                        Dispatcher.BeginInvoke(() => EmailField.Focus(), DispatcherPriority.Input);
                    if (e.PropertyName == nameof(FamilyViewModel.IsJoinFormOpen) && vm.IsJoinFormOpen)
                        Dispatcher.BeginInvoke(() => TokenField.Focus(), DispatcherPriority.Input);
                };
        };
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not FamilyViewModel vm) return;
        if (e.Key == Key.Enter)  vm.AddMemberCommand.Execute(null);
        if (e.Key == Key.Escape) vm.ToggleAddFormCommand.Execute(null);
    }

    private void JoinForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not FamilyViewModel vm) return;
        if (e.Key == Key.Enter)  vm.JoinGroupCommand.Execute(null);
        if (e.Key == Key.Escape) vm.ToggleJoinFormCommand.Execute(null);
    }
}
