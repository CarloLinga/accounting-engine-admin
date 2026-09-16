using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Models.JournalEntries;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.JournalEntries;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.JournalEntries;

public partial class JournalEntryEditDialog : ComponentBase
{
    [Inject] private IJournalEntriesApiClient Api { get; set; } = default!;
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public GeneralJournalFormModel Model { get; set; } = new();
    [Parameter] public bool IsCreate { get; set; } = true;
    [Parameter] public Guid? EntryId { get; set; }
    [Parameter] public IReadOnlyList<AccountResponse>? Accounts { get; set; }

    private List<AccountResponse> accounts = [];
    private bool saving;
    private string? error;

    private IEnumerable<AccountResponse> postableAccounts =>
        accounts.Where(a => (a.IsPostable && a.IsActive) || Model.Lines.Any(l => string.Equals(l.AccountCode, a.Code, StringComparison.OrdinalIgnoreCase)));

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
            // The engine validates account codes at save time; keep the dialog usable.
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

    private int LineNumber(JournalLineFormModel line)
    {
        var index = Model.Lines.IndexOf(line);
        return index >= 0 ? index + 1 : line.Sequence;
    }

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        try
        {
            var entry = IsCreate
                ? await Api.PostGeneralJournalAsync(Model.ToRequest())
                : await Api.UpdateAsync(EntryId ?? throw new InvalidOperationException("An entry ID is required when editing."), Model.ToUpdateRequest());
            DialogService.Close(new JournalDialogResult(JournalDialogOutcome.Saved, entry.Reference));
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
