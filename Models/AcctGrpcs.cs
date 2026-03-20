using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PlanningAPI.Models
{
    [Table("acct_grp")]
    public class AcctGrp
    {
        [Column("acct_grp_cd")]
        [MaxLength(3)]
        public string AcctGrpCd { get; set; }

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("acct_grp_desc")]
        [MaxLength(30)]
        public string AcctGrpDesc { get; set; }

        [Column("modified_by")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? RowVersion { get; set; }

        //// 🔗 Navigation (optional)
        //public Company? Company { get; set; }
    }
}
