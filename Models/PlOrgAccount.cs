using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    //public class OrgAccount
    //{
    //    [Key, Column(Order = 0)]
    //    [MaxLength(20)]
    //    public string OrgId { get; set; }

    //    [Key, Column(Order = 1)]
    //    [MaxLength(20)]
    //    public string AcctId { get; set; }

    //    [MaxLength(30)]
    //    public string? AccType { get; set; }

    //    public bool? ActiveFl { get; set; }

    //    [MaxLength(50)]
    //    public string? ModifiedBy { get; set; }

    //    public DateTime TimeStamp { get; set; }

    //    [JsonIgnore]
    //    public ICollection<PlPoolRate>? PlPoolRates { get; set; }
    //    [JsonIgnore]
    //    public Account? Account { get; set; }
    //    [JsonIgnore]
    //    public Organization? Organization { get; set; }
    //}

    public class OrgAccount
    {
        [Key, Column("org_id", Order = 0)]
        [MaxLength(20)]
        public string OrgId { get; set; }

        [Key, Column("acct_id", Order = 1)]
        [MaxLength(20)]
        public string AcctId { get; set; }

        [Column("acc_type")]
        [MaxLength(30)]
        public string? AccType { get; set; }

        [Column("active_fl")]
        public bool? ActiveFl { get; set; }

        [Column("fy_cd_fr")]
        [MaxLength(10)]
        public string? FyCdFr { get; set; }

        [Column("pd_no_fr")]
        public int? PdNoFr { get; set; }

        [Column("fy_cd_to")]
        [MaxLength(10)]
        public string? FyCdTo { get; set; }

        [Column("pd_no_to")]
        public int? PdNoTo { get; set; }

        [Column("rq_appr_proc_cd")]
        [MaxLength(20)]
        public string? RqApprProcCd { get; set; }

        [Column("modified_by")]
        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        // ✅ Concurrency Token
        [Column("rowversion")]
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation Properties
        [JsonIgnore]
        public ICollection<PlPoolRate>? PlPoolRates { get; set; }

        [JsonIgnore]
        [ForeignKey("AcctId")]
        public Account? Account { get; set; }

        [JsonIgnore]
        [ForeignKey("OrgId")]
        public Organization? Organization { get; set; }
    }

}
