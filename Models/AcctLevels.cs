using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("acct_levels", Schema = "public")]
    public class AcctLevel
    {
        [Key]
        [Column("level")]
        public int Level { get; set; }

        [Required]
        [Column("lenght")] // matches DB spelling
        public int Lenght { get; set; }
    }

    [Table("org_levels", Schema = "public")]
    public class OrgLevel
    {
        [Key]
        [Column("level")]
        public int Level { get; set; }

        [Required]
        [Column("lenght")] // matches DB spelling
        public int Lenght { get; set; }
    }
}
