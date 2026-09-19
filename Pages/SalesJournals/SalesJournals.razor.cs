using AccountingEngineAdmin.Models.SalesJournals;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.SalesJournals;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.SalesJournals;

public partial class SalesJournals : ComponentBase
{
    [Inject] private ISalesJournalsApiClient SalesJournalsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private List<SalesJournalResponse> journals = [];
    private bool isLoading = true;
    private string? loadError;
    private string searchText = string.Empty;

    private List<SalesJournalResponse> visibleJournals => string.IsNullOrWhiteSpace(searchText)
        ? journals
        : journals.Where(j =>
            j.InvoiceNo.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase) ||
            j.Customer.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase) ||
            j.TaxIDNo.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        StateHasChanged();

        try
        {
            journals = (await SalesJournalsApi.GetAllAsync()).OrderByDescending(j => j.InvoiceDate).ToList();
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

    private async Task OpenCreateAsync()
    {
        var result = await DialogService.OpenAsync<SalesJournalDialog>(
            "New Sale",
            new Dictionary<string, object?> { ["Model"] = new SalesJournalFormModel(), ["IsCreate"] = true },
            new DialogOptions { Width = "760px", Draggable = true, CloseDialogOnEsc = true });

        if (result is SalesJournalResponse created)
        {
            Notify(NotificationSeverity.Success, "Sale created", created.InvoiceNo);
            await LoadAsync();
        }
    }

    private async Task OpenViewAsync(SalesJournalResponse journal)
    {
        var result = await DialogService.OpenAsync<SalesJournalViewDialog>(
            $"Sale {journal.InvoiceNo}",
            new Dictionary<string, object?> { ["Journal"] = journal },
            new DialogOptions { Width = "820px", Draggable = true, CloseDialogOnEsc = true });

        if (result is not SalesJournalDialogResult dialogResult)
        {
            return;
        }

        Notify(
            NotificationSeverity.Success,
            dialogResult.Outcome == SalesJournalDialogOutcome.Deleted ? "Sale deleted" : "Sale updated",
            dialogResult.InvoiceNo);
        await LoadAsync();
    }

    private static string SalesClassification(SalesJournalResponse journal)
    {
        var parts = new List<string>(3);
        if (journal.VATableSale != 0) parts.Add($"VATable {journal.VATableSale:N2}");
        if (journal.ZeroRatedSale != 0) parts.Add($"Zero-rated {journal.ZeroRatedSale:N2}");
        if (journal.VatExemptSale != 0) parts.Add($"Exempt {journal.VatExemptSale:N2}");
        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private void Notify(NotificationSeverity severity, string summary, string detail) =>
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = detail,
            Duration = 4000
        });
}