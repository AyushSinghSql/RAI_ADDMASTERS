using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;
using PlanningAPI.Repositories;

namespace PlanningAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route("api/vendor-employees")]
    [ApiController]
    public class VendorEmployeeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorEmployeeController(MydatabaseContext context)
        {
            _context = context;
        }

        // =========================================================
        // ✅ GET ALL (Pagination + Sorting + Filtering)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? vendId = null,
            [FromQuery] string? companyId = null,
            [FromQuery] string sortBy = "vend_empl_id",
            [FromQuery] string sortOrder = "asc")
        {
            var query = _context.VendorEmployees.AsNoTracking().AsQueryable();

            // 🔹 Filtering
            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            // 🔹 Sorting
            query = (sortBy.ToLower(), sortOrder.ToLower()) switch
            {
                ("vend_empl_name", "desc") => query.OrderByDescending(x => x.VendEmplName),
                ("vend_empl_name", _) => query.OrderBy(x => x.VendEmplName),
                ("last_name", "desc") => query.OrderByDescending(x => x.LastName),
                ("last_name", _) => query.OrderBy(x => x.LastName),
                _ when sortOrder == "desc" => query.OrderByDescending(x => x.VendEmplId),
                _ => query.OrderBy(x => x.VendEmplId)
            };

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalRecords,
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                data
            });
        }

        // =========================================================
        // ✅ GET BY KEY
        // =========================================================
        [HttpGet("{vendEmplId}/{vendId}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendEmplId,
            string vendId,
            string companyId)
        {
            var entity = await _context.VendorEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.VendEmplId == vendEmplId &&
                    x.VendId == vendId &&
                    x.CompanyId == companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // =========================================================
        // ✅ CREATE
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Create(VendorEmployee model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            var exists = await _context.VendorEmployees.AnyAsync(x =>
                x.VendEmplId == model.VendEmplId &&
                x.VendId == model.VendId &&
                x.CompanyId == model.CompanyId);

            if (exists)
                return Conflict("Employee already exists");

            model.TimeStamp = DateTime.UtcNow;

            await _context.VendorEmployees.AddAsync(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get),
                new { model.VendEmplId, model.VendId, model.CompanyId },
                model);
        }

        // =========================================================
        // ✅ UPDATE
        // =========================================================
        [HttpPut("{vendEmplId}/{vendId}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendEmplId,
            string vendId,
            string companyId,
            VendorEmployee model)
        {
            if (model == null)
                return BadRequest();

            var existing = await _context.VendorEmployees.FirstOrDefaultAsync(x =>
                x.VendEmplId == vendEmplId &&
                x.VendId == vendId &&
                x.CompanyId == companyId);

            if (existing == null)
                return NotFound();

            // 🔹 Safe update
            _context.Entry(existing).CurrentValues.SetValues(model);
            existing.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // =========================================================
        // ✅ DELETE
        // =========================================================
        [HttpDelete("{vendEmplId}/{vendId}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            string companyId)
        {
            var entity = await _context.VendorEmployees.FirstOrDefaultAsync(x =>
                x.VendEmplId == vendEmplId &&
                x.VendId == vendId &&
                x.CompanyId == companyId);

            if (entity == null)
                return NotFound();

            _context.VendorEmployees.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================================================
        // ✅ BULK UPSERT (IMPORTANT)
        // =========================================================
        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(List<VendorEmployee> items)
        {
            if (items == null || !items.Any())
                return BadRequest("No data");

            foreach (var item in items)
            {
                var existing = await _context.VendorEmployees
                    .FirstOrDefaultAsync(x =>
                        x.VendEmplId == item.VendEmplId &&
                        x.VendId == item.VendId &&
                        x.CompanyId == item.CompanyId);

                if (existing == null)
                {
                    item.TimeStamp = DateTime.UtcNow;
                    await _context.VendorEmployees.AddAsync(item);
                }
                else
                {
                    _context.Entry(existing).CurrentValues.SetValues(item);
                    existing.TimeStamp = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Bulk upsert completed");
        }
    }
}
