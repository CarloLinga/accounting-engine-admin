using System.ComponentModel.DataAnnotations;

namespace AccountingEngineAdmin.Models.JournalEntries;

// -----------------------------------------------------------------------------
// Client-side mirrors of the Accounting Engine API Journals contract
// (/api/Journals). JSON: camelCase properties, enums as strings. Keep in sync
// with the backend. Posted journal entries are immutable - there are no
// update/delete endpoints (correct behavior for accounting).
// -----------------------------------------------------------------------------

/// <summary>One posting line of a journal entry as returned by the API.</summary>
public record JournalLineResponse(
    Guid Id,
    int Sequence,
    string AccountCode,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string? Description);

/// <summary>Journal entry as returned by the API (list, by id and by reference).</summary>
public record JournalEntryResponse(
    Guid Id,
    string Reference,
    string SourceType,
    string? Description,
    DateTimeOffset PostedAt,
    IReadOnlyList<JournalLineResponse> JournalLines);

/// <summary>Payload for POST /api/Journals (manual general journal entry).</summary>
public record JournalLineRequest(
    string AccountCode,
    decimal Debit,
    decimal Credit,
    string? Description,
    int Sequence);

/// <summary>Payload for POST /api/Journals. At least two lines are required and total debits must equal total credits.</summary>
public record PostGeneralJournalRequest(
    string SourceType,
    string Reference,
    DateTimeOffset PostedAt,
    string? Description,
    IReadOnlyList<JournalLineRequest> Lines);

// -----------------------------------------------------------------------------
// Editable form models for the posting dialogs. Kept separate from the API DTOs
// because DTO records use init-only setters (no two-way binding) and because
// form validation rules belong to the UI, not the wire contract.
// -----------------------------------------------------------------------------

/// <summary>One editable posting line in the "Post General Journal" dialog.</summary>
public sealed class JournalLineFormModel
{
    [Required(ErrorMessage = "Account is required.")]
    public string? AccountCode { get; set; }

    [Range(0.0000001, double.MaxValue, ErrorMessage = "Enter an amount greater than zero, or leave blank.")]
    public decimal? Debit { get; set; }

    [Range(0.0000001, double.MaxValue, ErrorMessage = "Enter an amount greater than zero, or leave blank.")]
    public decimal? Credit { get; set; }

    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sequence must be zero or greater.")]
    public int Sequence { get; set; }

    public bool IsFilled => Debit is > 0 || Credit is > 0;
}

/// <summary>View-model for the "Post General Journal" dialog.</summary>
public sealed class GeneralJournalFormModel
{
    [Required(ErrorMessage = "Source type is required.")]
    public string? SourceType { get; set; } = "GENERAL_JOURNAL";

    [Required(ErrorMessage = "Reference is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Reference must be between 1 and 100 characters.")]
    public string Reference { get; set; } = string.Empty;

    /// <summary>Date part only; sent to the API as midnight UTC.</summary>
    [Required(ErrorMessage = "Posting date is required.")]
    public DateTime? PostedOn { get; set; } = DateTime.Today;

    public string? Description { get; set; }

    public List<JournalLineFormModel> Lines { get; } = new();

    public decimal TotalDebits => Lines.Sum(l => l.Debit ?? 0);
    public decimal TotalCredits => Lines.Sum(l => l.Credit ?? 0);
    public bool IsBalanced => TotalDebits == TotalCredits;
    public int FilledLines => Lines.Count(l => l.IsFilled);

    public PostGeneralJournalRequest ToRequest() => new(
        SourceType!.Trim(),
        Reference.Trim(),
        new DateTimeOffset(PostedOn!.Value, TimeSpan.Zero),
        string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Lines
            .Select((line, index) => new JournalLineRequest(
                line.AccountCode!.Trim(),
                line.Debit ?? 0,
                line.Credit ?? 0,
                string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(),
                index + 1))
            .ToList());
}

/// <summary>One editable named amount (e.g. TOTAL_AMOUNT = 1150.00) for source posting.</summary>
public sealed class SourceAmountFormModel
{
    [Required(ErrorMessage = "Amount name is required.")]
    public string Key { get; set; } = string.Empty;

    [Range(0.0000001, double.MaxValue, ErrorMessage = "Enter an amount greater than zero.")]
    public decimal? Value { get; set; }
}

/// <summary>View-model for the "Post Source Transaction" dialog.</summary>
public sealed class SourceTransactionFormModel
{
    [Required(ErrorMessage = "Source type is required.")]
    public string? SourceType { get; set; }

    [Required(ErrorMessage = "Reference is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Reference must be between 1 and 100 characters.")]
    public string Reference { get; set; } = string.Empty;

    /// <summary>Date part only; sent to the API as midnight UTC.</summary>
    [Required(ErrorMessage = "Posting date is required.")]
    public DateTime? PostedOn { get; set; } = DateTime.Today;

    [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
    public string? Description { get; set; }

    public List<SourceAmountFormModel> Amounts { get; } = new();

    public PostSourceTransactionRequest ToRequest() => new(
        SourceType!.Trim(),
        Reference.Trim(),
        new DateTimeOffset(PostedOn!.Value, TimeSpan.Zero),
        string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Amounts
            .Where(a => !string.IsNullOrWhiteSpace(a.Key) && a.Value is > 0)
            .ToDictionary(a => a.Key.Trim(), a => a.Value!.Value));
}


/// <summary>
/// Payload for POST /api/Journals/source. The engine derives the posting lines
/// from the source rule of the given source type, consuming the named amounts
/// (keys usually match the rule's amount types, e.g. TOTAL_AMOUNT).
/// </summary>
public record PostSourceTransactionRequest(
    string SourceType,
    string Reference,
    DateTimeOffset PostedAt,
    string? Description,
    IReadOnlyDictionary<string, decimal> Amounts);
