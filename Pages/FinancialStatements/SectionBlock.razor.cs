using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// One titled section of a financial statement: lines in parent order with
/// contra accounts highlighted, then the section subtotal.
/// </summary>
public partial class SectionBlock : ComponentBase
{
    [Parameter] public StatementSection Section { get; set; } = default!;

    private List<SectionRow> rows => StatementRendering.SectionRows(Section);

    private static string AmountCss(StatementLine line) =>
        line.IsHeader ? "stk-row--header" : line.IsContra ? "stk-row--contra" : line.Amount < 0 ? "stk-row--negative" : string.Empty;
}
