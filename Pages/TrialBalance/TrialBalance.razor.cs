using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.Reports;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Reports;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.TrialBalance;

public partial class TrialBalance : ComponentBase
{
    [Inject] private IReportsApiClient ReportsApi { get; set; } = default!;

    private TrialBalanceResponse? report;
    private bool isLoading = true;
    private string? loadError;

    // Filters.
    private DateTime? asOfDate;
    private int? fiscalYear;
    private bool includeInactive;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            DateTimeOffset? asOf = null;
            if (asOfDate is not null)
            {
                asOf = new DateTimeOffset(asOfDate.Value, TimeSpan.Zero);
            }
            report = await ReportsApi.GetTrialBalanceAsync(asOf, fiscalYear, includeInactive);
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

    private string TypeBadge(TrialBalanceLine line) => AccountLabels.BadgeCss(line.AccountType);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void OpenLedgerAsync(TrialBalanceLine line) =>
        Navigation.NavigateTo($"/ledger?account={Uri.EscapeDataString(line.AccountCode)}");
}
