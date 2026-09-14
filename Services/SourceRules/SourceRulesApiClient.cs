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

    public async Task<SourceRuleResponse?> CreateAsync(
        CreateSourceRuleRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync("api/SourceRules", request, ApiJson.Options, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        // The API answers 200 with the created rule; the payload is parsed
        // tolerantly so an unexpected (or empty) body still counts as success.
        return await TryReadAsync<SourceRuleResponse>(response, cancellationToken);
    }

    public async Task SetActiveAsync(string sourceType, bool isActive, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/SourceRules/{Escape(sourceType)}/status?isActive={(isActive ? "true" : "false")}");

        using var response = await Http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
