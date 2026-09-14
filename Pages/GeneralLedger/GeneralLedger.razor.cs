using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.Reports;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.Reports;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Radzen;

namespace AccountingEngineAdmin.Pages.GeneralLedger;

public partial class GeneralLedger : ComponentBase
{
    [Inject] private IReportsApiClient ReportsApi { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private List<AccountResponse> accounts = [];

    private LedgerResponse? ledger;
    private bool isLoading = true;
    private string? loadError;

    // Filters.
    private string? accountCode;
    private DateTime? fromDate;
    private DateTime? toDate;

    protected override async Task OnInitializedAsync()
    {
        // Support deep links like /ledger?account=4000&from=2026-01-01&to=2026-12-31.
        try
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("account", out var account))
            {
                accountCode = account.ToString();
            }
            if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("from", out var from))
            {
                fromDate = DateTime.Parse(from.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }
            if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("to", out var to))
            {
                toDate = DateTime.Parse(to.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        catch (FormatException)
        {
            // Malformed deep link - fall back to defaults.
        }


        try
        {
            accounts = (await AccountsApi.SearchAsync(null, true)).ToList();
        }
        catch (ApiException)
        {
            // The account dropdown can stay empty; a deep link still works.
        }
        catch (HttpRequestException)
        {
            // Same as above.
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(accountCode))
        {
            ledger = null;
            isLoading = false;
            return;
        }


        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            DateTimeOffset? from = null;
            if (fromDate is not null)
            {
                from = new DateTimeOffset(fromDate.Value, TimeSpan.Zero);
            }

            DateTimeOffset? to = null;
            if (toDate is not null)
            {
                to = new DateTimeOffset(toDate.Value, TimeSpan.Zero);
            }

            ledger = await ReportsApi.GetLedgerAsync(accountCode.Trim(), from, to);
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
