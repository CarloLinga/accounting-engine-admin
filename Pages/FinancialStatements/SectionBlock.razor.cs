using System.Collections.Generic;
using System.Linq;
using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// One titled section of a financial statement (e.g. "Current Assets"): the
/// section heading with its subtotal, followed by a data grid of the accounts
/// that make up the section.
/// </summary>
public partial class SectionBlock : ComponentBase
{
    [Parameter] public StatementSection Section { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private List<SectionRow> rows = new();

    /// <summary>
    /// Accounts with a balance only: a zero line carries no information on a
    /// statement, so it is left out of the grid. Computed once per render pass
    /// so the grid keeps a stable Data reference.
    /// </summary>
    protected override void OnParametersSet()
    {
        rows = StatementRendering.SectionRows(Section)
            .Where(r => r.Line.Amount != 0)
            .ToList();
    }

    private void OpenLedger(string code) =>
        Navigation.NavigateTo($"/ledger?account={Uri.EscapeDataString(code)}");

    /// <summary>
    /// Classes for the account cell: the code/name pair, the hierarchy indent and
    /// the row modifiers (see .fs-account-cell / .fs-indent-* in app.css).
    /// </summary>
    private static string AccountCellCss(SectionRow row)
    {
        var classes = new List<string> { "fs-account-cell", "fs-grid-link", $"fs-indent-{row.Indent}" };

        var modifier = AmountCss(row.Line);
        if (modifier.Length > 0)
        {
            classes.Add(modifier);
        }

        return string.Join(' ', classes);
    }

    /// <summary>
    /// Modifier classes for a statement row. Names match the styles declared in
    /// wwwroot/css/app.css (.is-header / .is-contra / .is-negative).
    /// </summary>
    private static string AmountCss(StatementLine line) =>
        line.IsHeader ? "is-header" : line.IsContra ? "is-contra" : line.Amount < 0 ? "is-negative" : string.Empty;
}
