using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.Accounts;

public partial class Accounts : ComponentBase, IDisposable
{
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private List<AccountResponse> accounts = new();
    private bool isLoading = true;
    private string? loadError;
    private string searchText = string.Empty;
    private bool includeInactive;
    private CancellationTokenSource? searchCts;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task OnSearchInput()
    {
        searchCts?.Cancel();
        searchCts?.Dispose();
        searchCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(400, searchCts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await LoadAsync();
    }

    private async Task ReloadAfterToggle() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            var result = await AccountsApi.SearchAsync(
                string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim(),
                includeInactive);
            accounts = result.ToList();
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

    private static string StatementClass(AccountResponse a) =>
        a.BalanceSheetClass is not null ? AccountLabels.For(a.BalanceSheetClass.Value) :
        a.IncomeStatementClass is not null ? AccountLabels.For(a.IncomeStatementClass.Value) : "-";

    private async Task OpenCreateAsync()
    {
        var model = new AccountFormModel();
        var result = await DialogService.OpenAsync<AccountDialog>(
            "New Account",
            new Dictionary<string, object?> { ["Model"] = model },
            new DialogOptions { Width = "640px", Draggable = true, CloseDialogOnEsc = true });

        if (result is AccountResponse created)
        {
            Notify(NotificationSeverity.Success, "Account created", $"{created.Code} - {created.Name}");
            await LoadAsync();
        }
    }

    private async Task OpenEditAsync(AccountResponse a)
    {
        var model = new AccountFormModel
        {
            Code = a.Code,
            Name = a.Name,
            Type = a.Type,
            Statement = a.Statement,
            BalanceSheetClass = a.BalanceSheetClass,
            IncomeStatementClass = a.IncomeStatementClass,
            CashFlowActivity = a.CashFlowActivity,
            IsCashEquivalent = a.IsCashEquivalent,
            IsContra = a.IsContra,
            IsPostable = a.IsPostable,
            ParentAccountCode = a.ParentAccountCode,
            DisplayOrder = a.DisplayOrder,
            IsActive = a.IsActive
        };

        var result = await DialogService.OpenAsync<AccountDialog>(
            $"Edit {a.Code}",
            new Dictionary<string, object?> { ["Model"] = model, ["IsCreate"] = false },
            new DialogOptions { Width = "640px", Draggable = true, CloseDialogOnEsc = true });

        if (result is AccountResponse updated)
        {
            Notify(NotificationSeverity.Success, "Account updated", $"{updated.Code} - {updated.Name}");
            await LoadAsync();
        }
    }

    private async Task ToggleActiveAsync(AccountResponse a)
    {
        try
        {
            await AccountsApi.SetActiveAsync(a.Code, !a.IsActive);
            Notify(NotificationSeverity.Success, a.IsActive ? "Account deactivated" : "Account activated", a.Code);
            await LoadAsync();
        }
        catch (ApiException ex)
        {
            Notify(NotificationSeverity.Error, "Update failed", ex.Message);
        }
        catch (HttpRequestException)
        {
            Notify(NotificationSeverity.Error, "API unreachable", "Could not reach the Accounting Engine API.");
        }
    }

    private async Task DeleteAsync(AccountResponse a)
    {
        var confirmed = await DialogService.Confirm(
            $"Delete account {a.Code} - {a.Name}? This cannot be undone.",
            "Confirm delete",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed != true)
        {
            return;
        }

        try
        {
            await AccountsApi.DeleteAsync(a.Code);
            Notify(NotificationSeverity.Success, "Account deleted", a.Code);
            await LoadAsync();
        }
        catch (ApiException ex)
        {
            Notify(NotificationSeverity.Error, "Delete failed", ex.Message);
        }
        catch (HttpRequestException)
        {
            Notify(NotificationSeverity.Error, "API unreachable", "Could not reach the Accounting Engine API.");
        }
    }

    private void Notify(NotificationSeverity severity, string summary, string detail) =>
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = detail,
            Duration = 4000
        });

    public void Dispose()
    {
        searchCts?.Cancel();
        searchCts?.Dispose();
    }
}
