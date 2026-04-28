using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PlanningAPI.Models
{
    public class EmployeeDetails
    {
        [Key, ForeignKey("ProspectiveEntity")]
        public int ProspectiveId { get; set; }

        public decimal HrlyRate { get; set; }
        public string PLC { get; set; }
        public decimal Salary { get; set; }
        public string HomeOrg { get; set; }
        public DateTime CreatedAt { get; internal set; }
        public DateTime UpdatedAt { get; internal set; }
        public string? ModifiedBy { get; internal set; }

        public ProspectiveEntity? ProspectiveEntity { get; set; }
    }


    [Table("employee_default_timesheet")]
    public class EmployeeDefaultTimesheet
    {
        [Key]
        [Column("employee_id")]
        [MaxLength(12)]
        public string EmployeeId { get; set; }

        [Column("account_id")]
        [MaxLength(15)]
        public string? AccountId { get; set; }

        [Column("project_id")]
        [MaxLength(30)]
        public string? ProjectId { get; set; }

        [Column("general_labor_category_code")]
        [MaxLength(6)]
        public string? GeneralLaborCategoryCode { get; set; }

        [Column("work_comp_code")]
        [MaxLength(6)]
        public string? WorkCompCode { get; set; }

        [Column("pay_type")]
        [MaxLength(15)]
        public string? PayType { get; set; }

        [Column("ref_structure_1_id")]
        [MaxLength(20)]
        public string? RefStructure1Id { get; set; }

        [Column("ref_structure_2_id")]
        [MaxLength(20)]
        public string? RefStructure2Id { get; set; }

        [Column("charge_org_id")]
        [MaxLength(20)]
        public string? ChargeOrgId { get; set; }

        [Column("labor_location_code")]
        [MaxLength(6)]
        public string? LaborLocationCode { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }


    [Table("employee_phones")]
    public class EmployeePhone
    {
        [Key]
        [Column("employee_id", Order = 0)]
        [MaxLength(12)]
        public string EmployeeId { get; set; }

        [Key]
        [Column("phone_type_code", Order = 1)]
        [MaxLength(6)]
        public string PhoneTypeCode { get; set; }

        [Required]
        [Column("phone_number")]
        [MaxLength(25)]
        public string PhoneNumber { get; set; }

        [Column("phone_extension")]
        [MaxLength(6)]
        public string? PhoneExtension { get; set; }

        [Required]
        [Column("sequence_no")]
        public int SequenceNo { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }
    }
    [Table("employee_default_pay_types")]
    public class EmployeeDefaultPayType
    {
        [Key]
        [Column("employee_id", Order = 0)]
        [MaxLength(12)]
        public string EmployeeId { get; set; }

        [Key]
        [Required]
        [Column("pay_type", Order = 1)]
        [MaxLength(15)]
        public string PayType { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        [ForeignKey("PayType")]
        public PayType? PayTypeNavigation
        {
            get; set;
        }
    }

    [Table("pay_types")]
    public class PayType
    {
        [Key]
        [Column("pay_type")]
        [MaxLength(15)]
        public string PayTypeCode { get; set; }

        [Required]
        [Column("pay_type_desc")]
        public string Description { get; set; }

        [Column("pay_type_factor_qty")]
        public decimal Factor { get; set; }

        [Column("pay_type_amount")]
        public decimal Amount { get; set; }

        [Column("pay_type_use_code")]
        public string UseCode { get; set; }

        [Column("apply_to_exempt_flag")] public string ApplyToExempt { get; set; }
        [Column("cost_only_flag")] public string CostOnly { get; set; }
        [Column("include_weighted_avg_code")] public string WeightedAvg { get; set; }

        [Column("allow_recast_flag")] public string AllowRecast { get; set; }
        [Column("apply_salary_employee_flag")] public string ApplySalary { get; set; }
        [Column("overtime_pay_type_flag")] public string IsOvertime { get; set; }

        [Column("recast_pay_type")] public string? RecastPayType { get; set; }

        [Column("active_flag")] public string Active { get; set; }

        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        [Column("row_version")] public long? RowVersion { get; set; }
    }

    [Table("employee_leave_balances")]
    public class EmployeeLeaveBalance
    {
        [Key, Column("employee_id", Order = 0)]
        public string EmployeeId { get; set; }

        [Key, Column("leave_year", Order = 1)]
        public int LeaveYear { get; set; }

        [Key, Column("leave_type_code", Order = 2)]
        public string LeaveTypeCode { get; set; }

        [Column("begin_balance_amount")] public decimal BeginBalanceAmount { get; set; }
        [Column("begin_balance_hours")] public decimal BeginBalanceHours { get; set; }
        [Column("begin_lost_amount")] public decimal BeginLostAmount { get; set; }
        [Column("begin_lost_hours")] public decimal BeginLostHours { get; set; }
        [Column("begin_deferred_amount")] public decimal BeginDeferredAmount { get; set; }
        [Column("begin_deferred_hours")] public decimal BeginDeferredHours { get; set; }
        [Column("begin_future1_amount")] public decimal BeginFuture1Amount { get; set; }
        [Column("begin_future1_hours")] public decimal BeginFuture1Hours { get; set; }

        [Column("ytd_accrued_amount")] public decimal YtdAccruedAmount { get; set; }
        [Column("ytd_accrued_hours")] public decimal YtdAccruedHours { get; set; }
        [Column("ytd_lost_amount")] public decimal YtdLostAmount { get; set; }
        [Column("ytd_lost_hours")] public decimal YtdLostHours { get; set; }
        [Column("ytd_used_amount")] public decimal YtdUsedAmount { get; set; }
        [Column("ytd_used_hours")] public decimal YtdUsedHours { get; set; }
        [Column("ytd_deferred_amount")] public decimal YtdDeferredAmount { get; set; }
        [Column("ytd_deferred_hours")] public decimal YtdDeferredHours { get; set; }
        [Column("ytd_future1_amount")] public decimal YtdFuture1Amount { get; set; }
        [Column("ytd_future1_hours")] public decimal YtdFuture1Hours { get; set; }

        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        [Column("row_version")] public long? RowVersion { get; set; }

        [Column("trn_begin_accrued_amount")] public decimal TrnBeginAccruedAmount { get; set; }
        [Column("trn_begin_deferred_amount")] public decimal TrnBeginDeferredAmount { get; set; }
        [Column("trn_begin_lost_amount")] public decimal TrnBeginLostAmount { get; set; }
        [Column("trn_ytd_accrued_amount")] public decimal TrnYtdAccruedAmount { get; set; }
        [Column("trn_ytd_deferred_amount")] public decimal TrnYtdDeferredAmount { get; set; }
        [Column("trn_ytd_lost_amount")] public decimal TrnYtdLostAmount { get; set; }
        [Column("trn_ytd_used_amount")] public decimal TrnYtdUsedAmount { get; set; }

        [Column("new_leave_type_code")] public string? NewLeaveTypeCode { get; set; }
        [Column("new_leave_code")] public string? NewLeaveCode { get; set; }
        [Column("transfer_end_date")] public DateTime? TransferEndDate { get; set; }
        [Column("leave_type_transfer_flag")] public string? LeaveTypeTransferFlag { get; set; }

        [Column("bb_currency_code")] public string? BbCurrencyCode { get; set; }
        [Column("bb_rate_group_id")] public string? BbRateGroupId { get; set; }
        [Column("bb_currency_date")] public DateTime? BbCurrencyDate { get; set; }
        [Column("bb_trn_to_eur_rate")] public decimal? BbTrnToEurRate { get; set; }
        [Column("bb_eur_to_func_rate")] public decimal? BbEurToFuncRate { get; set; }
        [Column("bb_trn_to_eur_rate_flag")] public string? BbTrnToEurRateFlag { get; set; }

        [Column("expense_account_id")] public string? ExpenseAccountId { get; set; }
        [Column("expense_project_id")] public string? ExpenseProjectId { get; set; }
        [Column("expense_org_id")] public string? ExpenseOrgId { get; set; }

        [Column("accrual_account_id")] public string? AccrualAccountId { get; set; }
        [Column("accrual_project_id")] public string? AccrualProjectId { get; set; }
        [Column("accrual_org_id")] public string? AccrualOrgId { get; set; }

        [Column("begin_balance_transferred_flag")] public string? BeginBalanceTransferredFlag { get; set; }

        [Column("avg_functional_amount")] public decimal? AvgFunctionalAmount { get; set; }
        [Column("bb_avg_functional_amount")] public decimal? BbAvgFunctionalAmount { get; set; }

        [Column("employee_leave_balance_source_code")] public string? SourceCode { get; set; }
    }

    [Table("allowance_codes")]
    public class AllowanceCode
    {
        [Key]
        [Column("allowance_cd")]
        [MaxLength(25)]
        public string AllowanceCd { get; set; }

        [Required]
        [Column("allowance_desc")]
        [MaxLength(50)]
        public string AllowanceDesc { get; set; }

        [Required]
        [Column("pay_type")]
        [MaxLength(15)]
        public string PayType { get; set; }

        [Required]
        [Column("s_allow_basis_cd")]
        [MaxLength(2)]
        public string AllowBasisCd { get; set; }

        [Required]
        [Column("s_allow_rate_cd")]
        [MaxLength(1)]
        public string AllowRateCd { get; set; }

        [Required]
        [Column("allowance_rt_amt")]
        public decimal AllowanceRateAmount { get; set; }

        [Required]
        [Column("w_ceil_hrs")]
        public decimal WeeklyCeilHours { get; set; }

        [Required]
        [Column("b_ceil_hrs")]
        public decimal BiWeeklyCeilHours { get; set; }

        [Required]
        [Column("s_ceil_hrs")]
        public decimal SemiMonthlyCeilHours { get; set; }

        [Required]
        [Column("m_ceil_hrs")]
        public decimal MonthlyCeilHours { get; set; }

        [Required]
        [Column("s_add_ln_mthd_cd")]
        [MaxLength(1)]
        public string AddLineMethod { get; set; }

        [Column("proj_id")]
        public string? ProjectId { get; set; }

        [Column("acct_id")]
        public string? AccountId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("ref_1_id")]
        public string? Ref1Id { get; set; }

        [Column("ref_2_id")]
        public string? Ref2Id { get; set; }

        [Required]
        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        [Column("trn_crncy_cd")]
        public string? CurrencyCode { get; set; }

        [Column("mu_crncy_usage_cd")]
        public string? CurrencyUsageCode { get; set; }

        [Column("compute_hrs_basis")]
        public string? ComputeHoursBasis { get; set; }
    }
    public class AllowanceDto
    {
        public string AllowanceCd { get; set; }
        public string AllowanceDesc { get; set; }
        public string PayType { get; set; }
        public string AllowBasisCd { get; set; }
        public string AllowRateCd { get; set; }
        public decimal AllowanceRateAmount { get; set; }

        public decimal WeeklyCeilHours { get; set; }
        public decimal BiWeeklyCeilHours { get; set; }
        public decimal SemiMonthlyCeilHours { get; set; }
        public decimal MonthlyCeilHours { get; set; }

        public string AddLineMethod { get; set; }

        public string? ProjectId { get; set; }
        public string? AccountId { get; set; }
        public string? OrgId { get; set; }

        public string CompanyId { get; set; }
    }
    [Table("employee_allowances")]
    public class EmployeeAllowance
    {
        [Key, Column("employee_id", Order = 0)]
        [MaxLength(12)]
        public string EmployeeId { get; set; }

        [Key, Column("allowance_code", Order = 1)]
        [MaxLength(25)]
        public string AllowanceCode { get; set; }

        [Column("account_id")]
        public string? AccountId { get; set; }

        [Column("project_id")]
        public string? ProjectId { get; set; }

        [Column("organization_id")]
        public string? OrganizationId { get; set; }

        [Column("allowance_rate")]
        public decimal AllowanceRate { get; set; }

        [Column("effective_date")]
        public DateTime? EffectiveDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("ref1_id")]
        public string? Ref1Id { get; set; }

        [Column("ref2_id")]
        public string? Ref2Id { get; set; }

        [Column("general_labor_cat")]
        public string? GeneralLaborCategory { get; set; }

        [Column("billing_labor_cat")]
        public string? BillingLaborCategory { get; set; }

        [Column("labor_location_cd")]
        public string? LaborLocationCode { get; set; }

        [Column("work_comp_code")]
        public string? WorkCompCode { get; set; }

        [Required]
        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public int? RowVersion { get; set; }

        [Column("wh_state_code")]
        public string? WhStateCode { get; set; }
    }
    [Table("subcontractor_insurance_header")]
    public class SubcontractorInsuranceHeader
    {
        [Column("vendor_id"), MaxLength(12)]
        public string VendorId { get; set; }

        [Column("project_id"), MaxLength(30)]
        public string ProjectId { get; set; }

        [Column("policy_type"), MaxLength(8)]
        public string PolicyType { get; set; }

        [Column("required_start_date")]
        public DateTime? RequiredStartDate { get; set; }

        [Column("required_end_date")]
        public DateTime? RequiredEndDate { get; set; }

        [Column("require_payment_fl"), MaxLength(1)]
        public string RequirePaymentFl { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        public ICollection<SubcontractorInsuranceLine> Lines { get; set; }
    }

    [Table("subcontractor_insurance_line")]
    public class SubcontractorInsuranceLine
    {
        [Column("vendor_id"), MaxLength(12)]
        public string VendorId { get; set; }

        [Column("project_id"), MaxLength(30)]
        public string ProjectId { get; set; }

        [Column("policy_type"), MaxLength(8)]
        public string PolicyType { get; set; }

        [Column("line_key")]
        public long LineKey { get; set; }

        [Column("carrier_id"), MaxLength(12)]
        public string CarrierId { get; set; }

        [Column("policy_number"), MaxLength(15)]
        public string PolicyNumber { get; set; }

        [Column("effective_date")]
        public DateTime? EffectiveDate { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Column("insurance_amount")]
        public decimal InsuranceAmount { get; set; }

        [Column("notes"), MaxLength(254)]
        public string Notes { get; set; }

        [Column("modified_by"), MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("row_version")]
        public long? RowVersion { get; set; }

        public SubcontractorInsuranceHeader Header { get; set; }
    }

    [Table("s_lv_ceil_mthd", Schema = "public")]
    public class SLvCeilMthd
    {
        [Key]
        [Column("s_lv_ceil_mthd_cd"), StringLength(6)]
        public string Code { get; set; }

        [Required, Column("lv_ceil_mthd_desc"), StringLength(30)]
        public string Description { get; set; }

        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        [Column("rowversion")] public long? RowVersion { get; set; }

        public List<LvType> LvTypes { get; set; }
    }

    [Table("lv_type", Schema = "public")]
    public class LvType
    {
        [Key]
        [Column("lv_type_cd"), StringLength(4)]
        public string LvTypeCd { get; set; }

        [Column("lv_type_desc")] public string Description { get; set; }

        [Column("s_lv_ceil_mthd_cd")]
        public string CeilMethodCd { get; set; }

        [Column("expns_acct_id")] public string ExpnsAcctId { get; set; }
        [Column("accrl_acct_id")] public string? AccrlAcctId { get; set; }
        [Column("modified_by")] public string? ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        [Column("company_id")] public string CompanyId { get; set; }

        [Column("lv_bal_flr_amt")] public decimal LvBalFlrAmt { get; set; }

        public SLvCeilMthd CeilMethod { get; set; }
        public List<LvTable> Leaves { get; set; }
    }
    [Table("lv_table", Schema = "public")]
    public class LvTable
    {
        [Key]
        [Column("lv_cd")]
        public string LvCd { get; set; }

        [Column("lv_type_cd")]
        public string LvTypeCd { get; set; }

        [Column("lv_desc")]
        public string Description { get; set; }
        [Column("modified_by")] public string? ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }

        public LvType LvType { get; set; }
    }
    [Table("empl_lv_accrl", Schema = "public")]
    public class EmplLvAccrl
    {
        [Column("empl_id")]
        public string EmplId { get; set; }

        [Column("lv_type_cd")]
        public string LvTypeCd { get; set; }

        [Column("lv_cd")]
        public string LvCd { get; set; }

        [Column("lv_hire_dt")]
        public DateTime? LvHireDt { get; set; }

        [Column("modified_by")] public string? ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }

        public LvType LvType { get; set; }
        public LvTable Lv { get; set; }
    }


    [Table("empl_bond_hdr2", Schema = "public")]
    public class EmplBondHdr2
    {
        // 🔑 Composite Key
        [Column("empl_id"), StringLength(12)]
        public string EmplId { get; set; }

        [Column("ded_cd"), StringLength(6)]
        public string DedCd { get; set; }

        // 📅 Business Fields
        [Required]
        [Column("empl_bond_eff_dt")]
        public DateTime EmplBondEffDt { get; set; }

        [Required]
        [Column("bond_beg_bal")]
        public decimal BondBegBal { get; set; }

        // 🧾 Audit
        [Required, Column("modified_by"), StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required, Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }


    [Table("empl_ded", Schema = "public")]
    public class EmplDed
    {
        // 🔑 Composite Key
        [Column("empl_id"), StringLength(12)]
        public string EmplId { get; set; }

        [Column("ded_cd"), StringLength(6)]
        public string DedCd { get; set; }

        // 📌 Deduction Config
        [Required, Column("s_ded_mthd_cd"), StringLength(6)]
        public string SDedMthdCd { get; set; }

        [Required, Column("ded_rt_amt")]
        public decimal DedRtAmt { get; set; }

        [Required, Column("ded_ann_ceil_amt")]
        public decimal DedAnnCeilAmt { get; set; }

        [Required, Column("ded_priority_no")]
        public int DedPriorityNo { get; set; }

        // 📅 Dates
        [Column("ded_start_dt")]
        public DateTime? DedStartDt { get; set; }

        [Column("ded_end_dt")]
        public DateTime? DedEndDt { get; set; }

        [Column("ded_end_cvg_dt")]
        public DateTime? DedEndCvgDt { get; set; }

        [Column("ded_start_cvg_dt")]
        public DateTime? DedStartCvgDt { get; set; }

        // 🧾 Audit
        [Required, Column("modified_by"), StringLength(20)]
        public string ModifiedBy { get; set; }

        [Required, Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public long? RowVersion { get; set; }
    }

    [Table("empl_bond_ln2", Schema = "public")]
    public class EmplBondLn2
    {
        // 🔑 Composite Key
        [Column("empl_id"), StringLength(12)]
        public string EmplId { get; set; }

        [Column("ded_cd"), StringLength(6)]
        public string DedCd { get; set; }

        [Column("bond_ln_key")]
        public int BondLnKey { get; set; }

        // 📊 Fields
        [Column("seq_no")]
        public int SeqNo { get; set; }

        [Column("next_purch_fl")] public string NextPurchFl { get; set; }
        [Column("empl_is_owner_fl")] public string EmplIsOwnerFl { get; set; }
        [Column("reg_type")] public string RegType { get; set; }

        [Column("bond_owner_nm")] public string? BondOwnerNm { get; set; }
        [Column("bond_owner_ssn")] public string? BondOwnerSsn { get; set; }
        [Column("coowner_nm")] public string? CoownerNm { get; set; }

        [Column("empl_is_ben_fl")] public string EmplIsBenFl { get; set; }
        [Column("beneficiary_nm")] public string? BeneficiaryNm { get; set; }

        [Column("bond_series")] public string BondSeries { get; set; }

        [Column("bond_face_amt")] public decimal BondFaceAmt { get; set; }
        [Column("bond_cost_amt")] public decimal BondCostAmt { get; set; }

        [Column("use_empl_addr_fl")] public string UseEmplAddrFl { get; set; }

        [Column("designee_name")] public string? DesigneeName { get; set; }

        [Column("ln_1_adr")] public string? Ln1Adr { get; set; }
        [Column("ln_2_adr")] public string? Ln2Adr { get; set; }
        [Column("ln_3_adr")] public string? Ln3Adr { get; set; }

        [Column("city_name")] public string? CityName { get; set; }
        [Column("mail_st_dc")] public string? MailStDc { get; set; }
        [Column("postal_cd")] public string? PostalCd { get; set; }

        // 🧾 Audit
        [Column("modified_by")] public string ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime TimeStamp { get; set; }
        [Column("rowversion")] public long? RowVersion { get; set; }

        // 🔗 Navigation
        public EmplBondHdr2 Header { get; set; }
    }
}