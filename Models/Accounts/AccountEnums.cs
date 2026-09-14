namespace AccountingEngineAdmin.Models.Accounts;

// -----------------------------------------------------------------------------
// Client-side mirrors of AccountingEngine.Core.Domain.Enums.
// The API serializes enums as strings (JsonStringEnumConverter), so member
// names MUST match the backend exactly. Keep in sync with the backend.
// -----------------------------------------------------------------------------

/// <summary>Chart-of-accounts classification.</summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

/// <summary>Which primary financial statement an account rolls into.</summary>
public enum FinancialStatement
{
    BalanceSheet = 1,
    IncomeStatement = 2
}

/// <summary>Balance-sheet section grouping.</summary>
public enum BalanceSheetClass
{
    CurrentAsset = 1,
    NonCurrentAsset = 2,
    CurrentLiability = 3,
    NonCurrentLiability = 4,
    Equity = 5
}

/// <summary>Income-statement grouping for a presentable single-period report.</summary>
public enum IncomeStatementClass
{
    OperatingRevenue = 1,
    NonOperatingRevenue = 2,
    ContraRevenue = 3,
    CostOfGoodsSold = 4,
    OperatingExpense = 5,
    NonOperatingExpense = 6
}

/// <summary>Cash-flow section for balance-sheet movements (indirect method).</summary>
public enum CashFlowActivity
{
    Unclassified = 0,
    Operating = 1,
    Investing = 2,
    Financing = 3
}

/// <summary>A dropdown option pairing an enum value with a friendly label.</summary>
public sealed record EnumOption<T>(T Value, string Label) where T : struct, Enum;

/// <summary>Pre-built dropdown option lists for the account dialogs.</summary>
public static class AccountOptions
{
    public static List<EnumOption<AccountType>> AccountTypes { get; } =
    [
        new(AccountType.Asset, "Asset"),
        new(AccountType.Liability, "Liability"),
        new(AccountType.Equity, "Equity"),
        new(AccountType.Revenue, "Revenue"),
        new(AccountType.Expense, "Expense")
    ];

    public static List<EnumOption<FinancialStatement>> Statements { get; } =
    [
        new(FinancialStatement.IncomeStatement, "Income Statement"),
        new(FinancialStatement.BalanceSheet, "Balance Sheet")
    ];

    public static List<EnumOption<BalanceSheetClass>> BalanceSheetClasses { get; } =
    [
        new(BalanceSheetClass.CurrentAsset, "Current Asset"),
        new(BalanceSheetClass.NonCurrentAsset, "Non-current Asset"),
        new(BalanceSheetClass.CurrentLiability, "Current Liability"),
        new(BalanceSheetClass.NonCurrentLiability, "Non-current Liability"),
        new(BalanceSheetClass.Equity, "Equity")
    ];

    public static List<EnumOption<IncomeStatementClass>> IncomeStatementClasses { get; } =
    [
        new(IncomeStatementClass.OperatingRevenue, "Operating Revenue"),
        new(IncomeStatementClass.NonOperatingRevenue, "Non-operating Revenue"),
        new(IncomeStatementClass.ContraRevenue, "Contra Revenue"),
        new(IncomeStatementClass.CostOfGoodsSold, "Cost of Goods Sold"),
        new(IncomeStatementClass.OperatingExpense, "Operating Expense"),
        new(IncomeStatementClass.NonOperatingExpense, "Non-operating Expense")
    ];

    public static List<EnumOption<CashFlowActivity>> CashFlowActivities { get; } =
    [
        new(CashFlowActivity.Unclassified, "Unclassified"),
        new(CashFlowActivity.Operating, "Operating"),
        new(CashFlowActivity.Investing, "Investing"),
        new(CashFlowActivity.Financing, "Financing")
    ];
}

/// <summary>Human-readable labels and badge styling for account enums.</summary>
public static class AccountLabels
{
    public static string For(AccountType value) => value.ToString();

    public static string For(FinancialStatement value) => value switch
    {
        FinancialStatement.BalanceSheet => "Balance Sheet",
        FinancialStatement.IncomeStatement => "Income Statement",
        _ => value.ToString()
    };

    public static string For(BalanceSheetClass value) => value switch
    {
        BalanceSheetClass.CurrentAsset => "Current Asset",
        BalanceSheetClass.NonCurrentAsset => "Non-current Asset",
        BalanceSheetClass.CurrentLiability => "Current Liability",
        BalanceSheetClass.NonCurrentLiability => "Non-current Liability",
        BalanceSheetClass.Equity => "Equity",
        _ => value.ToString()
    };

    public static string For(IncomeStatementClass value) => value switch
    {
        IncomeStatementClass.OperatingRevenue => "Operating Revenue",
        IncomeStatementClass.NonOperatingRevenue => "Non-operating Revenue",
        IncomeStatementClass.ContraRevenue => "Contra Revenue",
        IncomeStatementClass.CostOfGoodsSold => "Cost of Goods Sold",
        IncomeStatementClass.OperatingExpense => "Operating Expense",
        IncomeStatementClass.NonOperatingExpense => "Non-operating Expense",
        _ => value.ToString()
    };

    public static string For(CashFlowActivity value) => value switch
    {
        CashFlowActivity.Unclassified => "Unclassified",
        CashFlowActivity.Operating => "Operating",
        CashFlowActivity.Investing => "Investing",
        CashFlowActivity.Financing => "Financing",
        _ => value.ToString()
    };

    /// <summary>CSS class used to colorize the account-type badge in the grid.</summary>
    public static string BadgeCss(AccountType value) => value switch
    {
        AccountType.Asset => "account-badge--asset",
        AccountType.Liability => "account-badge--liability",
        AccountType.Equity => "account-badge--equity",
        AccountType.Revenue => "account-badge--revenue",
        AccountType.Expense => "account-badge--expense",
        _ => "account-badge"
    };
}