using AccountingEngineAdmin.Models.Reports;
using AccountingEngineAdmin.Services;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Balance sheet tab: asset, liability and equity sections with subtotals and
/// the assets = liabilities + equity check.
/// </summary>
public partial class BalanceSheetTab : ComponentBase
{
    [Parameter] public BalanceSheetResponse? BalanceSheet { get; set; }

    private string SectionKey(StatementSection section) => section.Key;

    private bool ShowSection(StatementSection section) => section.Lines.Count > 0;
}
