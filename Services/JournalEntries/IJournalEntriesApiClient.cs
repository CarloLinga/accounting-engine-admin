using AccountingEngineAdmin.Models.JournalEntries;

namespace AccountingEngineAdmin.Services.JournalEntries;

/// <summary>
/// Typed client for the /api/Journals endpoints of the Accounting Engine API.
/// Implementations throw <see cref="ApiException"/> for non-success responses
/// and <see cref="HttpRequestException"/> when the API is unreachable.
/// </summary>
public interface IJournalEntriesApiClient
{
    /// <summary>GET /api/Journals?startDate=&amp;endDate=&amp;sourceType=</summary>
    Task<IReadOnlyList<JournalEntryResponse>> SearchAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? sourceType = null,
        CancellationToken cancellationToken = default);

    /// <summary>GET /api/Journals/{id}</summary>
    Task<JournalEntryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>GET /api/Journals/reference/{reference}</summary>
    Task<JournalEntryResponse> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

    /// <summary>POST /api/Journals (manual general journal entry).</summary>
    Task<JournalEntryResponse> PostGeneralJournalAsync(PostGeneralJournalRequest request, CancellationToken cancellationToken = default);

    /// <summary>POST /api/Journals/source (rule-driven posting from a source system).</summary>
    Task<JournalEntryResponse> PostSourceTransactionAsync(PostSourceTransactionRequest request, CancellationToken cancellationToken = default);
}
