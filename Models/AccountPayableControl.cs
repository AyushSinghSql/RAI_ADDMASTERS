namespace PlanningAPI.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ap_settings")]
    public class ApSettings
    {
        [Key]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Required]
        [Column("cur_cal_yr_no")]
        public int CurCalYrNo { get; set; }

        [Required]
        [Column("age2_days_to_no")]
        public int Age2DaysToNo { get; set; }

        [Required]
        [Column("age3_days_to_no")]
        public int Age3DaysToNo { get; set; }

        [Required]
        [Column("age4_days_to_no")]
        public int Age4DaysToNo { get; set; }

        [Required]
        [Column("age1_hdg_fld")]
        [MaxLength(10)]
        public string Age1HdgFld { get; set; }

        [Required]
        [Column("age2_hdg_fld")]
        [MaxLength(10)]
        public string Age2HdgFld { get; set; }

        [Required]
        [Column("age3_hdg_fld")]
        [MaxLength(10)]
        public string Age3HdgFld { get; set; }

        [Required]
        [Column("age4_hdg_fld")]
        [MaxLength(10)]
        public string Age4HdgFld { get; set; }

        [Required]
        [Column("age5_hdg_fld")]
        [MaxLength(10)]
        public string Age5HdgFld { get; set; }

        [Required]
        [Column("chk_limit_fl")]
        [MaxLength(1)]
        public string ChkLimitFl { get; set; }

        [Required]
        [Column("chk_limit_amt", TypeName = "numeric(17,2)")]
        public decimal ChkLimitAmt { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Column("s_ap_chk_frmt_cd")]
        [MaxLength(1)]
        public string SApChkFrmtCd { get; set; }

        [Column("eft_secure_fl")]
        [MaxLength(1)]
        public string? EftSecureFl { get; set; }

        [Column("chk_sig_limit", TypeName = "numeric(17,2)")]
        public decimal? ChkSigLimit { get; set; }

        [Column("prim_sig_fl")]
        [MaxLength(1)]
        public string? PrimSigFl { get; set; }

        [Column("second_sig_fl")]
        [MaxLength(1)]
        public string? SecondSigFl { get; set; }

        [Column("sig_reqd_message")]
        [MaxLength(60)]
        public string? SigReqdMessage { get; set; }

        [Required]
        [Column("addr_order_cd")]
        [MaxLength(1)]
        public string AddrOrderCd { get; set; }

        [Column("sig1_file_name")]
        [MaxLength(254)]
        public string? Sig1FileName { get; set; }

        [Column("sig2_file_name")]
        [MaxLength(254)]
        public string? Sig2FileName { get; set; }

        [Column("logo_file_name")]
        [MaxLength(254)]
        public string? LogoFileName { get; set; }

        [Column("vend_inf_upd_fl")]
        [MaxLength(1)]
        public string? VendInfUpdFl { get; set; }

        [Column("eft_file_creat_fl")]
        [MaxLength(1)]
        public string? EftFileCreatFl { get; set; }

        [Column("vend_upd_pwd_name")]
        [MaxLength(6)]
        public string? VendUpdPwdName { get; set; }

        [Column("prnt_chks_fl")]
        [MaxLength(1)]
        public string? PrntChksFl { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }

        [Required]
        [Column("vend_apprvl_fl")]
        [MaxLength(1)]
        public string VendApprvlFl { get; set; }

        [Column("dflt_rpt_res_cd")]
        [MaxLength(1)]
        public string? DfltRptResCd { get; set; }

        [Column("ap_chk_frmt_p3_cd")]
        [MaxLength(1)]
        public string? ApChkFrmtP3Cd { get; set; }

        [Column("prnt_arial_fl")]
        [MaxLength(1)]
        public string? PrntArialFl { get; set; }

        [Column("addr_elim_cd")]
        [MaxLength(2)]
        public string? AddrElimCd { get; set; }

        [Column("date_field_cd")]
        [MaxLength(10)]
        public string? DateFieldCd { get; set; }

        [Column("arial_line_cd")]
        [MaxLength(1)]
        public string? ArialLineCd { get; set; }

        [Required]
        [Column("vend_empl_aprvl_fl")]
        [MaxLength(1)]
        public string VendEmplAprvlFl { get; set; }

        [Required]
        [Column("ve_aprvl_grp_fl")]
        [MaxLength(1)]
        public string VeAprvlGrpFl { get; set; }

        [Column("fr_email_eft")]
        [MaxLength(100)]
        public string? FrEmailEft { get; set; }

        [Column("subj_email_eft")]
        [MaxLength(100)]
        public string? SubjEmailEft { get; set; }

        [Column("hdr_email_eft")]
        [MaxLength(240)]
        public string? HdrEmailEft { get; set; }

        [Column("ftr_email_eft")]
        [MaxLength(240)]
        public string? FtrEmailEft { get; set; }
    }

    [Table("default_ap_accounts")]
    public class DefaultApAccount
    {
        [Key]
        [Column("ap_accts_key")]
        public decimal ApAcctsKey { get; set; }

        [Required]
        [Column("acct_id")]
        [MaxLength(15)]
        public string AcctId { get; set; }

        [Required]
        [Column("org_id")]
        [MaxLength(20)]
        public string OrgId { get; set; }

        [Column("ref1_id")]
        [MaxLength(20)]
        public string? Ref1Id { get; set; }

        [Column("ref2_id")]
        [MaxLength(20)]
        public string? Ref2Id { get; set; }

        [Required]
        [Column("seq_no")]
        public int SeqNo { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Required]
        [Column("ap_accts_desc")]
        [MaxLength(30)]
        public string ApAcctsDesc { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }
    }


    [Table("default_cash_accounts", Schema = "public")]
    public class DfltCashAcct
    {
        [Key]
        [Column("cash_accts_key")]
        public decimal CashAcctsKey { get; set; }

        [Required]
        [Column("acct_id")]
        [MaxLength(15)]
        public string AcctId { get; set; }

        [Required]
        [Column("org_id")]
        [MaxLength(20)]
        public string OrgId { get; set; }

        [Column("ref1_id")]
        [MaxLength(20)]
        public string? Ref1Id { get; set; }

        [Column("ref2_id")]
        [MaxLength(20)]
        public string? Ref2Id { get; set; }

        [Required]
        [Column("seq_no")]
        public decimal SeqNo { get; set; }

        [Required]
        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Required]
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Required]
        [Column("cash_accts_desc")]
        [MaxLength(30)]
        public string CashAcctsDesc { get; set; }

        [Column("bank_acct_abbrv")]
        [MaxLength(6)]
        public string? BankAcctAbbrv { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }
    }

    [Table("voucher_settings")]
    public class VoucherSettings
    {
        [Key]
        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("approval_required_flag")]
        public string ApprovalRequiredFlag { get; set; }

        [Column("approval_required_amount")]
        public decimal ApprovalRequiredAmount { get; set; }

        [Column("voucher_number_method_code")]
        public string VoucherNumberMethodCode { get; set; }

        [Column("last_voucher_number")]
        public decimal LastVoucherNumber { get; set; }

        [Column("discount_method_code")]
        public string DiscountMethodCode { get; set; }

        [Column("discount_account_id")]
        public string DiscountAccountId { get; set; }

        [Column("discount_org_id_code")]
        public string DiscountOrgIdCode { get; set; }

        [Column("po_voucher_change_flag")]
        public string PoVoucherChangeFlag { get; set; }

        [Column("match_goods_code")]
        public string MatchGoodsCode { get; set; }

        [Column("match_services_code")]
        public string MatchServicesCode { get; set; }

        [Column("match_misc_code")]
        public string MatchMiscCode { get; set; }

        [Column("partial_receipt_flag")]
        public string PartialReceiptFlag { get; set; }

        [Column("service_receipt_flag")]
        public string ServiceReceiptFlag { get; set; }

        [Column("goods_receipt_flag")]
        public string GoodsReceiptFlag { get; set; }

        [Column("misc_receipt_flag")]
        public string MiscReceiptFlag { get; set; }

        [Column("discrepancy_unit_price_amount")]
        public decimal DiscrepancyUnitPriceAmount { get; set; }

        [Column("discrepancy_unit_price_rate")]
        public decimal DiscrepancyUnitPriceRate { get; set; }

        [Column("discrepancy_quantity_rate")]
        public decimal DiscrepancyQuantityRate { get; set; }

        [Column("default_po_tax_source_code")]
        public string DefaultPoTaxSourceCode { get; set; }

        [Column("default_ap_tax_source_code")]
        public string DefaultApTaxSourceCode { get; set; }

        [Column("post_discount_gl_code")]
        public string PostDiscountGlCode { get; set; }

        [Column("partial_match_option_code")]
        public string PartialMatchOptionCode { get; set; }

        [Column("goods_match_option_code")]
        public string GoodsMatchOptionCode { get; set; }

        [Column("services_match_option_code")]
        public string ServicesMatchOptionCode { get; set; }

        [Column("misc_match_option_code")]
        public string MiscMatchOptionCode { get; set; }

        [Column("default_use_tax_flag")]
        public string DefaultUseTaxFlag { get; set; }

        [Column("discrepancy_total_amount")]
        public decimal DiscrepancyTotalAmount { get; set; }

        [Column("auto_approve_po_flag")]
        public string AutoApprovePoFlag { get; set; }

        [Column("po_approval_required_amount")]
        public decimal PoApprovalRequiredAmount { get; set; }

        [Column("match_parts_code")]
        public string MatchPartsCode { get; set; }

        [Column("auto_voucher_code")]
        public string AutoVoucherCode { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime ModifiedAt { get; set; }

        [Column("discrepancy_po_total_amount")]
        public decimal DiscrepancyPoTotalAmount { get; set; }

        [Column("receipt_load_code")]
        public string ReceiptLoadCode { get; set; }

        [Column("discrepancy_tax_flag")]
        public string DiscrepancyTaxFlag { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }

        [Column("allow_duplicate_invoice_flag")]
        public string AllowDuplicateInvoiceFlag { get; set; }

        [Column("voucher_po_receipt_flag")]
        public string VoucherPoReceiptFlag { get; set; }

        [Column("exchange_rate_code")]
        public string ExchangeRateCode { get; set; }

        [Column("allow_iwo_voucher_flag")]
        public string AllowIwoVoucherFlag { get; set; }

        [Column("iwo_entry_user")]
        public string IwoEntryUser { get; set; }

        [Column("iwo_unapproved_voucher_flag")]
        public string IwoUnapprovedVoucherFlag { get; set; }

        [Column("recalc_detail_flag")]
        public string RecalcDetailFlag { get; set; }

        [Column("invoice_email_flag")]
        public string InvoiceEmailFlag { get; set; }

        [Column("invoice_email_id")]
        public string InvoiceEmailId { get; set; }

        [Column("individual_voucher_msg_code")]
        public string IndividualVoucherMsgCode { get; set; }

        [Column("individual_posted_voucher_msg_code")]
        public string IndividualPostedVoucherMsgCode { get; set; }

        [Column("total_voucher_msg_code")]
        public string TotalVoucherMsgCode { get; set; }

        [Column("total_posted_voucher_msg_code")]
        public string TotalPostedVoucherMsgCode { get; set; }

        [Column("receipt_email_flag")]
        public string ReceiptEmailFlag { get; set; }

        [Column("receipt_email_id")]
        public string ReceiptEmailId { get; set; }

        [Column("multi_level_approval_flag")]
        public string MultiLevelApprovalFlag { get; set; }

        [Column("multi_level_approval_tolerance")]
        public decimal? MultiLevelApprovalTolerance { get; set; }
    }

    [Table("voucher_approvers")]
    public class VoucherApprover
    {
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime ModifiedTs { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }
    }
    public class AddVoucherApproverDto
    {
        public string UserId { get; set; }
        public string CompanyId { get; set; }
    }

    public class VoucherApproverDto
    {
        public string UserId { get; set; }
        public string CompanyId { get; set; }
    }
    [Table("voucher_approver_users")]
    public class VoucherApproverUser
    {
        [Column("approver_user_id")]
        public string ApproverUserId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("modified_ts")]
        public DateTime ModifiedTs { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }
    }
    public class AssignUsersDto
    {
        public string ApproverUserId { get; set; }
        public string CompanyId { get; set; }
        public List<string> UserIds { get; set; }
    }

    public class ApproverUserDto
    {
        public string ApproverUserId { get; set; }
        public string UserId { get; set; }
        public string CompanyId { get; set; }
    }

}
