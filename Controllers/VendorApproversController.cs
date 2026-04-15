using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorApproversController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorApproversController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(VendorApproverDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.VendId) || string.IsNullOrWhiteSpace(dto.ApproverUserId))
                return BadRequest("Invalid data");

            var exists = await _context.VendorApprovers
                .AnyAsync(x => x.VendId == dto.VendId && x.ApproverUserId == dto.ApproverUserId);

            if (exists)
                return Conflict("Record already exists");

            var entity = new VendorApprover
            {
                VendId = dto.VendId,
                ApproverUserId = dto.ApproverUserId,
                CompanyId = dto.CompanyId,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.VendorApprovers.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorApprovers
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{vendId}/{approverUserId}")]
        public async Task<IActionResult> Get(string vendId, string approverUserId)
        {
            var entity = await _context.VendorApprovers
                .FindAsync(vendId, approverUserId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }
        [HttpPut("{vendId}/{approverUserId}")]
        public async Task<IActionResult> Update(string vendId, string approverUserId, VendorApproverDto dto)
        {
            var entity = await _context.VendorApprovers
                .FindAsync(vendId, approverUserId);

            if (entity == null)
                return NotFound();

            entity.CompanyId = dto.CompanyId;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        [HttpDelete("{vendId}/{approverUserId}")]
        public async Task<IActionResult> Delete(string vendId, string approverUserId)
        {
            var entity = await _context.VendorApprovers
                .FindAsync(vendId, approverUserId);

            if (entity == null)
                return NotFound();

            _context.VendorApprovers.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorApproverQuery query)
        {
            var q = _context.VendorApprovers.AsQueryable();

            // FILTER
            if (!string.IsNullOrWhiteSpace(query.VendId))
                q = q.Where(x => x.VendId == query.VendId);

            if (!string.IsNullOrWhiteSpace(query.ApproverUserId))
                q = q.Where(x => x.ApproverUserId == query.ApproverUserId);

            if (!string.IsNullOrWhiteSpace(query.CompanyId))
                q = q.Where(x => x.CompanyId == query.CompanyId);

            var total = await q.CountAsync();

            // SORT
            q = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
            {
                ("vend_id", "desc") => q.OrderByDescending(x => x.VendId),
                ("approver_user_id", "desc") => q.OrderByDescending(x => x.ApproverUserId),
                _ => q.OrderBy(x => x.VendId)
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
        public async Task<IActionResult> Sync(List<VendorApproverDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            input = input
                .GroupBy(x => new { x.VendId, x.ApproverUserId })
                .Select(g => g.First())
                .ToList();

            var existing = await _context.VendorApprovers.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.VendId}|{x.ApproverUserId}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.VendId}|{dto.ApproverUserId}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.CompanyId = dto.CompanyId;
                    db.ModifiedBy = dto.ModifiedBy;
                    db.TimeStamp = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorApprovers.Add(new VendorApprover
                    {
                        VendId = dto.VendId,
                        ApproverUserId = dto.ApproverUserId,
                        CompanyId = dto.CompanyId,
                        ModifiedBy = dto.ModifiedBy,
                        TimeStamp = DateTime.UtcNow
                    });
                    insert++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { insert, update });
        }
    }
}
