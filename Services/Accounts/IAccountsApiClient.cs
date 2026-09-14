using AccountingEngineAdmin.Models.Accounts;

namespace AccountingEngineAdmin.Services.Accounts;

/// <summary>
/// Typed client for the /api/accounts endpoints of the Accounting Engine API.
/// Implementations throw <see cref="ApiException"/> for non-success responses
/// and <see cref="HttpRequestException"/> when the API is unreachable.
/// </summary>
public interface IAccountsApiClient
{
    /// <summary>GET /api/accounts?search=&amp;includeInactive=</summary>
    Task<IReadOnlyList<AccountResponse>> SearchAsync(
        string? search = null, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>GET /api/accounts/{code}</summary>
    Task<AccountResponse> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>POST /api/accounts</summary>
    Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>PUT /api/accounts/{code}</summary>
    Task<AccountResponse> UpdateAsync(string code, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>PATCH /api/accounts/{code}/active?isActive=</summary>
    Task SetActiveAsync(string code, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>DELETE /api/accounts/{code} (409 Conflict when the account has postings).</summary>
    Task DeleteAsync(string code, CancellationToken cancellationToken = default);
}