using System.Linq;
using AccountingEngineAdmin.Models.Reports;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// Shared rendering logic for the financial-statement tabs: walks section
/// lines in parent order (headers first, children indented).
/// </summary>
public static class StatementRendering
{
    public static List<SectionRow> SectionRows(StatementSection section)
    {
        var rows = new List<SectionRow>();
        var byParent = section.Lines.ToLookup(l => l.ParentAccountCode);
        var accountCodes = section.Lines
            .Select(l => l.AccountCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var root in section.Lines.Where(l =>
            string.IsNullOrEmpty(l.ParentAccountCode) ||
            !accountCodes.Contains(l.ParentAccountCode)))
        {
            Walk(byParent, root, 0, rows);
        }

        return rows;
    }

    private static void Walk(
        ILookup<string?, StatementLine> byParent,
        StatementLine line,
        int indent,
        List<SectionRow> rows)
    {
        rows.Add(new SectionRow(line, indent));
        foreach (var child in byParent[line.AccountCode])
        {
            Walk(byParent, child, Math.Min(indent + 1, 3), rows);
        }
    }
}

/// <summary>One row of a statement section for display: a line plus its indent level.</summary>
public sealed record SectionRow(StatementLine Line, int Indent);
