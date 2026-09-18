using System.Collections.Generic;
using System.Linq;
using AccountingEngineAdmin.Models.Reports;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Shared rendering logic for the cash-flow sections. CashFlowAdjustment has no
/// parent link, so the adjustments are shown in the order the API returned them
/// (the previous parent-walk grouped a row with itself and could recurse
/// forever, or drop every row when no adjustment had an empty account code).
/// </summary>
public static class CashFlowRendering
{
    /// <summary>
    /// Rows of one cash-flow section in display order, keeping only the
    /// adjustments that carry an amount.
    /// </summary>
    public static List<CashFlowRow> CashFlowRows(CashFlowSection section) =>
        section.Adjustments
            .Where(a => a.Amount != 0)
            .Select(a => new CashFlowRow(a, 0))
            .ToList();

    /// <summary>True when the section has at least one adjustment with an amount.</summary>
    public static bool HasRows(CashFlowSection section) =>
        section.Adjustments.Any(a => a.Amount != 0);

    public static string AmountCss(CashFlowAdjustment adjustment) =>
        adjustment.Amount < 0 ? "is-negative" : string.Empty;
}

/// <summary>One row of a cash-flow section for display: an adjustment plus its indent level.</summary>
public sealed record CashFlowRow(CashFlowAdjustment Adjustment, int Indent);