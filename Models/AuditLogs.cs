using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PlanningAPI.Models
{

    [Table("audit_log", Schema = "public")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("table_name")]
        [MaxLength(100)]
        public string? TableName { get; set; }

        [Column("action")]
        [MaxLength(10)]
        public string? Action { get; set; }

        [Column("key_values", TypeName = "text")]
        public string? KeyValues { get; set; }

        [Column("old_values", TypeName = "jsonb")]
        public string? OldValues { get; set; }

        [Column("new_values", TypeName = "jsonb")]
        public string? NewValues { get; set; }

        [Column("changed_columns", TypeName = "jsonb")]
        public string? ChangedColumns { get; set; }

        [Column("modified_by")]
        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        [Column("company_id")]
        [MaxLength(10)]
        public string? CompanyId { get; set; }

        [Column("request_path")]
        [MaxLength(200)]
        public string? RequestPath { get; set; }

        [Column("http_method")]
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }
    }
}
