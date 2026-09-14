using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.JournalEntries;
using AccountingEngineAdmin.Models.SourceRules;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.JournalEntries;
using AccountingEngineAdmin.Services.SourceRules;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.JournalEntries;

public partial class JournalEntries : ComponentBase
{
    [Inject] private IJournalEntriesApiClient JournalsApi { get; set; } = default!;
    [Inject] private ISourceRulesApiClient SourceRulesApi { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private List<JournalEntryResponse> entries = [];
    private List<AccountResponse> accounts = [];
    private List<SourceRuleResponse> sourceRules = [];

    private bool isLoading = true;
    private string? loadError;

    // Filters.
    private DateTime? filterStart;
    private DateTime? filterEnd;
    private string filterSourceType = string.Empty;

    private string Amount(JournalEntryResponse entry) =>
        (entry.JournalLines.Sum(l => l.Debit)).ToString("N2");

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            var start = ToUtc(filterStart);
            var end = ToUtc(filterEnd);
            var sourceType = string.IsNullOrWhiteSpace(filterSourceType) ? null : filterSourceType.Trim();

            var entriesTask = JournalsApi.SearchAsync(start, end, sourceType);
            var accountsTask = AccountsApi.SearchAsync(null, true);
            var rulesTask = SourceRulesApi.GetAllAsync();

            await Task.WhenAll(entriesTask, accountsTask, rulesTask);
            entries = entriesTask.Result.ToList();
            accounts = accountsTask.Result.ToList();
            sourceRules = rulesTask.Result.ToList();
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

    private async Task OpenDetailAsync(JournalEntryResponse entry)
    {
        await DialogService.OpenAsync<JournalEntryDetailDialog>(
            $"Journal entry {entry.Reference}",
            new Dictionary<string, object?> { ["Entry"] = entry },
            new DialogOptions { Width = "820px", Draggable = true, CloseDialogOnEsc = true });
    }

    private async Task OpenGeneralJournalAsync()
    {
        var model = new GeneralJournalFormModel();
        var result = await DialogService.OpenAsync<GeneralJournalDialog>(
            "Post General Journal",
            new Dictionary<string, object?> { ["Model"] = model },
            new DialogOptions { Width = "820px", Draggable = true, CloseDialogOnEsc = true });

        if (result is JournalEntryResponse posted)
        {
            Notify(NotificationSeverity.Success, "Journal entry posted", posted.Reference);
            await LoadAsync();
        }
    }

    private async Task OpenSourceTransactionAsync()
    {
        var model = new SourceTransactionFormModel();
        var result = await DialogService.OpenAsync<SourceTransactionDialog>(
            "Post Source Transaction",
            new Dictionary<string, object?> { ["Model"] = model, ["Rules"] = sourceRules },
            new DialogOptions { Width = "700px", Draggable = true, CloseDialogOnEsc = true });

        if (result is JournalEntryResponse posted)
        {
            Notify(NotificationSeverity.Success, "Source transaction posted", posted.Reference);
            await LoadAsync();
        }
    }

    private async Task OpenEntryByReferenceAsync()
    {
        // Not bound to UI; kept for reference lookups from other pages.
        await Task.CompletedTask;
    }

    private static DateTimeOffset? ToUtc(DateTime? date) =>
        date is null ? null : new DateTimeOffset(date.Value, TimeSpan.Zero);

    private void Notify(NotificationSeverity severity, string summary, string detail) =>
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = detail,
            Duration = 4000
        });
}
