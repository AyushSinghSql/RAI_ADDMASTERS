
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PlanningAPI.Models
{
    [Table("udef_field", Schema = "public")]
    public class UdefField
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("table_id")]
        [MaxLength(20)]
        public string TableId { get; set; } = null!;

        [Required]
        [Column("field_name")]
        [MaxLength(50)]
        public string FieldName { get; set; } = null!;

        [Required]
        [Column("data_type")]
        [MaxLength(1)]
        public string DataType { get; set; } = null!; // T, N, D, L

        [Column("seq_no")]
        public int SeqNo { get; set; }

        [Column("is_required")]
        public bool IsRequired { get; set; }

        [Column("is_validated")]
        public bool IsValidated { get; set; }

        [Column("is_printable")]
        public bool IsPrintable { get; set; }

        [Column("is_multi_select")]
        public bool IsMultiSelect { get; set; }

        [Column("help_text")]
        public string? HelpText { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<UdefOption> Options { get; set; } = new List<UdefOption>();
    }

    [Table("udef_option", Schema = "public")]
    public class UdefOption
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Field")]
        [Column("field_id")]
        public int FieldId { get; set; }

        [Required]
        [Column("value")]
        [MaxLength(50)]
        public string Value { get; set; } = null!;

        [Required]
        [Column("label")]
        [MaxLength(100)]
        public string Label { get; set; } = null!;

        // Navigation
        public UdefField Field { get; set; } = null!;
    }

    [Table("udef_value", Schema = "public")]
    public class UdefValue
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("entity_id")]
        [MaxLength(50)]
        public string EntityId { get; set; } = null!;

        [ForeignKey("Field")]
        [Column("field_id")]
        public int FieldId { get; set; }

        [Column("value")]
        public string? Value { get; set; }

        [Column("gen_id")]
        [MaxLength(50)]
        public string? GenId { get; set; }

        [Required]
        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation
        public UdefField Field { get; set; } = null!;
    }
    //public class UdefSaveDto
    //{
    //    public int? FieldId { get; set; }
    //    public List<string> Values { get; set; } = new();
    //}

    public class UdefSaveDto
    {
        public int? FieldId { get; set; }
        public string? GenId { get; set; }
        public string? FieldName { get; set; }
        public string? DataType { get; set; }
        public bool IsMultiSelect { get; set; }
        public List<string> Values { get; set; } = new();
    }
    public class UdefSetValueDto
    {
        public int FieldId { get; set; }
        public List<string> Values { get; set; } = new();
    }

}
