using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-employee-labor-info")]
    [ApiController]
    public class VendorEmployeeLaborInfoController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorEmployeeLaborInfoController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (filter + pagination)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? companyId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorEmployeeLaborInfos.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.EffectStartDt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendEmplId}/{vendId}/{effectStartDt}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendEmplId,
            string vendId,
            DateOnly effectStartDt,
            string companyId)
        {
            var entity = await _context.VendorEmployeeLaborInfos.FindAsync(
                vendEmplId, vendId, effectStartDt, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorEmployeeLaborInfo dto)
        {
            if (dto.EffectEndDt.HasValue && dto.EffectEndDt < dto.EffectStartDt)
                return BadRequest("End date cannot be before start date");

            // ❌ STRICT OVERLAP CHECK
            var hasOverlap = await _context.VendorEmployeeLaborInfos.AnyAsync(x =>
                x.VendEmplId == dto.VendEmplId &&
                x.VendId == dto.VendId &&
                x.CompanyId == dto.CompanyId &&
                x.EffectStartDt <= (dto.EffectEndDt ?? DateOnly.MaxValue) &&
                (x.EffectEndDt ?? DateOnly.MaxValue) >= dto.EffectStartDt
            );

            if (hasOverlap)
            {
                // 👉 Instead of rejecting, we FIX it (enterprise behavior)
                await AutoClosePrevious(dto);
            }

            dto.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorEmployeeLaborInfos.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [NonAction]
        private async Task<bool> HasOverlap(VendorEmployeeLaborInfo dto)
        {
            return await _context.VendorEmployeeLaborInfos.AnyAsync(x =>
                x.VendEmplId == dto.VendEmplId &&
                x.VendId == dto.VendId &&
                x.CompanyId == dto.CompanyId &&

                // overlap condition
                x.EffectStartDt <= (dto.EffectEndDt ?? DateOnly.MaxValue) &&
                (x.EffectEndDt ?? DateOnly.MaxValue) >= dto.EffectStartDt
            );
        }
        [NonAction]
        private async Task AutoClosePrevious(VendorEmployeeLaborInfo dto)
        {
            var previous = await _context.VendorEmployeeLaborInfos
                .Where(x =>
                    x.VendEmplId == dto.VendEmplId &&
                    x.VendId == dto.VendId &&
                    x.CompanyId == dto.CompanyId &&
                    x.EffectStartDt < dto.EffectStartDt
                )
                .OrderByDescending(x => x.EffectStartDt)
                .FirstOrDefaultAsync();

            if (previous != null)
            {
                // Close previous record ONE DAY before new start
                previous.EffectEndDt = dto.EffectStartDt.AddDays(-1);
            }
        }

        // ✅ UPDATE
        [HttpPut("{vendEmplId}/{vendId}/{effectStartDt}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendEmplId,
            string vendId,
            DateOnly effectStartDt,
            string companyId,
            VendorEmployeeLaborInfo dto)
        {
            var entity = await _context.VendorEmployeeLaborInfos.FindAsync(
                vendEmplId, vendId, effectStartDt, companyId);

            if (entity == null)
                return NotFound();

            if (dto.EffectEndDt.HasValue && dto.EffectEndDt < dto.EffectStartDt)
                return BadRequest("Invalid end date");

            // ❌ Prevent overlap with OTHER records
            var overlap = await _context.VendorEmployeeLaborInfos.AnyAsync(x =>
                x.VendEmplId == dto.VendEmplId &&
                x.VendId == dto.VendId &&
                x.CompanyId == dto.CompanyId &&
                x.EffectStartDt != effectStartDt &&

                x.EffectStartDt <= (dto.EffectEndDt ?? DateOnly.MaxValue) &&
                (x.EffectEndDt ?? DateOnly.MaxValue) >= dto.EffectStartDt
            );

            if (overlap)
                return BadRequest("Update causes overlapping records");

            _context.Entry(entity).CurrentValues.SetValues(dto);
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("ex_no_overlap_vendor_labor") == true)
                {
                    return BadRequest("Overlapping date range is not allowed.");
                }

                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while saving changes.");
            }

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendEmplId}/{vendId}/{effectStartDt}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            DateOnly effectStartDt,
            string companyId)
        {
            var entity = await _context.VendorEmployeeLaborInfos.FindAsync(
                vendEmplId, vendId, effectStartDt, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorEmployeeLaborInfos.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorEmployeeLaborInfo> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendEmplId, x.VendId, x.EffectStartDt, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var companyIds = input.Select(x => x.CompanyId).Distinct().ToList();

                var existing = await _context.VendorEmployeeLaborInfos
                    .Where(x => companyIds.Contains(x.CompanyId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendEmplId}|{x.VendId}|{x.EffectStartDt}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendEmplId}|{item.VendId}|{item.EffectStartDt}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        _context.Entry(db).CurrentValues.SetValues(item);
                        db.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        await _context.VendorEmployeeLaborInfos.AddAsync(item);
                        inserted++;
                    }
                }

                // DELETE
                var keySet = input
                    .Select(x => $"{x.VendEmplId}|{x.VendId}|{x.EffectStartDt}|{x.CompanyId}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains(
                        $"{x.VendEmplId}|{x.VendId}|{x.EffectStartDt}|{x.CompanyId}"))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorEmployeeLaborInfos.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new { inserted, updated, deleted = toDelete.Count });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
    }
}
