using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using AccountingEngineAdmin.Services;
using AccountingEngineAdmin.Services.Accounts;
using AccountingEngineAdmin.Services.SourceRules;
using AccountingEngineAdmin.Services.JournalEntries;
using AccountingEngineAdmin.Services.Reports;
using AccountingEngineAdmin;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// -----------------------------------------------------------------------------
// Configuration: wwwroot/appsettings.json (+ appsettings.{Environment}.json).
// 'Api:BaseUrl' points at the Accounting Engine API; override it per environment
// (e.g. the Render URL in a production settings file when deploying).
// -----------------------------------------------------------------------------
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Missing configuration 'Api:BaseUrl'. Define it in wwwroot/appsettings.json.");

// -----------------------------------------------------------------------------
// UI toolkit: Radzen.Blazor (MIT licensed) - notifications, dialogs, grid, etc.
// -----------------------------------------------------------------------------
builder.Services.AddRadzenComponents();

// -----------------------------------------------------------------------------
// Typed HTTP client for the Accounting Engine API.
// The backend already ships a permissive CORS policy ("Frontend"), so browser
// calls from any origin are accepted.
// -----------------------------------------------------------------------------
builder.Services.AddHttpClient<AccountsApiClient>(http =>
{
    http.BaseAddress = new Uri(apiBaseUrl);
    http.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IAccountsApiClient>(sp => sp.GetRequiredService<AccountsApiClient>());

// Source rules (/api/SourceRules).
builder.Services.AddHttpClient<SourceRulesApiClient>(http =>
{
    http.BaseAddress = new Uri(apiBaseUrl);
    http.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ISourceRulesApiClient>(sp => sp.GetRequiredService<SourceRulesApiClient>());

// Journal entries (/api/Journals).
builder.Services.AddHttpClient<JournalEntriesApiClient>(http =>
{
    http.BaseAddress = new Uri(apiBaseUrl);
    http.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IJournalEntriesApiClient>(sp => sp.GetRequiredService<JournalEntriesApiClient>());

// Reports (/api/trial-balance, /api/ledger/{code}, /api/financial-statements/*).
builder.Services.AddHttpClient<ReportsApiClient>(http =>
{
    http.BaseAddress = new Uri(apiBaseUrl);
    http.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IReportsApiClient>(sp => sp.GetRequiredService<ReportsApiClient>());

await builder.Build().RunAsync();
