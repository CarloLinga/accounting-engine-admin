using AccountingEngineAdmin.Models.SalesJournals;

namespace AccountingEngineAdmin.Services.SalesJournals;

public interface ISalesJournalsApiClient
{
    Task<IReadOnlyList<SalesJournalResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SalesJournalResponse> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default);
    Task<SalesJournalResponse> CreateAsync(CreateSalesJournalRequest request, CancellationToken cancellationToken = default);
    Task<SalesJournalResponse> UpdateAsync(string invoiceNo, UpdateSalesJournalRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string invoiceNo, CancellationToken cancellationToken = default);
}