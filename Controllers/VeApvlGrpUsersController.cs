using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeApvlGrpUsersController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VeApvlGrpUsersController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Assign(VeApvlGrpUsersDto dto)
        {
            var error = await Validate(dto);
            if (error != null)
                return BadRequest(error);

            var entity = new VeApvlGrpUsers
            {
                VeApprvlGrpCd = dto.VeApprvlGrpCd,
                ApprvrUserId = dto.ApprvrUserId,
                CompanyId = dto.CompanyId,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.VeApvlGrpUsers.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        [HttpGet("by-group")]
        public async Task<IActionResult> GetByGroup(string groupCode, string companyId)
        {
            var data = await _context.VeApvlGrpUsers
                .Where(x => x.VeApprvlGrpCd == groupCode && x.CompanyId == companyId)
                .Select(x => new
                {
                    x.ApprvrUserId
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("by-user")]
        public async Task<IActionResult> GetByUser(string userId, string companyId)
        {
            var data = await _context.VeApvlGrpUsers
                .Where(x => x.ApprvrUserId == userId && x.CompanyId == companyId)
                .Select(x => new
                {
                    x.VeApprvlGrpCd
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(string groupCode, string userId, string companyId)
        {
            var entity = await _context.VeApvlGrpUsers
                .FindAsync(groupCode, userId, companyId);

            if (entity == null)
                return NotFound();

            _context.VeApvlGrpUsers.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("dropdown/users")]
        public async Task<IActionResult> UsersDropdown(string groupCode, string companyId)
        {
            var data = await _context.VeApvlGrpUsers
                .Where(x => x.VeApprvlGrpCd == groupCode && x.CompanyId == companyId)
                .Select(x => new {
                    value = x.ApprvrUserId,
                    label = x.ApprvrUserId
                })
                .ToListAsync();

            return Ok(data);
        }

        [NonAction]
        private async Task<string> Validate(VeApvlGrpUsersDto dto, bool isUpdate = false)
        {
            if (string.IsNullOrWhiteSpace(dto.VeApprvlGrpCd))
                return "Group Code required";

            if (string.IsNullOrWhiteSpace(dto.ApprvrUserId))
                return "User required";

            if (string.IsNullOrWhiteSpace(dto.CompanyId))
                return "Company required";

            // Prevent duplicate
            if (!isUpdate)
            {
                var exists = await _context.VeApvlGrpUsers.AnyAsync(x =>
                    x.VeApprvlGrpCd == dto.VeApprvlGrpCd &&
                    x.ApprvrUserId == dto.ApprvrUserId &&
                    x.CompanyId == dto.CompanyId);

                if (exists)
                    return "Duplicate mapping";
            }

            // Prevent self-approval (if same user creating)
            if (dto.ApprvrUserId == dto.ModifiedBy)
                return "Self-approval not allowed";

            return null;
        }
    }
}
