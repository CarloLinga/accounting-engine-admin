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
    /// The stable row key (GET /api/SourceRules/{id} / PUT /api/SourceRules/{id}).
    /// Renames edit Model.SourceType freely -- the Id in the route is what
    /// locates the row, so XXX -&gt; XXX_UPDATED can no longer 404 or hit the
    /// wrong row the way PUT-by-sourceType could.
    /// </summary>
    [Parameter] public Guid? RuleId { get; set; }

    /// <summary>
    /// Pre-edit sourceType, captured by the caller before the dialog opens.
    /// Only used as fallback context (e.g. legacy PUT-by-sourceType on old
    /// backends); the Id route itself does not need it.
    /// </summary>
    [Parameter] public string? OriginalSourceType { get; set; }

    [Parameter] public IReadOnlyList<AccountResponse>? Accounts { get; set; }

    private List<AccountResponse> accounts = [];

    /// <summary>
    /// Amount-type suggestions for the rule-line autocomplete. Seeded with
    /// the client-side defaults, then replaced by the API list
    /// (GET /api/SourceRules/amount-types: distinct types referenced by
    /// rules merged with the engine's defaults) when it responds.
    /// </summary>
    private List<string> amountTypeOptions = RuleAmountTypeOptions.AmountTypes;

    private bool saving;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            accounts = Accounts is { Count: > 0 }
                ? Accounts.ToList()
                : (await AccountsApi.SearchAsync(null, false)).ToList();
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

        try
        {
            var fromApi = await Api.GetAmountTypesAsync();
            if (fromApi.Count > 0)
            {
                amountTypeOptions = fromApi.ToList();
            }
        }
        catch (ApiException)
        {
            // Keep the client-side defaults; the API list is a superset of
            // them, so nothing is lost offline.
        }
        catch (HttpRequestException)
        {
            // Same - keep the dialog usable offline.
        }

        if (IsCreate && !Model.IsManualEntryAllowed && Model.RuleLines.Count == 0)
        {
            AddLine();
            AddLine();
        }
    }

    private IEnumerable<AccountResponse> postableAccounts =>
        accounts.Where(a => (a.IsPostable && a.IsActive) || Model.RuleLines.Any(l => string.Equals(l.AccountCode, a.Code, StringComparison.OrdinalIgnoreCase)));

    private void AddLine() =>
        Model.RuleLines.Add(new SourceRuleLineFormModel
        {
            EntryType = Model.RuleLines.Any(l => l.EntryType == RuleEntryType.Debit)
                ? RuleEntryType.Credit
                : RuleEntryType.Debit,
            AmountType = nameof(RuleAmountType.TOTAL_AMOUNT)
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

            // OriginalSourceType is the pre-edit code captured by the caller;
            // Model.SourceType may already be renamed by the user, so only the
            // captured value is valid fallback context for legacy callers.
            SourceRuleResponse result = IsCreate
                ? await Api.CreateAsync(Model.ToRequest())
                : RuleId.HasValue
                    ? await Api.UpdateByIdAsync(RuleId.Value, OriginalSourceType ?? Model.SourceType, Model.ToUpdateRequest())
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
