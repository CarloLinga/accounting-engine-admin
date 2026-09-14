using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AccountingEngineAdmin.Models.Accounts;

namespace AccountingEngineAdmin.Models.SourceRules;

// ----------------------------------------------------------------------------
// Client-side mirrors of the Accounting Engine API SourceRules contract
// (/api/SourceRules). JSON: camelCase properties, enums as strings. Keep in sync
// with the backend (see AccountingEngine.Application.DTOs.SourceRuleDtos).
// ----------------------------------------------------------------------------

/// <summary>One amount type in a source rule line (e.g. TOTAL_AMOUNT).</summary>
public enum RuleAmountType
{
    TOTAL_AMOUNT,
    BASE_AMOUNT,
    TAX_AMOUNT,
    FREIGHT_AMOUNT,
    DISCOUNT_AMOUNT,
    NET_AMOUNT,
    CUSTOM
}

public enum RuleEntryType
{
    Debit,
    Credit
}

/// <summary>One debit/credit line definition within a source rule.</summary>
public record SourceRuleLineResponse(
    Guid Id,
    string AccountCode,
    RuleEntryType EntryType,
    RuleAmountType AmountType,
    int Sequence,
    string? Description = null);

/// <summary>A source rule as returned by GET /api/SourceRules.</summary>
public record SourceRuleResponse(
    Guid Id,
    string SourceType,
    string Description,
    bool IsActive,
    bool IsManualEntryAllowed,
    IReadOnlyList<SourceRuleLineResponse> RuleLines);

/// <summary>Payload for POST /api/SourceRules.</summary>
public record CreateSourceRuleRequest(
    string SourceType,
    string Description,
    bool IsManualEntryAllowed,
    IReadOnlyList<CreateSourceRuleLineRequest> RuleLines);

/// <summary>Payload for PUT /api/SourceRules/{sourceType}.</summary>
public record UpdateSourceRuleRequest(
    string SourceType,
    string Description,
    bool IsManualEntryAllowed,
    IReadOnlyList<CreateSourceRuleLineRequest> RuleLines);

/// <summary>One template line in a create/update request.</summary>
public record CreateSourceRuleLineRequest(
    string AccountCode,
    RuleEntryType EntryType,
    RuleAmountType AmountType,
    int Sequence);

/// <summary>One editable posting-template row in the source rule dialog.</summary>
public sealed class SourceRuleLineFormModel
{
    [Required(ErrorMessage = "Account is required.")]
    public string? AccountCode { get; set; }

    [Required(ErrorMessage = "Posting side is required.")]
    public RuleEntryType? EntryType { get; set; }

    [Required(ErrorMessage = "Amount type is required.")]
    public RuleAmountType? AmountType { get; set; }

    public int Sequence { get; set; }

    public bool IsFilled => AccountCode is not null && EntryType is not null && AmountType is not null;
}

/// <summary>View-model for the source rule dialog.</summary>
public sealed class SourceRuleFormModel
{
    [Required(ErrorMessage = "Source type is required.")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Source type must be UPPERCASE with underscores (e.g. CASH_INVOICE).")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Source type must be between 1 and 100 characters.")]
    public string SourceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 255 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When true the rule is a manual header-only category: the backend rejects
    /// any template lines. The dialog hides the rule-line editor in that case.
    /// </summary>
    public bool IsManualEntryAllowed { get; set; }

    public List<SourceRuleLineFormModel> RuleLines { get; } = new();

    public CreateSourceRuleRequest ToRequest() => new(
        SourceType.Trim().ToUpperInvariant(),
        Description.Trim(),
        IsManualEntryAllowed,
        BuildLineRequests());

    public UpdateSourceRuleRequest ToUpdateRequest() => new(
        SourceType.Trim().ToUpperInvariant(),
        Description.Trim(),
        IsManualEntryAllowed,
        BuildLineRequests());

    private List<CreateSourceRuleLineRequest> BuildLineRequests() =>
        IsManualEntryAllowed
            ? []
            : RuleLines
                .Where(l => l.IsFilled)
                .Select((l, i) => new CreateSourceRuleLineRequest(
                    l.AccountCode!.Trim(),
                    l.EntryType!.Value,
                    l.AmountType!.Value,
                    i + 1))
                .ToList();
}

/// <summary>Pre-built dropdown options for rule posting sides and amount types.</summary>
public static class RuleEntryTypeOptions
{
    public static List<EnumOption<RuleEntryType>> EntryTypes { get; } =
    [
        new(RuleEntryType.Debit, "Debit"),
        new(RuleEntryType.Credit, "Credit")
    ];
}

/// <summary>Pre-built dropdown options for rule amount types.</summary>
public static class RuleAmountTypeOptions
{
    public static List<EnumOption<RuleAmountType>> AmountTypes { get; } =
    [
        new(RuleAmountType.TOTAL_AMOUNT, "Total amount"),
        new(RuleAmountType.BASE_AMOUNT, "Base amount"),
        new(RuleAmountType.TAX_AMOUNT, "Tax amount"),
        new(RuleAmountType.FREIGHT_AMOUNT, "Freight amount"),
        new(RuleAmountType.DISCOUNT_AMOUNT, "Discount amount"),
        new(RuleAmountType.NET_AMOUNT, "Net amount"),
        new(RuleAmountType.CUSTOM, "Custom")
    ];

    public static string For(RuleAmountType value) => value.ToString();
}
