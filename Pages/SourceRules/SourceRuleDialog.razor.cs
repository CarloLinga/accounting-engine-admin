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

        if (Model.RuleLines.Count == 0)
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
            DebitPercentage = 100,
            AmountType = RuleAmountType.TOTAL_AMOUNT
        });

    private void RemoveLine(SourceRuleLineFormModel line) => Model.RuleLines.Remove(line);

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        try
        {
            var result = await Api.CreateAsync(Model.ToRequest());
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
