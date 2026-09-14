using AccountingEngineAdmin.Models.Reports;

namespace AccountingEngineAdmin.Services.Reports;

/// <summary>
/// Typed client for the Accounting Engine API reporting endpoints
/// (/api/trial-balance, /api/ledger/{accountCode},
/// /api/financial-statements/balance-sheet|income-statement|cash-flow).
/// Implementations throw <see cref="ApiException"/> for non-success responses
/// and <see cref="HttpRequestException"/> when the API is unreachable.
/// </summary>
public interface IReportsApiClient
{
    /// <summary>GET /api/trial-balance?asOf=&amp;fiscalYear=&amp;includeInactiveAccounts=</summary>
    Task<TrialBalanceResponse> GetTrialBalanceAsync(
        DateTimeOffset? asOf = null,
        int? fiscalYear = null,
        bool includeInactiveAccounts = false,
        CancellationToken cancellationToken = default);

    /// <summary>GET /api/ledger/{accountCode}?from=&amp;to=</summary>
    Task<LedgerResponse> GetLedgerAsync(
        string accountCode,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>GET /api/financial-statements/balance-sheet?asOf=&amp;includeInactiveAccounts=</summary>
    Task<BalanceSheetResponse> GetBalanceSheetAsync(
        DateTimeOffset? asOf = null,
        bool includeInactiveAccounts = false,
        CancellationToken cancellationToken = default);

    /// <summary>GET /api/financial-statements/income-statement?from=&amp;to=&amp;includeInactiveAccounts=</summary>
    Task<IncomeStatementResponse> GetIncomeStatementAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        bool includeInactiveAccounts = false,
        CancellationToken cancellationToken = default);

    /// <summary>GET /api/financial-statements/cash-flow?from=&amp;to=</summary>
    Task<CashFlowResponse> GetCashFlowAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);
}
