using System.Net.Http.Json;
using System.Text.Json;
using AccountingEngineAdmin.Models.Accounts;

namespace AccountingEngineAdmin.Services.Accounts;

/// <summary>
/// IAccountsApiClient implementation targeting the Accounting Engine API
/// (base address configured in Program.cs from 'Api:BaseUrl').
/// </summary>
public sealed class AccountsApiClient : IAccountsApiClient
{
    private readonly HttpClient _http;

    public AccountsApiClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<AccountResponse>> SearchAsync(
        string? search = null, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }
        if (includeInactive)
        {
            query.Add("includeInactive=true");
        }

        var path = "api/accounts" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        var accounts = await _http.GetFromJsonAsync<List<AccountResponse>>(path, ApiJson.Options, cancellationToken);
        return accounts ?? [];
    }

    public async Task<AccountResponse> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync($"api/accounts/{Escape(code)}", cancellationToken);
        return await SendForValueAsync<AccountResponse>(response, cancellationToken);
    }

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("api/accounts", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<AccountResponse>(response, cancellationToken);
    }

    public async Task<AccountResponse> UpdateAsync(
        string code, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/accounts/{Escape(code)}", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<AccountResponse>(response, cancellationToken);
    }

    public async Task SetActiveAsync(string code, bool isActive, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/accounts/{Escape(code)}/active?isActive={(isActive ? "true" : "false")}");

        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        using var response = await _http.DeleteAsync($"api/accounts/{Escape(code)}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string Escape(string segment) => Uri.EscapeDataString(segment);

    private async Task<T> SendForValueAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken);
        return payload ?? throw new ApiException(
            (int)response.StatusCode, "The API returned an empty response.");
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string? message = null;
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorPayload>(ApiJson.Options, cancellationToken);
            message = error?.Error;
        }
        catch (JsonException)
        {
            // Body wasn't the API's {"error": "..."} shape (proxy page, HTML, etc.).
        }

        throw new ApiException(
            (int)response.StatusCode,
            string.IsNullOrWhiteSpace(message)
                ? $"The API returned {(int)response.StatusCode} {response.StatusCode}."
                : message!);
    }

    /// <summary>Error body produced by the API: {"error":"..."}</summary>
    private sealed record ApiErrorPayload(string? Error);
}