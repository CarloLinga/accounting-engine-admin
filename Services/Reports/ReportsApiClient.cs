using AccountingEngineAdmin.Models.Reports;

namespace AccountingEngineAdmin.Services.Reports;

/// <summary>
/// IReportsApiClient implementation targeting the Accounting Engine API
/// (base address configured in Program.cs from 'Api:BaseUrl').
/// </summary>
public sealed class ReportsApiClient : ApiClientBase, IReportsApiClient
{
    public ReportsApiClient(HttpClient http) : base(http)
    {
    }

    public async Task<TrialBalanceResponse> GetTrialBalanceAsync(
        DateTimeOffset? asOf = null,
        int? fiscalYear = null,
        bool includeInactiveAccounts = false,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>(3);
        AddDateRange(query, "asOf", asOf, "asOf", null);
        if (fiscalYear is not null)
        {
            query.Add($"fiscalYear={fiscalYear.Value}");
        }
        if (includeInactiveAccounts)
        {
            query.Add("includeInactiveAccounts=true");
        }

        var path = "api/trial-balance" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        using var response = await Http.GetAsync(path, cancellationToken);
        return await SendForValueAsync<TrialBalanceResponse>(response, cancellationToken);
    }

    public async Task<LedgerResponse> GetLedgerAsync(
        string accountCode,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>(2);
        AddDateRange(query, "from", from, "to", to);

        var path = $"api/ledger/{Escape(accountCode)}" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        using var response = await Http.GetAsync(path, cancellationToken);
        return await SendForValueAsync<LedgerResponse>(response, cancellationToken);
    }

    public async Task<BalanceSheetResponse> GetBalanceSheetAsync(
        DateTimeOffset? asOf = null,
        bool includeInactiveAccounts = false,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>(2);
        AddDateRange(query, "asOf", asOf, "asOf", null);
        if (includeInactiveAccounts)
        {
            query.Add("includeInactiveAccounts=true");
        }

        var path = "api/financial-statements/balance-sheet" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        using var response = await Http.GetAsync(path, cancellationToken);
        return await SendForValueAsync<BalanceSheetResponse>(response, cancellationToken);
    }

    public async Task<IncomeStatementResponse> GetIncomeStatementAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        bool includeInactiveAccounts = false,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>(3);
        AddDateRange(query, "from", from, "to", to);
        if (includeInactiveAccounts)
        {
            query.Add("includeInactiveAccounts=true");
        }

        var path = "api/financial-statements/income-statement" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        using var response = await Http.GetAsync(path, cancellationToken);
        return await SendForValueAsync<IncomeStatementResponse>(response, cancellationToken);
    }

    public async Task<CashFlowResponse> GetCashFlowAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>(2);
        AddDateRange(query, "from", from, "to", to);

        var path = "api/financial-statements/cash-flow" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        using var response = await Http.GetAsync(path, cancellationToken);
        return await SendForValueAsync<CashFlowResponse>(response, cancellationToken);
    }
}
