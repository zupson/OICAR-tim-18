using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    private readonly DataCache _data;
    private readonly ApiService      _api;

    public ObservableCollection<FamilyMember> Members => _data.FamilyMembers;

    [ObservableProperty] private bool   _isAddFormOpen;
    [ObservableProperty] private string _newEmail    = string.Empty;
    [ObservableProperty] private string _addError    = string.Empty;
    [ObservableProperty] private string _addSuccess  = string.Empty;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;
    public bool    HasNoMembers   => _data.FamilyMembers.Count == 0;

    public FamilyViewModel(DataCache data, ApiService api)
    {
        _data = data;
        _api  = api;
        _data.PropertyChanged += (_, _) => { OnPropertyChanged(nameof(CurrencySymbol)); OnPropertyChanged(nameof(ExchangeRate)); };
        _data.FamilyMembers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoMembers));
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        IsAddFormOpen = !IsAddFormOpen;
        NewEmail   = string.Empty;
        AddError   = string.Empty;
        AddSuccess = string.Empty;
    }

    private static bool IsValidEmail(string email)
    {
        var idx = email.IndexOf('@');
        return idx > 0 && idx < email.Length - 2 && email.LastIndexOf('.') > idx + 1;
    }

    private static string FriendlyError(Exception ex)
    {
        if (ex is HttpRequestException hre)
        {
            return hre.StatusCode switch
            {
                HttpStatusCode.NotFound            => "Korisnik s tim emailom nije pronađen.",
                HttpStatusCode.BadRequest          => "Neispravni podaci. Provjerite email adresu.",
                HttpStatusCode.Conflict            => "Korisnik je već član ili je pozivnica već poslana.",
                HttpStatusCode.Forbidden           => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.Unauthorized        => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.InternalServerError => "Greška na poslužitelju. Pokušajte ponovo.",
                _                                  => "Neočekivana greška. Pokušajte ponovo."
            };
        }
        return "Neočekivana greška. Pokušajte ponovo.";
    }

    [RelayCommand]
    private async Task AddMember()
    {
        AddError   = string.Empty;
        AddSuccess = string.Empty;

        if (string.IsNullOrWhiteSpace(NewEmail))  { AddError = "Unesite email adresu."; return; }
        if (!IsValidEmail(NewEmail))               { AddError = "Email adresa nije ispravna."; return; }

        var targetGroupId = _api.FamilyGroupId != 0 ? _api.FamilyGroupId : _api.PersonalGroupId;
        if (targetGroupId == 0) { AddError = "Greška: niste prijavljeni."; return; }

        try
        {
            await _api.PostAsync<object, object>(
                $"/api/Invitation/CreateNewInvitation/{targetGroupId}",
                new { email = NewEmail });

            AddSuccess    = $"Pozivnica je poslana na {NewEmail}.";
            NewEmail      = string.Empty;
            IsAddFormOpen = false;
        }
        catch (Exception ex)
        {
            AddError = FriendlyError(ex);
        }
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
