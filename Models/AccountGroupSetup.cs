using PlanningAPI.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
[Table("account_group_setup")]
public class AccountGroupSetup
{
    [Column("acct_grp_cd")]
    public string? AcctGroupCode { get; set; }

    [Column("acct_id")]
    public string? AccountId { get; set; }

    [Column("s_acct_func_dc")]
    public string? AccountFunctionDescription { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("time_stamp")]
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    [Column("company_id")]
    public string? CompanyId { get; set; }

    [Column("proj_acct_abbrv_cd")]
    public string? ProjectAccountAbbreviation { get; set; }

    [Column("active_fl")]
    public bool ActiveFlag { get; set; } = true;

    [Column("rev_map_acct")]
    public string? RevenueMappedAccount { get; set; }

    [Column("salcap_map_acct")]
    public string? SalaryCapMappedAccount { get; set; }

    [JsonIgnore]
    public virtual Account? Account { get; set; }

    // optional navigation
    [ForeignKey(nameof(AccountFunctionDescription))]
    public virtual AcctFunctionCode? AcctType { get; set; }
}


public class AccountGroupSetupDTO
{

    public string AccountId { get; set; }

    public string? AccountFunctionDescription { get; set; }
    public string? AcctName { get; set; }
    public string? BudgetSheet { get; set; }


}

public class AccountGroupSetupDTo
{
    public string AcctGroupCode { get; set; }

    public string AccountId { get; set; }

    public string? AccountFunctionDescription { get; set; }

    public string? ProjectAccountAbbreviation { get; set; }

    public bool ActiveFlag { get; set; } = true;

    public string? RevenueMappedAccount { get; set; }

    public string? SalaryCapMappedAccount { get; set; }
    public string? AccountName { get; set; }
    //public string? AccountType { get; set; }
}
