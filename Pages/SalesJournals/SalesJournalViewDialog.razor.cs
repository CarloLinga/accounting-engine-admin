using AccountingEngineAdmin.Models.SalesJournals;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.SalesJournals;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.SalesJournals;

public partial class SalesJournalViewDialog : ComponentBase
{
    [Inject] private ISalesJournalsApiClient Api { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public SalesJournalResponse Journal { get; set; } = default!;

    private bool busy;
    private string? error;

    private async Task EditAsync()
    {
        var result = await DialogService.OpenAsync<SalesJournalDialog>(
            $"Edit {Journal.InvoiceNo}",
            new Dictionary<string, object?>
            {
                ["Model"] = SalesJournalFormModel.FromJournal(Journal),
                ["IsCreate"] = false,
                ["OriginalInvoiceNo"] = Journal.InvoiceNo
            },
            new DialogOptions { Width = "760px", Draggable = true, CloseDialogOnEsc = true });

        DialogService.Close(result is SalesJournalResponse updated
            ? new SalesJournalDialogResult(SalesJournalDialogOutcome.Saved, updated.InvoiceNo)
            : null);
    }

    private async Task DeleteAsync()
    {
        var confirmed = await DialogService.Confirm(
            $"Delete sale {Journal.InvoiceNo}? This cannot be undone.",
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
            await Api.DeleteAsync(Journal.InvoiceNo);
            DialogService.Close(new SalesJournalDialogResult(SalesJournalDialogOutcome.Deleted, Journal.InvoiceNo));
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