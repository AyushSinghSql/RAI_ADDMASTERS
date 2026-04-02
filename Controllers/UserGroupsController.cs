using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PlanningAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Npgsql;
    using PlanningAPI.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class UserGroupsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public UserGroupsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.UserGroups
                .Include(x => x.Company)
                .Select(x => new UserGroupDto
                {
                    UserGroupId = x.UserGroupId,
                    OrgGroupName = x.OrgGroupName,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.CompanyName,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("GetAllUserGroups")]
        public async Task<IActionResult> GetAllUserGroups(string CompanyId)
        {
            var groups = await _context.UserGroups
                .Where(x => x.CompanyId == CompanyId)
                .ToListAsync();

            var userMappings = await _context.UserGroupSetups
                .Include(x => x.User)
                .Where(x => x.CompanyId == CompanyId)
                .ToListAsync();

            var result = groups.Select(g => new
            {
                g.UserGroupId,
                g.OrgGroupName,
                g.CompanyId,
                g.CreatedAt,

                Users = userMappings
                    .Where(x => x.UserGroupId == g.UserGroupId)
                    .Select(x => new
                    {
                        x.UserId,
                        x.User.Username,
                        x.User.FullName,
                        x.ModuleCd,
                        x.CompanyId
                    })
                    .Distinct()
                    .ToList()
            });

            return Ok(result);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.UserGroups
                .Include(x => x.Company)
                .Where(x => x.UserGroupId == id)
                .Select(x => new UserGroupDto
                {
                    UserGroupId = x.UserGroupId,
                    OrgGroupName = x.OrgGroupName,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.CompanyName,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "User group not found." });

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(UserGroupDto dto)
        {
            // 🔍 Validation
            if (string.IsNullOrWhiteSpace(dto.UserGroupId))
                return BadRequest(new { message = "UserGroupId is required." });

            if (string.IsNullOrWhiteSpace(dto.OrgGroupName))
                return BadRequest(new { message = "OrgGroupName is required." });

            if (string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest(new { message = "CompanyId is required." });

            // 🔍 Duplicate Check
            var exists = await _context.UserGroups
                .AnyAsync(x => x.UserGroupId == dto.UserGroupId);

            if (exists)
                return BadRequest(new { message = "UserGroup already exists." });

            var entity = new UserGroup
            {
                UserGroupId = dto.UserGroupId,
                OrgGroupName = dto.OrgGroupName,
                CompanyId = dto.CompanyId,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.UserGroups.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "User group created successfully." });
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UserGroupDto dto)
        {
            var entity = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.UserGroupId == id);

            if (entity == null)
                return NotFound(new { message = "User group not found." });

            if (string.IsNullOrWhiteSpace(dto.OrgGroupName))
                return BadRequest(new { message = "OrgGroupName is required." });

            entity.OrgGroupName = dto.OrgGroupName;
            entity.CompanyId = dto.CompanyId;
            entity.ModifiedBy = "system";
            entity.ModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "User group updated successfully." });
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.UserGroupId == id);

            if (entity == null)
                return NotFound(new { message = "User group not found." });

            _context.UserGroups.Remove(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "User group deleted successfully." });
        }

        // ✅ Error Handling
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505")
                    return "Duplicate user group exists.";

                if (pgEx.SqlState == "23503")
                    return "Invalid company reference.";

                if (pgEx.SqlState == "23502")
                    return "A required field is missing.";
            }

            return "An error occurred while saving data.";
        }
    }
}
