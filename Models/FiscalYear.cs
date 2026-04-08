using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PlanningAPI.Models
{
    [Table("fiscal_year", Schema = "public")]
    public class FiscalYear
    {
        [Key]
        [Column("fy_cd")]
        [MaxLength(6)]
        public string FyCd { get; set; } = null!;

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("status_cd")]
        [MaxLength(1)]
        [Required]
        public string StatusCd { get; set; } = null!;

        [Column("fy_desc")]
        [MaxLength(30)]
        [Required]
        public string FyDesc { get; set; } = null!;

        [Column("modified_by")]
        [MaxLength(20)]
        [Required]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("close_act_tgt_cd")]
        [MaxLength(1)]
        public string? CloseActTgtCd { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }
    }


    [Table("accounting_period", Schema = "public")]
    public class AccountingPeriod
    {
        [Column("fy_cd")]
        [MaxLength(6)]
        public string FyCd { get; set; } = null!;

        [Column("period_no")]
        public int PeriodNo { get; set; }

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("status_cd")]
        [MaxLength(1)]
        [Required]
        public string StatusCd { get; set; } = null!;

        [Column("period_end_date")]
        [Required]
        public DateOnly PeriodEndDate { get; set; }

        [Column("modified_by")]
        [MaxLength(20)]
        [Required]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("is_adjustment")]
        [MaxLength(1)]
        [Required]
        public string IsAdjustment { get; set; } = null!;

        [Column("adjustment_code")]
        [MaxLength(1)]
        [Required]
        public string AdjustmentCode { get; set; } = null!;

        [Column("adjustment_end_date")]
        public DateOnly? AdjustmentEndDate { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }

        // 🔗 Navigation
        public FiscalYear? FiscalYear { get; set; }
    }


    [Table("sub_period", Schema = "public")]
    public class SubPeriod
    {
        [Column("fy_cd")]
        [MaxLength(6)]
        public string FyCd { get; set; } = null!;

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("period_no")]
        public int PeriodNo { get; set; }

        [Column("sub_period_no")]
        public int SubPeriodNo { get; set; }

        [Column("sub_period_end_date")]
        public DateOnly SubPeriodEndDate { get; set; }

        [Column("status_cd")]
        [MaxLength(1)]
        public string StatusCd { get; set; } = null!;

        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }

        [Column("is_adjustment")]
        [MaxLength(1)]
        public string IsAdjustment { get; set; } = null!;

        [Column("adjustment_code")]
        [MaxLength(1)]
        public string AdjustmentCode { get; set; } = null!;

        [Column("adjustment_end_date")]
        public DateTime? AdjustmentEndDate { get; set; }

        // 🔗 Navigation
        public AccountingPeriod? AccountingPeriod { get; set; }

    }

    [Table("sub_period_journal_status", Schema = "public")]
    public class SubPeriodJournalStatus
    {
        [Column("journal_code")]
        [MaxLength(3)]
        public string JournalCode { get; set; } = null!;

        [Column("fy_cd")]
        [MaxLength(6)]
        public string FyCd { get; set; } = null!;

        [Column("period_no")]
        public int PeriodNo { get; set; }

        [Column("sub_period_no")]
        public int SubPeriodNo { get; set; }

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("is_open")]
        [MaxLength(1)] // Y/N
        public string IsOpen { get; set; } = null!;

        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }
        [NotMapped]
        public string? JournalDesc { get; set; } = null!;
        // 🔗 Navigation
        public SubPeriod? SubPeriod { get; set; }

        [ForeignKey(nameof(JournalCode))]
        public virtual JournalCode? JournalCodeRef { get; set; }
    }
    public class FiscalYearDto
    {
        public string FyCd { get; set; } = null!;
        public string StatusCd { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string FyDesc { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
        public string? CloseActTgtCd { get; set; }
    }
    public class AccountingPeriodDto
    {
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public string StatusCd { get; set; } = null!;
        public DateOnly PeriodEndDate { get; set; }
        public string ModifiedBy { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string IsAdjustment { get; set; } = null!;
        public string AdjustmentCode { get; set; } = null!;
        public DateOnly? AdjustmentEndDate { get; set; }
    }

    public class SubPeriodDto
    {
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public int SubPeriodNo { get; set; }
        public DateOnly SubPeriodEndDate { get; set; }
        public string StatusCd { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
        public string IsAdjustment { get; set; } = null!;
        public string AdjustmentCode { get; set; } = null!;
        public DateTime? AdjustmentEndDate { get; set; }
    }
    public class SubPeriodJournalStatusDto
    {
        public string JournalCode { get; set; } = null!;
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public int SubPeriodNo { get; set; }
        public string IsOpen { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
        public string JournalDesc { get; set; } = null!;

    }
    public class SubPeriodJournalStatusBulkDto
    {
        public string JournalCode { get; set; } = null!;
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public int SubPeriodNo { get; set; }
        public string IsOpen { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
    }

    public class YearEndCloseDto
    {
        public string FyCd { get; set; } = null!;
        public string NextFyCd { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
    }

    public class JournalControlDto
    {
        public string FyCd { get; set; } = null!;
        public int? PeriodNo { get; set; }
        public int? SubPeriodNo { get; set; }
        public string? JournalCode { get; set; }

        public string Status { get; set; } = null!; // Y/N
        public string CompanyId { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
    }

    public class PostingValidationDto
    {
        public string FyCd { get; set; } = null!;
        public int PeriodNo { get; set; }
        public int? SubPeriodNo { get; set; }
        public string JournalCode { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
    }

    public class FiscalYearCreateDto
    {
        public string FyCd { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string StatusCd { get; set; } = "O";
        public string FyDesc { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
        public string? CloseActTgtCd { get; set; }

        // 🔥 New
        public int TotalPeriods { get; set; } = 12;
        public int SubPeriodsPerPeriod { get; set; } = 1;

        public DateOnly StartDate { get; set; }
    }
}
