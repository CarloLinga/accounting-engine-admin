using System.ComponentModel.DataAnnotations;

namespace AccountingEngineAdmin.Models.SalesJournals;

public record SalesJournalResponse(
    Guid Id,
    string InvoiceNo,
    DateOnly InvoiceDate,
    string Customer,
    string TaxIDNo,
    decimal InvoiceAmount,
    decimal VatAmount,
    decimal VATableSale,
    decimal ZeroRatedSale,
    decimal VatExemptSale,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record CreateSalesJournalRequest(
    string InvoiceNo,
    DateOnly InvoiceDate,
    string Customer,
    string TaxIDNo,
    decimal InvoiceAmount,
    decimal VatAmount,
    decimal VATableSale,
    decimal ZeroRatedSale,
    decimal VatExemptSale);

public record UpdateSalesJournalRequest(
    string InvoiceNo,
    DateOnly InvoiceDate,
    string Customer,
    string TaxIDNo,
    decimal InvoiceAmount,
    decimal VatAmount,
    decimal VATableSale,
    decimal ZeroRatedSale,
    decimal VatExemptSale);

public sealed class SalesJournalFormModel
{
    [Required(ErrorMessage = "Invoice number is required.")]
    [StringLength(50, ErrorMessage = "Invoice number must be at most 50 characters.")]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Invoice date is required.")]
    public DateTime? InvoiceDate { get; set; } = DateTime.Today;

    [StringLength(255, ErrorMessage = "Customer must be at most 255 characters.")]
    public string Customer { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Tax ID number must be at most 50 characters.")]
    public string TaxIDNo { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Enter zero or a positive amount.")]
    public decimal InvoiceAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Enter zero or a positive amount.")]
    public decimal VatAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Enter zero or a positive amount.")]
    public decimal VATableSale { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Enter zero or a positive amount.")]
    public decimal ZeroRatedSale { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Enter zero or a positive amount.")]
    public decimal VatExemptSale { get; set; }

    public static SalesJournalFormModel FromJournal(SalesJournalResponse journal) => new()
    {
        InvoiceNo = journal.InvoiceNo,
        InvoiceDate = journal.InvoiceDate.ToDateTime(TimeOnly.MinValue),
        Customer = journal.Customer,
        TaxIDNo = journal.TaxIDNo,
        InvoiceAmount = journal.InvoiceAmount,
        VatAmount = journal.VatAmount,
        VATableSale = journal.VATableSale,
        ZeroRatedSale = journal.ZeroRatedSale,
        VatExemptSale = journal.VatExemptSale
    };

    public CreateSalesJournalRequest ToRequest() => new(
        InvoiceNo.Trim(),
        DateOnly.FromDateTime(InvoiceDate!.Value),
        Customer.Trim(),
        TaxIDNo.Trim(),
        InvoiceAmount,
        VatAmount,
        VATableSale,
        ZeroRatedSale,
        VatExemptSale);

    public UpdateSalesJournalRequest ToUpdateRequest() => new(
        InvoiceNo.Trim(),
        DateOnly.FromDateTime(InvoiceDate!.Value),
        Customer.Trim(),
        TaxIDNo.Trim(),
        InvoiceAmount,
        VatAmount,
        VATableSale,
        ZeroRatedSale,
        VatExemptSale);
}

public enum SalesJournalDialogOutcome
{
    Saved,
    Deleted
}

public sealed record SalesJournalDialogResult(SalesJournalDialogOutcome Outcome, string InvoiceNo);