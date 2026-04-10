using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("vendor_employee")]
    public class VendorEmployee
    {
        [Key, Column("vend_empl_id", Order = 0)]
        [MaxLength(50)]
        public string VendEmplId { get; set; }

        [Key, Column("vend_id", Order = 1)]
        [MaxLength(50)]
        public string? VendId { get; set; }

        [Column("company_id", Order = 2)]
        [MaxLength(50)]
        public string? CompanyId { get; set; }

        [Column("df_genl_lab_cat_cd")]
        [MaxLength(20)]
        public string? DfGenlLabCatCd { get; set; }

        [Column("modified_by")]
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("rowversion")]
        public int? Rowversion { get; set; } = 1;

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; } = DateTime.UtcNow;

        [Column("vend_empl_name")]
        [MaxLength(100)]
        public string? VendEmplName { get; set; }

        [Column("df_bill_lab_cat_cd")]
        [MaxLength(20)]
        public string? DfBillLabCatCd { get; set; }

        [Column("last_name")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Column("first_name")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Column("mid_name")]
        [MaxLength(50)]
        public string? MidName { get; set; }

        [Column("vend_empl_status")]
        [MaxLength(20)]
        public string? VendEmplStatus { get; set; }

        [Column("subctr_id")]
        [MaxLength(50)]
        public string? SubctrId { get; set; }

        [Column("te_empl_id")]
        [MaxLength(50)]
        public string? TeEmplId { get; set; }

        [Column("vend_empl_aprvr_id")]
        [MaxLength(50)]
        public string? VendEmplAprvrId { get; set; }

        [Column("vend_empl_aprvl_dt")]
        public DateTime? VendEmplAprvlDt { get; set; }

        [Column("vend_empl_aprvl_cd")]
        [MaxLength(20)]
        public string? VendEmplAprvlCd { get; set; }

        [Column("int_email")]
        [MaxLength(100)]
        public string? IntEmail { get; set; }

        [Column("ext_email")]
        [MaxLength(100)]
        public string? ExtEmail { get; set; }

        [Column("int_phone")]
        [MaxLength(20)]
        public string? IntPhone { get; set; }

        [Column("ext_phone")]
        [MaxLength(20)]
        public string? ExtPhone { get; set; }

        [Column("cell_phone")]
        [MaxLength(20)]
        public string? CellPhone { get; set; }

        [Column("cont_1_name")]
        [MaxLength(100)]
        public string? Cont1Name { get; set; }

        [Column("cont_1_rel")]
        [MaxLength(50)]
        public string? Cont1Rel { get; set; }

        [Column("cont_1_phone_1")]
        [MaxLength(20)]
        public string? Cont1Phone1 { get; set; }

        [Column("cont_1_phone_2")]
        [MaxLength(20)]
        public string? Cont1Phone2 { get; set; }

        [Column("cont_1_phone_3")]
        [MaxLength(20)]
        public string? Cont1Phone3 { get; set; }

        [Column("cont_2_name")]
        [MaxLength(100)]
        public string? Cont2Name { get; set; }

        [Column("cont_2_rel")]
        [MaxLength(50)]
        public string? Cont2Rel { get; set; }

        [Column("cont_2_phone_1")]
        [MaxLength(20)]
        public string? Cont2Phone1 { get; set; }

        [Column("cont_2_phone_2")]
        [MaxLength(20)]
        public string? Cont2Phone2 { get; set; }

        [Column("cont_2_phone_3")]
        [MaxLength(20)]
        public string? Cont2Phone3 { get; set; }

        [Column("us_citizen_fl")]
        public bool? UsCitizenFl { get; set; }

        [Column("itar_status")]
        [MaxLength(20)]
        public string? ItarStatus { get; set; }

        //// ✅ Missing column added
        //[Column("sp_created")]
        //[MaxLength(1)]
        //public string? SpCreated { get; set; }
    }

}
