using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/empl-ded")]
    [ApiController]
    public class EmplDedController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmplDedController(MydatabaseContext context)
        {
            _context = context;
        }

        // 🔍 GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EmplDeds
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // 🔍 GET BY KEY
        [HttpGet("{emplId}/{dedCd}")]
        public async Task<IActionResult> Get(string emplId, string dedCd)
        {
            var entity = await _context.EmplDeds
                .FindAsync(emplId, dedCd);

            return entity == null ? NotFound() : Ok(entity);
        }

        // ➕ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(EmplDed model)
        {
            // 🔥 Normalize
            model.EmplId = model.EmplId?.Trim().ToUpper();
            model.DedCd = model.DedCd?.Trim().ToUpper();
            model.SDedMthdCd = model.SDedMthdCd?.Trim().ToUpper();

            // 🔥 Business validations
            if (model.DedRtAmt < 0)
                return BadRequest("Deduction amount cannot be negative");

            if (model.DedAnnCeilAmt < 0)
                return BadRequest("Annual ceiling cannot be negative");

            if (model.DedPriorityNo <= 0)
                return BadRequest("Priority must be > 0");

            if (model.DedEndDt < model.DedStartDt)
                return BadRequest("End date cannot be before start date");

            var exists = await _context.EmplDeds
                .AnyAsync(x => x.EmplId == model.EmplId &&
                               x.DedCd == model.DedCd);

            if (exists)
                return Conflict("Record already exists");

            model.TimeStamp = DateTime.UtcNow;
            model.RowVersion = 1;

            _context.EmplDeds.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✏️ UPDATE
        [HttpPut("{emplId}/{dedCd}")]
        public async Task<IActionResult> Update(string emplId, string dedCd, EmplDed model)
        {
            var entity = await _context.EmplDeds
                .FindAsync(emplId, dedCd);

            if (entity == null)
                return NotFound();

            // 🔒 Concurrency
            if (entity.RowVersion != model.RowVersion)
                return Conflict("Record modified by another user");

            // 🔥 Update fields
            entity.SDedMthdCd = model.SDedMthdCd;
            entity.DedRtAmt = model.DedRtAmt;
            entity.DedAnnCeilAmt = model.DedAnnCeilAmt;
            entity.DedPriorityNo = model.DedPriorityNo;

            entity.DedStartDt = model.DedStartDt;
            entity.DedEndDt = model.DedEndDt;
            entity.DedEndCvgDt = model.DedEndCvgDt;
            entity.DedStartCvgDt = model.DedStartCvgDt;

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
            var entity = await _context.EmplDeds
                .FindAsync(emplId, dedCd);

            if (entity == null)
                return NotFound();

            _context.EmplDeds.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
