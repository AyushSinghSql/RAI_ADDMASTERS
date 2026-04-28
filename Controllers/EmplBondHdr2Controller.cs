using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/empl-bond")]
    [ApiController]
    public class EmplBondHdr2Controller : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmplBondHdr2Controller(MydatabaseContext context)
        {
            _context = context;
        }

        // 🔍 GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EmplBondHdr2s
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // 🔍 GET BY KEY
        [HttpGet("{emplId}/{dedCd}")]
        public async Task<IActionResult> Get(string emplId, string dedCd)
        {
            var entity = await _context.EmplBondHdr2s
                .FindAsync(emplId, dedCd);

            return entity == null ? NotFound() : Ok(entity);
        }

        // ➕ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(EmplBondHdr2 model)
        {
            // 🔥 Normalize
            model.EmplId = model.EmplId?.Trim().ToUpper();
            model.DedCd = model.DedCd?.Trim().ToUpper();

            // 🔥 Validation
            if (model.BondBegBal < 0)
                return BadRequest("Bond balance cannot be negative");

            var exists = await _context.EmplBondHdr2s
                .AnyAsync(x => x.EmplId == model.EmplId &&
                               x.DedCd == model.DedCd);

            if (exists)
                return Conflict("Record already exists");

            model.TimeStamp = DateTime.UtcNow;
            model.RowVersion = 1;

            _context.EmplBondHdr2s.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✏️ UPDATE
        [HttpPut("{emplId}/{dedCd}")]
        public async Task<IActionResult> Update(string emplId, string dedCd, EmplBondHdr2 model)
        {
            var entity = await _context.EmplBondHdr2s
                .FindAsync(emplId, dedCd);

            if (entity == null)
                return NotFound();

            // 🔒 Concurrency
            if (entity.RowVersion != model.RowVersion)
                return Conflict("Record modified by another user");

            entity.EmplBondEffDt = model.EmplBondEffDt;
            entity.BondBegBal = model.BondBegBal;
            entity.ModifiedBy = model.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.RowVersion++;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ❌ DELETE
        [HttpDelete("{emplId}/{dedCd}")]
        public async Task<IActionResult> Delete(string emplId, string dedCd)
        {
            var entity = await _context.EmplBondHdr2s
                .FindAsync(emplId, dedCd);

            if (entity == null)
                return NotFound();

            _context.EmplBondHdr2s.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
