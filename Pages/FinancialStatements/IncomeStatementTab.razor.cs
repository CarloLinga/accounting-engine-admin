using AccountingEngineAdmin.Models.Reports;
using AccountingEngineAdmin.Services;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>Income statement tab: revenue and expense sections with margin figures.</summary>
public partial class IncomeStatementTab : ComponentBase
{
    [Parameter] public IncomeStatementResponse? IncomeStatement { get; set; }

    private bool ShowSection(StatementSection section) => section.Lines.Count > 0;
}
