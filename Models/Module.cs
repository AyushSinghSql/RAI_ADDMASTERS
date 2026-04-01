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

    public class ModuleDto
    {
        public string ModuleCd { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}
