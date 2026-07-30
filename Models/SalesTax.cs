using System;
using System.Collections.Generic;

namespace PlanningAPI.Models;

public sealed class SalesTax
{
    public string CompanyId { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string CertificateNo { get; set; } = string.Empty;
    public bool Exempt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = "SYSTEM";
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public decimal CompositeTaxRate { get; set; }
    public decimal RecoveryPercent { get; set; }
    public decimal RecoveryPercentOverride { get; set; }
    public bool RequiresVatInfo { get; set; }

    public List<SalesTaxAccount> Accounts { get; set; } = new();
}
