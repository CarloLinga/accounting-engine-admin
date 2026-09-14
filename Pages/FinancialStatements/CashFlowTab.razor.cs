using AccountingEngineAdmin.Models.Reports;
using AccountingEngineAdmin.Services;
using Microsoft.AspNetCore.Components;

namespace AccountingEngineAdmin.Pages.FinancialStatements;

/// <summary>Cash flow tab: indirect method sections (operating, investing, financing).</summary>
public partial class CashFlowTab : ComponentBase
{
    [Parameter] public CashFlowResponse? CashFlow { get; set; }
}
