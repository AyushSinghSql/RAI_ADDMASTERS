using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityClearancesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SecurityClearancesController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("security-clearance")]
        public async Task<IActionResult> CreateClearance(SecurityClearance dto)
        {
            if (await _context.SecurityClearances.AnyAsync(x => x.ClearanceCode == dto.ClearanceCode))
                return BadRequest("Duplicate");

            _context.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet("security-clearance")]
        public async Task<IActionResult> GetClearances()
        {
            var data = await _context.SecurityClearances
                .Include(x => x.SecurityLevel)
                .ToListAsync();

            return Ok(data);
        }
        [HttpPut("security-clearance")]
        public async Task<IActionResult> UpdateClearance(SecurityClearance dto)
        {
            var entity = await _context.SecurityClearances
                .FindAsync(dto.ClearanceCode);

            if (entity == null)
                return NotFound();

            entity.Description = dto.Description;
            entity.SciFlag = dto.SciFlag;
            entity.SapFlag = dto.SapFlag;
            entity.HierarchyNo = dto.HierarchyNo;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }
        [HttpDelete("security-clearance/{code}")]
        public async Task<IActionResult> DeleteClearance(string code)
        {
            var entity = await _context.SecurityClearances.FindAsync(code);
            if (entity == null) return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown/security-clearance")]
        public async Task<IActionResult> ClearanceDropdown()
        {
            return Ok(await _context.SecurityClearances
                .Select(x => new {
                    value = x.ClearanceCode,
                    label = x.Description
                }).ToListAsync());
        }
    }
}
