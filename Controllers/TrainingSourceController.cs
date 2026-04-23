using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingSourceController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public TrainingSourceController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            string? type,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.TrainingSources.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.TrainSrceId.Contains(search) ||
                    x.TrainSrceDesc.Contains(search));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(x => x.SIntExtCd == type);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.TrainSrceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.TrainingSources.FindAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(TrainingSrce model)
        {
            if (await _context.TrainingSources.AnyAsync(x => x.TrainSrceId == model.TrainSrceId))
                return BadRequest("Training Source already exists");

            // Validate I/E
            model.SIntExtCd = model.SIntExtCd?.ToUpper() == "E" ? "E" : "I";

            model.TimeStamp = DateTime.UtcNow;

            _context.TrainingSources.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TrainingSrce model)
        {
            var db = await _context.TrainingSources.FindAsync(id);
            if (db == null)
                return NotFound();

            db.TrainSrceDesc = model.TrainSrceDesc;
            db.SIntExtCd = model.SIntExtCd?.ToUpper() == "E" ? "E" : "I";
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (Protected)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var db = await _context.TrainingSources.FindAsync(id);
            if (db == null)
                return NotFound();

            // 🔥 VALIDATION: prevent delete if used in SUBC_TRAININGS
            var isUsed = await _context.VendorEmployeeTrainings
                .AnyAsync(x => x.TrainSrceId == id);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete. Training Source is used in subcontractor trainings."
                });
            }

            _context.TrainingSources.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.TrainingSources
                .Select(x => new
                {
                    value = x.TrainSrceId,
                    label = x.TrainSrceDesc,
                    type = x.SIntExtCd
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
