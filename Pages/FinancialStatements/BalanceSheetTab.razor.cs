using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Balance sheet tab: asset, liability and equity sections, each rendered as a
/// heading with its subtotal followed by a grid of the accounts that make up the
/// section, plus the assets = liabilities + equity check.
/// </summary>
public partial class BalanceSheetTab : ComponentBase
{
    [Parameter] public BalanceSheetResponse? BalanceSheet { get; set; }

    /// <summary>Hide sections whose accounts all have a zero balance (empty grid).</summary>
    private static bool ShowSection(StatementSection section) => StatementRendering.HasRows(section);
}
