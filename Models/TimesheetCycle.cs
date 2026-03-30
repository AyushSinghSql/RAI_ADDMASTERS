namespace PlanningAPI.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("timesheet_cycle")]
    public class TimesheetCycle
    {
        [Key]
        [Column("timesheet_cycle_id")]
        [MaxLength(10)]
        public string TimesheetCycleId { get; set; }

        [Column("description")]
        [MaxLength(100)]
        [Required]
        public string Description { get; set; }

        [Column("frequency")]
        [Required]
        public string Frequency { get; set; } 

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_by")]
        [MaxLength(50)]
        [Required]
        public string CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_by")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class TimesheetCycleDto
    {
        public string TimesheetCycleId { get; set; }
        public string Description { get; set; }
        public string Frequency { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }

    public static class TimesheetCycleExtensions
    {
        public static TimesheetCycle ToEntity(this TimesheetCycleDto dto, string user)
        {
            return new TimesheetCycle
            {
                TimesheetCycleId = dto.TimesheetCycleId,
                Description = dto.Description,
                Frequency = dto.Frequency,
                IsActive = dto.IsActive,
                CreatedBy = user,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static TimesheetCycleDto ToDto(this TimesheetCycle entity)
        {
            return new TimesheetCycleDto
            {
                TimesheetCycleId = entity.TimesheetCycleId,
                Description = entity.Description,
                Frequency = entity.Frequency,
                IsActive = entity.IsActive
            };
        }

        public static void UpdateEntity(this TimesheetCycle entity, TimesheetCycleDto dto, string user)
        {
            entity.Description = dto.Description;
            entity.Frequency = dto.Frequency;
            entity.IsActive = dto.IsActive;
            entity.UpdatedBy = user;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}