using System.ComponentModel.DataAnnotations;
using PlanningAPI.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("organization")]
public class Organization
{
    [Key]
    [Column("org_id")]
    [MaxLength(30)]
    public string OrgId { get; set; }

    [Required]
    [Column("org_name")]
    [MaxLength(100)]
    public string OrgName { get; set; }

    [Column("lvl_no")]
    public int LvlNo { get; set; }

    // Existing hierarchy names
    [Column("l1_org_name")] public string? L1OrgName { get; set; }
    [Column("l2_org_name")] public string? L2OrgName { get; set; }
    [Column("l3_org_name")] public string? L3OrgName { get; set; }
    [Column("l4_org_name")] public string? L4OrgName { get; set; }
    [Column("l5_org_name")] public string? L5OrgName { get; set; }
    [Column("l6_org_name")] public string? L6OrgName { get; set; }
    [Column("l7_org_name")] public string? L7OrgName { get; set; }
    [Column("l8_org_name")] public string? L8OrgName { get; set; }
    [Column("l9_org_name")] public string? L9OrgName { get; set; }

    // ✅ New columns
    [Column("org_top_fl")] public string? OrgTopFl { get; set; }
    [Column("taxble_entity_id")] public string? TaxbleEntityId { get; set; }
    [Column("active_fl")] public string? ActiveFl { get; set; }
    [Column("fy_cd_fr")] public string? FyCdFr { get; set; }
    [Column("pd_no_fr")] public int? PdNoFr { get; set; }
    [Column("fy_cd_to")] public string? FyCdTo { get; set; }
    [Column("pd_no_to")] public int? PdNoTo { get; set; }
    [Column("ic_trckng_lvl_no")] public int? IcTrckngLvlNo { get; set; }
    [Column("org_lvls_no")] public int? OrgLvlsNo { get; set; }
    [Column("org_abbrv_cd")] public string? OrgAbbrvCd { get; set; }
    [Column("modified_by")] public string? ModifiedBy { get; set; }
    [Column("time_stamp")] public DateTime? TimeStamp { get; set; }
    [Column("company_id")] public string? CompanyId { get; set; }

    // Segment IDs
    [Column("l1_org_seg_id")] public string? L1OrgSegId { get; set; }
    [Column("l2_org_seg_id")] public string? L2OrgSegId { get; set; }
    [Column("l3_org_seg_id")] public string? L3OrgSegId { get; set; }
    [Column("l4_org_seg_id")] public string? L4OrgSegId { get; set; }
    [Column("l5_org_seg_id")] public string? L5OrgSegId { get; set; }
    [Column("l6_org_seg_id")] public string? L6OrgSegId { get; set; }
    [Column("l7_org_seg_id")] public string? L7OrgSegId { get; set; }
    [Column("l8_org_seg_id")] public string? L8OrgSegId { get; set; }
    [Column("l9_org_seg_id")] public string? L9OrgSegId { get; set; }
    [Column("l10_org_seg_id")] public string? L10OrgSegId { get; set; }

    // ICR fields
    [Column("icr_acct_id_fr")] public string? IcrAcctIdFr { get; set; }
    [Column("icr_ref1_id_fr")] public string? IcrRef1IdFr { get; set; }
    [Column("icr_ref2_id_fr")] public string? IcrRef2IdFr { get; set; }
    [Column("icr_acct_id_to")] public string? IcrAcctIdTo { get; set; }
    [Column("icr_ref1_id_to")] public string? IcrRef1IdTo { get; set; }
    [Column("icr_ref2_id_to")] public string? IcrRef2IdTo { get; set; }

    [Column("ec_app_proc_cd")] public string? EcAppProcCd { get; set; }
    [Column("rowversion")] public long? Rowversion { get; set; }
    [Column("tc_org_fl")] public string? TcOrgFl { get; set; }
    [Column("cobra_org")] public string? CobraOrg { get; set; }
    [Column("sft_fl")] public string? SftFl { get; set; }
    [Column("mes_fl")] public string? MesFl { get; set; }
    [Column("tm_org_fl")] public string? TmOrgFl { get; set; }
    public virtual ICollection<PlEmployee>? PlEmployees { get; set; } = new List<PlEmployee>();

    public virtual ICollection<PlProject>? PlProjects { get; set; } = new List<PlProject>();

    public ICollection<OrgGroupOrgMapping>? OrgGroupMappings { get; set; }
    = new List<OrgGroupOrgMapping>();
}

public class OrganizationDto
{
    public string OrgId { get; set; }
    public string OrgName { get; set; }
    public int LvlNo { get; set; }
}
[Table("org_groups")]
public class OrgGroup
{
    [Key]
    [Column("org_group_id")]
    public int OrgGroupId { get; set; }

    [Column("org_group_code")]
    [MaxLength(50)]
    public string? OrgGroupCode { get; set; }

    [Column("org_group_name")]
    [MaxLength(150)]
    public string? OrgGroupName { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
    [Column("company_id")]
    public string? CompanyId { get; set; }


    // Navigation Properties
    public ICollection<OrgGroupUserMapping> UserMappings { get; set; }
        = new List<OrgGroupUserMapping>();

    public ICollection<OrgGroupOrgMapping> OrgMappings { get; set; }
        = new List<OrgGroupOrgMapping>();
}

[Table("org_sec_grp_setup", Schema = "public")]
public class OrgSecGrpSetup
{
    [Column("org_sec_grp_cd")]
    public string OrgSecGrpCd { get; set; }

    [Column("s_module_cd")]
    public string ModuleCd { get; set; }

    [Column("company_id")]
    public string CompanyId { get; set; }

    [Column("org_sec_prof_cd")]
    public string OrgSecProfCd { get; set; }

    [Column("modified_by")]
    public string ModifiedBy { get; set; }

    [Column("time_stamp")]
    public DateTime TimeStamp { get; set; }

    [Column("rowversion")]
    public int? Rowversion { get; set; }

    // 🔗 Navigation
    public Module Module { get; set; }
    public OrgSecProfile OrgSecProfile { get; set; }
}

[Table("org_sec_profile", Schema = "public")]
public class OrgSecProfile
{
    [Column("org_sec_prof_cd")]
    public string OrgSecProfCd { get; set; }

    [Column("company_id")]
    public string CompanyId { get; set; }

    [Column("org_sec_prof_name")]
    public string Name { get; set; }

    // 🔗 Navigation
    public ICollection<OrgSecGrpSetup> OrgSecGrpSetups { get; set; }
}
public class OrgSecGrpSetupDto
{
    public string OrgSecGrpCd { get; set; }
    public string ModuleCd { get; set; }
    public string CompanyId { get; set; }
    public string OrgSecProfCd { get; set; }

    public string ModuleName { get; set; }
    public string ProfileName { get; set; }
}
public class OrgGroupOrgMapping
{
    public int OrgGroupId { get; set; }
    public string OrgId { get; set; }

    public OrgGroup OrgGroup { get; set; }
    public Organization Organization { get; set; }
}

public class BulkGroupOrgsToggleRequest
{
    public int GroupId { get; set; }
    public List<string> OrgIds { get; set; } = new();
}

public class BulkUserOrgsToggleRequest
{
    public int UserId { get; set; }
    public List<string> OrgIds { get; set; } = new();
}

public class OrgGroupCreateUpdateDto
{
    public string OrgGroupCode { get; set; } = null!;
    public string OrgGroupName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
