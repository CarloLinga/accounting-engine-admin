using System.Linq;
using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.SourceRules;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.SourceRules;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.SourceRules;

/// <summary>
/// Lists all source rules with their rule lines, account names and active state.
/// Provides create, edit, toggle-active and delete actions wired to the
/// Accounting Engine API (mirrors the Accounts page behavior).
/// </summary>
public partial class SourceRules : ComponentBase
{
    [Inject] private ISourceRulesApiClient SourceRulesApi { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private List<SourceRuleResponse> rules = [];
    private List<AccountResponse> accounts = [];
    private bool isLoading = true;
    private string? loadError;
    private bool showInactive;

    private IEnumerable<SourceRuleResponse> visibleRules =>
        showInactive ? rules : rules.Where(r => r.IsActive);

    private int VisibleRulesCount => visibleRules.Count();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            var rulesTask = SourceRulesApi.GetAllAsync();
            var accountsTask = AccountsApi.SearchAsync(null, true);

            await Task.WhenAll(rulesTask, accountsTask);
            rules = rulesTask.Result.ToList();
            accounts = accountsTask.Result.ToList();
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

    private async Task OpenCreateAsync()
    {
        var model = new SourceRuleFormModel();
        var result = await DialogService.OpenAsync<SourceRuleDialog>(
            "New Source Rule",
            new Dictionary<string, object?> { ["Model"] = model, ["IsCreate"] = true },
            new DialogOptions { Width = "820px", Draggable = true, CloseDialogOnEsc = true });

        if (result is SourceRuleResponse created)
        {
            Notify(NotificationSeverity.Success, "Source rule created", created.SourceType);
            await LoadAsync();
        }
    }

    private async Task OpenEditAsync(SourceRuleResponse rule)
    {
        var model = new SourceRuleFormModel
        {
            SourceType = rule.SourceType,
            Description = rule.Description,
            IsManualEntryAllowed = rule.IsManualEntryAllowed
        };

        foreach (var line in rule.RuleLines.OrderBy(l => l.Sequence))
        {
            model.RuleLines.Add(new SourceRuleLineFormModel
            {
                AccountCode = line.AccountCode,
                EntryType = line.EntryType,
                AmountType = line.AmountType,
                Sequence = line.Sequence
            });
        }

        var result = await DialogService.OpenAsync<SourceRuleDialog>(
            $"Edit {rule.SourceType}",
            new Dictionary<string, object?>
            {
                ["Model"] = model,
                ["IsCreate"] = false,
                // Stable row key for PUT-by-Id (renames edit Model.SourceType
                // freely). OriginalSourceType is kept as fallback context for
                // legacy PUT-by-sourceType callers / old backends.
                ["RuleId"] = rule.Id,
                ["OriginalSourceType"] = rule.SourceType
            },
            new DialogOptions { Width = "820px", Draggable = true, CloseDialogOnEsc = true });

        if (result is SourceRuleResponse updated)
        {
            Notify(NotificationSeverity.Success, "Source rule updated", updated.SourceType);
            await LoadAsync();
        }
    }

    private async Task ToggleActiveAsync(SourceRuleResponse rule)
    {
        try
        {
            await SourceRulesApi.SetActiveAsync(rule.SourceType, !rule.IsActive);
            Notify(NotificationSeverity.Success,
                rule.IsActive ? "Source rule deactivated" : "Source rule activated",
                rule.SourceType);
            await LoadAsync();
        }
        catch (ApiException ex)
        {
            Notify(NotificationSeverity.Error, "Update failed", ex.Message);
        }
        catch (HttpRequestException)
        {
            Notify(NotificationSeverity.Error, "API unreachable",
                "Could not reach the Accounting Engine API.");
        }
    }

    private async Task DeleteAsync(SourceRuleResponse rule)
    {
        var confirmed = await DialogService.Confirm(
            $"Delete source rule {rule.SourceType} - {rule.Description}? This cannot be undone.",
            "Confirm delete",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed != true)
        {
            return;
        }

        try
        {
            await SourceRulesApi.DeleteByIdAsync(rule.Id, rule.SourceType);
            Notify(NotificationSeverity.Success, "Source rule deleted", rule.SourceType);
            await LoadAsync();
        }
        catch (ApiException ex)
        {
            Notify(NotificationSeverity.Error, "Delete failed", ex.Message);
        }
        catch (HttpRequestException)
        {
            Notify(NotificationSeverity.Error, "API unreachable",
                "Could not reach the Accounting Engine API.");
        }
    }

    private string AccountName(string accountCode) =>
        accounts.FirstOrDefault(a => a.Code == accountCode) is { } account
            ? $"{account.Code} - {account.Name}"
            : accountCode;

    private string LineSummary(SourceRuleResponse rule)
    {
        if (rule.RuleLines.Count == 0)
        {
            return "No rule lines";
        }

        var debitLines = rule.RuleLines.Count(l => l.EntryType == RuleEntryType.Debit);
        var creditLines = rule.RuleLines.Count(l => l.EntryType == RuleEntryType.Credit);
        return $"{rule.RuleLines.Count} lines ({debitLines} D / {creditLines} C)";
    }

    private string LineDetails(SourceRuleResponse rule) =>
        rule.RuleLines.Count == 0
            ? "Posts nothing by itself; used for manual entry or reference."
            : string.Join("  |  ", rule.RuleLines
                .OrderBy(l => l.Sequence)
                .Select(l => $"{AccountName(l.AccountCode)} {(l.EntryType == RuleEntryType.Debit ? "Dr" : "Cr")} {RuleAmountTypeOptions.For(l.AmountType)}"));

    private void Notify(NotificationSeverity severity, string summary, string detail) =>
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = detail,
            Duration = 4000
        });
}