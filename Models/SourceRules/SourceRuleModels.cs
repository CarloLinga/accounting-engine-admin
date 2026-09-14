using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AccountingEngineAdmin.Models.Accounts;

namespace AccountingEngineAdmin.Models.SourceRules;

// -----------------------------------------------------------------------------
// Client-side mirrors of the Accounting Engine API SourceRules contract
// (/api/SourceRules). JSON: camelCase properties, enums as strings. Keep in sync
// with the backend.
// -----------------------------------------------------------------------------

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
    int Sequence,
    RuleAmountType AmountType,
    string AccountCode,
    RuleEntryType EntryType,
    string? Description);

/// <summary>A source rule as returned by GET /api/SourceRules.</summary>
public record SourceRuleResponse(
    string SourceType,
    string Description,
    bool IsActive,
    bool IsManualEntryAllowed,
    IReadOnlyList<SourceRuleLineResponse> RuleLines);

/// <summary>Payload for POST /api/SourceRules.</summary>
public record CreateSourceRuleRequest(
    string SourceType,
    string Description,
    bool IsActive,
    IReadOnlyList<CreateSourceRuleLineRequest> RuleLines);

/// <summary>One line in a create request.</summary>
public record CreateSourceRuleLineRequest(
    RuleAmountType AmountType,
    string AccountCode,
    decimal DebitPercentage,
    decimal CreditPercentage,
    string? Description);

/// <summary>One editable amount row in the source rule dialog.</summary>
public sealed class SourceRuleLineFormModel
{
    [Required(ErrorMessage = "Amount type is required.")]
    public RuleAmountType? AmountType { get; set; }

    [Required(ErrorMessage = "Account is required.")]
    public string? AccountCode { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Debit percentage must be zero or greater.")]
    public decimal? DebitPercentage { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Credit percentage must be zero or greater.")]
    public decimal? CreditPercentage { get; set; }

    public string? Description { get; set; }

    public int Sequence { get; set; }

    public bool IsFilled => AmountType is not null && AccountCode is not null &&
                            (DebitPercentage is > 0 || CreditPercentage is > 0);
}

/// <summary>View-model for the source rule dialog.</summary>
public sealed class SourceRuleFormModel
{
    [Required(ErrorMessage = "Source type is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Source type must be between 1 and 50 characters.")]
    public string SourceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 200 characters.")]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<SourceRuleLineFormModel> RuleLines { get; } = new();

    public CreateSourceRuleRequest ToRequest() => new(
        SourceType.Trim(),
        Description.Trim(),
        IsActive,
        RuleLines
            .Where(l => l.IsFilled)
            .Select((l, i) => new CreateSourceRuleLineRequest(
                l.AmountType!.Value,
                l.AccountCode!.Trim(),
                l.DebitPercentage ?? 0,
                l.CreditPercentage ?? 0,
                string.IsNullOrWhiteSpace(l.Description) ? null : l.Description.Trim()))
            .ToList());
}

/// <summary>Pre-built dropdown options for rule amount types.</summary>
public static class RuleAmountTypeOptions
{
    public static List<EnumOption<RuleAmountType>> AmountTypes { get; } =
    [
        new(RuleAmountType.TOTAL_AMOUNT, "Total amount"),
        new(RuleAmountType.TAX_AMOUNT, "Tax amount"),
        new(RuleAmountType.FREIGHT_AMOUNT, "Freight amount"),
        new(RuleAmountType.DISCOUNT_AMOUNT, "Discount amount"),
        new(RuleAmountType.NET_AMOUNT, "Net amount"),
        new(RuleAmountType.CUSTOM, "Custom")
    ];

    public static string For(RuleAmountType value) => value.ToString();
}
