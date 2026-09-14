using System.Collections.Generic;
using System.Linq;
using AccountingEngineAdmin.Models.Reports;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Shared rendering logic for the cash-flow tabs: walks section adjustments in
/// parent order (headers first, children indented).
/// </summary>
public static class CashFlowRendering
{
    public static List<(CashFlowAdjustment Adjustment, int Indent)> CashFlowRows(CashFlowSection section)
    {
        var rows = new List<(CashFlowAdjustment, int)>();
        var byParent = section.Adjustments.ToLookup(a => a.AccountCode);
        foreach (var root in section.Adjustments.Where(a => string.IsNullOrEmpty(a.AccountCode)))
        {
            Walk(byParent, root, 0, rows);
        }

        return rows;
    }

    private static void Walk(
        ILookup<string?, CashFlowAdjustment> byParent,
        CashFlowAdjustment adjustment,
        int indent,
        List<(CashFlowAdjustment Adjustment, int Indent)> rows)
    {
        rows.Add((adjustment, indent));
        foreach (var child in byParent[adjustment.AccountCode])
        {
            Walk(byParent, child, Math.Min(indent + 1, 3), rows);
        }
    }

    public static string AmountCss(CashFlowAdjustment adjustment) =>
        adjustment.Amount < 0 ? "stk-row--negative" : string.Empty;
}