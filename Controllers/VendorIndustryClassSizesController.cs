using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-industry-class-sizes")]
    [ApiController]
    public class VendorIndustryClassSizesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorIndustryClassSizesController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (filter + pagination)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? indClassCd,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorIndustryClassSizes.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(indClassCd))
                query = query.Where(x => x.IndClassCd == indClassCd);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.IndClassCd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendId}/{indClassCd}")]
        public async Task<IActionResult> Get(string vendId, string indClassCd)
        {
            var entity = await _context.VendorIndustryClassSizes
                .FindAsync(vendId, indClassCd);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorIndustryClassSize dto)
        {
            // 🔴 Validate Y/N field
            if (!string.IsNullOrEmpty(dto.SmallBusinessCode) &&
                dto.SmallBusinessCode.Length > 1)
                return BadRequest("small_business_code must be Y or N");

            dto.ModifiedTs = DateTime.UtcNow;

            await _context.VendorIndustryClassSizes.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{vendId}/{indClassCd}")]
        public async Task<IActionResult> Update(
            string vendId,
            string indClassCd,
            VendorIndustryClassSize dto)
        {
            var entity = await _context.VendorIndustryClassSizes
                .FindAsync(vendId, indClassCd);

            if (entity == null)
                return NotFound();

            entity.SmallBusinessCode = dto.SmallBusinessCode;
            entity.CompanyId = dto.CompanyId;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendId}/{indClassCd}")]
        public async Task<IActionResult> Delete(string vendId, string indClassCd)
        {
            var entity = await _context.VendorIndustryClassSizes
                .FindAsync(vendId, indClassCd);

            if (entity == null)
                return NotFound();

            _context.VendorIndustryClassSizes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorIndustryClassSize> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendId, x.IndClassCd })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var vendIds = input.Select(x => x.VendId).Distinct().ToList();

                var existing = await _context.VendorIndustryClassSizes
                    .Where(x => vendIds.Contains(x.VendId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendId}|{x.IndClassCd}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendId}|{item.IndClassCd}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        db.SmallBusinessCode = item.SmallBusinessCode;
                        db.CompanyId = item.CompanyId;
                        db.ModifiedBy = item.ModifiedBy;
                        db.ModifiedTs = DateTime.UtcNow;
                        updated++;
                    }
                    else
                    {
                        item.ModifiedTs = DateTime.UtcNow;
                        await _context.VendorIndustryClassSizes.AddAsync(item);
                        inserted++;
                    }
                }

                // ❌ DELETE (sync)
                var keySet = input
                    .Select(x => $"{x.VendId}|{x.IndClassCd}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.VendId}|{x.IndClassCd}"))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorIndustryClassSizes.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    inserted,
                    updated,
                    deleted = toDelete.Count
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
    }
}
