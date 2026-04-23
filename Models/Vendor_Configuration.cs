using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("vendor_certification_setup")]
    public class VendorCertificationSetup
    {
        [Key]
        [Column("cert_cd")]
        public string CertCode { get; set; }

        [Column("cert_name")]
        public string CertName { get; set; }

        [Column("show_lookup_fl")]
        public string ShowLookupFl { get; set; }

        [Column("prime_agency_id")]
        public string? PrimeAgencyId { get; set; }

        [Column("prof_org_id")]
        public string? ProfOrgId { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateOnly TimeStamp { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }
    }

    [Table("certification_levels")]
    public class CertificationLevel
    {
        [Column("cert_level_cd")]
        public string CertLevelCd { get; set; }

        [Column("cert_cd")]
        public string CertCd { get; set; }

        [Column("cert_level_desc")]
        public string CertLevelDesc { get; set; }

        [Column("show_lookup_fl")]
        public string ShowLookupFl { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }

        // Navigation
        public VendorCertificationSetup? Certification { get; set; }
    }

    [Table("certification_status")]
    public class CertificationStatus
    {
        [Column("cert_status_cd")]
        public string CertStatusCd { get; set; }

        [Column("cert_cd")]
        public string CertCd { get; set; }

        [Column("cert_status_desc")]
        public string CertStatusDesc { get; set; }

        [Column("show_lookup_fl")]
        public string ShowLookupFl { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }

        // Navigation
        public VendorCertificationSetup? Certification { get; set; }
    }

    public class DropdownDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    [Table("prof_org")]
    public class ProfOrg
    {
        [Key]
        [Column("prof_org_id")]
        [MaxLength(10)]
        public string ProfOrgId { get; set; }

        [Required]
        [Column("prof_org_desc")]
        [MaxLength(30)]
        public string ProfOrgDesc { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }
    }

    [Table("skill")]
    public class Skill
    {
        [Key]
        [Column("skill_id")]
        [MaxLength(10)]
        public string SkillId { get; set; }

        [Required]
        [Column("skill_desc")]
        [MaxLength(255)]
        public string SkillDesc { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        [Required]
        [Column("active_fl")]
        [MaxLength(1)]
        public string ActiveFl { get; set; } = "Y";
    }

    [Table("h_skill_lvl")]
    public class HSkillLvl
    {
        [Key]
        [Column("skill_lvl_cd")]
        [MaxLength(10)]
        public string SkillLvlCd { get; set; }

        [Required]
        [Column("skill_lvl_desc")]
        [MaxLength(30)]
        public string SkillLvlDesc { get; set; }

        [Column("misc1_fld")]
        [MaxLength(20)]
        public string? Misc1Fld { get; set; }

        [Column("misc1_dt")]
        public DateTime? Misc1Dt { get; set; }

        [Column("misc1_fl")]
        [MaxLength(1)]
        public string? Misc1Fl { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }

    [Table("training")]
    public class Training
    {
        [Key]
        [Column("train_id")]
        [MaxLength(10)]
        public string TrainId { get; set; }

        [Required]
        [Column("train_desc")]
        [MaxLength(60)]
        public string TrainDesc { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Column("train_ceu_cred")]
        public decimal TrainCeuCred { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        [Required]
        [Column("detl_job_valid_mthd")]
        [MaxLength(1)]
        public string DetlJobValidMthd { get; set; } = "N";
    }

    [Table("training_detl_job_titles")]
    public class TrainingDetlJobTitle
    {
        [Column("train_id")]
        [MaxLength(10)]
        public string TrainId { get; set; }

        [Column("detl_job_cd")]
        [MaxLength(10)]
        public string DetlJobCd { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Column("rowversion")]
        public decimal RowVersion { get; set; }

        // Navigation (optional)
        public Training Training { get; set; }
    }
    [Table("training_srce")]
    public class TrainingSrce
    {
        [Key]
        [Column("train_srce_id")]
        [MaxLength(10)]
        public string TrainSrceId { get; set; }

        [Required]
        [Column("train_srce_desc")]
        [MaxLength(30)]
        public string TrainSrceDesc { get; set; }

        [Required]
        [Column("s_int_ext_cd")]
        [MaxLength(1)]
        public string SIntExtCd { get; set; }  // I / E

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }
    [Table("h_company_prop")]
    public class CompanyProperty
    {
        [Key]
        [Column("prop_id")]
        [MaxLength(25)]
        public string PropId { get; set; }

        [Required]
        [Column("prop_desc")]
        [MaxLength(30)]
        public string PropDesc { get; set; }

        [Required]
        [Column("manuf_name")]
        [MaxLength(25)]
        public string ManufName { get; set; }

        [Required]
        [Column("serial_id")]
        [MaxLength(25)]
        public string SerialId { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }
    [Table("h_empl_prop")]
    public class EmployeeProperty
    {
        [Column("empl_id")]
        [MaxLength(12)]
        public string EmplId { get; set; }

        [Column("prop_id")]
        [MaxLength(25)]
        public string PropId { get; set; }

        [Column("issue_dt")]
        public DateOnly IssueDt { get; set; }

        [Required]
        [Column("prop_amt")]
        public decimal PropAmt { get; set; }

        [Required]
        [Column("whse_name")]
        [MaxLength(12)]
        public string WhseName { get; set; }

        [Required]
        [Column("control_id")]
        [MaxLength(25)]
        public string ControlId { get; set; }

        [Column("return_dt")]
        public DateOnly? ReturnDt { get; set; }

        [Required]
        [Column("other_s")]
        [MaxLength(30)]
        public string OtherS { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public CompanyProperty Property { get; set; }
    }
    [Table("reason_codes")]
    public class ReasonCode
    {
        [Column("rsn_cd")]
        public string RsnCd { get; set; }

        [Column("s_rsn_wh_used_cd")]
        public string SRsnWhUsedCd { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("rsn_desc")]
        public string RsnDesc { get; set; }

        [Column("upd_last_ct_dt_fl")]
        public string UpdLastCtDtFl { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }
    [Table("reason_usage_codes")]
    public class ReasonUsageCode
    {
        [Key]
        [Column("usage_code")]
        [MaxLength(1)]
        public string UsageCode { get; set; }

        [Required]
        [Column("usage_description")]
        [MaxLength(30)]
        public string UsageDescription { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }

    [Table("policy_types")]
    public class PolicyType
    {
        [Key]
        [Column("policy_type")]
        [MaxLength(8)]
        public string PolicyTypeCode { get; set; }

        [Required]
        [Column("policy_type_description")]
        [MaxLength(30)]
        public string PolicyTypeDescription { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }
    [Table("bond_types")]
    public class BondType
    {
        [Key]
        [Column("bond_type")]
        [MaxLength(8)]
        public string BondTypeCode { get; set; }

        [Required]
        [Column("bond_type_description")]
        [MaxLength(30)]
        public string BondTypeDescription { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }
    [Table("subcontractor_lien")]
    public class SubcontractorLien
    {
        [Column("vendor_id"), MaxLength(12)]
        public string VendorId { get; set; }

        [Column("project_id"), MaxLength(30)]
        public string ProjectId { get; set; }

        [Column("lien_key")]
        public long LienKey { get; set; }

        [Column("lien_released_fl"), MaxLength(1)]
        public string LienReleasedFl { get; set; }

        [Column("lien_released_date")]
        public DateTime? LienReleasedDate { get; set; }

        [Column("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [Column("issued_by_name"), MaxLength(25)]
        public string IssuedByName { get; set; }

        [Column("phone_number"), MaxLength(25)]
        public string PhoneNumber { get; set; }

        [Column("address_line1"), MaxLength(40)]
        public string AddressLine1 { get; set; }

        [Column("address_line2"), MaxLength(40)]
        public string AddressLine2 { get; set; }

        [Column("address_line3"), MaxLength(40)]
        public string AddressLine3 { get; set; }

        [Column("city"), MaxLength(25)]
        public string City { get; set; }

        [Column("state_code"), MaxLength(15)]
        public string StateCode { get; set; }

        [Column("country_code"), MaxLength(8)]
        public string CountryCode { get; set; }

        [Column("postal_code"), MaxLength(10)]
        public string PostalCode { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }
    [Table("subcontractor_carrier")]
    public class SubcontractorCarrier
    {
        [Key]
        [Column("carrier_id"), MaxLength(12)]
        public string CarrierId { get; set; }

        [Column("carrier_name"), MaxLength(25)]
        public string CarrierName { get; set; }

        [Column("agent_name"), MaxLength(25)]
        public string AgentName { get; set; }

        [Column("agent_title"), MaxLength(25)]
        public string AgentTitle { get; set; }

        [Column("address_line1"), MaxLength(40)]
        public string AddressLine1 { get; set; }

        [Column("address_line2"), MaxLength(40)]
        public string AddressLine2 { get; set; }

        [Column("address_line3"), MaxLength(40)]
        public string AddressLine3 { get; set; }

        [Column("city"), MaxLength(25)]
        public string City { get; set; }

        [Column("state_code"), MaxLength(15)]
        public string StateCode { get; set; }

        [Column("country_code"), MaxLength(8)]
        public string CountryCode { get; set; }

        [Column("postal_code"), MaxLength(10)]
        public string PostalCode { get; set; }

        [Column("phone_number"), MaxLength(25)]
        public string PhoneNumber { get; set; }

        [Column("fax_number"), MaxLength(25)]
        public string FaxNumber { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }

    [Table("cis_code")]
    public class CisCode
    {
        [Column("cis_code"), MaxLength(6)]
        public string CisCodeId { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("description"), MaxLength(20)]
        public string Description { get; set; }

        [Column("withholding_rate")]
        public decimal? WithholdingRate { get; set; }

        [Column("account_id"), MaxLength(15)]
        public string AccountId { get; set; }

        [Column("organization_id"), MaxLength(20)]
        public string OrganizationId { get; set; }

        [Column("reference1_id"), MaxLength(20)]
        public string Reference1Id { get; set; }

        [Column("reference2_id"), MaxLength(20)]
        public string Reference2Id { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }
    [Table("security_level")]
    public class SecurityLevel
    {
        [Key]
        [Column("security_level_code"), MaxLength(6)]
        public string SecurityLevelCode { get; set; }

        [Column("description"), MaxLength(30)]
        public string Description { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        public ICollection<SecurityClearance> Clearances { get; set; }
    }

    [Table("security_clearance")]
    public class SecurityClearance
    {
        [Key]
        [Column("clearance_code"), MaxLength(6)]
        public string ClearanceCode { get; set; }

        [Column("hierarchy_no")]
        public int HierarchyNo { get; set; }

        [Column("description"), MaxLength(30)]
        public string Description { get; set; }

        [Column("security_level_code"), MaxLength(6)]
        public string SecurityLevelCode { get; set; }

        [Column("sci_flag"), MaxLength(1)]
        public string SciFlag { get; set; }

        [Column("sap_flag"), MaxLength(1)]
        public string SapFlag { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        public SecurityLevel SecurityLevel { get; set; }
    }

    [Table("recurring_voucher_group")]
    public class RecurringVoucherGroup
    {
        [Column("voucher_group_code"), MaxLength(15)]
        public string VoucherGroupCode { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }
    [Table("recurring_voucher_period")]
    public class RecurringVoucherPeriod
    {
        [Column("voucher_group_code"), MaxLength(15)]
        public string VoucherGroupCode { get; set; }

        [Column("fiscal_year_code"), MaxLength(6)]
        public string FiscalYearCode { get; set; }

        [Column("period_no")]
        public int PeriodNo { get; set; }

        [Column("sub_period_no")]
        public int SubPeriodNo { get; set; }

        [Column("company_id"), MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        public RecurringVoucherGroup VoucherGroup { get; set; }
    }

    [Table("lien_waiver_document")]
    public class LienWaiverDocument
    {
        [Key]
        [Column("document_code"), MaxLength(6)]
        public string DocumentCode { get; set; }

        [Column("document_name"), MaxLength(254)]
        public string DocumentName { get; set; }

        [Column("document_description"), MaxLength(30)]
        public string DocumentDescription { get; set; }

        [Column("ap_supp_detail_flag"), MaxLength(1)]
        public string ApSuppDetailFlag { get; set; }

        [Column("ap_all_detail_flag"), MaxLength(1)]
        public string ApAllDetailFlag { get; set; }

        [Column("ar_supp_detail_flag"), MaxLength(1)]
        public string ArSuppDetailFlag { get; set; }

        [Column("ar_all_detail_flag"), MaxLength(1)]
        public string ArAllDetailFlag { get; set; }

        [Column("document_detail_name"), MaxLength(254)]
        public string DocumentDetailName { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }

    [Table("scisap_clearance")]
    public class ScisapClearance
    {
        [Key]
        [Column("clearance_code"), MaxLength(6)]
        public string ClearanceCode { get; set; }

        [Column("clearance_description"), MaxLength(30)]
        public string ClearanceDescription { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }

    [Table("ve_apvl_grp")]
    public class VeApvlGrp
    {
        [Column("ve_apprvl_grp_cd", TypeName = "varchar(6)")]
        public string VeApprvlGrpCd { get; set; }

        [Column("company_id", TypeName = "varchar(10)")]
        public string CompanyId { get; set; }

        [Required]
        [Column("ve_apprvl_grp_desc", TypeName = "varchar(30)")]
        public string VeApprvlGrpDesc { get; set; }

        [Required]
        [Column("modified_by", TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }
    public class VeApvlGrpDto
    {
        public string VeApprvlGrpCd { get; set; }
        public string VeApprvlGrpDesc { get; set; }
        public string CompanyId { get; set; }
        public string ModifiedBy { get; set; }
    }

    [Table("ve_apvl_grp_users")]
    public class VeApvlGrpUsers
    {
        [Column("ve_apprvl_grp_cd", TypeName = "varchar(6)")]
        public string VeApprvlGrpCd { get; set; }

        [Column("apprvr_user_id", TypeName = "varchar(20)")]
        public string ApprvrUserId { get; set; }

        [Column("company_id", TypeName = "varchar(10)")]
        public string CompanyId { get; set; }

        [Required]
        [Column("modified_by", TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }

    public class VeApvlGrpUsersDto
    {
        public string VeApprvlGrpCd { get; set; }
        public string ApprvrUserId { get; set; }
        public string CompanyId { get; set; }
        public string ModifiedBy { get; set; }
    }
}

