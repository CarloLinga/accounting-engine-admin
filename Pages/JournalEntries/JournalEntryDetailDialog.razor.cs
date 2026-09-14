using AccountingEngineAdmin.Models.JournalEntries;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.JournalEntries;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.JournalEntries;

public partial class JournalEntryDetailDialog : ComponentBase
{
    [Inject] private IJournalEntriesApiClient Api { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public JournalEntryResponse Entry { get; set; } = default!;

    private bool loading;
    private string? error;
    private JournalEntryResponse entry = default!;

    private decimal TotalDebits => (entry?.JournalLines.Sum(l => l.Debit)) ?? 0;
    private decimal TotalCredits => (entry?.JournalLines.Sum(l => l.Credit)) ?? 0;

    protected override async Task OnInitializedAsync()
    {
        loading = true;
        error = null;
        try
        {
            // Re-fetch by id so the dialog shows the latest state of the entry.
            entry = await Api.GetByIdAsync(Entry.Id);
        }
        catch (ApiException ex)
        {
            entry = Entry;
            error = ex.Message;
        }
        catch (HttpRequestException)
        {
            entry = Entry;
            error = "Cannot reach the Accounting Engine API; showing cached data.";
        }
        finally
        {
            loading = false;
        }
    }

    private void Close() => DialogService.Close(null);
}
