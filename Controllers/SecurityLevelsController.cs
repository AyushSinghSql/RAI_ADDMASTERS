using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityLevelsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SecurityLevelsController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("security-level")]
        public async Task<IActionResult> CreateLevel(SecurityLevel dto)
        {
            if (await _context.SecurityLevels.AnyAsync(x => x.SecurityLevelCode == dto.SecurityLevelCode))
                return BadRequest("Duplicate");

            _context.Add(dto);
            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpGet("security-level")]
        public async Task<IActionResult> GetLevels()
        {
            return Ok(await _context.SecurityLevels.ToListAsync());
        }

        [HttpDelete("security-level/{code}")]
        public async Task<IActionResult> DeleteLevel(string code)
        {
            var used = await _context.SecurityClearances
                .AnyAsync(x => x.SecurityLevelCode == code);

            if (used)
                return BadRequest("Used in clearance");

            var entity = await _context.SecurityLevels.FindAsync(code);
            if (entity == null) return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown/security-level")]
        public async Task<IActionResult> LevelDropdown()
        {
            return Ok(await _context.SecurityLevels
                .Select(x => new {
                    value = x.SecurityLevelCode,
                    label = x.Description
                }).ToListAsync());
        }
    }
}
