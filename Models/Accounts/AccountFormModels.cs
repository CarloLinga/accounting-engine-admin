using System.ComponentModel.DataAnnotations;

namespace AccountingEngineAdmin.Models.Accounts;

// -----------------------------------------------------------------------------
// Editable form models for the account dialogs. Kept separate from the API DTOs
// because DTO records use init-only setters (no two-way binding) and because
// form validation rules belong to the UI, not the wire contract.
// -----------------------------------------------------------------------------

/// <summary>View-model for the "New Account" dialog.</summary>
public sealed class AccountFormModel
{
    [Required(ErrorMessage = "Account code is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Code must be between 1 and 50 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account type is required.")]
    public AccountType? Type { get; set; }

    public FinancialStatement Statement { get; set; } = FinancialStatement.IncomeStatement;

    public BalanceSheetClass? BalanceSheetClass { get; set; }

    public IncomeStatementClass? IncomeStatementClass { get; set; }

    public CashFlowActivity CashFlowActivity { get; set; } = CashFlowActivity.Unclassified;

    public bool IsCashEquivalent { get; set; }

    public bool IsContra { get; set; }

    public bool IsPostable { get; set; } = true;

    public string? ParentAccountCode { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be zero or greater.")]
    public int DisplayOrder { get; set; }

    /// <summary>Used in edit mode to preserve the active flag (init-only on DTOs).</summary>
    public bool IsActive { get; set; } = true;

    public CreateAccountRequest ToRequest() => new(
        Code.Trim(),
        Name.Trim(),
        Type!.Value,
        Statement,
        BalanceSheetClass,
        IncomeStatementClass,
        CashFlowActivity,
        IsCashEquivalent,
        IsContra,
        IsPostable,
        string.IsNullOrWhiteSpace(ParentAccountCode) ? null : ParentAccountCode.Trim(),
        DisplayOrder
    );

    public UpdateAccountRequest ToUpdateRequest() => new(
        Name.Trim(),
        Type!.Value,
        Statement,
        BalanceSheetClass,
        IncomeStatementClass,
        CashFlowActivity,
        IsCashEquivalent,
        IsContra,
        IsPostable,
        string.IsNullOrWhiteSpace(ParentAccountCode) ? null : ParentAccountCode.Trim(),
        DisplayOrder,        
        IsActive
    );
}

/// <summary>View-model for the "Edit Account" dialog (Name + IsActive only).</summary>
public sealed class EditAccountFormModel
{
    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}