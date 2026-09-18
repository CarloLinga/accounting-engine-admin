using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Cash flow tab: indirect method sections (operating, investing, financing)
/// with the net-income bridge and the period totals. Each section renders its
/// own grid through <see cref="CashFlowSectionBlock"/>.
/// </summary>
public partial class CashFlowTab : ComponentBase
{
    [Parameter] public CashFlowResponse? CashFlow { get; set; }

    /// <summary>Hide sections whose adjustments all have a zero amount (empty grid).</summary>
    private static bool ShowSection(CashFlowSection section) => CashFlowRendering.HasRows(section);
}
