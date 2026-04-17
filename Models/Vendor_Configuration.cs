using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("vendor_certification_setup")]
    public class VendorCertificationSetup
    {
        [Key]
        [Column("cert_cd")]
        public string CertCode { get; set; }

        [Column("cert_name")]
        public string CertName { get; set; }

        [Column("show_lookup_fl")]
        public string ShowLookupFl { get; set; }

        [Column("prime_agency_id")]
        public string? PrimeAgencyId { get; set; }

        [Column("prof_org_id")]
        public string? ProfOrgId { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateOnly TimeStamp { get; set; }

        [Column("row_version")]
        public decimal? RowVersion { get; set; }
    }

    [Table("certification_levels")]
    public class CertificationLevel
    {
        [Column("cert_level_cd")]
        public string CertLevelCd { get; set; }

        [Column("cert_cd")]
        public string CertCd { get; set; }

        [Column("cert_level_desc")]
        public string CertLevelDesc { get; set; }

        [Column("show_lookup_fl")]
        public string ShowLookupFl { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }

        // Navigation
        public VendorCertificationSetup? Certification { get; set; }
    }

    [Table("certification_status")]
    public class CertificationStatus
    {
        [Column("cert_status_cd")]
        public string CertStatusCd { get; set; }

        [Column("cert_cd")]
        public string CertCd { get; set; }

        [Column("cert_status_desc")]
        public string CertStatusDesc { get; set; }

        [Column("show_lookup_fl")]
        public string ShowLookupFl { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("rowversion")]
        public decimal? Rowversion { get; set; }

        // Navigation
        public VendorCertificationSetup? Certification { get; set; }
    }

    public class DropdownDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
