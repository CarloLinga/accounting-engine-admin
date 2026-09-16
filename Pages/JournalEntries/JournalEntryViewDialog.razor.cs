using AccountingEngineAdmin.Models.JournalEntries;
using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.JournalEntries;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.JournalEntries;

public partial class JournalEntryViewDialog : ComponentBase
{
    [Inject] private IJournalEntriesApiClient Api { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public JournalEntryResponse Entry { get; set; } = default!;

    private JournalEntryResponse? entry;
    private IReadOnlyList<AccountResponse> accounts = [];
    private bool busy;
    private string? error;

    private decimal TotalDebits => entry?.JournalLines.Sum(l => l.Debit) ?? 0;
    private decimal TotalCredits => entry?.JournalLines.Sum(l => l.Credit) ?? 0;

    protected override void OnInitialized()
    {
        entry = Entry;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            accounts = await AccountsApi.SearchAsync(null, false);
        }
        catch (ApiException)
        {
            // The view remains usable; the edit dialog can retry the account load.
        }
        catch (HttpRequestException)
        {
            // Same as above.
        }
    }

    private async Task EditAsync()
    {
        if (entry is null)
        {
            return;
        }

        var result = await DialogService.OpenAsync<JournalEntryEditDialog>(
            $"Edit journal entry {entry.Reference}",
            new Dictionary<string, object?>
            {
                ["Model"] = GeneralJournalFormModel.FromEntry(entry),
                ["IsCreate"] = false,
                ["EntryId"] = entry.Id,
                ["Accounts"] = accounts.Count > 0 ? accounts : null
            },
            new DialogOptions { Width = "1100px", Resizable = true, Draggable = true, CloseDialogOnEsc = true });

        if (result is JournalDialogResult)
        {
            DialogService.Close(result);
        }
    }

    private async Task DeleteAsync()
    {
        if (entry is null)
        {
            return;
        }

        var confirmed = await DialogService.Confirm(
            $"Delete journal entry {entry.Reference}? This cannot be undone.",
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
            await Api.DeleteAsync(entry.Id);
            DialogService.Close(new JournalDialogResult(JournalDialogOutcome.Deleted, entry.Reference));
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
