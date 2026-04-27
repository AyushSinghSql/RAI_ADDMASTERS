using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PlanningAPI.Models.ArCrRating;

namespace PlanningAPI.Models
{
    [Table("cust", Schema = "public")]
    public class Cust
    {
        [Key, Column("cust_id"), MaxLength(12)]
        public string CustId { get; set; }

        [Key, Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Required, Column("cust_name"), MaxLength(25)]
        public string CustName { get; set; }

        [Column("vend_id"), MaxLength(12)]
        public string? VendId { get; set; }

        [Required, Column("ext_tax_exmpt_id"), MaxLength(20)]
        public string ExtTaxExemptId { get; set; }

        [Required, Column("apply_fin_chg_fl"), MaxLength(1)]
        public string ApplyFinChgFl { get; set; }

        [Column("grace_days_no")]
        public int GraceDaysNo { get; set; }

        [Column("annl_fin_rt")]
        public decimal AnnlFinRt { get; set; }

        [Column("ar_cr_limit_key")]
        public int? ArCrLimitKey { get; set; }

        [Column("ar_cr_rating_key")]
        public int? ArCrRatingKey { get; set; }

        [Column("sales_terr_key")]
        public int? SalesTerrKey { get; set; }

        [Required, Column("cust_type_dc"), MaxLength(15)]
        public string CustTypeDc { get; set; }

        [Column("sales_abbrv_cd"), MaxLength(6)]
        public string? SalesAbbrvCd { get; set; }

        [Required, Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required, Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        // 🔗 Navigation Properties
        public CustType? CustType { get; set; }
        public ArCrLimit? ArCrLimit { get; set; }
        public ArCrRating? ArCrRating { get; set; }
        public ArSalesTerr? SalesTerr { get; set; }
        public SalesAbbrvCd? SalesAbbrv { get; set; }
        public ICollection<CustAlias>? Aliases { get; set; }
        public ICollection<CustLimitCrncy>? CustLimitCrncies { get; set; }
        public ICollection<CustDfltAcct>? CustDefaultAccounts { get; set; }
        //public IssueByAddr? IssueByAddr { get; set; }    
    }

    [Table("cust_type", Schema = "public")]
    public class CustType
    {
        [Key]
        [Column("cust_type_dc")]
        public string CustTypeDc { get; set; }

        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        public ICollection<Cust>? Customers { get; set; }
    }

    [Table("ar_cr_limit", Schema = "public")]
    public class ArCrLimit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ar_cr_limit_key")]
        public int ArCrLimitKey { get; set; }

        [Column("cr_limit_dc")] public string CrLimitDc { get; set; }
        [Column("limit_amt")] public decimal LimitAmt { get; set; }
        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        public ICollection<Cust>? Customers { get; set; } = new HashSet<Cust>();
    }

    [Table("ar_cr_rating", Schema = "public")]
    public class ArCrRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ar_cr_rating_key")]
        public int ArCrRatingKey { get; set; }

        [Column("cr_rating_cd")] public string CrRatingCd { get; set; }
        [Column("cr_rating_desc")] public string CrRatingDesc { get; set; }
        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }

        public ICollection<Cust>? Customers { get; set; } = new HashSet<Cust>();
    }

    [Table("ar_sales_terr", Schema = "public")]
    public class ArSalesTerr
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("sales_terr_key")]
        public int SalesTerrKey { get; set; }

        [Column("sales_terr_dc")] public string SalesTerrDc { get; set; }
        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        public ICollection<Cust>? Customers { get; set; } = new HashSet<Cust>();
    }

    [Table("issue_by_addr", Schema = "public")]
    public class IssueByAddr
    {
        [Key]
        [Column("issue_by_addr_cd")]
        public string IssueByAddrCd { get; set; }

        [Column("issue_by_addr_name")] public string IssueByAddrName { get; set; }
        [Column("city_name")] public string CityName { get; set; }

        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        public ICollection<Cust>? Customers { get; set; } = new HashSet<Cust>();

    }

    [Table("sales_abbrv_cd", Schema = "public")]
    public class SalesAbbrvCd
    {
        [Key]
        [Column("sales_abbrv_cd")]
        public string SalesAbbrvCdId { get; set; }

        [Column("sales_abbrv_desc")] public string SalesAbbrvDesc { get; set; }
        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        public ICollection<Cust>? Customers { get; set; } = new HashSet<Cust>();
    }

    public class CustDto
    {
        [Required, MaxLength(12)]
        public string CustId { get; set; }

        [Required, MaxLength(10)]
        public string CompanyId { get; set; }

        [Required]
        public string CustName { get; set; }

        public string CustTypeDc { get; set; }

        public int? ArCrLimitKey { get; set; }
        public int? ArCrRatingKey { get; set; }
        public int? SalesTerrKey { get; set; }
    }

    [Table("cust_addr", Schema = "public")]
    public class CustAddr
    {
        [Column("cust_id"), MaxLength(12)]
        public string CustId { get; set; }

        [Column("addr_dc"), MaxLength(10)]
        public string AddrDc { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("phone_id"), Required, MaxLength(25)]
        public string PhoneId { get; set; }

        [Column("fax_id"), Required, MaxLength(25)]
        public string FaxId { get; set; }

        [Column("ln_1_addr"), Required, MaxLength(40)]
        public string Ln1Addr { get; set; }

        [Column("ln_2_addr"), Required, MaxLength(40)]
        public string Ln2Addr { get; set; }

        [Column("ln_3_addr"), Required, MaxLength(40)]
        public string Ln3Addr { get; set; }

        [Column("city_name"), Required, MaxLength(25)]
        public string CityName { get; set; }

        [Column("mail_state_dc")]
        public string? MailStateDc { get; set; }

        [Column("country_cd")]
        public string? CountryCd { get; set; }

        [Column("postal_cd"), Required]
        public string PostalCd { get; set; }

        [Column("sales_tax_cd")]
        public string? SalesTaxCd { get; set; }

        [Column("ship_id")]
        public string? ShipId { get; set; }

        [Column("oth_phone_id"), Required]
        public string OthPhoneId { get; set; }

        [Column("s_bill_addr_cd"), Required]
        public string SBillAddrCd { get; set; }

        [Column("s_ship_addr_cd"), Required]
        public string SShipAddrCd { get; set; }

        [Column("s_mark_for_addr_cd"), Required]
        public string SMarkForAddrCd { get; set; }

        [Column("ext_tax_exmpt_id"), Required]
        public string ExtTaxExmptId { get; set; }

        [Column("email_id")]
        public string? EmailId { get; set; }

        [Column("modified_by"), Required]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        // 🔗 Navigation
        public Cust? Cust { get; set; }

        public ICollection<CustAddrCntact>? Contacts { get; set; }
    }

    public class CustAddrDto
    {
        [Required] public string CustId { get; set; }
        [Required] public string AddrDc { get; set; }
        [Required] public string CompanyId { get; set; }

        [Required] public string PhoneId { get; set; }
        [Required] public string Ln1Addr { get; set; }
        [Required] public string CityName { get; set; }
        [Required] public string PostalCd { get; set; }
    }

    [Table("cust_addr_cntact", Schema = "public")]
    public class CustAddrCntact
    {
        [Column("cust_id"), MaxLength(12)]
        public string CustId { get; set; }

        [Column("addr_dc"), MaxLength(10)]
        public string AddrDc { get; set; }

        [Column("cntact_id"), MaxLength(10)]
        public string CntactId { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("cntact_first_name"), Required, MaxLength(20)]
        public string CntactFirstName { get; set; }

        [Column("cntact_last_name"), Required, MaxLength(25)]
        public string CntactLastName { get; set; }

        [Column("phone_id"), Required, MaxLength(25)]
        public string PhoneId { get; set; }

        [Column("fax_id"), Required, MaxLength(25)]
        public string FaxId { get; set; }

        [Column("oth_phone_id"), Required, MaxLength(25)]
        public string OthPhoneId { get; set; }

        [Column("cntact_title_name"), Required, MaxLength(25)]
        public string CntactTitleName { get; set; }

        [Column("notes"), Required, MaxLength(254)]
        public string Notes { get; set; }

        [Column("modified_by"), Required, MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("email_id"), MaxLength(100)]
        public string? EmailId { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        // 🔗 Navigation
        public CustAddr CustAddr { get; set; }
    }
    public class CustAddrCntactDto
    {
        [Required] public string CustId { get; set; }
        [Required] public string AddrDc { get; set; }
        [Required] public string CntactId { get; set; }
        [Required] public string CompanyId { get; set; }

        [Required] public string CntactFirstName { get; set; }
        [Required] public string CntactLastName { get; set; }

        [Required] public string PhoneId { get; set; }
        [Required] public string CntactTitleName { get; set; }

        [Required] public string Notes { get; set; }

        [EmailAddress]
        public string? EmailId { get; set; }
    }

    [Table("cust_alias", Schema = "public")]
    public class CustAlias
    {
        [Column("cust_id"), MaxLength(12)]
        public string CustId { get; set; }

        [Column("cust_alias_key")]
        public int CustAliasKey { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("cust_alias_name"), Required, MaxLength(40)]
        public string CustAliasName { get; set; }

        [Column("modified_by"), Required, MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        // 🔗 Navigation
        public Cust Cust { get; set; }
    }

    public class CustAliasDto
    {
        [Required] public string CustId { get; set; }
        [Required] public int CustAliasKey { get; set; }
        [Required] public string CompanyId { get; set; }

        [Required]
        [MaxLength(40)]
        public string CustAliasName { get; set; }
    }

    [Table("s_cust_trn_type", Schema = "public")]
    public class SCustTrnType
    {
        [Key]
        [Column("s_cust_trn_type")]
        [MaxLength(1)]
        public string SCustTrnTypeCode { get; set; }

        [Required]
        [Column("cust_trn_type_desc")]
        [MaxLength(30)]
        public string Description { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        public ICollection<CustDfltAcct> CustDefaultAccounts { get; set; }
    }

    [Table("cust_dflt_acct", Schema = "public")]
    public class CustDfltAcct
    {
        [Column("cust_id")]
        [MaxLength(12)]
        public string CustId { get; set; }

        [Column("s_cust_trn_type")]
        [MaxLength(1)]
        public string SCustTrnType { get; set; }

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("acct_id")]
        public string AcctId { get; set; }

        [Column("org_id")]
        public string OrgId { get; set; }

        [Column("ref1_id")]
        public string Ref1Id { get; set; }

        [Column("ref2_id")]
        public string Ref2Id { get; set; }

        [Column("proj_id")]
        public string ProjId { get; set; }

        [Required]
        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        [Column("bank_acct_abbrv")]
        public string BankAcctAbbrv { get; set; }

        // Navigation
        public SCustTrnType SCustTrnTypeNavigation { get; set; }
        public Cust Cust { get; set; }
    }

    public class CustDfltAcctDto
    {
        public string CustId { get; set; }
        public string SCustTrnType { get; set; }
        public string CompanyId { get; set; }
        public string AcctId { get; set; }
        public string OrgId { get; set; }
        public string ProjId { get; set; }
    }

    public class SCustTrnTypeDto
    {
        public string SCustTrnTypeCode { get; set; }
        public string CustTrnTypeDesc { get; set; }
    }

    public class CreateSCustTrnTypeDto
    {
        [Required, MaxLength(1)]
        public string SCustTrnTypeCode { get; set; }

        [Required, MaxLength(30)]
        public string CustTrnTypeDesc { get; set; }
    }

    public class UpdateSCustTrnTypeDto
    {
        [Required, MaxLength(30)]
        public string CustTrnTypeDesc { get; set; }
    }

    [Table("cust_limit_crncy", Schema = "public")]
    public class CustLimitCrncy
    {
        [Column("cust_id"), MaxLength(12)]
        public string CustId { get; set; }

        [Column("s_crncy_cd"), MaxLength(3)]
        public string SCrncyCd { get; set; }

        [Column("crncy_type_cd"), MaxLength(1)]
        public string CrncyTypeCd { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        // Navigation
        public Cust Cust { get; set; }
    }

    public class CustLimitCrncyDto
    {
        [Required, MaxLength(12)]
        public string CustId { get; set; }

        [Required, MaxLength(3)]
        public string SCrncyCd { get; set; }

        [Required, MaxLength(1)]
        public string CrncyTypeCd { get; set; }

        [Required, MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }

        [Required, MaxLength(10)]
        public string CompanyId { get; set; }
    }
}
