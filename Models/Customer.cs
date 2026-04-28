using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PlanningAPI.Models.ArCrRating;

namespace PlanningAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cust", Schema = "public")]
    public class Cust
    {
        // ✅ Keys (NOT NULL)
        [Key, Column("cust_id"), MaxLength(12)]
        public string CustId { get; set; }

        [Key, Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        // ✅ Existing fields (converted to nullable)
        [Column("cust_name"), MaxLength(25)]
        public string? CustName { get; set; }

        [Column("vend_id"), MaxLength(12)]
        public string? VendId { get; set; }

        [Column("ext_tax_exmpt_id"), MaxLength(20)]
        public string? ExtTaxExemptId { get; set; }

        [Column("apply_fin_chg_fl"), MaxLength(1)]
        public string? ApplyFinChgFl { get; set; }

        [Column("grace_days_no")]
        public int? GraceDaysNo { get; set; }

        [Column("annl_fin_rt", TypeName = "numeric(5,4)")]
        public decimal? AnnlFinRt { get; set; }

        [Column("ar_cr_limit_key")]
        public int ArCrLimitKey { get; set; }

        [Column("ar_cr_rating_key")]
        public int ArCrRatingKey { get; set; }

        [Column("sales_terr_key")]
        public int SalesTerrKey { get; set; }

        [Column("cust_type_dc"), MaxLength(15)]
        public string? CustTypeDc { get; set; }

        [Column("sales_abbrv_cd"), MaxLength(6)]
        public string? SalesAbbrvCd { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        // ✅ New fields (aligned same style)

        [Column("cr_rating_dt")]
        public DateTime? CrRatingDt { get; set; }

        [Column("ext_cr_id"), MaxLength(15)]
        public string? ExtCrId { get; set; }

        [Column("fob_fld"), MaxLength(15)]
        public string? FobFld { get; set; }

        [Column("s_credit_status_cd"), MaxLength(6)]
        public string? SCreditStatusCd { get; set; }

        [Column("ovrshp_allow_fl"), MaxLength(1)]
        public string? OvrshpAllowFl { get; set; }

        [Column("srce_insp_fl"), MaxLength(1)]
        public string? SrceInspFl { get; set; }

        [Column("cert_of_cnfrm_fl"), MaxLength(1)]
        public string? CertOfCnfrmFl { get; set; }

        [Column("partial_ship_fl"), MaxLength(1)]
        public string? PartialShipFl { get; set; }

        [Column("allow_subst_fl"), MaxLength(1)]
        public string? AllowSubstFl { get; set; }

        [Column("acceptance_pt_fl"), MaxLength(1)]
        public string? AcceptancePtFl { get; set; }

        [Column("user_def1_fld"), MaxLength(20)]
        public string? UserDef1Fld { get; set; }

        [Column("user_def2_fld"), MaxLength(20)]
        public string? UserDef2Fld { get; set; }

        [Column("cust_long_name"), MaxLength(40)]
        public string? CustLongName { get; set; }

        [Column("apprvd_ord_bal_amt", TypeName = "numeric(17,2)")]
        public decimal? ApprvdOrdBalAmt { get; set; }

        [Column("disc_allow_fl"), MaxLength(1)]
        public string? DiscAllowFl { get; set; }

        [Column("disc_pct_rt", TypeName = "numeric(5,4)")]
        public decimal? DiscPctRt { get; set; }

        [Column("sls_cnt_first_name"), MaxLength(25)]
        public string? SlsCntFirstName { get; set; }

        [Column("sls_cnt_last_name"), MaxLength(25)]
        public string? SlsCntLastName { get; set; }

        [Column("phone_id"), MaxLength(25)]
        public string? PhoneId { get; set; }

        [Column("fax_id"), MaxLength(25)]
        public string? FaxId { get; set; }

        [Column("ackn_reqd_fl"), MaxLength(1)]
        public string? AcknReqdFl { get; set; }

        [Column("issue_by_addr_cd"), MaxLength(6)]
        public string? IssueByAddrCd { get; set; }

        [Column("use_wawf_fl"), MaxLength(1)]
        public string? UseWawfFl { get; set; }

        // 🔗 Navigation Properties (unchanged)

        public CustType? CustType { get; set; }
        public ArCrLimit? ArCrLimit { get; set; }
        public ArCrRating? ArCrRating { get; set; }
        public ArSalesTerr? SalesTerr { get; set; }
        public SalesAbbrvCd? SalesAbbrv { get; set; }
        public IssueByAddr? IssueByAddr { get; set; }

        public ICollection<CustAlias>? Aliases { get; set; }
        public ICollection<CustLimitCrncy>? CustLimitCrncies { get; set; }
        public ICollection<CustDfltAcct>? CustDefaultAccounts { get; set; }
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


    [Table("cust_notes", Schema = "public")]
    public class CustNotes
    {
        [Column("cust_id")]
        [StringLength(12)]
        public string CustId { get; set; }

        [Column("company_id")]
        [StringLength(10)]
        public string CompanyId { get; set; }

        [Column("notes_tx")]
        public string? NotesTx { get; set; }

        [Required]
        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }


    [Table("cust_vat_info", Schema = "public")]
    public class CustVatInfo
    {
        [Column("cust_id")]
        [StringLength(12)]
        public string CustId { get; set; }

        [Column("tax_id")]
        [StringLength(20)]
        public string TaxId { get; set; }

        [Column("tax_loc_cd")]
        [StringLength(30)]
        public string TaxLocCd { get; set; }

        [Required]
        [Column("dflt_tax_id_fl")]
        [StringLength(1)]
        public string DfltTaxIdFl { get; set; }

        [Required]
        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("company_id")]
        [StringLength(10)]
        public string CompanyId { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }

    [Table("ar_ship_mthd", Schema = "public")]
    public class ArShipMthd
    {
        [Key]
        [Column("ar_ship_mthd_key")]
        public long ArShipMthdKey { get; set; }

        [Required]
        [Column("ship_mthd_dc")]
        [StringLength(15)]
        public string ShipMthdDc { get; set; }

        [Required]
        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }

    [Table("cust_terms", Schema = "public")]
    public class CustTerms
    {
        [Key]
        [Column("cust_terms_key")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CustTermsKey { get; set; }

        [Required, StringLength(15)]
        [Column("cust_terms_dc")]
        public string CustTermsDc { get; set; }

        [Column("disc_pct_rt")]
        public decimal DiscPctRt { get; set; }

        [Column("disc_days_no")]
        public int DiscDaysNo { get; set; }

        [Column("s_terms_basis_cd")]
        public string STermsBasisCd { get; set; }

        [Column("s_due_date_cd")]
        public string SDueDateCd { get; set; }

        [Column("no_days_no")]
        public int NoDaysNo { get; set; }

        [Column("day_of_mth_due_no")]
        public int? DayOfMthDueNo { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        public List<CustTermsSch>? Schedules { get; set; }
    }

    [Table("cust_terms_sch", Schema = "public")]
    public class CustTermsSch
    {
        [Column("cust_terms_key")]
        public long CustTermsKey { get; set; }

        [Column("cust_terms_sch_key")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CustTermsSchKey { get; set; }

        [Column("from_day_no")]
        public int FromDayNo { get; set; }

        [Column("to_day_no")]
        public int ToDayNo { get; set; }

        [Column("due_day_no")]
        public int DueDayNo { get; set; }

        [Column("s_cur_next_mth_cd")]
        public string SCurNextMthCd { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }

        public CustTerms? CustTerms { get; set; }
    }
}
