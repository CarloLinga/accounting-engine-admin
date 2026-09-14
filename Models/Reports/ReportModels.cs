using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AccountingEngineAdmin.Models.Accounts;

namespace AccountingEngineAdmin.Models.Reports;

// -----------------------------------------------------------------------------
// Client-side mirrors of the Accounting Engine API reporting contracts
// (/api/trial-balance, /api/ledger/{accountCode}, /api/financial-statements/*).
// JSON: camelCase properties, enums as strings. Keep in sync with the backend.
// Amounts are rendered as plain numbers by the API (no currency code), so the
// UI formats them with two decimals and thousand separators.
// -----------------------------------------------------------------------------

/// <summary>One account row of the trial balance.</summary>
public record TrialBalanceLine(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountType AccountType,
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal OpeningBalance,
    decimal PeriodDebit,
    decimal PeriodCredit,
    decimal PeriodBalance,
    decimal ClosingDebit,
    decimal ClosingCredit,
    decimal ClosingBalance);

/// <summary>Trial balance as returned by GET /api/trial-balance.</summary>
public record TrialBalanceResponse(
    int FiscalYear,
    DateTimeOffset PeriodStart,
    DateTimeOffset AsOf,
    bool IncludeInactiveAccounts,
    decimal TotalDebits,
    decimal TotalCredits,
    bool IsBalanced,
    IReadOnlyList<TrialBalanceLine> Lines);

/// <summary>One posting row of an account ledger.</summary>
public record LedgerLine(
    Guid Id,
    Guid JournalEntryId,
    string Reference,
    string SourceType,
    string? Description,
    DateTimeOffset PostedAt,
    decimal Debit,
    decimal Credit,
    decimal Balance,
    int Sequence);

/// <summary>Account ledger as returned by GET /api/ledger/{accountCode}.</summary>
public record LedgerResponse(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountType AccountType,
    bool IsActive,
    DateTimeOffset? From,
    DateTimeOffset To,
    decimal BeginningBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal EndingBalance,
    IReadOnlyList<LedgerLine> Lines);

/// <summary>One account line of a financial statement section.</summary>
public record StatementLine(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountType AccountType,
    bool IsContra,
    bool IsHeader,
    decimal Amount,
    int DisplayOrder,
    string? ParentAccountCode);

/// <summary>A titled section of a financial statement (with subtotal).</summary>
public record StatementSection(
    string Key,
    string Title,
    IReadOnlyList<StatementLine> Lines,
    decimal Subtotal);

/// <summary>Balance sheet as returned by GET /api/financial-statements/balance-sheet.</summary>
public record BalanceSheetResponse(
    DateTimeOffset AsOf,
    bool IncludeInactiveAccounts,
    IReadOnlyList<StatementSection> AssetSections,
    IReadOnlyList<StatementSection> LiabilitySections,
    IReadOnlyList<StatementSection> EquitySections,
    decimal CurrentEarnings,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    bool IsBalanced);

/// <summary>Shared display helpers for report pages (money, dates, badges).</summary>
public static class ReportFormats
{
    /// <summary>Amount with thousands separators, e.g. 1,234.50 (dashes for zero).</summary>
    public static string Money(decimal value) =>
        value == 0 ? "-" : value.ToString("N2", CultureInfo.CurrentCulture);

    /// <summary>Amount that always keeps its sign, e.g. -1,234.50 (zeros blanked).</summary>
    public static string MoneySigned(decimal value) =>
        value == 0 ? "-" : (value > 0 ? string.Empty : "-") + Math.Abs(value).ToString("N2", CultureInfo.CurrentCulture);

    /// <summary>Short date, e.g. 13 Sep 2026.</summary>
    public static string Date(DateTimeOffset? value) =>
        value?.ToString("dd MMM yyyy", CultureInfo.CurrentCulture) ?? "-";

    /// <summary>Date and time, e.g. 13 Sep 2026 07:52.</summary>
    public static string DateTime(DateTimeOffset? value) =>
        value?.ToString("dd MMM yyyy HH:mm", CultureInfo.CurrentCulture) ?? "-";

    /// <summary>CSS badge class for the account type.</summary>
    public static string Badge(AccountType type) => AccountLabels.BadgeCss(type);
}

/// <summary>Income statement as returned by GET /api/financial-statements/income-statement.</summary>
public record IncomeStatementResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    bool IncludeInactiveAccounts,
    IReadOnlyList<StatementSection> RevenueSections,
    IReadOnlyList<StatementSection> ExpenseSections,
    decimal TotalRevenue,
    decimal TotalCostOfGoodsSold,
    decimal GrossProfit,
    decimal TotalOperatingExpenses,
    decimal OperatingIncome,
    decimal TotalNonOperating,
    decimal NetIncome);

/// <summary>One adjustment row of a cash-flow section (indirect method).</summary>
public record CashFlowAdjustment(
    string Label,
    decimal Amount,
    string? AccountCode);

/// <summary>A titled section of the cash flow statement.</summary>
public record CashFlowSection(
    string Key,
    string Title,
    IReadOnlyList<CashFlowAdjustment> Adjustments,
    decimal Subtotal);

/// <summary>Cash flow statement as returned by GET /api/financial-statements/cash-flow.</summary>
public record CashFlowResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    decimal NetIncome,
    CashFlowSection Operating,
    CashFlowSection Investing,
    CashFlowSection Financing,
    decimal NetChangeInCash,
    decimal BeginningCash,
    decimal EndingCash,
    bool IsBalanced,
    IReadOnlyList<object> Unmapped);
