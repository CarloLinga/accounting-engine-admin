using AccountingEngineAdmin.Models.Accounts;
using AccountingEngineAdmin.Pages.Accounts;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;

namespace AccountingEngineAdmin.Pages;

public partial class Home : ComponentBase
{
    [Inject] private IAccountsApiClient AccountsApi { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;

    private bool loading = true;
    private bool apiOnline;
    private string apiBaseUrl = string.Empty;
    private List<AccountResponse> accounts = new();
    private List<TypeSlice> typeSlices = new();

    private int TotalCount => accounts.Count;
    private int ActiveCount => accounts.Count(a => a.IsActive);
    private int InactiveCount => accounts.Count(a => !a.IsActive);
    private int TypesInUse => accounts.Select(a => a.Type).Distinct().Count();
    private IEnumerable<AccountResponse> RecentAccounts =>
        accounts.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).Take(5);

    private static readonly AccountType[] AllTypes =
        Enum.GetValues<AccountType>();

    protected override async Task OnInitializedAsync()
    {
        apiBaseUrl = Configuration["Api:BaseUrl"] ?? string.Empty;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        loading = true;
        StateHasChanged();

        try
        {
            accounts = (await AccountsApi.SearchAsync(null, true)).ToList();
            typeSlices = Enum.GetValues<AccountType>()
                .Select(t => new TypeSlice(AccountLabels.For(t), accounts.Count(a => a.Type == t)))
                .Where(s => s.Count > 0)
                .ToList();
            apiOnline = true;
        }
        catch
        {
            apiOnline = false;
            accounts = new List<AccountResponse>();
            typeSlices = new();
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    private int CountByType(AccountType type) => accounts.Count(a => a.Type == type);

    private sealed record TypeSlice(string Label, int Count);

    private async Task OpenCreateAsync()
    {
        var result = await DialogService.OpenAsync<AccountDialog>(
            "New Account",
            new Dictionary<string, object?> { ["Model"] = new AccountFormModel() },
            new DialogOptions { Width = "640px", Draggable = true, CloseDialogOnEsc = true });

        if (result is AccountResponse created)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Account created",
                Detail = $"{created.Code} - {created.Name}",
                Duration = 4000
            });
            await RefreshAsync();
        }
    }

    private void GoToAccounts() => Navigation.NavigateTo("accounts");

    private void OpenScalarDocs() =>
        Navigation.NavigateTo($"{apiBaseUrl.TrimEnd('/')}/scalar/v1", true);

    private void OpenHealth() =>
        Navigation.NavigateTo($"{apiBaseUrl.TrimEnd('/')}/health", true);
}
