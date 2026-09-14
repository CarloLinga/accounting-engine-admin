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

    public async Task<SourceRuleResponse> CreateAsync(
        CreateSourceRuleRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync("api/SourceRules", request, ApiJson.Options, cancellationToken);
        return await SendForValueAsync<SourceRuleResponse>(response, cancellationToken);
    }

    public async Task<SourceRuleResponse> UpdateAsync(
        string sourceType, UpdateSourceRuleRequest request, CancellationToken cancellationToken = default)
    {
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

    public async Task DeleteAsync(string sourceType, CancellationToken cancellationToken = default)
    {
        using var response = await Http.DeleteAsync($"api/SourceRules/{Escape(sourceType)}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
