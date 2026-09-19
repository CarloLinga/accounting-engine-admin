using AccountingEngineAdmin.Models.SalesJournals;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.SalesJournals;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.SalesJournals;

public partial class SalesJournalDialog : ComponentBase
{
    [Inject] private ISalesJournalsApiClient Api { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public SalesJournalFormModel Model { get; set; } = new();
    [Parameter] public bool IsCreate { get; set; } = true;
    [Parameter] public string? OriginalInvoiceNo { get; set; }

    private bool saving;
    private string? error;

    private async Task SaveAsync()
    {
        saving = true;
        error = null;
        try
        {
            var result = IsCreate
                ? await Api.CreateAsync(Model.ToRequest())
                : await Api.UpdateAsync(OriginalInvoiceNo ?? Model.InvoiceNo, Model.ToUpdateRequest());
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