using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.JournalEntries;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.JournalEntries;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.JournalEntries;

public partial class GeneralJournalDialog : ComponentBase
{
    [Inject] private IJournalEntriesApiClient Api { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public GeneralJournalFormModel Model { get; set; } = new();

    [Parameter] public bool IsCreate { get; set; } = true;

    [Parameter] public Guid? EntryId { get; set; }

    private List<AccountResponse> accounts = [];
    private bool saving;
    private string? error;

    private IEnumerable<AccountResponse> postableAccounts =>
        accounts.Where(a => a.IsPostable && a.IsActive);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            accounts = (await AccountsApi.SearchAsync(null, false)).ToList();
        }
        catch (ApiException)
        {
            // The engine validates accounts at save time; keep the dialog usable.
        }
        catch (HttpRequestException)
        {
            // Same - the dropdown can be populated later via Refresh.
        }

        if (Model.Lines.Count == 0)
        {
            Model.Lines.Add(new JournalLineFormModel());
            Model.Lines.Add(new JournalLineFormModel());
        }
    }

    private void AddLine() => Model.Lines.Add(new JournalLineFormModel());

    private void RemoveLine(JournalLineFormModel line) => Model.Lines.Remove(line);

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        try
        {
            var result = IsCreate
                ? await Api.PostGeneralJournalAsync(Model.ToRequest())
                : await Api.UpdateAsync(EntryId ?? throw new InvalidOperationException("An entry ID is required when editing."), Model.ToUpdateRequest());
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
