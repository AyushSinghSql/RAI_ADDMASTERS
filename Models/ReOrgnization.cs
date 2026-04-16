using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("reorganization")]
    public class Reorganization
    {
        [Key]
        [Column("reorg_id")]
        [MaxLength(30)]
        public string ReorgId { get; set; }

        [Required]
        [Column("reorg_name")]
        [MaxLength(200)]
        public string? ReorgName { get; set; }

        [Column("reorg_top_fl")]
        [MaxLength(1)]
        public string? ReorgTopFl { get; set; }

        [Column("lvl_no")]
        public int? LvlNo { get; set; }

        [Column("reorg_lvls_no")]
        public int? ReorgLvlsNo { get; set; }

        [Column("l1_reorg_name")] public string? L1ReorgName { get; set; }
        [Column("l2_reorg_name")] public string? L2ReorgName { get; set; }
        [Column("l3_reorg_name")] public string? L3ReorgName { get; set; }
        [Column("l4_reorg_name")] public string? L4ReorgName { get; set; }
        [Column("l5_reorg_name")] public string? L5ReorgName { get; set; }
        [Column("l6_reorg_name")] public string? L6ReorgName { get; set; }
        [Column("l7_reorg_name")] public string? L7ReorgName { get; set; }
        [Column("l8_reorg_name")] public string? L8ReorgName { get; set; }
        [Column("l9_reorg_name")] public string? L9ReorgName { get; set; }

        [Column("l1_reorg_seg_id")] public string? L1ReorgSegId { get; set; }
        [Column("l2_reorg_seg_id")] public string? L2ReorgSegId { get; set; }
        [Column("l3_reorg_seg_id")] public string? L3ReorgSegId { get; set; }
        [Column("l4_reorg_seg_id")] public string? L4ReorgSegId { get; set; }
        [Column("l5_reorg_seg_id")] public string? L5ReorgSegId { get; set; }
        [Column("l6_reorg_seg_id")] public string? L6ReorgSegId { get; set; }
        [Column("l7_reorg_seg_id")] public string? L7ReorgSegId { get; set; }
        [Column("l8_reorg_seg_id")] public string? L8ReorgSegId { get; set; }
        [Column("l9_reorg_seg_id")] public string? L9ReorgSegId { get; set; }
        [Column("l10_reorg_seg_id")] public string? L10ReorgSegId { get; set; }

        [Column("modified_by")]
        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(30)]
        public string? CompanyId { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        // Navigation
        public ICollection<ReorganizationLevel>? Levels { get; set; }
        public ICollection<ReorganizationOrgMap>? OrgMaps { get; set; }
    }

    [Table("reorganization_levels")]
    public class ReorganizationLevel
    {
        [Key]
        [Column("reorg_lvl_key")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReorgLvlKey { get; set; }

        [Required]
        [Column("reorg_id_top")]
        [MaxLength(30)]
        public string ReorgIdTop { get; set; }

        [Required]
        [Column("lvl_no")]
        public int LvlNo { get; set; }

        [Column("id_seg_len_no")]
        public int? IdSegLenNo { get; set; }

        [Column("reorg_lvl_desc")]
        [MaxLength(200)]
        public string ReorgLvlDesc { get; set; }

        [Column("modified_by")]
        [MaxLength(50)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(30)]
        public string CompanyId { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        // Navigation
        [ForeignKey("ReorgIdTop")]
        public Reorganization Reorganization { get; set; }
    }

    [Table("reorganization_org_map")]
    public class ReorganizationOrgMap
    {
        [Column("reorg_id")]
        [MaxLength(30)]
        public string ReorgId { get; set; }

        [Column("org_id")]
        [MaxLength(30)]
        public string OrgId { get; set; }

        [Column("company_id")]
        [MaxLength(30)]
        public string CompanyId { get; set; }

        [Column("modified_by")]
        [MaxLength(50)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("row_version")]
        public long RowVersion { get; set; }

        // Navigation
        [ForeignKey("ReorgId")]
        public Reorganization Reorganization { get; set; }
    }

    public class ReorganizationDto
    {
        public string ReorgId { get; set; }
        public string ReorgName { get; set; }
        public string ReorgTopFl { get; set; }
        public int? LvlNo { get; set; }
        public int? ReorgLvlsNo { get; set; }
        public string CompanyId { get; set; }
    }

    public class CreateReorganizationDto
    {
        public string ReorgId { get; set; }
        public string ReorgName { get; set; }
        public string CompanyId { get; set; }
        public string ReorgTopFl { get; set; }
        public int? LvlNo { get; set; }


    }

    public class ReorganizationLevelDto
    {
        public long ReorgLvlKey { get; set; }
        public string ReorgIdTop { get; set; }
        public int LvlNo { get; set; }
        public int? IdSegLenNo { get; set; }
        public string ReorgLvlDesc { get; set; }
        public string CompanyId { get; set; }
    }

    public class CreateReorganizationLevelDto
    {
        public string ReorgIdTop { get; set; }
        public int LvlNo { get; set; }
        public int? IdSegLenNo { get; set; }
        public string ReorgLvlDesc { get; set; }
        public string CompanyId { get; set; }
    }

    public class ReorganizationOrgMapDto
    {
        public string ReorgId { get; set; }
        public string OrgId { get; set; }
        public string CompanyId { get; set; }
    }
}
