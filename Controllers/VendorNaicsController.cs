using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-naics")]
    [ApiController]
    public class VendorNaicsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorNaicsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (filter + pagination)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? naicsCode,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorNaics.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(naicsCode))
                query = query.Where(x => x.OppNaicsCode == naicsCode);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.OppNaicsCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendId}/{code}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendId,
            string code,
            string companyId)
        {
            var entity = await _context.VendorNaics
                .FindAsync(vendId, code, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorNaics dto)
        {
            // 🔴 Validate flags
            if (!IsValidFlag(dto.PrimeNaicFlag) ||
                !IsValidFlag(dto.NaicsSmallBusinessFlag) ||
                !IsValidFlag(dto.NaicsLargeBusinessFlag))
            {
                return BadRequest("Flags must be Y or N");
            }

            dto.TimeStamp = DateTime.UtcNow;

            await _context.VendorNaics.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{vendId}/{code}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendId,
            string code,
            string companyId,
            VendorNaics dto)
        {
            var entity = await _context.VendorNaics
                .FindAsync(vendId, code, companyId);

            if (entity == null)
                return NotFound();

            entity.OppNaicsDescription = dto.OppNaicsDescription;
            entity.PrimeNaicFlag = dto.PrimeNaicFlag;
            entity.NaicsSmallBusinessFlag = dto.NaicsSmallBusinessFlag;
            entity.NaicsLargeBusinessFlag = dto.NaicsLargeBusinessFlag;
            entity.NaicsCertAgency = dto.NaicsCertAgency;
            entity.NaicsNotes = dto.NaicsNotes;
            entity.EffectiveDate = dto.EffectiveDate;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendId}/{code}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendId,
            string code,
            string companyId)
        {
            var entity = await _context.VendorNaics
                .FindAsync(vendId, code, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorNaics.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorNaics> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendId, x.OppNaicsCode, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var vendIds = input.Select(x => x.VendId).Distinct().ToList();

                var existing = await _context.VendorNaics
                    .Where(x => vendIds.Contains(x.VendId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendId}|{x.OppNaicsCode}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendId}|{item.OppNaicsCode}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        db.OppNaicsDescription = item.OppNaicsDescription;
                        db.PrimeNaicFlag = item.PrimeNaicFlag;
                        db.NaicsSmallBusinessFlag = item.NaicsSmallBusinessFlag;
                        db.NaicsLargeBusinessFlag = item.NaicsLargeBusinessFlag;
                        db.NaicsCertAgency = item.NaicsCertAgency;
                        db.NaicsNotes = item.NaicsNotes;
                        db.EffectiveDate = item.EffectiveDate;
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = DateTime.UtcNow;

                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateTime.UtcNow;
                        await _context.VendorNaics.AddAsync(item);
                        inserted++;
                    }
                }

                // ❌ DELETE (sync)
                var keySet = input
                    .Select(x => $"{x.VendId}|{x.OppNaicsCode}|{x.CompanyId}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.VendId}|{x.OppNaicsCode}|{x.CompanyId}"))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorNaics.RemoveRange(toDelete);

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

        // 🔧 Helper
        private bool IsValidFlag(string? value)
        {
            return string.IsNullOrEmpty(value) || value == "Y" || value == "N";
        }
    }
}
