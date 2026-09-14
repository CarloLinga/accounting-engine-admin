using System.Net.Http.Json;
using AccountingEngineAdmin.Models.JournalEntries;

namespace AccountingEngineAdmin.Services.JournalEntries;

/// <summary>
/// IJournalEntriesApiClient implementation targeting the Accounting Engine API
/// (base address configured in Program.cs from 'Api:BaseUrl').
/// </summary>
public sealed class JournalEntriesApiClient : ApiClientBase, IJournalEntriesApiClient
{
    public JournalEntriesApiClient(HttpClient http) : base(http)
    {
    }

    public async Task<IReadOnlyList<JournalEntryResponse>> SearchAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>(3);
        AddDateRange(query, "startDate", startDate, "endDate", endDate);
        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            query.Add($"sourceType={Escape(sourceType.Trim())}");
        }

        var path = "api/Journals" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        using var response = await Http.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var entries = await TryReadAsync<List<JournalEntryResponse>>(response, cancellationToken);
        return entries ?? [];
    }

    public async Task<JournalEntryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync($"api/Journals/{Escape(id.ToString())}", cancellationToken);
        return await SendForValueAsync<JournalEntryResponse>(response, cancellationToken);
    }

    public async Task<JournalEntryResponse> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync($"api/Journals/reference/{Escape(reference)}", cancellationToken);
        return await SendForValueAsync<JournalEntryResponse>(response, cancellationToken);
    }

    public async Task<JournalEntryResponse> PostGeneralJournalAsync(
        PostGeneralJournalRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync("api/Journals", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<JournalEntryResponse>(response, cancellationToken);
    }

    public async Task<JournalEntryResponse> PostSourceTransactionAsync(
        PostSourceTransactionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync("api/Journals/source", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<JournalEntryResponse>(response, cancellationToken);
    }
}
