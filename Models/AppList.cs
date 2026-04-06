namespace PlanningAPI.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("app_list")]
    public class AppList
    {
        [Key]
        [Column("app_id")]
        [MaxLength(30)]
        public string AppId { get; set; } = null!;

        [Column("name")]
        [MaxLength(60)]
        public string? Name { get; set; }

        [Column("app_type")]
        [MaxLength(10)]
        public string? AppType { get; set; }

        [Column("title")]
        [MaxLength(60)]
        public string? Title { get; set; }

        [Column("modified_by")]
        [MaxLength(20)]
        [Required]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        [Required]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion", TypeName = "numeric(10,0)")]
        public decimal? Rowversion { get; set; }

        [Column("org_sec_fl")]
        [MaxLength(1)]
        public string? OrgSecFl { get; set; }
    }

    public class AppListDto
    {
        public string AppId { get; set; } = null!;
        public string? Name { get; set; }
        public string? AppType { get; set; }
        public string? Title { get; set; }
        public string ModifiedBy { get; set; } = null!;
        public decimal? Rowversion { get; set; }
        public string? OrgSecFl { get; set; }
    }
}
