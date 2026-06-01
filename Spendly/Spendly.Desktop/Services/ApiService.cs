using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Spendly.Desktop.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:5153") };
    }

    internal ApiService(HttpMessageHandler handler)
    {
        _http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5153") };
    }

    public string? Token            { get; set; }
    public int     UserId           { get; set; }
    public int     PersonalGroupId     { get; set; }
    public int     PersonalUserGroupId { get; set; }
    public int     FamilyGroupId       { get; set; }
    public int     FamilyUserGroupId   { get; set; }
    public string  FirstName           { get; set; } = string.Empty;
    public string  LastName            { get; set; } = string.Empty;
    public string  Email               { get; set; } = string.Empty;
    public string  Username            { get; set; } = string.Empty;

    public void Reset()
    {
        Token = null;
        UserId = PersonalGroupId = PersonalUserGroupId = FamilyGroupId = FamilyUserGroupId = 0;
        FirstName = LastName = Email = Username = string.Empty;
    }

    private void SetAuth()
        => _http.DefaultRequestHeaders.Authorization =
            Token is null ? null : new AuthenticationHeaderValue("Bearer", Token);

    public async Task<T?> GetAsync<T>(string path)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<T>(path);
    }

    public async Task<TRes?> PostAsync<TReq, TRes>(string path, TReq body)
    {
        SetAuth();
        var r = await _http.PostAsJsonAsync(path, body);
        if (!r.IsSuccessStatusCode)
        {
            var body2 = await r.Content.ReadAsStringAsync();
            throw new HttpRequestException(body2, null, r.StatusCode);
        }
        return await r.Content.ReadFromJsonAsync<TRes>();
    }

    public async Task PutAsync<TReq>(string path, TReq body)
    {
        SetAuth();
        var r = await _http.PutAsJsonAsync(path, body);
        if (!r.IsSuccessStatusCode)
        {
            var body2 = await r.Content.ReadAsStringAsync();
            throw new HttpRequestException(body2, null, r.StatusCode);
        }
    }

    public async Task DeleteAsync(string path)
    {
        SetAuth();
        var r = await _http.DeleteAsync(path);
        if (!r.IsSuccessStatusCode)
        {
            var body = await r.Content.ReadAsStringAsync();
            throw new HttpRequestException(body, null, r.StatusCode);
        }
    }
}
