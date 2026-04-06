using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace PlanningAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RoleId { get; set; }
        [NotMapped]
        public string? ProjectName { get; set; }

        [NotMapped]
        public string? ProjecId { get; set; }

        //public ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
        public ICollection<UserProjectMap> UserProjects { get; set; }
                = new List<UserProjectMap>();
        public ICollection<OrgGroupUserMapping> OrgGroupMappings { get; set; }
                = new List<OrgGroupUserMapping>();

        //public ICollection<UserOrgMapping> UserOrgMapping { get; set; }
        //        = new List<UserOrgMapping>();

        public Role UserRole { get; set; } = null!;

        public ICollection<UserScreenPermission> ScreenOverrides { get; set; } = new List<UserScreenPermission>();
        public ICollection<UserFieldPermission> FieldOverrides { get; set; } = new List<UserFieldPermission>();

        public ICollection<UserGroupSetup> UserGroups { get; set; }
        = new List<UserGroupSetup>();

        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();

    }

    public class UserProjectMap
    {
        public int UserId { get; set; }
        public string ProjId { get; set; } = null!;
        public DateTime AssignedAt { get; set; }

        public User User { get; set; } = null!;
        public PlProject Project { get; set; } = null!;
    }

    public class UserGroupMap
    {
        public int UserId { get; set; }
        public string ProjId { get; set; } = null!;
        public DateTime AssignedAt { get; set; }

        public User User { get; set; } = null!;
        public OrgGroup OrgGroup { get; set; } = null!;
    }

    [Table("user_group_users", Schema = "public")]
    public class UserGroupSetup
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("user_group_id")]
        public string UserGroupId { get; set; }

        [Column("module_cd")]
        public string? ModuleCd { get; set; }

        [Column("company_id")]
        public string CompanyId { get; set; }

        // 🔗 Navigation
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        //[ForeignKey(nameof(ModuleCd))]
        //public Module Module { get; set; }

        [ForeignKey(nameof(UserGroupId))]
        public UserGroup UserGroup { get; set; }
    }
    public class OrgGroupUserMapping
    {
        public int OrgGroupId { get; set; }
        public int UserId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? AssignedAt { get; set; }
        public string? AssignedBy { get; set; }

        public OrgGroup OrgGroup { get; set; }
        public User User { get; set; }
    }

    //[Table("user_org_mapping", Schema = "public")]
    //public class UserOrgMapping
    //{
    //    public string OrgId { get; set; }
    //    public int UserId { get; set; }
    //    public PlOrgnization Orgnization { get; set; }
    //    public User User { get; set; }
    //}



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class UserConfiguration
    {
        public int UserId { get; set; }
        public Visibility Visibility { get; set; }
        public string configType { get; set; }
    }

    public class Visibility
    {
        public bool projectHours { get; set; }

        [JsonProperty("projectHours.idType")]
        public bool projectHoursidType { get; set; }

        [JsonProperty("projectHours.emplId")]
        public bool projectHoursemplId { get; set; }

        [JsonProperty("projectHours.warning")]
        public bool projectHourswarning { get; set; }

        [JsonProperty("projectHours.name")]
        public bool projectHoursname { get; set; }

        [JsonProperty("projectHours.acctId")]
        public bool projectHoursacctId { get; set; }

        [JsonProperty("projectHours.acctName")]
        public bool projectHoursacctName { get; set; }

        [JsonProperty("projectHours.orgId")]
        public bool projectHoursorgId { get; set; }

        [JsonProperty("projectHours.glcPlc")]
        public bool projectHoursglcPlc { get; set; }

        [JsonProperty("projectHours.isRev")]
        public bool projectHoursisRev { get; set; }

        [JsonProperty("projectHours.isBrd")]
        public bool projectHoursisBrd { get; set; }

        [JsonProperty("projectHours.status")]
        public bool projectHoursstatus { get; set; }

        [JsonProperty("projectHours.perHourRate")]
        public bool projectHoursperHourRate { get; set; }

        [JsonProperty("projectHours.total")]
        public bool projectHourstotal { get; set; }
        public bool projectAmounts { get; set; }

        [JsonProperty("projectAmounts.idType")]
        public bool projectAmountsidType { get; set; }

        [JsonProperty("projectAmounts.emplId")]
        public bool projectAmountsemplId { get; set; }

        [JsonProperty("projectAmounts.name")]
        public bool projectAmountsname { get; set; }

        [JsonProperty("projectAmounts.acctId")]
        public bool projectAmountsacctId { get; set; }

        [JsonProperty("projectAmounts.acctName")]
        public bool projectAmountsacctName { get; set; }

        [JsonProperty("projectAmounts.orgId")]
        public bool projectAmountsorgId { get; set; }

        [JsonProperty("projectAmounts.isRev")]
        public bool projectAmountsisRev { get; set; }

        [JsonProperty("projectAmounts.isBrd")]
        public bool projectAmountsisBrd { get; set; }

        [JsonProperty("projectAmounts.status")]
        public bool projectAmountsstatus { get; set; }

        [JsonProperty("projectAmounts.total")]
        public bool projectAmountstotal { get; set; }
    }




    public class BulkProjectUserToggleRequest
    {
        public string ProjId { get; set; } = null!;
        public List<int> UserIds { get; set; } = new();
    }
    public class BulkUserGroupsToggleRequest
    {
        public int UserId { get; set; }
        public List<int> GroupIds { get; set; } = new();
    }

    [Table("user_groups", Schema = "public")]
    public class UserGroup
    {
        [Key]
        [Column("user_group_id")]
        [MaxLength(50)]
        public string UserGroupId { get; set; }

        [Column("org_group_name")]
        [MaxLength(150)]
        public string OrgGroupName { get; set; }

        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_by")]
        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        [Column("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        // 🔗 Navigation
        public Company Company { get; set; }

        public ICollection<UserGroupSetup> Users { get; set; }
        = new List<UserGroupSetup>();
        public ICollection<ModuleRights>? ModuleRights { get; set; }
        public ICollection<UserGroupScreenPermission>? ScreenPermissions { get; set; }
    }

    [Table("usergroup_screen_permissions", Schema = "public")]
    public class UserGroupScreenPermission
    {
        [Column("user_group_id")]
        [MaxLength(50)]
        public string UserGroupId { get; set; } = null!;

        [Column("company_id")]
        [MaxLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        [Column("screen_code")]
        [MaxLength(100)]
        public string ScreenCode { get; set; } = null!;

        [Column("can_view")]
        public bool? CanView { get; set; }

        [Column("can_edit")]
        public bool? CanEdit { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        //// 🔗 Navigation Property
        //public UserGroup? UserGroup { get; set; }
    }

    public class UserGroupScreenPermissionBulkDto
    {
        public string UserGroupId { get; set; } = null!;
        public string ScreenCode { get; set; } = null!;
        public bool? CanView { get; set; }
        public bool? CanEdit { get; set; }
        public string CompanyId { get; set; }
        public string CreatedBy { get; set; }

    }

    public class AddUserToGroupDto
    {
        public int UserId { get; set; }
        public string UserGroupId { get; set; }
        public string CompanyId { get; set; }
    }

    public class UserWithGroupsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public List<UserGroupDto> Groups { get; set; }
    }

    public class userGroupWithUsersDto
    {
        public string UserGroupCD { get; set; }
        public string UserGroupName { get; set; }
        public List<UserGroupDto> Groups { get; set; }
    }

    public class UserGroupDto
    {
        public string UserGroupId { get; set; }
        public string OrgGroupName { get; set; }
        public string CompanyId { get; set; }

        public string CompanyName { get; set; } // 🔥 join field
        public DateTime CreatedAt { get; set; }

        public List<User>? Users { get; set; }
    }

    public class UserGroupSetupDTO
    {
        public int UserId { get; set; }

        public string UserGroupId { get; set; }

        public string? ModuleCd { get; set; }

        public string CompanyId { get; set; }

    }

    [Table("user_favorites")]
    public class UserFavorite
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }

}
