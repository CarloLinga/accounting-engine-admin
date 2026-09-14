using AccountingEngineAdmin.Models.Reports;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Reports;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

public partial class FinancialStatements : ComponentBase
{
    [Inject] private IReportsApiClient ReportsApi { get; set; } = default!;

    private BalanceSheetResponse? balanceSheet;
    private IncomeStatementResponse? incomeStatement;
    private CashFlowResponse? cashFlow;

    private bool isLoading = true;
    private string? loadError;

    // Filters.
    private DateTime? asOfDate;
    private DateTime? fromDate;
    private DateTime? toDate;
    private bool includeInactive;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            DateTimeOffset? asOf = asOfDate is null ? (DateTimeOffset?)null : new DateTimeOffset(asOfDate.Value, TimeSpan.Zero);
            DateTimeOffset? from = fromDate is null ? (DateTimeOffset?)null : new DateTimeOffset(fromDate.Value, TimeSpan.Zero);
            DateTimeOffset? to = toDate is null ? (DateTimeOffset?)null : new DateTimeOffset(toDate.Value, TimeSpan.Zero);

            var balanceTask = ReportsApi.GetBalanceSheetAsync(asOf, includeInactive);
            var incomeTask = ReportsApi.GetIncomeStatementAsync(from, to, includeInactive);
            var cashFlowTask = ReportsApi.GetCashFlowAsync(from, to);

            await Task.WhenAll(balanceTask, incomeTask, cashFlowTask);
            balanceSheet = balanceTask.Result;
            incomeStatement = incomeTask.Result;
            cashFlow = cashFlowTask.Result;
        }
        catch (ApiException ex)
        {
            loadError = ex.Message;
        }
        catch (HttpRequestException)
        {
            loadError = "Cannot reach the Accounting Engine API. Start the API and check Api:BaseUrl.";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}