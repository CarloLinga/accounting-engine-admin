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

    /// <summary>
    /// GET /api/SourceRules/amount-types — amount-type identifiers for rule
    /// lines: distinct values referenced by existing rules merged with the
    /// engine's well-known defaults, sorted alphabetically.
    /// </summary>
    Task<IReadOnlyList<string>> GetAmountTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>GET /api/SourceRules/{id} (stable key; SourceType is mutable).</summary>
    Task<SourceRuleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>POST /api/SourceRules</summary>
    Task<SourceRuleResponse> CreateAsync(CreateSourceRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// PUT /api/SourceRules/{id} (preferred: the route Id identifies the row so
    /// renames can't miss). Requires an API with the PUT-by-Id route.
    /// currentSourceType is the pre-edit code, kept in the signature so
    /// callers must pass it explicitly and old-backend fallback code has it
    /// available; the new route itself does not use it.
    /// </summary>
    Task<SourceRuleResponse> UpdateByIdAsync(Guid id, string currentSourceType, UpdateSourceRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>PUT /api/SourceRules/{sourceType} (legacy; prefer UpdateByIdAsync for renames).</summary>
    Task<SourceRuleResponse> UpdateAsync(string sourceType, UpdateSourceRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>PATCH /api/SourceRules/{sourceType}/status?isActive=</summary>
    Task SetActiveAsync(string sourceType, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// DELETE /api/SourceRules/{id} (preferred). Requires an API with the
    /// DELETE-by-Id route. currentSourceType is the pre-delete code, kept so
    /// callers pass it explicitly and old-backend fallback code can use it.
    /// 400 when the rule is used by journal entries.
    /// </summary>
    Task DeleteByIdAsync(Guid id, string currentSourceType, CancellationToken cancellationToken = default);

    /// <summary>DELETE /api/SourceRules/{sourceType} (legacy; prefer DeleteByIdAsync).</summary>
    Task DeleteAsync(string sourceType, CancellationToken cancellationToken = default);
}
