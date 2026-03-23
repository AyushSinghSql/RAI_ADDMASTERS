using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models;

public partial class PlProject
{
    public string ProjId { get; set; } = null!;

    public string ProjTypeDc { get; set; } = null!;

    public string ProjName { get; set; } = null!;

    public string OrgId { get; set; } = null!;

    public string? CustId { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ProjEndDt { get; set; }

    public DateOnly? ProjStartDt { get; set; }

    public string AcctGrpCd { get; set; } = null!;

    public string ActiveFl { get; set; } = null!;
    public int? LevelNo { get; set; }
    public string CompanyId { get; set; } = null!;

    public string AcctGrpFl { get; set; } = null!;

    public string ProjMgrName { get; set; } = null!;

    public string ProjLongName { get; set; } = null!;
    public decimal? proj_v_tot_amt { get; set; } = 0m;
    public decimal? proj_f_tot_amt { get; set; } = 0m;
    public decimal? proj_v_fee_amt { get; set; } = 0m;
    public decimal? proj_v_cst_amt { get; set; } = 0m;
    public decimal? proj_f_fee_amt { get; set; } = 0m;
    public decimal? proj_f_cst_amt { get; set; } = 0m;

    public DateOnly? InactiveDt { get; set; }

    //[ForeignKey("OrgId")]
    public virtual Organization? Org { get; set; } = null!;

    public virtual ICollection<PlForecast> PlForecasts { get; set; } = new List<PlForecast>();

    public virtual ICollection<PlProjectPlan> PlProjectPlans { get; set; } = new List<PlProjectPlan>();
    public ICollection<UserProjectMap> UserProjects { get; set; } = new List<UserProjectMap>();

    //public Organization? Organization { get; set; }
    //[JsonIgnore]
    public ICollection<ProjectFlag>? Flags { get; set; }
    //[JsonIgnore]
    public ProjectFinancial? Financial { get; set; }
    //[JsonIgnore]
    public ICollection<ProjectHierarchy>? Hierarchy { get; set; }
    //[JsonIgnore]
    public ProjectContract? Contract { get; set; }
    //[JsonIgnore]
    public ProjectAddress? Address { get; set; }
}

public partial class ProjectUpdateDateDto
{
    public string? ProjId { get; set; }
    public DateOnly? ProjEndDt { get; set; }
    public DateOnly? ProjStartDt { get; set; }


}


//public class Project
//{
//    [Key]
//    [Column("proj_id")]
//    [MaxLength(30)]
//    public string ProjId { get; set; }


//    [Column("proj_name")]
//    [MaxLength(25)]
//    public string? ProjName { get; set; }

//    [Column("proj_long_name")]
//    [MaxLength(50)]
//    public string? ProjLongName { get; set; }

//    [Column("classification")]
//    [MaxLength(20)]
//    public string? Classification { get; set; }

//    [Column("proj_type_dc")]
//    [MaxLength(15)]
//    public string? ProjTypeDc { get; set; }

//    [Column("acct_grp_cd")]
//    [MaxLength(20)]
//    public string? AcctGrpCd { get; set; }
//    [Column("level_no")]
//    public int? LevelNo { get; set; }

//    [Column("org_id")]
//    [MaxLength(20)]
//    public string? OrgId { get; set; }


//    [Column("company_id")]
//    [MaxLength(10)]
//    public string? CompanyId { get; set; }


//    [Column("proj_mgr_name")]
//    [MaxLength(25)]
//    public string? ProjMgrName { get; set; }


//    [Column("proj_abbrv_cd")]
//    [MaxLength(6)]
//    public string? ProjAbbrvCd { get; set; }

//    [Column("proj_start_dt")]
//    public DateTime? ProjStartDt { get; set; }

//    [Column("proj_end_dt")]
//    public DateTime? ProjEndDt { get; set; }


//    [Column("active_fl")]
//    public string? ActiveFl { get; set; }


//    [Column("modified_by")]
//    [MaxLength(20)]
//    public string? ModifiedBy { get; set; }


//    [Column("time_stamp")]
//    public DateTime TimeStamp { get; set; }

//    //[JsonIgnore]
//    public Organization? Organization { get; set; }
//    //[JsonIgnore]
//    public ICollection<ProjectFlag>? Flags { get; set; }
//    //[JsonIgnore]
//    public ProjectFinancial? Financial { get; set; }
//    //[JsonIgnore]
//    public ICollection<ProjectHierarchy>? Hierarchy { get; set; }
//    //[JsonIgnore]
//    public ProjectContract? Contract { get; set; }
//    //[JsonIgnore]
//    public ProjectAddress? Address { get; set; }
//    //public ICollection<ProjectEmployee>? ProjectEmployees { get; set; }
//}
[Table("proj_financials")]
public class ProjectFinancial
{
    [Key]
    [Column("proj_id")]
    public string? ProjectId { get; set; }

    [Column("proj_v_tot_amt")]
    public decimal ProjVTotAmt { get; set; }

    [Column("proj_f_tot_amt")]
    public decimal ProjFTotAmt { get; set; }

    [Column("proj_v_fee_amt")]
    public decimal ProjVFeeAmt { get; set; }

    [Column("proj_v_cst_amt")]
    public decimal ProjVCstAmt { get; set; }

    [Column("proj_f_fee_amt")]
    public decimal ProjFFeeAmt { get; set; }

    [Column("proj_f_cst_amt")]
    public decimal ProjFCstAmt { get; set; }

    [Column("proj_v_awd_fee_amt")]
    public decimal ProjVAwdFeeAmt { get; set; }

    [Column("proj_f_awd_fee_amt")]
    public decimal ProjFAwdFeeAmt { get; set; }

    public PlProject? Project { get; set; }
}
[Table("proj_flags")]
public class ProjectFlag
{
    
    [Column("proj_id")]
    public string ProjectId { get; set; }

    [Column("flag_name")]
    [MaxLength(50)]
    public string FlagName { get; set; }

    [Column("flag_value")]
    [MaxLength(1)]
    public string FlagValue { get; set; }

    public PlProject? Project { get; set; }
}
[Table("proj_hierarchy")]
public class ProjectHierarchy
{
    
    [Column("proj_id")]
    public string ProjectId { get; set; }

    [Column("level_no")]
    public int? LevelNo { get; set; }

    [Column("proj_seg_id")]
    public string ProjSegId { get; set; }

    [Column("proj_seg_name")]
    public string ProjSegName { get; set; }

    public PlProject? Project { get; set; }
}

[Table("proj_contracts")]
public class ProjectContract
{
    [Key]
    [Column("proj_id")]
    public string ProjectId { get; set; }

    [Column("prime_contr_id")]
    public string PrimeContrId { get; set; }

    [Column("subctr_id")]
    public string SubctrId { get; set; }

    [Column("cust_po_id")]
    public string CustPoId { get; set; }

    [Column("cntr_id")]
    public string CntrId { get; set; }

    [Column("opp_id")]
    public string OppId { get; set; }

    [Column("task_order_no")]
    public string TaskOrderNo { get; set; }

    public PlProject? Project { get; set; }
}

[Table("proj_address")]
public class ProjectAddress
{
    [Key]
    [Column("proj_id")]
    public string ProjectId { get; set; }

    [Column("proj_ln_1_adr")]
    public string ProjLn1Adr { get; set; }

    [Column("proj_ln_2_adr")]
    public string ProjLn2Adr { get; set; }

    [Column("proj_ln_3_adr")]
    public string ProjLn3Adr { get; set; }

    [Column("city_name")]
    public string CityName { get; set; }

    [Column("mail_state_dc")]
    public string MailStateDc { get; set; }

    [Column("postal_cd")]
    public string PostalCd { get; set; }

    [Column("country_cd")]
    public string CountryCd { get; set; }

    public PlProject? Project { get; set; }
}
//[Table("proj_empl")]
//public class ProjectEmployee
//{
//    [Column("proj_id")]
//    public string ProjId { get; set; }

//    [Column("empl_id")]
//    public string EmplId { get; set; }

//    //[Column("assigned_dt")]
//    //public DateTime? AssignedDt { get; set; }

//    //[Column("role_cd")]
//    //public string? RoleCd { get; set; }

//    public Project? Project { get; set; }

//    public Employee? Employee { get; set; }
//}

public class ProjectDto
{
    public string ProjId { get; set; }
    public string ProjName { get; set; }
    public string ProjTypeDc { get; set; }
}


[Table("project_modifications")]
public class ProjectModification
{
    [Column("proj_id")]
    public string? ProjId { get; set; }

    [Column("proj_mod_desc")]
    public string? ProjModDesc { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("proj_start_dt")]
    public DateTime? ProjStartDt { get; set; }

    [Column("proj_end_dt")]
    public DateTime? ProjEndDt { get; set; }

    [Column("proj_v_cst_amt", TypeName = "numeric")]
    public decimal? ProjVCstAmt { get; set; }

    [Column("proj_v_fee_amt", TypeName = "numeric")]
    public decimal? ProjVFeeAmt { get; set; }

    [Column("proj_f_cst_amt", TypeName = "numeric")]
    public decimal? ProjFCstAmt { get; set; }

    [Column("proj_f_fee_amt", TypeName = "numeric")]
    public decimal? ProjFFeeAmt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("time_stamp")]
    public DateTime? TimeStamp { get; set; }

    [Column("proj_v_proft_rt", TypeName = "numeric")]
    public decimal? ProjVProftRt { get; set; }

    [Column("proj_f_proft_rt", TypeName = "numeric")]
    public decimal? ProjFProftRt { get; set; }

    [Column("proj_mod_id")]
    public string? ProjModId { get; set; }

    [Column("deliv_units_qty", TypeName = "numeric")]
    public decimal? DelivUnitsQty { get; set; }

    [Column("est_unit_cst_amt", TypeName = "numeric")]
    public decimal? EstUnitCstAmt { get; set; }

    [Column("unit_price_amt", TypeName = "numeric")]
    public decimal? UnitPriceAmt { get; set; }

    [Column("item_key")]
    public string? ItemKey { get; set; }

    [Column("clin_id")]
    public string? ClinId { get; set; }

    [Column("effect_dt")]
    public DateTime? EffectDt { get; set; }

    [Column("rowversion")]
    public int? RowVersion { get; set; }

    [Column("cntr_id")]
    public string? CntrId { get; set; }

    [Column("cntr_mod_id")]
    public string? CntrModId { get; set; }

    [Column("subcntr_id")]
    public string? SubCntrId { get; set; }

    [Column("subcntr_mod_id")]
    public string? SubCntrModId { get; set; }
}
