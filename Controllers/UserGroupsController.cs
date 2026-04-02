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

        // ✅ UPSERT (for user group setup)
        [HttpPost("UserGroupSetup/upsert")]
        public async Task<IActionResult> Upsert(UserGroupSetupDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserGroupId))
                return BadRequest(new { message = "UserGroupId is required." });

            if (dto.UserId == 0)
                return BadRequest(new { message = "UserId is required." });

            if (string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest(new { message = "CompanyId is required." });

            var existing = await _context.UserGroupSetups
                .FirstOrDefaultAsync(x =>
                    x.UserGroupId == dto.UserGroupId &&
                    x.UserId == dto.UserId &&
                    x.CompanyId == dto.CompanyId &&
                    x.ModuleCd == dto.ModuleCd);

            if (existing != null)
            {
                // ✅ UPDATE (if you want to update anything later)
                // currently nothing to update, but keeping for future
                _context.UserGroupSetups.Update(existing);

                return Ok(new { message = "Already exists. Updated successfully." });
            }
            else
            {
                // ✅ INSERT
                var entity = new UserGroupSetup
                {
                    UserGroupId = dto.UserGroupId,
                    UserId = dto.UserId,
                    CompanyId = dto.CompanyId,
                    ModuleCd = dto.ModuleCd
                };

                _context.UserGroupSetups.Add(entity);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User group mapping upserted successfully." });
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

        // ✅ CREATE
        [HttpPost("UserGroupSetup")]
        public async Task<IActionResult> UserGroupSetup(UserGroupSetupDTO dto)
        {
            // 🔍 Validation
            if (string.IsNullOrWhiteSpace(dto.UserGroupId))
                return BadRequest(new { message = "UserGroupId is required." });

            if (dto.UserId == 0)
                return BadRequest(new { message = "UserId is required." });

            if (string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest(new { message = "CompanyId is required." });

            // 🔍 Duplicate Check
            var exists = await _context.UserGroupSetups
                .AnyAsync(x => x.UserGroupId == dto.UserGroupId && x.UserId == dto.UserId && dto.CompanyId == x.CompanyId && dto.ModuleCd == x.ModuleCd);

            if (exists)
                return BadRequest(new { message = "User Group setup already exists." });

            var entity = new UserGroupSetup
            {
                UserGroupId = dto.UserGroupId,
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                ModuleCd = dto.ModuleCd
            };

            _context.UserGroupSetups.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "User successfully mapped with user group ." });
        }


        // ✅ DELETE
        [HttpDelete("UserGroupSetup")]
        public async Task<IActionResult> Delete(UserGroupSetupDTO dto)
        {
            var existing = await _context.UserGroupSetups
                .FirstOrDefaultAsync(x =>
                    x.UserGroupId == dto.UserGroupId &&
                    x.UserId == dto.UserId &&
                    x.CompanyId == dto.CompanyId &&
                    x.ModuleCd == dto.ModuleCd);

            if (existing == null)
                return NotFound(new { message = "Mapping not found." });

            _context.UserGroupSetups.Remove(existing);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Mapping deleted successfully." });
        }

        // ✅ Bulk SYNC
        [HttpPost("UserGroupSetup/sync")]
        public async Task<IActionResult> Sync(List<UserGroupSetupDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided.");

            var groupId = dtos.First().UserGroupId;
            var companyId = dtos.First().CompanyId;

            var existing = await _context.UserGroupSetups
                .Where(x => x.UserGroupId == groupId && x.CompanyId == companyId)
                .ToListAsync();

            var incomingKeys = dtos.Select(d =>
                $"{d.UserGroupId}|{d.UserId}|{d.CompanyId}|{d.ModuleCd}")
                .ToHashSet();

            var existingKeys = existing.Select(e =>
                $"{e.UserGroupId}|{e.UserId}|{e.CompanyId}|{e.ModuleCd}")
                .ToList();

            // 🔴 DELETE missing
            var toDelete = existing
                .Where(e => !incomingKeys.Contains(
                    $"{e.UserGroupId}|{e.UserId}|{e.CompanyId}|{e.ModuleCd}"))
                .ToList();

            // 🟢 INSERT new
            var toInsert = dtos
                .Where(d => !existingKeys.Contains(
                    $"{d.UserGroupId}|{d.UserId}|{d.CompanyId}|{d.ModuleCd}"))
                .Select(d => new UserGroupSetup
                {
                    UserGroupId = d.UserGroupId,
                    UserId = d.UserId,
                    CompanyId = d.CompanyId,
                    ModuleCd = d.ModuleCd
                })
                .ToList();

            if (toDelete.Any())
                _context.UserGroupSetups.RemoveRange(toDelete);

            if (toInsert.Any())
                await _context.UserGroupSetups.AddRangeAsync(toInsert);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Inserted = toInsert.Count,
                Deleted = toDelete.Count,
                Total = dtos.Count
            });
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
