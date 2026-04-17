using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-employee-labor-trn-rate")]
    [ApiController]
    public class VendorEmployeeLaborTrnRateController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorEmployeeLaborTrnRateController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? currency,
            string? companyId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorEmployeeLaborTrnRates.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(currency))
                query = query.Where(x => x.TrnCrncyCd == currency);

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
        [HttpGet("{vendEmplId}/{vendId}/{effectStartDt}/{currency}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendEmplId,
            string vendId,
            DateOnly effectStartDt,
            string currency,
            string companyId)
        {
            var entity = await _context.VendorEmployeeLaborTrnRates.FindAsync(
                vendEmplId, vendId, effectStartDt, currency, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorEmployeeLaborTrnRate dto)
        {
            dto.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            try
            {
                await _context.VendorEmployeeLaborTrnRates.AddAsync(dto);
                await _context.SaveChangesAsync();

                return Ok(dto);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("ex_no_overlap_trn_rate") == true)
                    return BadRequest("Overlapping transaction rate not allowed.");

                throw;
            }
        }

        // ✅ UPDATE
        [HttpPut("{vendEmplId}/{vendId}/{effectStartDt}/{currency}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendEmplId,
            string vendId,
            DateOnly effectStartDt,
            string currency,
            string companyId,
            VendorEmployeeLaborTrnRate dto)
        {
            var entity = await _context.VendorEmployeeLaborTrnRates.FindAsync(
                vendEmplId, vendId, effectStartDt, currency, companyId);

            if (entity == null)
                return NotFound();

            _context.Entry(entity).CurrentValues.SetValues(dto);
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(entity);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("ex_no_overlap_trn_rate") == true)
                    return BadRequest("Update causes overlapping rates.");

                throw;
            }
        }

        // ✅ DELETE
        [HttpDelete("{vendEmplId}/{vendId}/{effectStartDt}/{currency}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            DateOnly effectStartDt,
            string currency,
            string companyId)
        {
            var entity = await _context.VendorEmployeeLaborTrnRates.FindAsync(
                vendEmplId, vendId, effectStartDt, currency, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorEmployeeLaborTrnRates.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK SYNC
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorEmployeeLaborTrnRate> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendEmplId, x.VendId, x.EffectStartDt, x.TrnCrncyCd, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.VendorEmployeeLaborTrnRates.ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendEmplId}|{x.VendId}|{x.EffectStartDt}|{x.TrnCrncyCd}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendEmplId}|{item.VendId}|{item.EffectStartDt}|{item.TrnCrncyCd}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        _context.Entry(db).CurrentValues.SetValues(item);
                        db.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        await _context.VendorEmployeeLaborTrnRates.AddAsync(item);
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
    }
}
