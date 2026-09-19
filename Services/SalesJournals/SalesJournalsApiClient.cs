using System.Net.Http.Json;
using AccountingEngineAdmin.Models.SalesJournals;

namespace AccountingEngineAdmin.Services.SalesJournals;

public sealed class SalesJournalsApiClient : ApiClientBase, ISalesJournalsApiClient
{
    public SalesJournalsApiClient(HttpClient http) : base(http)
    {
    }

    public async Task<IReadOnlyList<SalesJournalResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync("api/SalesJournals", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await TryReadAsync<List<SalesJournalResponse>>(response, cancellationToken) ?? [];
    }

    public async Task<SalesJournalResponse> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync($"api/SalesJournals/{Escape(invoiceNo)}", cancellationToken);
        return await SendForValueAsync<SalesJournalResponse>(response, cancellationToken);
    }

    public async Task<SalesJournalResponse> CreateAsync(CreateSalesJournalRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync("api/SalesJournals", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<SalesJournalResponse>(response, cancellationToken);
    }

    public async Task<SalesJournalResponse> UpdateAsync(string invoiceNo, UpdateSalesJournalRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PutAsJsonAsync($"api/SalesJournals/{Escape(invoiceNo)}", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<SalesJournalResponse>(response, cancellationToken);
    }

    public async Task DeleteAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        using var response = await Http.DeleteAsync($"api/SalesJournals/{Escape(invoiceNo)}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}