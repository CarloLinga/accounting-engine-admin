# Accounting Engine Admin

Blazor WebAssembly admin front-end for the [Accounting Engine API](https://github.com/) (headless REST API hosted on Render).

## Stack

- **.NET 9 / Blazor WebAssembly** (standalone, no hosted server needed in production)
- **Radzen.Blazor 11.x** (MIT) — grid, dialogs, notifications, forms, charts
- Typed `HttpClient` clients via `Microsoft.Extensions.Http` (`IHttpClientFactory`)

## Structure

```
AccountingEngineAdmin/
├── Layout/                  # MainLayout, NavMenu (+ Radzen service hosts)
├── Models/
│   └── Accounts/            # DTOs, enums, form models, display helpers
├── Pages/
│   ├── Home.razor(.cs/.css) # Dashboard: stats, donut chart, API status
│   └── Accounts/            # Chart of Accounts CRUD
│       ├── Accounts.razor(.cs/.css)   # list/search/filter/activate/delete
│       ├── AccountDialog.razor(.cs)   # create/edit dialog
│       ├── AccountDialogBase.cs
│       └── AccountOptions.cs
├── Properties/launchSettings.json     # dev server profile (http://localhost:5001)
├── Services/
│   ├── Accounts/            # IAccountsApiClient + REST implementation
│   ├── ApiJson.cs           # JSON options (camelCase, enums-as-strings)
│   └── ApiException.cs      # ProblemDetails-aware API errors
└── wwwroot/
    ├── appsettings.json               # dev config (Api:BaseUrl = localhost:5000)
    ├── appsettings.Production.json    # deploy config — SET THE RENDER URL HERE
    └── _redirects                     # Cloudflare Pages SPA fallback
```

## Run locally

1. Start the Accounting Engine API (expects it on `http://localhost:5000`).
2. `dotnet run --project AccountingEngineAdmin` → opens `http://localhost:5001`.

## Deploy to Cloudflare Pages (free tier)

1. `dotnet publish -c Release` → output folder `bin/Release/net9.0/publish/wwwroot`.
2. Point Cloudflare Pages at that folder (direct upload or Git CI with build command `dotnet publish -c Release`).
3. `_redirects` (`/* /index.html 200`) is already included so deep links like `/accounts` work.
4. **Before deploying**, replace the placeholder in `wwwroot/appsettings.Production.json`
   (`Api:BaseUrl`) with the real Render URL of your API, including the trailing slash.

## Notes

- `DisableBuildCompression` is set in the csproj — workaround for a .NET SDK 9.0.3xx
  static-web-assets bug (`MSB4018: Endpoints not found for related asset`). Cloudflare
  compresses on the edge, so nothing is lost.
- API CORS: the backend accepts any origin, so no proxying is required.
