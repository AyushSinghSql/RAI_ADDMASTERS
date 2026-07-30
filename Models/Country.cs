namespace PlanningAPI.Models;

public sealed class Country
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "SYSTEM";
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
