namespace PlanningAPI.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vendors")]
    public class Vendor
    {
        [Key, Column("vend_id", Order = 0)]
        [MaxLength(12)]
        public string VendId { get; set; } = null!;

        [Key, Column("company_id", Order = 1)]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("terms_dc")] public string? TermsDc { get; set; }
        [Column("s_vend_po_cntl_cd")] public string? SVendPoCntlCd { get; set; }
        [Column("fob_fld")] public string? FobFld { get; set; }
        [Column("ship_via_fld")] public string? ShipViaFld { get; set; }

        [Column("hold_pmt_fl")] public string? HoldPmtFl { get; set; }
        [Column("cl_disadv_fl")] public string? ClDisadvFl { get; set; }
        [Column("cl_wom_own_fl")] public string? ClWomOwnFl { get; set; }
        [Column("cl_lab_srpl_fl")] public string? ClLabSrplFl { get; set; }
        [Column("cl_hist_bl_clg_fl")] public string? ClHistBlClgFl { get; set; }

        [Column("prnt_1099_fl")] public string? Prnt1099Fl { get; set; }
        [Column("s_ap_1099_type_cd")] public string? SAp1099TypeCd { get; set; }
        [Column("ap_1099_tax_id")] public string? Ap1099TaxId { get; set; }

        [Column("cust_acct_fld")] public string? CustAcctFld { get; set; }
        [Column("vend_notes")] public string? VendNotes { get; set; }

        [Column("vend_name")] public string? VendName { get; set; }
        [Column("vend_name_ext")] public string? VendNameExt { get; set; }

        [Column("ap_accts_key")] public decimal? ApAcctsKey { get; set; }
        [Column("cash_accts_key")] public decimal? CashAcctsKey { get; set; }

        [Column("pay_when_paid_fl")] public string? PayWhenPaidFl { get; set; }
        [Column("exp_proj_id")] public string? ExpProjId { get; set; }
        [Column("ap_chk_vend_id")] public string? ApChkVendId { get; set; }
        [Column("empl_id")] public string? EmplId { get; set; }
        [Column("user_id")] public string? UserId { get; set; }

        [Column("entry_dtt")] public DateOnly? EntryDtt { get; set; }

        [Column("ed_vch_pay_vend_fl")] public string? EdVchPayVendFl { get; set; }
        [Column("auto_vchr_fl")] public string? AutoVchrFl { get; set; }

        [Column("modified_by")] public string? ModifiedBy { get; set; }
        [Column("time_stamp")] public DateTime? TimeStamp { get; set; }

        [Column("recpt_ln_no")] public decimal? RecptLnNo { get; set; }

        [Column("calc_start_dt")] public DateOnly? CalcStartDt { get; set; }
        [Column("calc_end_dt")] public DateOnly? CalcEndDt { get; set; }

        [Column("rej_pct_rt")] public decimal? RejPctRt { get; set; }
        [Column("late_recpt_pct_rt")] public decimal? LateRecptPctRt { get; set; }
        [Column("early_recpt_pct_rt")] public decimal? EarlyRecptPctRt { get; set; }
        [Column("late_rec_orig_rt")] public decimal? LateRecOrigRt { get; set; }

        [Column("s_cl_sm_bus_cd")] public string? SClSmBusCd { get; set; }
        [Column("vend_cert_dt")] public DateOnly? VendCertDt { get; set; }

        [Column("vend_long_name")] public string? VendLongName { get; set; }
        [Column("chk_memo_s")] public string? ChkMemoS { get; set; }
        [Column("vend_grp_cd")] public string? VendGrpCd { get; set; }

        [Column("s_subctr_pay_cd")] public string? SSubctrPayCd { get; set; }
        [Column("subctr_fl")] public string? SubctrFl { get; set; }

        [Column("limit_trn_crncy_fl")] public string? LimitTrnCrncyFl { get; set; }
        [Column("limit_pay_crncy_fl")] public string? LimitPayCrncyFl { get; set; }

        [Column("dflt_rt_grp_id")] public string? DfltRtGrpId { get; set; }
        [Column("dflt_trn_crncy_cd")] public string? DfltTrnCrncyCd { get; set; }
        [Column("dflt_pay_crncy_cd")] public string? DfltPayCrncyCd { get; set; }

        [Column("vend_cert_id")] public string? VendCertId { get; set; }

        [Column("sep_chk_fl")] public string? SepChkFl { get; set; }
        [Column("pr_vend_fl")] public string? PrVendFl { get; set; }

        [Column("cl_vet_fl")] public string? ClVetFl { get; set; }
        [Column("cl_sd_vet_fl")] public string? ClSdVetFl { get; set; }

        [Column("eprocure_fl")] public string? EprocureFl { get; set; }

        [Column("rowversion")] public decimal? RowVersion { get; set; }

        [Column("tc_exp_cls_cd")] public string? TcExpClsCd { get; set; }
        [Column("vend_apprvl_cd")] public string? VendApprvlCd { get; set; }

        [Column("cl_anc_it_fl")] public string? ClAncItFl { get; set; }

        [Column("vend_1099_name")] public string? Vend1099Name { get; set; }
        [Column("duns_no")] public string? DunsNo { get; set; }

        [Column("sm_subctr_fl")] public string? SmSubctrFl { get; set; }

        [Column("ve_apprvl_grp_cd")] public string? VeApprvlGrpCd { get; set; }
        [Column("vend_prospect_id")] public string? VendProspectId { get; set; }

        [Column("vend_spclty")] public string? VendSpclty { get; set; }
        [Column("vend_web_site")] public string? VendWebSite { get; set; }

        [Column("cage_cd")] public string? CageCd { get; set; }

        [Column("cl_8a_fl")] public string? Cl8aFl { get; set; }
        [Column("cl_abil_one_fl")] public string? ClAbilOneFl { get; set; }

        [Column("govwin_comp_id")] public string? GovwinCompId { get; set; }

        [Column("last_gwiq_synch_dtt")] public DateOnly? LastGwiqSynchDtt { get; set; }
        [Column("last_gwiq_analyst_upd_dtt")] public DateOnly? LastGwiqAnalystUpdDtt { get; set; }

        [Column("gwiq_refresh_fl")] public string? GwiqRefreshFl { get; set; }

        [Column("uei_no")] public string? UeiNo { get; set; }

        [Column("avg_rating_percent")] public decimal? AvgRatingPercent { get; set; }

        [Column("digital_sig_fl")] public string? DigitalSigFl { get; set; }
        [Column("supplier_portal_fl")] public string? SupplierPortalFl { get; set; }
        [Column("ic_vend_fl")] public string? IcVendFl { get; set; }

        [Column("perf_company_id")] public string? PerfCompanyId { get; set; }

        [Column("cmmc_level")] public string? CmmcLevel { get; set; }

        [Column("admin_email")] public string? AdminEmail { get; set; }

        [Column("cl_lgbtq_fl")] public string? ClLgbtqFl { get; set; }

        // Navigation
        public ICollection<VendorAddress> Addresses { get; set; }
        public ICollection<Vendor1099Detail> Vendor1099Details { get; set; }
    }


    [Table("vendor_1099_details")]
    public class Vendor1099Detail
    {
        [Column("taxable_entity_id")]
        public string TaxableEntityId { get; set; }

        [Column("calendar_year")]
        public int CalendarYear { get; set; }

        [Column("form_1099_type_code")]
        public string Form1099TypeCode { get; set; }

        [Column("pay_vendor_id")]
        public string PayVendorId { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("taxable_amount")]
        public decimal? TaxableAmount { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("vendor_name")]
        public string? VendorName { get; set; }

        [Column("taxable_entity_name")]
        public string? TaxableEntityName { get; set; }

        [Column("tax_id")]
        public string? TaxId { get; set; }

        [Column("vendor_1099_tax_id")]
        public string? Vendor1099TaxId { get; set; }

        [Column("cash_org_id")]
        public string? CashOrgId { get; set; }

        [Column("vendor_address_code")]
        public string? VendorAddressCode { get; set; }

        [Column("foreign_indicator")]
        public string? ForeignIndicator { get; set; }

        [Column("vendor_long_name")]
        public string? VendorLongName { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
    }
    [Table("vendor_addresses")]
    public class VendorAddress
    {
        [Column("vend_id")]
        public string VendorId { get; set; }

        [Column("addr_code")]
        public string AddrCode { get; set; }

        [Column("address_line1")]
        public string? AddressLine1 { get; set; }

        [Column("address_line2")]
        public string? AddressLine2 { get; set; }

        [Column("address_line3")]
        public string? AddressLine3 { get; set; }

        [Column("city_name")]
        public string? CityName { get; set; }

        [Column("state_code")]
        public string? StateCode { get; set; }

        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [Column("country_code")]
        public string? CountryCode { get; set; }

        [Column("email_id")]
        public string? EmailId { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
        public ICollection<VendorAddressContact>? Contacts { get; set; }
    }
    [Table("vendor_address_contacts")]
    public class VendorAddressContact
    {
        [Column("vend_id")]
        public string VendId { get; set; }

        [Column("addr_code")]
        public string AddrCode { get; set; }

        [Column("vendor_address_contact_key")]
        public decimal VendorAddressContactKey { get; set; }

        [Column("sequence_no")]
        public decimal? SequenceNo { get; set; }

        [Column("contact_first_name")]
        public string? ContactFirstName { get; set; }

        [Column("contact_last_name")]
        public string? ContactLastName { get; set; }

        [Column("email_id")]
        public string? EmailId { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public VendorAddress VendorAddress { get; set; }
    }


    public class VendorTransactionRequest
    {
        public Vendor Vendor { get; set; }
        public List<VendorAddress>? Addresses { get; set; }
        public List<VendorEmployee>? Employees { get; set; }
        public List<Vendor1099Detail>? TaxDetails { get; set; }
    }

    public class PagedResultDTO<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Data { get; set; }
    }


    [Table("vendor_terms")]
    public class VendorTerm
    {
        [Key]
        [Column("terms_dc")]
        public string TermsDc { get; set; }

        [Column("disc_pct_rt")]
        public decimal? DiscPctRt { get; set; }

        [Column("disc_days_no")]
        public int? DiscDaysNo { get; set; }

        [Column("s_terms_basis_cd")]
        public string? STermsBasisCd { get; set; }

        [Column("s_due_date_cd")]
        public string? SDueDateCd { get; set; }

        [Column("no_days_no")]
        public int? NoDaysNo { get; set; }

        [Column("day_of_mth_due_no")]
        public int? DayOfMthDueNo { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public ICollection<VendorTermSchedule>? Schedules { get; set; }
    }

    [Table("vendor_terms_schedules")]
    public class VendorTermSchedule
    {
        [Column("terms_dc")]
        public string TermsDc { get; set; }

        [Column("vend_terms_sch_key")]
        public decimal VendTermsSchKey { get; set; }

        [Column("due_day_no")]
        public int? DueDayNo { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("from_no")]
        public int? FromNo { get; set; }

        [Column("to_no")]
        public int? ToNo { get; set; }

        [Column("s_cur_next_mth_cd")]
        public string? SCurNextMthCd { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public VendorTerm? VendorTerm { get; set; }
    }
    public class VendorTermDto
    {
        public string TermsDc { get; set; }
        public decimal? DiscPctRt { get; set; }
        public int? DiscDaysNo { get; set; }
        public string? STermsBasisCd { get; set; }
        public string? SDueDateCd { get; set; }
        public int? NoDaysNo { get; set; }
        public int? DayOfMthDueNo { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class VendorTermWithSchedulesDto
    {
        public VendorTermDto Term { get; set; }
        public List<VendorTermScheduleDto>? Schedules { get; set; }
    }

    public class VendorTermScheduleDto
    {
        public decimal VendTermsSchKey { get; set; }
        public int? DueDayNo { get; set; }
        public int? FromNo { get; set; }
        public int? ToNo { get; set; }
        public string? SCurNextMthCd { get; set; }
        public string? ModifiedBy { get; set; }
    }

    [Table("vend_action")]
    public class VendAction
    {
        [Column("vend_id")]
        public string VendId { get; set; }

        [Column("action_key")]
        public decimal ActionKey { get; set; }

        [Column("portal_action_code")]
        public string? PortalActionCode { get; set; }

        [Column("vendor_address_contact_flag")]
        public string? VendorAddressContactFlag { get; set; }

        [Column("vendor_employee_flag")]
        public string? VendorEmployeeFlag { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("action_notes")]
        public string? ActionNotes { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        [Column("vendor_labor_info_flag")]
        public string? VendorLaborInfoFlag { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
    }

    public class VendActionDto
    {
        public string VendId { get; set; }
        public decimal ActionKey { get; set; }
        public string? PortalActionCode { get; set; }
        public string? VendorAddressContactFlag { get; set; }
        public string? VendorEmployeeFlag { get; set; }
        public string? VendorLaborInfoFlag { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ActionNotes { get; set; }
    }

    [Table("vendor_approvers")]
    public class VendorApprover
    {
        [Column("vend_id")]
        public string VendId { get; set; }

        [Column("approver_user_id")]
        public string ApproverUserId { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
    }
    public class VendorApproverDto
    {
        public string VendId { get; set; }
        public string ApproverUserId { get; set; }
        public string? CompanyId { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class VendorApproverQuery
    {
        public string? VendId { get; set; }
        public string? ApproverUserId { get; set; }
        public string? CompanyId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; } = "vend_id";
        public string? SortOrder { get; set; } = "asc";
    }

    [Table("vendor_ceiling")]
    public class VendorCeiling
    {
        [Column("project_id")]
        public string ProjectId { get; set; }

        [Column("billing_labor_category_code")]
        public string BillingLaborCategoryCode { get; set; }

        [Column("vend_id")]
        public string VendId { get; set; }

        [Column("ceiling_hours")]
        public decimal? CeilingHours { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime? ModifiedTs { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
    }
    public class VendorCeilingDto
    {
        public string ProjectId { get; set; }
        public string BillingLaborCategoryCode { get; set; }
        public string VendId { get; set; }

        public decimal? CeilingHours { get; set; }
        public string? CompanyId { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class VendorCeilingQuery
    {
        public string? ProjectId { get; set; }
        public string? VendId { get; set; }
        public string? CompanyId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; } = "project_id";
        public string? SortOrder { get; set; } = "asc";
    }

    [Table("vendor_check_history")]
    public class VendorCheckHistory
    {
        [Column("check_number")]
        public decimal CheckNumber { get; set; }

        [Column("pay_vendor_id")]
        public string PayVendorId { get; set; }

        [Column("cash_account_id")]
        public string? CashAccountId { get; set; }

        [Column("cash_org_id")]
        public string? CashOrgId { get; set; }

        [Column("cash_reference_1")]
        public string? CashReference1 { get; set; }

        [Column("cash_reference_2")]
        public string? CashReference2 { get; set; }

        [Column("source_code")]
        public string? SourceCode { get; set; }

        [Column("fiscal_year_code")]
        public string? FiscalYearCode { get; set; }

        [Column("period_no")]
        public int? PeriodNo { get; set; }

        [Column("sub_period_no")]
        public int? SubPeriodNo { get; set; }

        [Column("check_amount")]
        public decimal? CheckAmount { get; set; }

        [Column("check_date")]
        public DateTime? CheckDate { get; set; }

        [Column("journal_code")]
        public string? JournalCode { get; set; }

        [Column("post_sequence_no")]
        public decimal? PostSequenceNo { get; set; }

        [Column("approver_user_id")]
        public string? ApproverUserId { get; set; }

        [Column("approval_ts")]
        public DateTime? ApprovalTs { get; set; }

        [Column("void_amount")]
        public decimal? VoidAmount { get; set; }

        [Column("status_code")]
        public string? StatusCode { get; set; }

        [Column("payment_user_id")]
        public string? PaymentUserId { get; set; }

        [Column("void_date")]
        public DateTime? VoidDate { get; set; }

        [Column("check_stub_notes")]
        public string? CheckStubNotes { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime? ModifiedTs { get; set; }

        [Column("pay_address_code")]
        public string? PayAddressCode { get; set; }

        [Column("bank_account_abbrev")]
        public string? BankAccountAbbrev { get; set; }

        [Column("bank_reconcile_status_code")]
        public string? BankReconcileStatusCode { get; set; }

        [Column("void_fiscal_year_code")]
        public string? VoidFiscalYearCode { get; set; }

        [Column("void_period_no")]
        public int? VoidPeriodNo { get; set; }

        [Column("void_sub_period_no")]
        public int? VoidSubPeriodNo { get; set; }

        [Column("payment_currency_code")]
        public string? PaymentCurrencyCode { get; set; }

        [Column("payment_check_amount")]
        public decimal? PaymentCheckAmount { get; set; }

        [Column("joint_pay_vendor_name")]
        public string? JointPayVendorName { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
    }
    public class VendorCheckHistoryDto
    {
        public decimal CheckNumber { get; set; }
        public string PayVendorId { get; set; }

        public decimal? CheckAmount { get; set; }
        public DateTime? CheckDate { get; set; }

        public string? StatusCode { get; set; }
        public string? PaymentUserId { get; set; }

        public string? ModifiedBy { get; set; }
    }

    public class VendorCheckHistoryQuery
    {
        public string? PayVendorId { get; set; }
        public string? StatusCode { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    [Table("vendor_check_voucher_detail")]
    public class VendorCheckVoucherDetail
    {
        [Column("check_number")]
        public decimal CheckNumber { get; set; }

        [Column("voucher_key")]
        public decimal VoucherKey { get; set; }

        [Column("cash_account_id")]
        public string? CashAccountId { get; set; }

        [Column("cash_org_id")]
        public string? CashOrgId { get; set; }

        [Column("paid_amount")]
        public decimal? PaidAmount { get; set; }

        [Column("discount_taken_amount")]
        public decimal? DiscountTakenAmount { get; set; }

        [Column("check_date")]
        public DateTime? CheckDate { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime? ModifiedTs { get; set; }

        [Column("voucher_vendor_id")]
        public string? VoucherVendorId { get; set; }

        [Column("transaction_paid_amount")]
        public decimal? TransactionPaidAmount { get; set; }

        [Column("transaction_discount_amount")]
        public decimal? TransactionDiscountAmount { get; set; }

        [Column("payment_paid_amount")]
        public decimal? PaymentPaidAmount { get; set; }

        [Column("payment_discount_amount")]
        public decimal? PaymentDiscountAmount { get; set; }

        [Column("realized_gain")]
        public decimal? RealizedGain { get; set; }

        [Column("realized_loss")]
        public decimal? RealizedLoss { get; set; }

        [Column("unrealized_gain")]
        public decimal? UnrealizedGain { get; set; }

        [Column("unrealized_loss")]
        public decimal? UnrealizedLoss { get; set; }

        [Column("exchange_rate")]
        public decimal? ExchangeRate { get; set; }

        [Column("vat_recover_amount")]
        public decimal? VatRecoverAmount { get; set; }

        [Column("transaction_to_eur_rate")]
        public decimal? TransactionToEurRate { get; set; }

        [Column("eur_to_functional_rate")]
        public decimal? EurToFunctionalRate { get; set; }

        [Column("functional_to_eur_rate")]
        public decimal? FunctionalToEurRate { get; set; }

        [Column("eur_to_payment_rate")]
        public decimal? EurToPaymentRate { get; set; }

        [Column("transaction_to_eur_rate_flag")]
        public string? TransactionToEurRateFlag { get; set; }

        [Column("functional_to_eur_rate_flag")]
        public string? FunctionalToEurRateFlag { get; set; }

        [Column("transaction_currency_date")]
        public DateTime? TransactionCurrencyDate { get; set; }

        [Column("payment_currency_date")]
        public DateTime? PaymentCurrencyDate { get; set; }

        [Column("rate_group_id")]
        public string? RateGroupId { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public VendorCheckHistory? CheckHistory { get; set; }
        public Vendor? Vendor { get; set; }
    }

    public class VendorCheckVoucherDetailDto
    {
        public decimal CheckNumber { get; set; }
        public decimal VoucherKey { get; set; }

        public decimal? PaidAmount { get; set; }
        public decimal? DiscountTakenAmount { get; set; }

        public decimal? ExchangeRate { get; set; }

        public string? VoucherVendorId { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class VendorCheckVoucherQuery
    {
        public decimal? CheckNumber { get; set; }
        public string? VendorId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    [Table("vendor_cis_information")]
    public class VendorCisInformation
    {
        [Column("vend_id")]
        public string VendId { get; set; }

        [Column("cis_code")]
        public string CisCode { get; set; }

        [Column("cis_type")]
        public string? CisType { get; set; }

        [Column("certificate_registration_no")]
        public string? CertificateRegistrationNo { get; set; }

        [Column("national_insurance_no")]
        public string? NationalInsuranceNo { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Column("authorized_user_name")]
        public string? AuthorizedUserName { get; set; }

        [Column("concern_name")]
        public string? ConcernName { get; set; }

        [Column("bank_name")]
        public string? BankName { get; set; }

        [Column("bank_address")]
        public string? BankAddress { get; set; }

        [Column("trading_name")]
        public string? TradingName { get; set; }

        [Column("address_line_1")]
        public string? AddressLine1 { get; set; }

        [Column("address_line_2")]
        public string? AddressLine2 { get; set; }

        [Column("address_line_3")]
        public string? AddressLine3 { get; set; }

        [Column("city_name")]
        public string? CityName { get; set; }

        [Column("state_code")]
        public string? StateCode { get; set; }

        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [Column("country_code")]
        public string? CountryCode { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime? ModifiedTs { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        // Navigation
        public Vendor? Vendor { get; set; }
    }
    public class VendorCisInformationDto
    {
        public string VendId { get; set; }
        public string CisCode { get; set; }

        public string? CisType { get; set; }
        public string? CertificateRegistrationNo { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string? CompanyId { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class VendorCisQuery
    {
        public string? VendId { get; set; }
        public string? CisType { get; set; }
        public string? CompanyId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
