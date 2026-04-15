using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorCeilingController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCeilingController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(VendorCeilingDto dto)
        {
            if (dto == null)
                return BadRequest();

            var exists = await _context.VendorCeilings.AnyAsync(x =>
                x.ProjectId == dto.ProjectId &&
                x.BillingLaborCategoryCode == dto.BillingLaborCategoryCode &&
                x.VendId == dto.VendId);

            if (exists)
                return Conflict("Record already exists");

            var entity = new VendorCeiling
            {
                ProjectId = dto.ProjectId,
                BillingLaborCategoryCode = dto.BillingLaborCategoryCode,
                VendId = dto.VendId,
                CeilingHours = dto.CeilingHours,
                CompanyId = dto.CompanyId,
                ModifiedBy = dto.ModifiedBy,
                ModifiedTs = DateTime.UtcNow
            };

            _context.VendorCeilings.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorCeilings
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("{projectId}/{labCat}/{vendId}")]
        public async Task<IActionResult> Get(string projectId, string labCat, string vendId)
        {
            var entity = await _context.VendorCeilings
                .FindAsync(projectId, labCat, vendId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut("{projectId}/{labCat}/{vendId}")]
        public async Task<IActionResult> Update(string projectId, string labCat, string vendId, VendorCeilingDto dto)
        {
            var entity = await _context.VendorCeilings
                .FindAsync(projectId, labCat, vendId);

            if (entity == null)
                return NotFound();

            entity.CeilingHours = dto.CeilingHours;
            entity.CompanyId = dto.CompanyId;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{projectId}/{labCat}/{vendId}")]
        public async Task<IActionResult> Delete(string projectId, string labCat, string vendId)
        {
            var entity = await _context.VendorCeilings
                .FindAsync(projectId, labCat, vendId);

            if (entity == null)
                return NotFound();

            _context.VendorCeilings.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorCeilingQuery query)
        {
            var q = _context.VendorCeilings.AsQueryable();

            // FILTER
            if (!string.IsNullOrWhiteSpace(query.ProjectId))
                q = q.Where(x => x.ProjectId == query.ProjectId);

            if (!string.IsNullOrWhiteSpace(query.VendId))
                q = q.Where(x => x.VendId == query.VendId);

            if (!string.IsNullOrWhiteSpace(query.CompanyId))
                q = q.Where(x => x.CompanyId == query.CompanyId);

            var total = await q.CountAsync();

            // SORT
            q = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
            {
                ("project_id", "desc") => q.OrderByDescending(x => x.ProjectId),
                ("ceiling_hours", "desc") => q.OrderByDescending(x => x.CeilingHours),
                _ => q.OrderBy(x => x.ProjectId)
            };

            // PAGINATION
            var data = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                total,
                query.Page,
                query.PageSize,
                data
            });
        }
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorCeilingDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            input = input
                .GroupBy(x => new { x.ProjectId, x.BillingLaborCategoryCode, x.VendId })
                .Select(g => g.First())
                .ToList();

            var existing = await _context.VendorCeilings.ToListAsync();

            var dict = existing.ToDictionary(x =>
                $"{x.ProjectId}|{x.BillingLaborCategoryCode}|{x.VendId}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.ProjectId}|{dto.BillingLaborCategoryCode}|{dto.VendId}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.CeilingHours = dto.CeilingHours;
                    db.CompanyId = dto.CompanyId;
                    db.ModifiedBy = dto.ModifiedBy;
                    db.ModifiedTs = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorCeilings.Add(new VendorCeiling
                    {
                        ProjectId = dto.ProjectId,
                        BillingLaborCategoryCode = dto.BillingLaborCategoryCode,
                        VendId = dto.VendId,
                        CeilingHours = dto.CeilingHours,
                        CompanyId = dto.CompanyId,
                        ModifiedBy = dto.ModifiedBy,
                        ModifiedTs = DateTime.UtcNow
                    });
                    insert++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { insert, update });
        }
    }
}
