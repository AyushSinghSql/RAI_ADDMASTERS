namespace PlanningAPI.Models
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("company")]
    public class Company
    {
        [Key]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("company_name")]
        [MaxLength(100)]
        [Required]
        public string CompanyName { get; set; }

        [Column("company_short_name")]
        [MaxLength(50)]
        public string? CompanyShortName { get; set; }

        [Column("active_fl")]
        public bool ActiveFlag { get; set; } = true;

        [Column("modified_by")]
        [MaxLength(20)]
        [Required]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public TaxableEntity TaxableEntity { get; set; }
        public ICollection<AcctType>? AcctTypes { get; set; }
        public ICollection<AcctGrpCd>? AcctGrps { get; set; }
    }

    public class CompanyDto
    {
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string? CompanyShortName { get; set; }
        public bool ActiveFlag { get; set; }
        public TaxableEntity TaxableEntity { get; set; }

    }
    public static class CompanyExtensions
    {
        public static Company ToEntity(this CompanyDto dto, string modifiedBy)
        {
            return new Company
            {
                CompanyId = dto.CompanyId,
                CompanyName = dto.CompanyName,
                CompanyShortName = dto.CompanyShortName,
                ActiveFlag = dto.ActiveFlag,
                ModifiedBy = modifiedBy,
                TimeStamp = DateTime.UtcNow,
                TaxableEntity = dto.TaxableEntity
            };
        }

        public static CompanyDto ToDto(this Company entity)
        {
            return new CompanyDto
            {
                CompanyId = entity.CompanyId,
                CompanyName = entity.CompanyName,
                CompanyShortName = entity.CompanyShortName,
                ActiveFlag = entity.ActiveFlag,
                TaxableEntity = entity.TaxableEntity
            };
        }
    }
}
