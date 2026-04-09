namespace PlanningAPI.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    [Table("acct_type")]
    public class AcctType
    {
        [Column("acct_type_cd", Order = 0)]
        [MaxLength(10)]
        public string AcctTypeCode { get; set; }

        [Column("company_id", Order = 1)]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("acct_type_desc")]
        [MaxLength(50)]
        [Required]
        public string AcctTypeDescription { get; set; }

        [Column("active_fl")]
        public bool ActiveFlag { get; set; } = true;

        [Column("modified_by")]
        [MaxLength(20)]
        [Required]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        //[JsonIgnore]
        //public virtual ICollection<AccountGroupSetup>? AccountGroupSetups { get; set; }

    }

    public class AcctTypeDto
    {
        //public string AcctTypeCode { get; set; }
        public string CompanyId { get; set; }
        public string AcctTypeDescription { get; set; }
        public bool ActiveFlag { get; set; }
    }

    //public static class AcctTypeExtensions
    //{
    //    public static AcctType ToEntity(this AcctTypeDto dto, string modifiedBy)
    //    {
    //        return new AcctType
    //        {
    //            AcctTypeCode = dto.AcctTypeCode,
    //            CompanyId = dto.CompanyId,
    //            AcctTypeDescription = dto.AcctTypeDescription,
    //            ActiveFlag = dto.ActiveFlag,
    //            ModifiedBy = modifiedBy,
    //            TimeStamp = DateTime.UtcNow
    //        };
    //    }

    //    public static AcctTypeDto ToDto(this AcctType entity)
    //    {
    //        return new AcctTypeDto
    //        {
    //            AcctTypeCode = entity.AcctTypeCode,
    //            CompanyId = entity.CompanyId,
    //            AcctTypeDescription = entity.AcctTypeDescription,
    //            ActiveFlag = entity.ActiveFlag
    //        };
    //    }
    //}
}
