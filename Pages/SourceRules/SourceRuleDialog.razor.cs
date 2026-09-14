using System.ComponentModel.DataAnnotations;
using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.SourceRules;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.SourceRules;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.SourceRules;

public partial class SourceRuleDialog : ComponentBase
{
    [Inject] private ISourceRulesApiClient Api { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public SourceRuleFormModel Model { get; set; } = new();

    [Parameter] public bool IsCreate { get; set; } = true;

    /// <summary>
    /// Route key for PUT /api/SourceRules/{sourceType}. The backend allows
    /// renaming, so this preserves the original value while Model.SourceType
    /// carries the (possibly edited) new value.
    /// </summary>
    [Parameter] public string? OriginalSourceType { get; set; }

    private List<AccountResponse> accounts = [];
    private bool saving;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            accounts = (await AccountsApi.SearchAsync(null, false)).ToList();
        }
        catch (ApiException)
        {
            // The engine validates account codes at save time; leave the
            // dropdown empty instead of failing the whole dialog.
        }
        catch (HttpRequestException)
        {
            // Same as above - keep the dialog usable offline.
        }

        if (IsCreate && !Model.IsManualEntryAllowed && Model.RuleLines.Count == 0)
        {
            AddLine();
            AddLine();
        }
    }

    private IEnumerable<AccountResponse> postableAccounts =>
        accounts.Where(a => a.IsPostable && a.IsActive);

    private void AddLine() =>
        Model.RuleLines.Add(new SourceRuleLineFormModel
        {
            EntryType = Model.RuleLines.Any(l => l.EntryType == RuleEntryType.Debit)
                ? RuleEntryType.Credit
                : RuleEntryType.Debit,
            AmountType = RuleAmountType.TOTAL_AMOUNT
        });

    private void RemoveLine(SourceRuleLineFormModel line) => Model.RuleLines.Remove(line);

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        try
        {
            if (!Model.IsManualEntryAllowed)
            {
                var filled = Model.RuleLines.Where(l => l.IsFilled).ToList();
                if (filled.Count < 2)
                {
                    error = "Automated source rules must define at least two template lines.";
                    return;
                }

                if (!filled.Any(l => l.EntryType == RuleEntryType.Debit) ||
                    !filled.Any(l => l.EntryType == RuleEntryType.Credit))
                {
                    error = "Automated source rules must contain at least one Debit line and one Credit line.";
                    return;
                }
            }

            SourceRuleResponse result = IsCreate
                ? await Api.CreateAsync(Model.ToRequest())
                : await Api.UpdateAsync(OriginalSourceType ?? Model.SourceType, Model.ToUpdateRequest());
            DialogService.Close(result);
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
            saving = false;
        }
    }

    private void Cancel() => DialogService.Close(null);
}
