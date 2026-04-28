using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustTermsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustTermsController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET with schedules
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var data = await _context.CustTerms
                .Include(x => x.Schedules)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustTermsKey == id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // CREATE (with schedules)
        [HttpPost]
        public async Task<IActionResult> Create(CustTerms model)
        {
            model.TimeStamp = DateTime.UtcNow;
            model.RowVersion = 1;

            foreach (var sch in model.Schedules ?? [])
            {
                if (sch.FromDayNo > sch.ToDayNo)
                    return BadRequest("Invalid range.");

                sch.TimeStamp = DateTime.UtcNow;
                sch.RowVersion = 1;
            }

            _context.CustTerms.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // UPDATE (replace schedules)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CustTerms model)
        {
            var entity = await _context.CustTerms
                .Include(x => x.Schedules)
                .FirstOrDefaultAsync(x => x.CustTermsKey == id);

            if (entity == null)
                return NotFound();

            if (entity.RowVersion != model.RowVersion)
                return Conflict("Concurrency issue");

            entity.CustTermsDc = model.CustTermsDc;
            entity.DiscPctRt = model.DiscPctRt;
            entity.ModifiedBy = model.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.RowVersion++;

            // Replace schedules (ERP style)
            _context.CustTermsSch.RemoveRange(entity.Schedules);

            foreach (var sch in model.Schedules ?? [])
            {
                sch.CustTermsKey = id;
                sch.TimeStamp = DateTime.UtcNow;
                sch.RowVersion = 1;
            }

            await _context.CustTermsSch.AddRangeAsync(model.Schedules ?? []);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.CustTerms.FindAsync(id);

            if (entity == null)
                return NotFound();

            _context.CustTerms.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
