using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.Accounts;

public partial class AccountDialog : ComponentBase
{
    [Inject] private IAccountsApiClient Api { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public AccountFormModel Model { get; set; } = new();
    [Parameter] public bool IsCreate { get; set; } = true;

    private bool saving;
    private string? error;

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        try
        {
            AccountResponse result = IsCreate
                ? await Api.CreateAsync(Model.ToRequest())
                : await Api.UpdateAsync(Model.Code, Model.ToUpdateRequest());

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
