using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Account
{
    [Key]
    [StringLength(15)]
    [Column("acct_id")]
    public string AcctId { get; set; }

    [Required]
    [StringLength(25)]
    [Column("acct_name")]
    public string AcctName { get; set; }

    [Required]
    [StringLength(1)]
    [Column("active_fl")]
    public string ActiveFlag { get; set; }

    [Column("fy_cd_fr")]
    [StringLength(6)]
    public string? FyCdFrom { get; set; }

    [Column("pd_no_fr")]
    public int? PdNoFrom { get; set; }

    [Column("fy_cd_to")]
    [StringLength(6)]
    public string? FyCdTo { get; set; }

    [Column("pd_no_to")]
    public int? PdNoTo { get; set; }

    [Column("acct_entr_grp_cd")]
    [StringLength(6)]
    public string? AcctEntryGroupCd { get; set; }

    [Column("proj_reqd_fl")]
    [StringLength(1)]
    public string? ProjectRequiredFlag { get; set; }

    [Column("detl_fl")]
    [StringLength(1)]
    public string? DetailFlag { get; set; }

    [Column("top_fl")]
    [StringLength(1)]
    public string? TopFlag { get; set; }

    [Column("modified_by")]
    [StringLength(20)]
    public string? ModifiedBy { get; set; }

    [Column("time_stamp")]
    public DateTime? Timestamp { get; set; }

    [Column("l1_acct_name")]
    [StringLength(25)]
    public string? L1AcctName { get; set; }

    [Column("l2_acct_name")]
    [StringLength(25)]
    public string? L2AcctName { get; set; }

    [Column("l3_acct_name")]
    [StringLength(25)]
    public string? L3AcctName { get; set; }

    [Column("l4_acct_name")]
    [StringLength(25)]
    public string? L4AcctName { get; set; }

    [Column("l5_acct_name")]
    [StringLength(25)]
    public string? L5AcctName { get; set; }

    [Column("l6_acct_name")]
    [StringLength(25)]
    public string? L6AcctName { get; set; }

    [Column("l7_acct_name")]
    [StringLength(25)]
    public string? L7AcctName { get; set; }

    [Column("lvl_no")]
    public decimal? LvlNo { get; set; }

    [Column("l1_acct_seg_id")]
    [StringLength(25)]
    public string? L1AcctSegId { get; set; }

    [Column("l2_acct_seg_id")]
    [StringLength(25)]
    public string? L2AcctSegId { get; set; }

    [Column("l3_acct_seg_id")]
    [StringLength(25)]
    public string? L3AcctSegId { get; set; }

    [Column("l4_acct_seg_id")]
    [StringLength(25)]
    public string? L4AcctSegId { get; set; }

    [Column("l5_acct_seg_id")]
    [StringLength(25)]
    public string? L5AcctSegId { get; set; }

    [Column("l6_acct_seg_id")]
    [StringLength(25)]
    public string? L6AcctSegId { get; set; }

    [Column("l7_acct_seg_id")]
    [StringLength(25)]
    public string? L7AcctSegId { get; set; }

    [Column("l8_acct_seg_id")]
    [StringLength(25)]
    public string? L8AcctSegId { get; set; }

    [Column("tc_acct_type_cd")]
    [StringLength(10)]
    public string? TcAcctTypeCd { get; set; }

    [Column("rowversion")]
    public decimal? RowVersion { get; set; }

    [Column("sft_fl")]
    [StringLength(1)]
    public string? SftFlag { get; set; }

    [Column("mes_fl")]
    [StringLength(1)]
    public string? MesFlag { get; set; }

    [Column("fs_ln")]
    [StringLength(50)]
    public string? FinalcialStatementLine { get; set; }

    

    [Column("s_acct_type_cd")]
    [StringLength(1)]
    public string? SAcctTypeCd { get; set; }



    [JsonIgnore]
    public virtual AccountGroupSetup? AccountGroupSetup { get; set; }
}

[Table("acct_function_code", Schema = "public")]
public class AcctFunctionCode
{
    [Key] // optional if table has PK, else remove
    [Column("func_code")]
    public string FuncCode { get; set; } = null!;
}
public class AcctMasterDto
{
    public string AcctId { get; set; }
    public string AcctName { get; set; }
    public decimal LvlNo { get; set; }
}
//public class Account
//{
//    [Key]
//    [StringLength(30)]
//    public string AcctId { get; set; }

//    [StringLength(100)]
//    public string AcctName { get; set; }
//    public string? ActiveFlag { get; set; }


//    public string? L1AcctName { get; set; }
//    public string? L2AcctName { get; set; }
//    public string? L3AcctName { get; set; }
//    public string? L4AcctName { get; set; }
//    public string? L5AcctName { get; set; }
//    public string? L6AcctName { get; set; }
//    public string? L7AcctName { get; set; }
//    [NotMapped]
//    public string? ModifiedBy { get; set; }
//    [NotMapped]
//    public DateTime? Createdat { get; set; }
//    [NotMapped]
//    public DateTime? Updatedat { get; set; }
//    public int LvlNo { get; set; }
//    public string? SAcctTypeCd { get; set; }

//    [JsonIgnore]
//    public virtual AccountGroupSetup? AccountGroupSetup { get; set; }
//}


[Table("chart_of_accounts")]
public class ChartOfAccount
{
    [Key]
    [Column("account_id")]
    [MaxLength(50)]
    public string AccountId { get; set; } = null!;

    [Required]
    [Column("account_name")]
    [MaxLength(200)]
    public string? AccountName { get; set; } = null!;

    [Required]
    [Column("cost_type")]
    [MaxLength(50)]
    public string? CostType { get; set; } = null!;

    [Required]
    [Column("account_type")]
    [MaxLength(100)]
    public string? AccountType { get; set; } = null!;

    [Column("budget_sheet")]
    [MaxLength(50)]
    public string? BudgetSheet { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

//public class LaborProjectAccount
//{
//    public string? LaborGroup { get; set; }
//    public string? LaborGroupDescription { get; set; }
//    public string? ProjectAccountGroup { get; set; }
//    public string? ProjectAccountGroupDescription { get; set; }
//    public string? Account { get; set; }
//    public string? AccountName { get; set; }
//}

[Table("labor_project_accounts")]
public class LaborProjectAccount
{
    [Column("labor_group")]
    public string? LaborGroup { get; set; }

    [Column("labor_group_description")]
    public string? LaborGroupDescription { get; set; }

    [Column("project_account_group")]
    public string? ProjectAccountGroup { get; set; }

    [Column("project_account_group_description")]
    public string? ProjectAccountGroupDescription { get; set; }

    [Column("account")]
    public string? Account { get; set; }

    [Column("account_name")]
    public string? AccountName { get; set; }
}