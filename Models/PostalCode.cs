namespace PlanningAPI.Models;

public sealed class PostalCode
{
    public int? PostalKey { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string PostalCd { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "SYSTEM";
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
