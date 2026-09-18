using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Income statement tab: revenue and expense sections, each rendered as a heading
/// with its subtotal followed by a grid of the accounts that make up the section,
/// plus the margin figures and the net-income footer.
/// </summary>
public partial class IncomeStatementTab : ComponentBase
{
    [Parameter] public IncomeStatementResponse? IncomeStatement { get; set; }

    /// <summary>Hide sections whose accounts all have a zero balance (empty grid).</summary>
    private static bool ShowSection(StatementSection section) => StatementRendering.HasRows(section);
}
