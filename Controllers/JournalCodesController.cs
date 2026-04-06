using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/journal-codes")]
    public class JournalCodesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public JournalCodesController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _context.JournalCodes.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(JournalCodeDto dto)
        {
            var entity = new JournalCode
            {
                JournalCodeId = dto.JournalCode,
                JournalDesc = dto.JournalDesc,
                IsActive = dto.IsActive,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.JournalCodes.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, JournalCodeDto dto)
        {
            var entity = await _context.JournalCodes.FindAsync(code);
            if (entity == null) return NotFound();

            entity.JournalDesc = dto.JournalDesc;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var entity = await _context.JournalCodes.FindAsync(code);
            if (entity == null) return NotFound();

            _context.JournalCodes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
