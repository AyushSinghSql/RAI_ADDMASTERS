using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeePropertyController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeePropertyController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? emplId,
            string? propId,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.EmployeeProperties.AsQueryable();

            if (!string.IsNullOrEmpty(emplId))
                query = query.Where(x => x.EmplId == emplId);

            if (!string.IsNullOrEmpty(propId))
                query = query.Where(x => x.PropId == propId);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.IssueDt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET SINGLE
        [HttpGet("{emplId}/{propId}/{issueDt}")]
        public async Task<IActionResult> Get(string emplId, string propId, DateOnly issueDt)
        {
            var data = await _context.EmployeeProperties
                .FindAsync(emplId, propId, issueDt);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeProperty model)
        {
            var exists = await _context.EmployeeProperties
                .AnyAsync(x =>
                    x.EmplId == model.EmplId &&
                    x.PropId == model.PropId &&
                    x.IssueDt == model.IssueDt);

            if (exists)
                return BadRequest("Property already assigned for this date");

            model.TimeStamp = DateTime.UtcNow;

            _context.EmployeeProperties.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{emplId}/{propId}/{issueDt}")]
        public async Task<IActionResult> Update(
            string emplId,
            string propId,
            DateOnly issueDt,
            EmployeeProperty model)
        {
            var db = await _context.EmployeeProperties
                .FindAsync(emplId, propId, issueDt);

            if (db == null)
                return NotFound();

            db.PropAmt = model.PropAmt;
            db.WhseName = model.WhseName;
            db.ControlId = model.ControlId;
            db.ReturnDt = model.ReturnDt;
            db.OtherS = model.OtherS;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE
        [HttpDelete("{emplId}/{propId}/{issueDt}")]
        public async Task<IActionResult> Delete(
            string emplId,
            string propId,
            DateOnly issueDt)
        {
            var db = await _context.EmployeeProperties
                .FindAsync(emplId, propId, issueDt);

            if (db == null)
                return NotFound();

            _context.EmployeeProperties.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ RETURN PROPERTY (Business Logic)
        [HttpPost("return")]
        public async Task<IActionResult> ReturnProperty(
            string emplId,
            string propId,
            DateOnly issueDt)
        {
            var db = await _context.EmployeeProperties
                .FindAsync(emplId, propId, issueDt);

            if (db == null)
                return NotFound();

            if (db.ReturnDt != null)
                return BadRequest("Property already returned");

            db.ReturnDt = DateOnly.FromDateTime(DateTime.UtcNow);
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Property returned successfully");
        }
    }
}
