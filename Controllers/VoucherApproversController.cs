using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/voucher-approvers")]
    public class VoucherApproversController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VoucherApproversController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ 1. Add Approver
        [HttpPost]
        public async Task<IActionResult> AddApprover(AddVoucherApproverDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest("UserId and CompanyId are required.");

            // ✅ Ensure user exists
            var userExists = await _context.Users
                .AnyAsync(x => x.Username == dto.UserId);

            if (!userExists)
                return BadRequest($"User '{dto.UserId}' does not exist.");

            // ✅ Prevent duplicate
            var exists = await _context.VoucherApprovers
                .AnyAsync(x => x.UserId == dto.UserId && x.CompanyId == dto.CompanyId);

            if (exists)
                return Conflict("Approver already exists for this company.");

            var entity = new VoucherApprover
            {
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                ModifiedBy = User?.Identity?.Name ?? "system",
                ModifiedTs = DateTime.UtcNow
            };

            _context.VoucherApprovers.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ 2. Remove Approver
        [HttpDelete]
        public async Task<IActionResult> RemoveApprover(string userId, string companyId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(companyId))
                return BadRequest("UserId and CompanyId are required.");

            var entity = await _context.VoucherApprovers
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CompanyId == companyId);

            if (entity == null)
                return NotFound("Approver not found.");

            _context.VoucherApprovers.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Approver removed successfully.");
        }

        // ✅ 3. Get Approvers by Company
        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetByCompany(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId is required.");

            var data = await _context.VoucherApprovers
                .Where(x => x.CompanyId == companyId)
                .Select(x => new VoucherApproverDto
                {
                    UserId = x.UserId,
                    CompanyId = x.CompanyId
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
