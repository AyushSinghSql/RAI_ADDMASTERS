using System;

namespace PlanningAPI.Models;

public sealed class SalesTaxAccount
{
    public string CompanyId { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public int AccountKey { get; set; }
    public string Account { get; set; } = string.Empty;
    public string AccountDesc { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string OrgDesc { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string TaxType { get; set; } = "SALES/USE";
    public string RefNo1 { get; set; } = string.Empty;
    public string RefNo2 { get; set; } = string.Empty;
    public decimal EffectiveTaxRate { get; set; }
    public string Recoverable { get; set; } = "N";
    public bool CompoundTax { get; set; }
    public decimal AcctRecovPct { get; set; }
    public string RecAccount { get; set; } = string.Empty;
    public string RecOrg { get; set; } = string.Empty;
    public string RecRefNo1 { get; set; } = string.Empty;
    public string RecRefNo2 { get; set; } = string.Empty;
    public string SuspenseAccount { get; set; } = string.Empty;
    public string SuspenseOrg { get; set; } = string.Empty;
    public string SuspenseRefNo1 { get; set; } = string.Empty;
    public string SuspenseRefNo2 { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "SYSTEM";
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
