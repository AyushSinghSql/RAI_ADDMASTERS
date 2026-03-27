using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{

    [Table("empl")]
    public class EmployeeMaster
    {
        [Key]
        [Column("empl_id")]
        [MaxLength(20)]
        public string EmplId { get; set; } = null!;

        [Column("lv_pd_cd"), MaxLength(10)]
        public string? LvPdCd { get; set; }

        [Column("taxble_entity_id"), MaxLength(20)]
        public string? TaxbleEntityId { get; set; }

        [Column("ssn_id"), MaxLength(20)]
        public string? SsnId { get; set; }

        [Column("orig_hire_dt")]
        public DateOnly? OrigHireDt { get; set; }

        [Column("adj_hire_dt")]
        public DateOnly? AdjHireDt { get; set; }

        [Column("term_dt")]
        public DateTime? TermDt { get; set; }

        [Column("s_empl_status_cd"), MaxLength(10)]
        public string? SEmplStatusCd { get; set; }

        [Column("spvsr_name"), MaxLength(100)]
        public string? SpvsrName { get; set; }

        [Column("last_name"), MaxLength(50)]
        public string? LastName { get; set; }

        [Column("first_name"), MaxLength(50)]
        public string? FirstName { get; set; }

        [Column("mid_name"), MaxLength(50)]
        public string? MidName { get; set; }

        [Column("pref_name"), MaxLength(50)]
        public string? PrefName { get; set; }

        [Column("name_prfx_cd"), MaxLength(10)]
        public string? NamePrfxCd { get; set; }

        [Column("name_sfx_cd"), MaxLength(10)]
        public string? NameSfxCd { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("ts_pd_cd"), MaxLength(10)]
        public string? TsPdCd { get; set; }

        [Column("birth_dt")]
        public DateTime? BirthDt { get; set; }

        [Column("city_name"), MaxLength(100)]
        public string? CityName { get; set; }

        [Column("country_cd"), MaxLength(5)]
        public string? CountryCd { get; set; }

        [Column("last_first_name"), MaxLength(120)]
        public string? LastFirstName { get; set; }

        [Column("ln_1_adr"), MaxLength(100)]
        public string? Ln1Adr { get; set; }

        [Column("ln_2_adr"), MaxLength(100)]
        public string? Ln2Adr { get; set; }

        [Column("ln_3_adr"), MaxLength(100)]
        public string? Ln3Adr { get; set; }

        [Column("mail_state_dc"), MaxLength(10)]
        public string? MailStateDc { get; set; }

        [Column("postal_cd"), MaxLength(20)]
        public string? PostalCd { get; set; }

        [Column("modified_by"), MaxLength(50)]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }

        [Column("locator_cd"), MaxLength(20)]
        public string? LocatorCd { get; set; }

        [Column("prir_name"), MaxLength(100)]
        public string? PrirName { get; set; }

        [Column("company_id"), MaxLength(20)]
        public string? CompanyId { get; set; }

        [Column("last_review_dt")]
        public DateOnly? LastReviewDt { get; set; }

        [Column("next_review_dt")]
        public DateOnly? NextReviewDt { get; set; }

        [Column("sex_cd"), MaxLength(1)]
        public string? SexCd { get; set; }

        [Column("marital_cd"), MaxLength(10)]
        public string? MaritalCd { get; set; }

        [Column("elig_auto_pay_fl")]
        public bool? EligAutoPayFl { get; set; }

        [Column("email_id"), MaxLength(100)]
        public string? EmailId { get; set; }

        [Column("home_email_id"), MaxLength(100)]
        public string? HomeEmailId { get; set; }

        [Column("mgr_empl_id"), MaxLength(20)]
        public string? MgrEmplId { get; set; }

        [Column("s_race_cd"), MaxLength(10)]
        public string? SRaceCd { get; set; }

        [Column("pr_serv_empl_id"), MaxLength(20)]
        public string? PrServEmplId { get; set; }

        [Column("county_name"), MaxLength(100)]
        public string? CountyName { get; set; }

        [Column("ts_pd_reg_hrs_no", TypeName = "numeric(6,2)")]
        public decimal? TsPdRegHrsNo { get; set; }

        [Column("pay_pd_reg_hrs_no", TypeName = "numeric(6,2)")]
        public decimal? PayPdRegHrsNo { get; set; }

        [Column("disabled_fl")]
        public bool? DisabledFl { get; set; }

        [Column("mos_review_no")]
        public int? MosReviewNo { get; set; }

        [Column("cont_name_1"), MaxLength(100)]
        public string? ContName1 { get; set; }

        [Column("cont_name_2"), MaxLength(100)]
        public string? ContName2 { get; set; }

        [Column("cont_phone_1"), MaxLength(20)]
        public string? ContPhone1 { get; set; }

        [Column("cont_phone_2"), MaxLength(20)]
        public string? ContPhone2 { get; set; }

        [Column("cont_rel_1"), MaxLength(50)]
        public string? ContRel1 { get; set; }

        [Column("cont_rel_2"), MaxLength(50)]
        public string? ContRel2 { get; set; }

        [Column("union_empl_fl")]
        public bool? UnionEmplFl { get; set; }

        [Column("visa_type_cd"), MaxLength(20)]
        public string? VisaTypeCd { get; set; }

        [Column("vet_status_s")]
        public bool? VetStatusS { get; set; }

        [Column("vet_status_v")]
        public bool? VetStatusV { get; set; }

        [Column("vet_status_o")]
        public bool? VetStatusO { get; set; }

        [Column("vet_status_r")]
        public bool? VetStatusR { get; set; }

        [Column("ess_pin_id"), MaxLength(50)]
        public string? EssPinId { get; set; }

        [Column("pin_updated_fl")]
        public bool? PinUpdatedFl { get; set; }

        [Column("s_ess_cos_cd"), MaxLength(10)]
        public string? SEssCosCd { get; set; }

        [Column("rowversion")]
        public byte[]? Rowversion { get; set; }

        [Column("vet_release_dt")]
        public DateTime? VetReleaseDt { get; set; }

        [Column("contractor_fl")]
        public bool? ContractorFl { get; set; }

        [Column("blind_fl")]
        public bool? BlindFl { get; set; }

        [Column("visa_dt")]
        public DateOnly? VisaDt { get; set; }

        [Column("vet_status_d")]
        public bool? VetStatusD { get; set; }

        [Column("vet_status_a")]
        public bool? VetStatusA { get; set; }

        [Column("time_entry_type"), MaxLength(20)]
        public string? TimeEntryType { get; set; }

        [Column("badge_group"), MaxLength(20)]
        public string? BadgeGroup { get; set; }

        [Column("badge_id"), MaxLength(20)]
        public string? BadgeId { get; set; }

        [Column("login_id"), MaxLength(50)]
        public string? LoginId { get; set; }

        [Column("sft_fl")]
        public bool? SftFl { get; set; }

        [Column("mes_fl")]
        public bool? MesFl { get; set; }

        [Column("clock_fl")]
        public bool? ClockFl { get; set; }

        [Column("plant_id"), MaxLength(20)]
        public string? PlantId { get; set; }

        [Column("empl_source_cd"), MaxLength(10)]
        public string? EmplSourceCd { get; set; }

        [Column("sr_export_dt")]
        public DateOnly? SrExportDt { get; set; }

        [Column("hrsmart_export_dt")]
        public DateOnly? HrsmartExportDt { get; set; }

        [Column("vet_status_p")]
        public bool? VetStatusP { get; set; }

        [Column("birth_city_name"), MaxLength(100)]
        public string? BirthCityName { get; set; }

        [Column("birth_mail_state_dc"), MaxLength(10)]
        public string? BirthMailStateDc { get; set; }

        [Column("birth_country_cd"), MaxLength(5)]
        public string? BirthCountryCd { get; set; }

        [Column("user_login_id"), MaxLength(50)]
        public string? UserLoginId { get; set; }

        [Column("empl_auth_mthd"), MaxLength(20)]
        public string? EmplAuthMthd { get; set; }

        [Column("ess_user_fl")]
        public bool? EssUserFl { get; set; }

        [Column("last_day_dt")]
        public DateOnly? LastDayDt { get; set; }

        [Column("govwiniq_login_id"), MaxLength(50)]
        public string? GovwiniqLoginId { get; set; }

        [Column("hua_id"), MaxLength(20)]
        public string? HuaId { get; set; }

        [Column("hua_actv_map_fl")]
        public bool? HuaActvMapFl { get; set; }

        [Column("vet_status_np")]
        public bool? VetStatusNp { get; set; }

        [Column("vet_status_declined")]
        public bool? VetStatusDeclined { get; set; }

        [Column("vet_status_rs")]
        public bool? VetStatusRs { get; set; }
    }

    [Table("empl_lab_info")]
    public class EmplLabInfo
    {
        
        [Column("empl_id"), MaxLength(20)]
        public string? EmplId { get; set; }

        [Column("effect_dt")]
        public DateTime? EffectDt { get; set; }

        [Column("s_hrly_sal_cd"), MaxLength(10)]
        public string? SHrlySalCd { get; set; }

        [Column("hrly_amt", TypeName = "numeric(12,2)")]
        public decimal? HrlyAmt { get; set; }

        [Column("sal_amt", TypeName = "numeric(12,2)")]
        public decimal? SalAmt { get; set; }

        [Column("annl_amt", TypeName = "numeric(14,2)")]
        public decimal? AnnlAmt { get; set; }

        [Column("exmpt_fl"), MaxLength(2)]
        public string? ExmptFl { get; set; }

        [Column("s_empl_type_cd"), MaxLength(10)]
        public string? SEmplTypeCd { get; set; }

        [Column("org_id"), MaxLength(20)]
        public string? OrgId { get; set; }

        [Column("title_desc"), MaxLength(100)]
        public string? TitleDesc { get; set; }

        [Column("work_state_cd"), MaxLength(5)]
        public string? WorkStateCd { get; set; }

        [Column("std_est_hrs", TypeName = "numeric(6,2)")]
        public decimal? StdEstHrs { get; set; }

        [Column("std_effect_amt", TypeName = "numeric(12,2)")]
        public decimal? StdEffectAmt { get; set; }

        [Column("lab_grp_type"), MaxLength(10)]
        public string? LabGrpType { get; set; }

        [Column("genl_lab_cat_cd"), MaxLength(10)]
        public string? GenlLabCatCd { get; set; }

        [Column("modified_by1"), MaxLength(50)]
        public string? ModifiedBy1 { get; set; }

        [Column("time_stamp1")]
        public DateTime? TimeStamp1 { get; set; }

        [Column("pct_incr_rt", TypeName = "numeric(5,2)")]
        public decimal? PctIncrRt { get; set; }

        [Column("home_ref1_id"), MaxLength(20)]
        public string? HomeRef1Id { get; set; }

        [Column("home_ref2_id"), MaxLength(20)]
        public string? HomeRef2Id { get; set; }

        [Column("reason_desc")]
        public string? ReasonDesc { get; set; }

        [Column("detl_job_cd"), MaxLength(20)]
        public string? DetlJobCd { get; set; }

        [Column("pers_act_rsn_cd"), MaxLength(10)]
        public string? PersActRsnCd { get; set; }

        [Column("lab_loc_cd"), MaxLength(20)]
        public string? LabLocCd { get; set; }

        [Column("merit_pct_rt", TypeName = "numeric(5,2)")]
        public decimal? MeritPctRt { get; set; }

        [Column("promo_pct_rt", TypeName = "numeric(5,2)")]
        public decimal? PromoPctRt { get; set; }

        [Column("comp_plan_cd"), MaxLength(20)]
        public string? CompPlanCd { get; set; }

        [Column("sal_grade_cd"), MaxLength(10)]
        public string? SalGradeCd { get; set; }

        [Column("s_step_no"), MaxLength(5)]
        public string? SStepNo { get; set; }

        [Column("review_form_id"), MaxLength(20)]
        public string? ReviewFormId { get; set; }

        [Column("overall_rt"), MaxLength(10)]
        public string? OverallRt { get; set; }

        [Column("mgr_empl_id1"), MaxLength(20)]
        public string? MgrEmplId1 { get; set; }

        [Column("end_dt")]
        public DateTime? EndDt { get; set; }

        [Column("sec_org_id"), MaxLength(20)]
        public string? SecOrgId { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("empl_class_cd"), MaxLength(30)]
        public string? EmplClassCd { get; set; }

        [Column("work_yr_hrs_no", TypeName = "numeric(6,2)")]
        public decimal? WorkYrHrsNo { get; set; }

        [Column("bill_lab_cat_cd"), MaxLength(10)]
        public string? BillLabCatCd { get; set; }

        [Column("pers_act_rsn_cd_2"), MaxLength(10)]
        public string? PersActRsnCd2 { get; set; }

        [Column("pers_act_rsn_cd_3"), MaxLength(10)]
        public string? PersActRsnCd3 { get; set; }

        [Column("reason_desc_2")]
        public string? ReasonDesc2 { get; set; }

        [Column("reason_desc_3")]
        public string? ReasonDesc3 { get; set; }

        [Column("corp_ofcr_fl")]
        public bool? CorpOfcrFl { get; set; }

        [Column("season_empl_fl")]
        public bool? SeasonEmplFl { get; set; }

        [Column("hire_dt_fl")]
        public bool? HireDtFl { get; set; }

        [Column("term_dt_fl")]
        public bool? TermDtFl { get; set; }

        [Column("aff_plan_cd"), MaxLength(20)]
        public string? AffPlanCd { get; set; }

        [Column("job_group_cd"), MaxLength(10)]
        public string? JobGroupCd { get; set; }

        [Column("aa_comments")]
        public string? AaComments { get; set; }

        [Column("tc_ts_sched_cd"), MaxLength(20)]
        public string? TcTsSchedCd { get; set; }

        [Column("tc_work_sched_cd"), MaxLength(20)]
        public string? TcWorkSchedCd { get; set; }

        [Column("rowversion1")]
        public byte[]? Rowversion1 { get; set; }

        [Column("hr_org_id"), MaxLength(20)]
        public string? HrOrgId { get; set; }

        [Column("variable_hrs_fl")]
        public bool? VariableHrsFl { get; set; }

        [Column("dflt_rt_grp_id"), MaxLength(20)]
        public string? DfltRtGrpId { get; set; }

        [Column("trn_crncy_cd"), MaxLength(5)]
        public string? TrnCrncyCd { get; set; }

        [Column("spvsr_empl_id"), MaxLength(20)]
        public string? SpvsrEmplId { get; set; }

        [Column("req_no"), MaxLength(20)]
        public string? ReqNo { get; set; }

        [Column("ca_remote_worker"), MaxLength(10)]
        public string? CaRemoteWorker { get; set; }
    }
}
