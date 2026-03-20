using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models;

public partial class PlOrgnization
{
    public string OrgId { get; set; } = null!;

    public string OrgName { get; set; } = null!;

    [Column("org_top_fl")]
    [MaxLength(1)]
    public string OrgTopFl { get; set; }

    public decimal IcTrckngLvlNo { get; set; }

    public decimal OrgLvlsNo { get; set; }

    public string OrgAbbrvCd { get; set; } = null!;

    [Column("lvl_no")]
    public int LvlNo { get; set; }

    [Column("taxble_entity_id")]
    [MaxLength(10)]
    public string TaxbleEntityId { get; set; }


    [Column("active_fl")]
    [MaxLength(1)]
    public string ActiveFl { get; set; }

    [Column("fy_cd_fr")]
    [MaxLength(6)]
    public string? FyCdFr { get; set; }

    [Column("pd_no_fr")]
    public int? PdNoFr { get; set; }

    [Column("fy_cd_to")]
    [MaxLength(6)]
    public string? FyCdTo { get; set; }

    [Column("pd_no_to")]
    public int? PdNoTo { get; set; }

    public string ModifiedBy { get; set; } = null!;

    public DateTime TimeStamp { get; set; }

    public string CompanyId { get; set; } = null!;

    //public virtual ICollection<EmployeeMaster> PlEmployees { get; set; } = new List<EmployeeMaster>();

    public virtual ICollection<PlProject> PlProjects { get; set; } = new List<PlProject>();
}
