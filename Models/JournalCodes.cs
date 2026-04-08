namespace PlanningAPI.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("journal_codes", Schema = "public")]
    public class JournalCode
    {
        [Key]
        [Column("journal_code")]
        [MaxLength(3)]
        public string? JournalCodeId { get; set; } = null!;

        [Column("journal_desc")]
        [MaxLength(50)]
        public string? JournalDesc { get; set; } = null!;

        [Column("is_active")]
        [MaxLength(1)]
        public string? IsActive { get; set; } = "Y";

        [Column("modified_by")]
        [MaxLength(20)]
        public string? ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }

        public virtual ICollection<JournalStatus>? JournalStatuses { get; set; } = new List<JournalStatus>();
        public virtual ICollection<SubPeriodJournalStatus>? SubperiodJournalStatuses { get; set; } = new List<SubPeriodJournalStatus>();

    }

    [Table("journal_status", Schema = "public")]
    public class JournalStatus
    {
        [Column("journal_code")]
        public string JournalCode { get; set; } = null!;

        [Column("fy_cd")]
        public string FyCd { get; set; } = null!;

        [Column("period_no")]
        public int PeriodNo { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; } = null!;

        [Column("is_open")]
        public string IsOpen { get; set; } = null!;

        [Column("modified_by")]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }
        [NotMapped]
        public string? JournalDesc { get; set; } = null!;

        // 🔥 Navigation to JournalCode
        [ForeignKey(nameof(JournalCode))]
        public virtual JournalCode? JournalCodeRef { get; set; }
    }

    public class JournalCodeDto
    {
        public string JournalCode { get; set; } = null!;
        public string JournalDesc { get; set; } = null!;
        public string IsActive { get; set; } = "Y";
        public string ModifiedBy { get; set; } = null!;
    }

    public class JournalStatusDto
    {
        public string JournalCode { get; set; } = null!;
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public string CompanyId { get; set; } = null!;
        public string IsOpen { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
        public string JournalDesc { get; set; } = null!;

    }
}
