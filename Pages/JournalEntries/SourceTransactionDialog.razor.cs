using AccountingEngineAdmin.Models.JournalEntries;
using AccountingEngineAdmin.Models.SourceRules;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.JournalEntries;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.JournalEntries;

public partial class SourceTransactionDialog : ComponentBase
{
    [Inject] private IJournalEntriesApiClient Api { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public SourceTransactionFormModel Model { get; set; } = new();

    /// <summary>Active source rules passed from the journals page.</summary>
    [Parameter] public IReadOnlyList<SourceRuleResponse> Rules { get; set; } = [];

    private bool saving;
    private string? error;

    private List<(string Value, string Label)> ruleOptions =>
        Rules
            .Where(r => r.IsActive)
            .Select(r => (r.SourceType, $"{r.SourceType} - {r.Description}"))
            .ToList();

    protected override void OnInitialized()
    {
        if (Model.Amounts.Count == 0)
        {
            Model.Amounts.Add(new SourceAmountFormModel { Key = nameof(RuleAmountType.TOTAL_AMOUNT) });
        }
    }

    private void OnRuleChanged(object value)
    {
        // Re-seed the amount rows from the selected rule's distinct amount types.
        var sourceType = value as string;
        Model.SourceType = sourceType;
        Model.Amounts.Clear();

        var rule = Rules.FirstOrDefault(r => r.SourceType == sourceType);
        if (rule is null)
        {
            Model.Amounts.Add(new SourceAmountFormModel { Key = nameof(RuleAmountType.TOTAL_AMOUNT) });
            return;
        }

        foreach (var amountType in rule.RuleLines.Select(l => l.AmountType).Distinct())
        {
            Model.Amounts.Add(new SourceAmountFormModel { Key = amountType });
        }
    }

    private void AddAmount() => Model.Amounts.Add(new SourceAmountFormModel());

    private void RemoveAmount(SourceAmountFormModel amount) => Model.Amounts.Remove(amount);

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        try
        {
            var result = await Api.PostSourceTransactionAsync(Model.ToRequest());
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
