namespace PlanningAPI.Models;

public sealed class State
{
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "SYSTEM";
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
