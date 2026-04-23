using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScisapClearancesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ScisapClearancesController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(ScisapClearance dto)
        {

            // Required
            if (string.IsNullOrWhiteSpace(dto.ClearanceCode) ||
                string.IsNullOrWhiteSpace(dto.ClearanceDescription))
                return BadRequest("Code & Description required");

            // Duplicate
            var exists = await _context.ScisapClearances
                .AnyAsync(x => x.ClearanceCode == dto.ClearanceCode);

            if (exists)
                return BadRequest("Duplicate Clearance Code");

            if (await _context.ScisapClearances
                .AnyAsync(x => x.ClearanceCode == dto.ClearanceCode))
                return BadRequest("Duplicate");

            _context.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.ScisapClearances.ToListAsync());
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetById(string code)
        {
            var data = await _context.ScisapClearances.FindAsync(code);
            if (data == null) return NotFound();

            return Ok(data);
        }
        [HttpPut]
        public async Task<IActionResult> Update(ScisapClearance dto)
        {
            var entity = await _context.ScisapClearances
                .FindAsync(dto.ClearanceCode);

            if (entity == null)
                return NotFound();

            entity.ClearanceDescription = dto.ClearanceDescription;
            entity.ModifiedBy = dto.ModifiedBy;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var entity = await _context.ScisapClearances.FindAsync(code);
            if (entity == null) return NotFound();

            // Example: prevent delete if used in security clearance
            var isUsed = await _context.SecurityClearances
                .AnyAsync(x => x.SciFlag == code || x.SapFlag == code);

            if (isUsed)
                return BadRequest("Clearance is in use");

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.ScisapClearances
                .Select(x => new {
                    value = x.ClearanceCode,
                    label = x.ClearanceDescription
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
