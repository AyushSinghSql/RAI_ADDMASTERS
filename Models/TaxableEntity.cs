using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("taxable_entity")]
    public class TaxableEntity
    {
        [Column("taxable_name")]
        [MaxLength(255)]
        [Required]
        public string TaxableName { get; set; }
        [Key]
        [Column("tax_id")]
        [MaxLength(100)]
        [Required]
        public string TaxId { get; set; }

        [Column("company_id")]
        [MaxLength(50)]
        [Required]
        public string CompanyId { get; set; }

        [Column("active_fl")]
        [Required]
        public string ActiveFlag { get; set; } = "N";

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

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

    }

    public class TaxableEntityDto
    {
        public int TaxableId { get; set; }
        public string TaxableName { get; set; } = null!;
        public string TaxId { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string ActiveFlag { get; set; } = "Y";
    }

    public static class TaxableEntityExtensions
    {
        public static TaxableEntity ToEntity(this TaxableEntityDto dto, string user)
        {
            return new TaxableEntity
            {
                TaxableName = dto.TaxableName,
                TaxId = dto.TaxId,
                CompanyId = dto.CompanyId,
                ActiveFlag = dto.ActiveFlag,
                ModifiedBy = user,
                CreatedBy = user, // For initial creation
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
        }

        public static TaxableEntityDto ToDto(this TaxableEntity entity)
        {
            return new TaxableEntityDto
            {
                TaxableName = entity.TaxableName,
                TaxId = entity.TaxId,
                CompanyId = entity.CompanyId,
                ActiveFlag = entity.ActiveFlag
            };
        }
    }
}