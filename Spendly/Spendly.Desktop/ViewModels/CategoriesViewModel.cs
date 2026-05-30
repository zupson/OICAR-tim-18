using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public class CategoryItem
{
    public int     TypeId               { get; init; }
    public string  Name                 { get; init; } = string.Empty;
    public bool    IsRevenue            { get; init; }
    public int     Count                { get; init; }
    public decimal Total                { get; init; }
    public int     Pct                  { get; init; }
    public int     Remaining            => 100 - Pct;
    public string  Color                { get; init; } = "#3B82F6";
    public string  TypeLabel            => IsRevenue ? "Prihod" : "Trošak";
    public string  Symbol               { get; init; } = "€";
    public decimal Rate                 { get; init; } = 1m;
    public bool    IsConfirmingDelete   { get; init; }
    public bool    IsNotConfirmingDelete => !IsConfirmingDelete;
}

public partial class CategoriesViewModel : ObservableObject
{
    private readonly DataCache  _data;
    private readonly ApiService _api;

    private static readonly string[] Colors = ["#3B82F6", "#22C55E", "#F59E0B", "#8B5CF6", "#EF4444", "#06B6D4"];

    public ObservableCollection<CategoryItem> ExpenseCategories { get; } = [];
    public ObservableCollection<CategoryItem> RevenueCategories { get; } = [];

    [ObservableProperty] private bool   _isModalOpen;
    [ObservableProperty] private string _modalName  = string.Empty;
    [ObservableProperty] private bool   _isRevenue;
    [ObservableProperty] private bool   _isEditMode;
    [ObservableProperty] private string _modalError  = string.Empty;
    [ObservableProperty] private bool   _isSaving;
    [ObservableProperty] private string _deleteError = string.Empty;
    private int _editingId;
    private int _pendingDeleteTypeId = -1;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;

    public CategoriesViewModel(DataCache data, ApiService api)
    {
        _data = data;
        _api  = api;
        data.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrencySymbol));
            OnPropertyChanged(nameof(ExchangeRate));
            Rebuild();
        };
        data.Transactions.CollectionChanged += (_, _) => Rebuild();
        Rebuild();
    }

    public bool HasNoExpenseCategories => ExpenseCategories.Count == 0;
    public bool HasNoRevenueCategories => RevenueCategories.Count == 0;

    public void Rebuild()
    {
        var allExp   = _data.Transactions.Where(t => t.Type == TransactionType.Expense).ToList();
        var allInc   = _data.Transactions.Where(t => t.Type == TransactionType.Income).ToList();
        var totalExp = allExp.Sum(t => t.Amount) is var te && te == 0 ? 1m : te;
        var totalInc = allInc.Sum(t => t.Amount) is var ti && ti == 0 ? 1m : ti;

        ExpenseCategories.Clear();
        var expTypes = _data.TypeOptions.Where(t => !t.IsRevenue).ToList();
        for (int i = 0; i < expTypes.Count; i++)
        {
            var t    = expTypes[i];
            var txns = allExp.Where(x => x.TypeId == t.TypeId).ToList();
            var tot  = txns.Sum(x => x.Amount);
            ExpenseCategories.Add(new CategoryItem { TypeId = t.TypeId, Name = t.Name, IsRevenue = false,
                Count = txns.Count, Total = tot, Pct = (int)Math.Round(tot / totalExp * 100),
                Color = Colors[i % Colors.Length], Symbol = CurrencySymbol, Rate = ExchangeRate,
                IsConfirmingDelete = t.TypeId == _pendingDeleteTypeId });
        }

        RevenueCategories.Clear();
        var incTypes = _data.TypeOptions.Where(t => t.IsRevenue).ToList();
        for (int i = 0; i < incTypes.Count; i++)
        {
            var t    = incTypes[i];
            var txns = allInc.Where(x => x.TypeId == t.TypeId).ToList();
            var tot  = txns.Sum(x => x.Amount);
            RevenueCategories.Add(new CategoryItem { TypeId = t.TypeId, Name = t.Name, IsRevenue = true,
                Count = txns.Count, Total = tot, Pct = (int)Math.Round(tot / totalInc * 100),
                Color = Colors[i % Colors.Length], Symbol = CurrencySymbol, Rate = ExchangeRate,
                IsConfirmingDelete = t.TypeId == _pendingDeleteTypeId });
        }

        OnPropertyChanged(nameof(HasNoExpenseCategories));
        OnPropertyChanged(nameof(HasNoRevenueCategories));
    }

    private static string FriendlyError(Exception ex)
    {
        if (ex is HttpRequestException hre)
        {
            var body = hre.Message?.Trim() ?? string.Empty;
            return hre.StatusCode switch
            {
                HttpStatusCode.BadRequest          => "Zahtjev nije ispravan.",
                HttpStatusCode.NotFound            => "Stavka nije pronađena.",
                HttpStatusCode.Conflict            => "Kategorija već postoji.",
                HttpStatusCode.Forbidden           => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.Unauthorized        => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.InternalServerError => "Kategorija se ne može obrisati jer postoje vezane transakcije u bazi (uključujući prethodno obrisane).",
                _ when !string.IsNullOrWhiteSpace(body) && !body.StartsWith('{') && !body.StartsWith('[')
                                                   => body,
                _                                  => $"Greška ({(int?)hre.StatusCode})."
            };
        }
        var msg = ex.Message?.Trim();
        return string.IsNullOrEmpty(msg) ? "Neočekivana greška." : msg;
    }

    [RelayCommand]
    private void SetType(string? typeStr)
    {
        if (!IsEditMode) IsRevenue = typeStr == "revenue";
    }

    [RelayCommand]
    private void OpenAdd(string? typeStr)
    {
        _editingId  = 0;
        IsRevenue   = typeStr == "revenue";
        IsEditMode  = false;
        ModalName   = string.Empty;
        ModalError  = string.Empty;
        IsModalOpen = true;
    }

    [RelayCommand]
    private void OpenEdit(CategoryItem item)
    {
        _editingId  = item.TypeId;
        IsRevenue   = item.IsRevenue;
        IsEditMode  = true;
        ModalName   = item.Name;
        ModalError  = string.Empty;
        IsModalOpen = true;
    }

    [RelayCommand]
    private void CloseModal() => IsModalOpen = false;

    [RelayCommand]
    private void RequestDeleteCategory(CategoryItem item)
    {
        _pendingDeleteTypeId = item.TypeId;
        Rebuild();
    }

    [RelayCommand]
    private void CancelDeleteCategory(CategoryItem item)
    {
        _pendingDeleteTypeId = -1;
        Rebuild();
    }

    [RelayCommand]
    private async Task Save()
    {
        var name = ModalName.Trim();
        if (string.IsNullOrEmpty(name)) { ModalError = "Unesite naziv kategorije."; return; }
        if (_api.PersonalGroupId == 0)  { ModalError = "Greška: nema grupe."; return; }

        IsSaving = true; ModalError = string.Empty;
        try
        {
            if (IsEditMode)
            {
                if (IsRevenue)
                    await _api.PutAsync($"/api/RevenueType/EditRevenueType/{_editingId}", new { name });
                else
                    await _api.PutAsync($"/api/CostType/EditCostType/{_editingId}", new { name });
            }
            else
            {
                if (IsRevenue)
                    await _api.PostAsync<object, object>("/api/RevenueType/CreateNewRevenueType", new { name, groupId = _api.PersonalGroupId });
                else
                    await _api.PostAsync<object, object>("/api/CostType/CreateNewCostType", new { name, groupId = _api.PersonalGroupId });
            }
            await _data.ReloadTypesAsync(_api);
            Rebuild();
            IsModalOpen = false;
        }
        catch (Exception ex) { ModalError = FriendlyError(ex); }
        finally { IsSaving = false; }
    }

    [RelayCommand]
    private async Task Delete(CategoryItem item)
    {
        DeleteError = string.Empty;
        _pendingDeleteTypeId = -1;

        var tombstone = $"__deleted_{item.TypeId}";
        try
        {
            if (item.IsRevenue)
                await _api.PutAsync($"/api/RevenueType/EditRevenueType/{item.TypeId}", new { name = tombstone });
            else
                await _api.PutAsync($"/api/CostType/EditCostType/{item.TypeId}", new { name = tombstone });
        }
        catch
        {
            DeleteError = "Greška pri brisanju kategorije. Pokušajte ponovo.";
            return;
        }

        try
        {
            if (item.IsRevenue)
                await _api.DeleteAsync($"/api/RevenueType/DeleteRevenueType/{item.TypeId}");
            else
                await _api.DeleteAsync($"/api/CostType/DeleteCostType/{item.TypeId}");
        }
        catch (HttpRequestException hre) when (hre.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
        }
        catch (HttpRequestException hre)
        {
            try
            {
                if (item.IsRevenue)
                    await _api.PutAsync($"/api/RevenueType/EditRevenueType/{item.TypeId}", new { name = item.Name });
                else
                    await _api.PutAsync($"/api/CostType/EditCostType/{item.TypeId}", new { name = item.Name });
            }
            catch { }
            DeleteError = FriendlyError(hre);
            return;
        }
        catch (Exception ex)
        {
            DeleteError = FriendlyError(ex);
            return;
        }

        _data.TypeOptions.RemoveAll(t => t.TypeId == item.TypeId);
        Rebuild();
    }
}
