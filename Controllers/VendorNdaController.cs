using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-nda")]
    [ApiController]
    public class VendorNdaController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorNdaController(MydatabaseContext context)
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
            var query = _context.VendorNdas.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.NdaDateReceived)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendId}/{ndaKey}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendId,
            decimal ndaKey,
            string companyId)
        {
            var entity = await _context.VendorNdas
                .FindAsync(vendId, ndaKey, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorNda dto)
        {
            // 🔴 Validate expiry
            if (dto.NdaExpiryDate.HasValue &&
                dto.NdaDateReceived.HasValue &&
                dto.NdaExpiryDate < dto.NdaDateReceived)
            {
                return BadRequest("Expiry date cannot be before received date");
            }

            dto.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorNdas.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{vendId}/{ndaKey}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendId,
            decimal ndaKey,
            string companyId,
            VendorNda dto)
        {
            var entity = await _context.VendorNdas
                .FindAsync(vendId, ndaKey, companyId);

            if (entity == null)
                return NotFound();

            // 🔴 Validate expiry
            if (dto.NdaExpiryDate.HasValue &&
                dto.NdaDateReceived.HasValue &&
                dto.NdaExpiryDate < dto.NdaDateReceived)
            {
                return BadRequest("Expiry date cannot be before received date");
            }

            entity.NdaDateReceived = dto.NdaDateReceived;
            entity.NdaExpiryDate = dto.NdaExpiryDate;
            entity.NdaDetail = dto.NdaDetail;
            entity.FileLocation = dto.FileLocation;
            entity.FileName = dto.FileName;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendId}/{ndaKey}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendId,
            decimal ndaKey,
            string companyId)
        {
            var entity = await _context.VendorNdas
                .FindAsync(vendId, ndaKey, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorNdas.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorNda> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendId, x.NdaKey, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var vendIds = input.Select(x => x.VendId).Distinct().ToList();

                var existing = await _context.VendorNdas
                    .Where(x => vendIds.Contains(x.VendId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendId}|{x.NdaKey}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendId}|{item.NdaKey}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        db.NdaDateReceived = item.NdaDateReceived;
                        db.NdaExpiryDate = item.NdaExpiryDate;
                        db.NdaDetail = item.NdaDetail;
                        db.FileLocation = item.FileLocation;
                        db.FileName = item.FileName;
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        await _context.VendorNdas.AddAsync(item);
                        inserted++;
                    }
                }

                // ❌ DELETE (sync behavior)
                var keySet = input
                    .Select(x => $"{x.VendId}|{x.NdaKey}|{x.CompanyId}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.VendId}|{x.NdaKey}|{x.CompanyId}"))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorNdas.RemoveRange(toDelete);

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

        [HttpGet("compliance-summary")]
        public async Task<IActionResult> Summary()
        {
            var total = await _context.Vendors.CountAsync();

            var compliant = await _context.VendorNdas
                .Where(x => x.NdaExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .Select(x => x.VendId)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                total,
                compliant,
                percentage = total == 0 ? 0 : (compliant * 100 / total)
            });
        }

        [HttpGet("expired-nda")]
        public async Task<IActionResult> GetExpiredNda()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var data = await _context.VendorNdas
                .Where(x => x.NdaExpiryDate < today)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("vendor-compliance")]
        public async Task<IActionResult> GetVendorCompliance(string? vendId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var vendors = _context.Vendors.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                vendors = vendors.Where(v => v.VendId == vendId);

            var nda = await _context.VendorNdas
                .Where(x => x.NdaExpiryDate >= today)
                .Select(x => x.VendId)
                .Distinct()
                .ToListAsync();

            var vat = await _context.VendorVatInfos
                .Select(x => x.VendId)
                .Distinct()
                .ToListAsync();

            var cis = await _context.VendorCisInformations
                .Select(x => x.VendId)
                .Distinct()
                .ToListAsync();

            var v1099 = await _context.Vendor1099Details
                .Select(x => x.PayVendorId)
                .Distinct()
                .ToListAsync();

            var result = vendors.Select(v => new VendorComplianceDto
            {
                VendId = v.VendId,
                HasActiveNda = nda.Contains(v.VendId),
                HasVat = vat.Contains(v.VendId),
                HasCis = cis.Contains(v.VendId),
                Has1099 = v1099.Contains(v.VendId),
                ComplianceScore =
                    (nda.Contains(v.VendId) ? 25 : 0) +
                    (vat.Contains(v.VendId) ? 25 : 0) +
                    (cis.Contains(v.VendId) ? 25 : 0) +
                    (v1099.Contains(v.VendId) ? 25 : 0)
            }).ToList();

            return Ok(result);
        }

    }
}
