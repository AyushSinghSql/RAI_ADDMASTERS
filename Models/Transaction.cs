namespace PlanningAPI.Models
{
    public class TransactionValidationDto
    {
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public int SubPeriodNo { get; set; }
        public string JournalCode { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
    }
}
