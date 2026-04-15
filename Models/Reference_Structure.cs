using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("ref_struc")]
    public class RefStruc
    {
        [Column("ref_struc_id")]
        public string RefStrucId { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("ref_struc_top_fl")]
        public string RefStrucTopFl { get; set; }

        [Column("lvl_no")]
        public int LvlNo { get; set; }

        [Column("ref_struc_name")]
        public string RefStrucName { get; set; }

        [Column("ref_struc_lvls_no")]
        public int RefStrucLvlsNo { get; set; }

        [Column("ref_data_entry_fl")]
        public string RefDataEntryFl { get; set; }

        [Column("s_ref_entry_cd")]
        public string SRefEntryCd { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateOnly TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public ICollection<OrgAcctRefStruc>? OrgAcctRefStrucs { get; set; }
    }

    [Table("org_acct_ref_struc")]
    public class OrgAcctRefStruc
    {
        [Column("org_id")]
        public string OrgId { get; set; }

        [Column("acct_id")]
        public string AcctId { get; set; }

        [Column("ref_struc_id")]
        public string RefStrucId { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateOnly TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public RefStruc? RefStruc { get; set; }
    }
    public class RefStrucDto
    {
        public string RefStrucId { get; set; }
        public string CompanyId { get; set; }

        public string RefStrucTopFl { get; set; }  // Y/N
        public int LvlNo { get; set; }
        public string RefStrucName { get; set; }
        public int RefStrucLvlsNo { get; set; }

        public string RefDataEntryFl { get; set; } // Y/N
        public string SRefEntryCd { get; set; }    // 1 char

        public string ModifiedBy { get; set; }
    }
}
