using AccountingEngineAdmin.Models.SourceRules;

namespace AccountingEngineAdmin.Services.SourceRules;

/// <summary>
/// Typed client for the /api/SourceRules endpoints of the Accounting Engine API.
/// Implementations throw <see cref="ApiException"/> for non-success responses
/// and <see cref="HttpRequestException"/> when the API is unreachable.
/// </summary>
public interface ISourceRulesApiClient
{
    /// <summary>GET /api/SourceRules</summary>
    Task<IReadOnlyList<SourceRuleResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>POST /api/SourceRules</summary>
    Task<SourceRuleResponse> CreateAsync(CreateSourceRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>PUT /api/SourceRules/{sourceType}</summary>
    Task<SourceRuleResponse> UpdateAsync(string sourceType, UpdateSourceRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>PATCH /api/SourceRules/{sourceType}/status?isActive=</summary>
    Task SetActiveAsync(string sourceType, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>DELETE /api/SourceRules/{sourceType} (400 when the rule is used by journal entries).</summary>
    Task DeleteAsync(string sourceType, CancellationToken cancellationToken = default);
}
