using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public TrainingController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Trainings.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.TrainId.Contains(search) ||
                    x.TrainDesc.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.TrainId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.Trainings.FindAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(Training model)
        {
            if (await _context.Trainings.AnyAsync(x => x.TrainId == model.TrainId))
                return BadRequest("Training already exists");

            model.TimeStamp = DateTime.UtcNow;

            // Validate FL field
            model.DetlJobValidMthd = model.DetlJobValidMthd == "Y" ? "Y" : "N";

            _context.Trainings.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Training model)
        {
            var db = await _context.Trainings.FindAsync(id);
            if (db == null)
                return NotFound();

            db.TrainDesc = model.TrainDesc;
            db.TrainCeuCred = model.TrainCeuCred;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;
            db.DetlJobValidMthd = model.DetlJobValidMthd == "Y" ? "Y" : "N";

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (Protected)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var db = await _context.Trainings.FindAsync(id);
            if (db == null)
                return NotFound();

            // 🔥 VALIDATION: prevent delete if used in SUBC_TRAININGS
            var isUsed = await _context.VendorEmployeeTrainings
                .AnyAsync(x => x.TrainId == id);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete. Training is used in subcontractor trainings."
                });
            }

            _context.Trainings.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.Trainings
                .Select(x => new
                {
                    value = x.TrainId,
                    label = x.TrainDesc
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
