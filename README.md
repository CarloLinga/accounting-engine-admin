# Accounting Engine Admin

Blazor WebAssembly administration front end for the Accounting Engine REST API. The application provides a browser-based workspace for maintaining accounts, configuring source rules, entering and reviewing journals, and viewing accounting reports.

The app is a standalone client-side WebAssembly application. The API is deployed separately, so the published `wwwroot` folder can be hosted by a static site provider such as Cloudflare Pages.

## Features

- Dashboard with API status and accounting summary information
- Chart of Accounts management, including search, filtering, create/edit, activation, and deletion
- Source rule management
- Journal entry list and general journal workflows
- Trial balance reporting
- General ledger reporting by account
- Income statement, balance sheet, and cash flow views
- Radzen dialogs, grids, forms, notifications, and charts

## Routes

| Area | Route |
| --- | --- |
| Dashboard | `/` |
| Accounts | `/accounts` |
| Source rules | `/source-rules` |
| Journal entries | `/journals` |
| Trial balance | `/trial-balance` |
| General ledger | `/ledger` |
| Financial statements | `/financial-statements` |

## Technology

- **.NET 9 / Blazor WebAssembly**
- **Radzen.Blazor 11.3.2** for the UI components
- Typed `HttpClient` services through `Microsoft.Extensions.Http`
- Nullable reference types and implicit usings enabled

## Project structure

```text
AccountingEngineAdmin/
├── Layout/                         # Application shell and navigation
├── Models/
│   ├── Accounts/                   # Account DTOs, enums, and form models
│   ├── JournalEntries/             # Journal entry models
│   ├── Reports/                    # Trial balance and statement models
│   └── SourceRules/                # Source rule models
├── Pages/
│   ├── Accounts/                   # Chart of Accounts and account dialog
│   ├── FinancialStatements/        # Income statement, balance sheet, cash flow
│   ├── GeneralLedger/              # General ledger report
│   ├── JournalEntries/             # Journal list and journal dialogs
│   ├── SourceRules/                # Source rule list and dialog
│   └── TrialBalance/               # Trial balance report
├── Services/
│   ├── Accounts/                   # Accounts API client
│   ├── JournalEntries/             # Journals API client
│   ├── Reports/                    # Reporting API client
│   ├── SourceRules/                # Source rules API client
│   ├── ApiClientBase.cs            # Shared HTTP client behavior
│   ├── ApiException.cs             # API and ProblemDetails errors
│   └── ApiJson.cs                  # Shared JSON serialization options
├── Properties/launchSettings.json  # Local development profile
└── wwwroot/
    ├── appsettings.json            # Development API configuration
    ├── appsettings.Production.json # Production API configuration
    └── _redirects                  # SPA fallback for Cloudflare Pages
```

## Prerequisites

- .NET 9 SDK
- A running Accounting Engine API, unless you only need to build or preview the static shell

The client expects the API to be reachable from the browser and to allow the deployed client origin through CORS.

## Configuration

The API URL is read from `Api:BaseUrl` in the static configuration files:

- Development: `wwwroot/appsettings.json`, currently `http://localhost:5255/`
- Production: `wwwroot/appsettings.Production.json`, which contains the deployed API URL placeholder

Keep the trailing slash on `Api:BaseUrl`. Because these files are shipped to the browser, do not put secrets or private credentials in them.

## Run locally

1. Start the Accounting Engine API at the URL configured in `wwwroot/appsettings.json`.
2. From the repository root, run:

   ```powershell
   dotnet run
   ```

3. Open <http://localhost:5001> when the development server starts.

To use a different API locally, update `Api:BaseUrl` in `wwwroot/appsettings.json` before starting the client.

## Build and publish

Restore dependencies and create a production publish output with:

```powershell
dotnet restore
dotnet build
dotnet publish -c Release
```

The static site files are written to:

```text
bin/Release/net9.0/publish/wwwroot
```

The `bin/` and `obj/` directories are local build output and are excluded from Git.

## Deploy to Cloudflare Pages

1. Set the real API URL in `wwwroot/appsettings.Production.json`, including its trailing slash.
2. Publish the application with `dotnet publish -c Release`.
3. Deploy `bin/Release/net9.0/publish/wwwroot` as the Cloudflare Pages output directory.
4. If using a Git-connected build, use `dotnet publish -c Release` as the build command and `bin/Release/net9.0/publish/wwwroot` as the output directory.

The included `wwwroot/_redirects` file contains `/* /index.html 200`, which allows direct navigation to client-side routes such as `/accounts` and `/ledger`.

## Troubleshooting notes

- If the dashboard cannot load data, verify the API URL, that the API is running, and that its CORS policy allows the browser origin.
- If a deep link returns a hosting-provider 404, verify that the `_redirects` file was included in the deployed `wwwroot` output.
- `DisableBuildCompression` is enabled in the project file as a workaround for a .NET SDK 9.0.3xx static-web-assets compression issue (`MSB4018: Endpoints not found for related asset`). Static hosting still provides compression at the edge.
