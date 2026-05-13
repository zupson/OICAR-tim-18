using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    private readonly MockDataService _data;

    public ObservableCollection<FamilyMember> Members => _data.FamilyMembers;

    [ObservableProperty] private bool   _isAddFormOpen;
    [ObservableProperty] private string _newName  = string.Empty;
    [ObservableProperty] private string _newEmail = string.Empty;
    [ObservableProperty] private string _newRole  = "Član";
    [ObservableProperty] private string _addError = string.Empty;

    public List<string> RoleOptions { get; } = ["Član", "Vlasnik"];

    private static readonly string[] AvatarColors = ["#3B82F6", "#22C55E", "#F59E0B", "#8B5CF6", "#EF4444", "#06B6D4"];

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;
    public bool    HasNoMembers   => _data.FamilyMembers.Count == 0;

    public FamilyViewModel(MockDataService data)
    {
        _data = data;
        _data.PropertyChanged += (_, _) => { OnPropertyChanged(nameof(CurrencySymbol)); OnPropertyChanged(nameof(ExchangeRate)); };
        _data.FamilyMembers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoMembers));
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        IsAddFormOpen = !IsAddFormOpen;
        NewName  = string.Empty;
        NewEmail = string.Empty;
        NewRole  = "Član";
        AddError = string.Empty;
    }

    [RelayCommand]
    private void AddMember()
    {
        if (string.IsNullOrWhiteSpace(NewName))  { AddError = "Unesite ime člana."; return; }
        if (string.IsNullOrWhiteSpace(NewEmail)) { AddError = "Unesite email adresu."; return; }

        var color = AvatarColors[_data.FamilyMembers.Count % AvatarColors.Length];

        _data.FamilyMembers.Add(new FamilyMember
        {
            Name         = NewName,
            Email        = NewEmail,
            Role         = NewRole,
            AvatarColor  = color,
            TotalIncome  = 0,
            TotalSpent   = 0,
        });

        IsAddFormOpen = false;
        AddError      = string.Empty;
    }

    [RelayCommand]
    private void RequestDeleteMember(FamilyMember member)
    {
        foreach (var m in _data.FamilyMembers) m.IsConfirmingDelete = false;
        member.IsConfirmingDelete = true;
    }

    [RelayCommand]
    private void ConfirmDeleteMember(FamilyMember member) => _data.FamilyMembers.Remove(member);

    [RelayCommand]
    private void CancelDeleteMember(FamilyMember member) => member.IsConfirmingDelete = false;
}
