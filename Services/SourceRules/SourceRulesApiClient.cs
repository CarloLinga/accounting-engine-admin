using System.Net.Http.Json;
using AccountingEngineAdmin.Models.SourceRules;

namespace AccountingEngineAdmin.Services.SourceRules;

/// <summary>
/// ISourceRulesApiClient implementation targeting the Accounting Engine API
/// (base address configured in Program.cs from 'Api:BaseUrl').
/// </summary>
public sealed class SourceRulesApiClient : ApiClientBase, ISourceRulesApiClient
{
    public SourceRulesApiClient(HttpClient http) : base(http)
    {
    }

    public async Task<IReadOnlyList<SourceRuleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync("api/SourceRules", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var rules = await TryReadAsync<List<SourceRuleResponse>>(response, cancellationToken);
        return rules ?? [];
    }

    public async Task<IReadOnlyList<string>> GetAmountTypesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync("api/SourceRules/amount-types", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var amountTypes = await TryReadAsync<List<string>>(response, cancellationToken);
        return amountTypes ?? [];
    }

    public async Task<SourceRuleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync($"api/SourceRules/{id:D}", cancellationToken);
        return await SendForValueAsync<SourceRuleResponse>(response, cancellationToken);
    }

    public async Task<SourceRuleResponse> CreateAsync(
        CreateSourceRuleRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync("api/SourceRules", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<SourceRuleResponse>(response, cancellationToken);
    }

    public async Task<SourceRuleResponse> UpdateByIdAsync(
        Guid id, string currentSourceType, UpdateSourceRuleRequest request, CancellationToken cancellationToken = default)
    {
        // The route Id (not the mutable SourceType) identifies the row, so a
        // rename (XXX -> XXX_UPDATED) can never target the wrong row or miss
        // it. currentSourceType is unused by the new route -- it is kept in
        // the signature so callers pass the pre-edit value explicitly and the
        // legacy UpdateAsync stays available for old backends.
        _ = currentSourceType;
        using var response = await Http.PutAsJsonAsync($"api/SourceRules/{id:D}", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<SourceRuleResponse>(response, cancellationToken);
    }

    public async Task<SourceRuleResponse> UpdateAsync(
        string sourceType, UpdateSourceRuleRequest request, CancellationToken cancellationToken = default)
    {
        // Legacy route keyed by the (mutable) sourceType: the route value is
        // the CURRENT code and the body carries the new one. Prefer
        // UpdateByIdAsync, which cannot miss or hit the wrong row on renames.
        using var response = await Http.PutAsJsonAsync($"api/SourceRules/{Escape(sourceType)}", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<SourceRuleResponse>(response, cancellationToken);
    }

    public async Task SetActiveAsync(string sourceType, bool isActive, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/SourceRules/{Escape(sourceType)}/status?isActive={(isActive ? "true" : "false")}");

        using var response = await Http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, string currentSourceType, CancellationToken cancellationToken = default)
    {
        // Id-keyed delete mirrors UpdateByIdAsync: the route Id identifies the
        // row. currentSourceType is unused by the new route (kept for symmetry
        // and old-backend fallback use).
        _ = currentSourceType;
        using var response = await Http.DeleteAsync($"api/SourceRules/{id:D}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(string sourceType, CancellationToken cancellationToken = default)
    {
        // Legacy sourceType-keyed delete; prefer DeleteByIdAsync.
        using var response = await Http.DeleteAsync($"api/SourceRules/{Escape(sourceType)}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
