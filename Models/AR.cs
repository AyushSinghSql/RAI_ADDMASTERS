using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("bank_acct", Schema = "public")]
    public class BankAcct
    {
        [Column("bank_acct_abbrv")]
        public string BankAcctAbbrv { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("bank_aba_no")]
        public long? BankAbaNo { get; set; }

        [Column("non_us_bank_id")]
        public string? NonUsBankId { get; set; }

        [Column("bank_acct_desc")]
        public string BankAcctDesc { get; set; }

        [Required]
        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        public DirDepBank? UsBank { get; set; }
        public NonUsBank? NonUsBank { get; set; }

        public List<ArDfltAcct> DefaultAccounts { get; set; }
    }

    [Table("ar_dflt_acct", Schema = "public")]
    public class ArDfltAcct
    {
        // 🔑 Composite Primary Key
        [Key]
        [Column("s_ar_trn_type")]
        [StringLength(1)]
        public string SArTrnType { get; set; }

        [Key]
        [Column("company_id")]
        [StringLength(10)]
        public string CompanyId { get; set; }

        // 🔗 Foreign Key to BankAcct (part of composite FK)
        [Column("bank_acct_abbrv")]
        [StringLength(6)]
        public string? BankAcctAbbrv { get; set; }

        // 🔗 Account Fields
        [Column("acct_id")]
        [StringLength(15)]
        public string? AcctId { get; set; }

        [Column("org_id")]
        [StringLength(20)]
        public string? OrgId { get; set; }

        [Column("ref1_id")]
        [StringLength(20)]
        public string? Ref1Id { get; set; }

        [Column("ref2_id")]
        [StringLength(20)]
        public string? Ref2Id { get; set; }

        // 🧾 Audit Fields
        [Required]
        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        // 🔗 Navigation Properties
        public BankAcct? BankAcct { get; set; }

        public SArTrnType? TrnType { get; set; }
    }
    [Table("dir_dep_bank", Schema = "public")]
    public class DirDepBank
    {
        [Key]
        [Column("bank_aba_no")]
        public long BankAbaNo { get; set; }

        [Required, StringLength(30)]
        [Column("bank_name")]
        public string BankName { get; set; }

        [Required]
        [Column("s_usage_cd")]
        public string SUsageCd { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        public List<BankAcct> BankAccounts { get; set; }
    }

    [Table("non_us_banks", Schema = "public")]
    public class NonUsBank
    {
        [Key]
        [Column("non_us_bank_id")]
        public string NonUsBankId { get; set; }

        [Required]
        [Column("bank_name_non_us")]
        public string BankName { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        public List<BankAcct> BankAccounts { get; set; }
    }
    [Table("s_ar_trn_type", Schema = "public")]
    public class SArTrnType
    {
        [Key]
        [Column("s_ar_trn_type")]
        public string SArTrnTypeId { get; set; }

        [Column("ar_trn_type_desc")]
        public string Description { get; set; }
    }
}
