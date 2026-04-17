using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-employee-skills")]
    [ApiController]
    public class VendorEmployeeSkillController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorEmployeeSkillController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (with filtering + pagination)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? skillId,
            string? companyId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorEmployeeSkills.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(skillId))
                query = query.Where(x => x.SkillId == skillId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.VendEmplId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendEmplId}/{vendId}/{skillId}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendEmplId,
            string vendId,
            string skillId,
            string companyId)
        {
            var entity = await _context.VendorEmployeeSkills.FindAsync(
                vendEmplId, vendId, skillId, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorEmployeeSkill dto)
        {
            if (dto.ExpiryDt < dto.CompleteDt)
                return BadRequest("Expiry date cannot be before completion date");

            var status = dto.ExpiryDt < DateOnly.FromDateTime(DateTime.UtcNow)
                ? "EXPIRED"
                : "ACTIVE";

            dto.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorEmployeeSkills.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{vendEmplId}/{vendId}/{skillId}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendEmplId,
            string vendId,
            string skillId,
            string companyId,
            VendorEmployeeSkill dto)
        {
            var entity = await _context.VendorEmployeeSkills.FindAsync(
                vendEmplId, vendId, skillId, companyId);

            if (entity == null)
                return NotFound();

            if (dto.ExpiryDt < dto.CompleteDt)
                return BadRequest("Expiry date cannot be before completion date");

            var status = dto.ExpiryDt < DateOnly.FromDateTime(DateTime.UtcNow)
                ? "EXPIRED"
                : "ACTIVE";

            _context.Entry(entity).CurrentValues.SetValues(dto);
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendEmplId}/{vendId}/{skillId}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            string skillId,
            string companyId)
        {
            var entity = await _context.VendorEmployeeSkills.FindAsync(
                vendEmplId, vendId, skillId, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorEmployeeSkills.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC STYLE)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorEmployeeSkill> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendEmplId, x.VendId, x.SkillId, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.VendorEmployeeSkills.ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendEmplId}|{x.VendId}|{x.SkillId}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendEmplId}|{item.VendId}|{item.SkillId}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        _context.Entry(db).CurrentValues.SetValues(item);
                        db.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        await _context.VendorEmployeeSkills.AddAsync(item);
                        inserted++;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new { inserted, updated });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive(string vendId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var data = await _context.VendorEmployeeSkills
                .Where(x => x.VendId == vendId &&
                       (x.ExpiryDt == null || x.ExpiryDt >= today))
                .ToListAsync();

            return Ok(data);
        }
    }
}
