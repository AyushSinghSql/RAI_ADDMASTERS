namespace PlanningAPI.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("modules", Schema = "public")]
    public class Module
    {
        [Key]
        [Column("module_cd", Order = 0)]
        [MaxLength(6)]
        public string ModuleCd { get; set; }

        [Key]
        [Column("company_id", Order = 1)]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("name")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Column("domain")]
        [MaxLength(50)]
        public string Domain { get; set; }

        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public int? Rowversion { get; set; }

        public ICollection<OrgSecGrpSetup> OrgSecGrpSetups { get; set; }

    }

[Table("module_rights", Schema = "public")]
    public class ModuleRights
    {
        [Column("sec_obj_id")]
        [MaxLength(20)]
        public string UserGroupId { get; set; } = null!;

        [Column("module_id")]
        [MaxLength(2)]
        public string ModuleId { get; set; } = null!;

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("access_fl")]
        [MaxLength(1)]
        [Required]
        public string AccessFl { get; set; } = null!;

        [Column("modified_by")]
        [MaxLength(20)]
        [Required]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }

        [Column("s_rights_status_cd")]
        [MaxLength(1)]
        public string? SRightsStatusCd { get; set; }
    }
    public class ModuleDto
    {
        public string ModuleCd { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
    }

    public class ModuleRightsDto
    {
        public string UserGroupId { get; set; } = null!;
        public string ModuleCD { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string AccessFl { get; set; } = null!;
        public string ModifiedBy { get; set; } = null!;
        public decimal? Rowversion { get; set; }
        public string? SRightsStatusCd { get; set; }
    }
}
