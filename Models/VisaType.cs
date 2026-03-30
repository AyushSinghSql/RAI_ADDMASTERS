namespace PlanningAPI.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("visa_type")]
    public class VisaType
    {
        [Key]
        [Column("visa_type_code")]
        [MaxLength(10)]
        public string VisaTypeCode { get; set; }

        [Column("description")]
        [MaxLength(255)]
        [Required]
        public string Description { get; set; }

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

    public class VisaTypeDto
    {
        public string VisaTypeCode { get; set; }
        public string Description { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }

    public static class VisaTypeExtensions
    {
        public static VisaType ToEntity(this VisaTypeDto dto, string user)
        {
            return new VisaType
            {
                VisaTypeCode = dto.VisaTypeCode,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedBy = user,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static VisaTypeDto ToDto(this VisaType entity)
        {
            return new VisaTypeDto
            {
                VisaTypeCode = entity.VisaTypeCode,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        public static void UpdateEntity(this VisaType entity, VisaTypeDto dto, string user)
        {
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
            entity.UpdatedBy = user;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}


