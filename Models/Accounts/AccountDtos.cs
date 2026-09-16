namespace AccountingEngineAdmin.Models.Accounts;

// -----------------------------------------------------------------------------
// Client-side mirrors of AccountingEngine.Application.DTOs (Accounts).
// JSON contract: camelCase properties, enums as strings. Keep in sync with the
// backend; the API identifies accounts by their Code, not by Id.
// -----------------------------------------------------------------------------

/// <summary>Account as returned by the API.</summary>
public record AccountResponse(
    Guid Id,
    string Code,
    string Name,
    AccountType Type,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    FinancialStatement Statement,
    BalanceSheetClass? BalanceSheetClass,
    IncomeStatementClass? IncomeStatementClass,
    CashFlowActivity CashFlowActivity,
    bool IsCashEquivalent,
    bool IsContra,
    bool IsPostable,
    string? ParentAccountCode,
    int DisplayOrder);

public enum AccountDialogOutcome
{
    Saved,
    Deleted
}

public sealed record AccountDialogResult(AccountDialogOutcome Outcome, string Code, AccountResponse? Account = null);

/// <summary>Payload for POST /api/accounts. Code and Type are immutable after creation.</summary>
public record CreateAccountRequest(
    string Code,
    string Name,
    AccountType Type,
    FinancialStatement Statement,
    BalanceSheetClass? BalanceSheetClass,
    IncomeStatementClass? IncomeStatementClass,
    CashFlowActivity CashFlowActivity,
    bool IsCashEquivalent,
    bool IsContra,
    bool IsPostable,
    string? ParentAccountCode,
    int DisplayOrder);

/// <summary>
/// Payload for PUT /api/accounts/{code}. The backend only accepts Name and
/// IsActive changes - the code, type and financial-statement classification of
/// an account are permanent once created (correct behavior for accounting).
/// </summary>
public record UpdateAccountRequest(
    string Name, 
    AccountType Type,
    FinancialStatement Statement,
    BalanceSheetClass? BalanceSheetClass,
    IncomeStatementClass? IncomeStatementClass,
    CashFlowActivity CashFlowActivity,
    bool IsCashEquivalent,
    bool IsContra,
    bool IsPostable,
    string? ParentAccountCode,
    int DisplayOrder,
    bool IsActive
);