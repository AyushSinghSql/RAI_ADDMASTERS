using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("race")]
    public class Race
    {

        [Key]
        [Column("race_id")]
        [MaxLength(100)]
        public string RaceId { get; set; }

        [Column("race_description")]
        [MaxLength(255)]
        [Required]
        public string Description { get; set; }

        [Column("active_fl")]
        [Required]
        public string ActiveFlag { get; set; } = "Y";

        [Column("created_by")]
        [MaxLength(100)]
        [Required]
        public string CreatedBy { get; set; }

        [Column("modified_by")]
        [MaxLength(100)]
        [Required]
        public string ModifiedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("modified_at")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    }

    public class RaceDto
    {
        public string RaceId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ActiveFlag { get; set; } = "Y";
    }

    public static class RaceExtension 
    {
        public static Race ToEntity(this RaceDto dto, string user)
        {
            return new Race 
            {
                RaceId = dto.RaceId,
                Description = dto.Description,
                ActiveFlag = dto.ActiveFlag,
                CreatedBy = user,
                ModifiedBy = user,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
        }

        public static RaceDto ToDto(this Race entity)
        {
            return new RaceDto
            {
                RaceId = entity.RaceId,
                Description = entity.Description,
                ActiveFlag = entity.ActiveFlag
            };
        }
    }

}
