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
    [ObservableProperty] private string _actionError = string.Empty;

    [ObservableProperty] private bool   _isJoinFormOpen;
    [ObservableProperty] private string _joinToken   = string.Empty;
    [ObservableProperty] private string _joinError   = string.Empty;
    [ObservableProperty] private string _joinSuccess = string.Empty;

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
        if (IsAddFormOpen) IsJoinFormOpen = false;
    }

    [RelayCommand]
    private void ToggleJoinForm()
    {
        IsJoinFormOpen = !IsJoinFormOpen;
        JoinToken   = string.Empty;
        JoinError   = string.Empty;
        JoinSuccess = string.Empty;
        if (IsJoinFormOpen) IsAddFormOpen = false;
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
                HttpStatusCode.NotFound            => ServerMessage(hre) ?? "Korisnik nije pronađen.",
                HttpStatusCode.BadRequest          => ServerMessage(hre) ?? "Neispravan zahtjev.",
                HttpStatusCode.Conflict            => "Korisnik je već član ili je pozivnica već poslana.",
                HttpStatusCode.Forbidden           => ServerMessage(hre) ?? "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.Unauthorized        => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.InternalServerError => "Greška na poslužitelju. Pokušajte ponovo.",
                _                                  => "Neočekivana greška. Pokušajte ponovo."
            };
        }
        return "Neočekivana greška. Pokušajte ponovo.";
    }

    // ApiService baca HttpRequestException s tijelom odgovora kao porukom.
    // Vraćamo tu poruku ako je čitljiv tekst (ne JSON).
    private static string? ServerMessage(HttpRequestException hre)
    {
        var body = hre.Message?.Trim();
        if (string.IsNullOrEmpty(body) || body.StartsWith('{') || body.StartsWith('[')) return null;
        return body;
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
    private async Task JoinGroup()
    {
        JoinError   = string.Empty;
        JoinSuccess = string.Empty;

        var token = JoinToken?.Trim() ?? string.Empty;
        if (token.Length == 0) { JoinError = "Unesite token pozivnice."; return; }

        try
        {
            // Postojeći endpoint: token se prosljeđuje kao query parametar.
            await _api.PostAsync<object, ApiUserGroup>(
                $"/api/Invitation/ClaimInvitation?token={Uri.EscapeDataString(token)}",
                new { });

            await RefreshGroupsAndData();

            JoinSuccess    = "Uspješno ste se pridružili grupi.";
            JoinToken      = string.Empty;
            IsJoinFormOpen = false;
        }
        catch (Exception ex)
        {
            JoinError = TranslateJoinError(ex);
        }
    }

    // Nakon pridruživanja ponovno razriješi grupe i učitaj podatke.
    private async Task RefreshGroupsAndData()
    {
        var allGroups    = await _api.GetAsync<ApiUserGroup[]>("/api/UserGroup/GetAllUserGroups") ?? [];
        var personal     = allGroups.FirstOrDefault(g => g.IsPersonal);
        var familyGroups = allGroups.Where(g => !g.IsPersonal).OrderBy(g => g.Id).ToArray();

        if (personal != null)
        {
            _api.PersonalGroupId     = personal.GroupId;
            _api.PersonalUserGroupId = personal.Id;
        }
        if (familyGroups.Length > 0)
        {
            _api.FamilyGroupId     = familyGroups[0].GroupId;
            _api.FamilyUserGroupId = familyGroups[0].Id;
        }

        await _data.LoadFromApiAsync(_api);
    }

    private static string TranslateJoinError(Exception ex)
    {
        if (ex is HttpRequestException hre)
        {
            var msg = ServerMessage(hre);
            if (msg != null)
            {
                var lower = msg.ToLowerInvariant();
                if (lower.Contains("invalid token"))                return "Neispravan token pozivnice.";
                if (lower.Contains("not sent to your account"))     return "Ova pozivnica nije poslana na vaš račun.";
                if (lower.Contains("expired"))                      return "Pozivnica je istekla.";
                if (lower.Contains("already used"))                 return "Pozivnica je već iskorištena.";
                if (lower.Contains("already in group"))             return "Već ste član ove grupe.";
                if (lower.Contains("already a member of a family")) return "Već ste član obiteljske grupe. Napustite je prije pridruživanja novoj.";
                return msg;
            }
        }
        return FriendlyError(ex);
    }

    [RelayCommand]
    private void RequestDeleteMember(FamilyMember member)
    {
        ActionError = string.Empty;
        foreach (var m in _data.FamilyMembers) m.IsConfirmingDelete = false;
        member.IsConfirmingDelete = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteMember(FamilyMember member)
    {
        ActionError = string.Empty;

        if (member.IsCurrentUser)
        {
            member.IsConfirmingDelete = false;
            ActionError = "Ne možete ukloniti sami sebe iz grupe.";
            return;
        }

        try
        {
            await _api.DeleteAsync($"/api/UserGroup/RemoveMember/{member.UserGroupId}");
            _data.FamilyMembers.Remove(member);
        }
        catch (Exception ex)
        {
            member.IsConfirmingDelete = false;
            ActionError = FriendlyError(ex);
        }
    }

    [RelayCommand]
    private void CancelDeleteMember(FamilyMember member) => member.IsConfirmingDelete = false;
}
