using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AccountingEngineAdmin.Pages.Accounts;

public partial class AccountViewDialog : ComponentBase
{
    [Inject] private IAccountsApiClient Api { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public AccountResponse Account { get; set; } = default!;

    private bool busy;
    private string? error;

    private string StatementClass => Account.BalanceSheetClass is not null
        ? AccountLabels.For(Account.BalanceSheetClass.Value)
        : Account.IncomeStatementClass is not null
            ? AccountLabels.For(Account.IncomeStatementClass.Value)
            : "-";

    private async Task EditAsync()
    {
        var model = new AccountFormModel
        {
            Code = Account.Code,
            Name = Account.Name,
            Type = Account.Type,
            Statement = Account.Statement,
            BalanceSheetClass = Account.BalanceSheetClass,
            IncomeStatementClass = Account.IncomeStatementClass,
            CashFlowActivity = Account.CashFlowActivity,
            IsCashEquivalent = Account.IsCashEquivalent,
            IsContra = Account.IsContra,
            IsPostable = Account.IsPostable,
            ParentAccountCode = Account.ParentAccountCode,
            DisplayOrder = Account.DisplayOrder,
            IsActive = Account.IsActive
        };

        var result = await DialogService.OpenAsync<AccountDialog>(
            $"Edit {Account.Code}",
            new Dictionary<string, object?> { ["Model"] = model, ["IsCreate"] = false },
            new DialogOptions { Width = "640px", Draggable = true, CloseDialogOnEsc = true });

        DialogService.Close(result is AccountResponse updated
            ? new AccountDialogResult(AccountDialogOutcome.Saved, updated.Code, updated)
            : null);
    }

    private async Task DeleteAsync()
    {
        var confirmed = await DialogService.Confirm(
            $"Delete account {Account.Code} - {Account.Name}? This cannot be undone.",
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
            await Api.DeleteAsync(Account.Code);
            DialogService.Close(new AccountDialogResult(AccountDialogOutcome.Deleted, Account.Code));
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
