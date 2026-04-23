using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/voucher-approver-users")]
    public class VoucherApproverUsersController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VoucherApproverUsersController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ 1. Assign users to approver (Bulk)
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUsers(AssignUsersDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ApproverUserId) ||
                string.IsNullOrWhiteSpace(dto.CompanyId) ||
                dto.UserIds == null || !dto.UserIds.Any())
                return BadRequest("Invalid input.");

            // ✅ Validate approver exists
            var approverExists = await _context.Users
                .AnyAsync(x => x.Username == dto.ApproverUserId);

            if (!approverExists)
                return BadRequest($"Approver '{dto.ApproverUserId}' does not exist.");

            // ✅ Fetch valid users
            var validUsers = await _context.Users
                .Where(x => dto.UserIds.Contains(x.Username))
                .Select(x => x.Username)
                .ToListAsync();

            var invalidUsers = dto.UserIds.Except(validUsers).ToList();
            if (invalidUsers.Any())
                return BadRequest($"Invalid Users: {string.Join(", ", invalidUsers)}");

            var existingMappings = await _context.VoucherApproverUsers
                .Where(x => x.ApproverUserId == dto.ApproverUserId
                         && x.CompanyId == dto.CompanyId
                         && dto.UserIds.Contains(x.UserId))
                .Select(x => x.UserId)
                .ToListAsync();

            var newMappings = new List<VoucherApproverUser>();

            foreach (var userId in validUsers)
            {
                // ❌ Prevent self-approval
                if (userId == dto.ApproverUserId)
                    continue;

                // ❌ Prevent duplicate
                if (existingMappings.Contains(userId))
                    continue;

                newMappings.Add(new VoucherApproverUser
                {
                    ApproverUserId = dto.ApproverUserId,
                    UserId = userId,
                    CompanyId = dto.CompanyId,
                    ModifiedBy = User?.Identity?.Name ?? "system",
                    ModifiedTs = DateTime.UtcNow
                });
            }

            if (!newMappings.Any())
                return Conflict("No new mappings to insert.");

            await _context.VoucherApproverUsers.AddRangeAsync(newMappings);
            await _context.SaveChangesAsync();

            return Ok(newMappings);
        }

        // ✅ 2. Remove mapping
        [HttpDelete]
        public async Task<IActionResult> RemoveMapping(string approverUserId, string userId, string companyId)
        {
            if (string.IsNullOrWhiteSpace(approverUserId) ||
                string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(companyId))
                return BadRequest("Invalid input.");

            var entity = await _context.VoucherApproverUsers
                .FirstOrDefaultAsync(x =>
                    x.ApproverUserId == approverUserId &&
                    x.UserId == userId &&
                    x.CompanyId == companyId);

            if (entity == null)
                return NotFound("Mapping not found.");

            _context.VoucherApproverUsers.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Mapping removed.");
        }

        // ✅ 3. Get users by approver
        [HttpGet("by-approver/{approverUserId}/{companyId}")]
        public async Task<IActionResult> GetUsersByApprover(string approverUserId, string companyId)
        {
            var users = await _context.VoucherApproverUsers
                .Where(x => x.ApproverUserId == approverUserId && x.CompanyId == companyId)
                .Select(x => new ApproverUserDto
                {
                    ApproverUserId = x.ApproverUserId,
                    UserId = x.UserId,
                    CompanyId = x.CompanyId
                })
                .ToListAsync();

            return Ok(users);
        }

        // ✅ 4. Get approver by user
        [HttpGet("by-user/{userId}/{companyId}")]
        public async Task<IActionResult> GetApproverByUser(string userId, string companyId)
        {
            var approvers = await _context.VoucherApproverUsers
                .Where(x => x.UserId == userId && x.CompanyId == companyId)
                .Select(x => new ApproverUserDto
                {
                    ApproverUserId = x.ApproverUserId,
                    UserId = x.UserId,
                    CompanyId = x.CompanyId
                })
                .ToListAsync();

            return Ok(approvers);
        }
    }
}
