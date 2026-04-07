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
    public class LevelDto
    {
        public int Level { get; set; }
        public int Lenght { get; set; }

        public int Count { get; set; } // 🔥 New field

    }
    public class LevelResponseDto
    {
        public bool IsEditAllowed { get; set; }
        public bool IsAddNextLevelAllowed { get; set; }

        public List<LevelDto> Levels { get; set; } = new();
    }
    //[Table("org_levels", Schema = "public")]
    //public class OrgLevel
    //{
    //    [Key]
    //    [Column("level")]
    //    public int Level { get; set; }

    //    [Required]
    //    [Column("lenght")] // matches DB spelling
    //    public int Lenght { get; set; }
    //}
}
