using System.Collections.Generic;
using AccountingEngineAdmin.Models.Reports;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>
/// One titled section of the cash flow statement (operating, investing or
/// financing): the section heading with its subtotal, followed by a data grid of
/// the adjustments that make up the section.
/// </summary>
public partial class CashFlowSectionBlock : ComponentBase
{
    [Parameter] public CashFlowSection Section { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private List<CashFlowRow> rows = new();

    /// <summary>
    /// Adjustments with an amount only; computed once per render pass so the grid
    /// keeps a stable Data reference.
    /// </summary>
    protected override void OnParametersSet() => rows = CashFlowRendering.CashFlowRows(Section);

    /// <summary>
    /// Only adjustments tied to an account can be opened in the ledger, so the
    /// link affordance is added for those rows alone.
    /// </summary>
    private void OpenLedger(string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        Navigation.NavigateTo($"/ledger?account={Uri.EscapeDataString(code)}");
    }

    /// <summary>Classes for the item cell: label, optional account code and indent.</summary>
    private static string ItemCellCss(CashFlowRow row)
    {
        var classes = new List<string> { "fs-account-cell", $"fs-indent-{row.Indent}" };

        if (!string.IsNullOrEmpty(row.Adjustment.AccountCode))
        {
            classes.Add("fs-grid-link");
        }

        var modifier = CashFlowRendering.AmountCss(row.Adjustment);
        if (modifier.Length > 0)
        {
            classes.Add(modifier);
        }

        return string.Join(' ', classes);
    }
}