using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// One titled section of the cash flow statement: adjustments in parent order
/// with the section subtotal.
/// </summary>
public partial class CashFlowSectionBlock : ComponentBase
{
    [Parameter] public CashFlowSection Section { get; set; } = default!;

    private List<(CashFlowAdjustment Adjustment, int Indent)> rows => CashFlowRendering.CashFlowRows(Section);
}