using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PlanningAPI.Models
{

    [Table("acct_grp_cd", Schema = "public")]
    public class AcctGrpCd
    {
        [Column("acct_grp_cd")]
        [MaxLength(3)]
        public string AcctGrpCode { get; set; } = null!;

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("acct_grp_desc")]
        [MaxLength(30)]
        public string AcctGrpDesc { get; set; } = null!;

        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }

        [Column("active_fl")]
        [MaxLength(1)]
        public string ActiveFl { get; set; } = "Y";
    }
}
