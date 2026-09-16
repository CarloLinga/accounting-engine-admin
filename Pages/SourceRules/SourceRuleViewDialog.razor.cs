using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.SourceRules;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.SourceRules;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.SourceRules;

public partial class SourceRuleViewDialog : ComponentBase
{
    [Inject] private ISourceRulesApiClient Api { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public SourceRuleResponse Rule { get; set; } = default!;

    private IReadOnlyList<AccountResponse> accounts = [];
    private bool busy;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            accounts = await AccountsApi.SearchAsync(null, true);
        }
        catch (ApiException)
        {
        }
        catch (HttpRequestException)
        {
        }
    }

    private async Task EditAsync()
    {
        var model = new SourceRuleFormModel
        {
            SourceType = Rule.SourceType,
            Description = Rule.Description,
            IsManualEntryAllowed = Rule.IsManualEntryAllowed
        };

        foreach (var line in Rule.RuleLines.OrderBy(l => l.Sequence))
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
            $"Edit {Rule.SourceType}",
            new Dictionary<string, object?>
            {
                ["Model"] = model,
                ["IsCreate"] = false,
                ["RuleId"] = Rule.Id,
                ["OriginalSourceType"] = Rule.SourceType,
                ["Accounts"] = accounts.Count > 0 ? accounts : null
            },
            new DialogOptions { Width = "820px", Draggable = true, CloseDialogOnEsc = true });

        DialogService.Close(result is SourceRuleResponse updated
            ? new SourceRuleDialogResult(SourceRuleDialogOutcome.Saved, updated.SourceType, updated)
            : null);
    }

    private async Task DeleteAsync()
    {
        var confirmed = await DialogService.Confirm(
            $"Delete source rule {Rule.SourceType} - {Rule.Description}? This cannot be undone.",
            "Confirm delete",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed != true)
        {
            return;
        }

        busy = true;
        error = null;
        try
        {
            await Api.DeleteByIdAsync(Rule.Id, Rule.SourceType);
            DialogService.Close(new SourceRuleDialogResult(SourceRuleDialogOutcome.Deleted, Rule.SourceType));
        }
        catch (ApiException ex)
        {
            error = ex.Message;
        }
        catch (HttpRequestException)
        {
            error = "Cannot reach the Accounting Engine API. Check Api:BaseUrl.";
        }
        finally
        {
            busy = false;
        }
    }

    private void Close() => DialogService.Close(null);
}
