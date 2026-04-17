using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorCertificationSetupController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCertificationSetupController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? certCode,
            string? showLookupFl,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.VendorCertificationSetups.AsQueryable();

            if (!string.IsNullOrEmpty(certCode))
                query = query.Where(x => x.CertCode.Contains(certCode));

            if (!string.IsNullOrEmpty(showLookupFl))
                query = query.Where(x => x.ShowLookupFl == showLookupFl);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CertCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
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
        [HttpGet("{certCode}")]
        public async Task<IActionResult> GetById(string certCode)
        {
            var entity = await _context.VendorCertificationSetups.FindAsync(certCode);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorCertificationSetup model)
        {
            model.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorCertificationSetups.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(VendorCertificationSetup model)
        {

            var existing = await _context.VendorCertificationSetups
                .FindAsync(model.CertCode);

            if (existing == null)
                return NotFound();

            existing.CertName = model.CertName;
            existing.ShowLookupFl = model.ShowLookupFl;
            existing.PrimeAgencyId = model.PrimeAgencyId;
            existing.ProfOrgId = model.ProfOrgId;

            existing.ModifiedBy = model.ModifiedBy;
            existing.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ✅ DELETE
        [HttpDelete("{certCode}")]
        public async Task<IActionResult> Delete(string certCode)
        {
            var entity = await _context.VendorCertificationSetups
                .FindAsync(certCode);

            if (entity == null)
                return NotFound();

            // ✅ CHECK USAGE
            var isUsed = await _context.VendorCertifications
                .AnyAsync(x => x.CertCd == certCode);

            if (isUsed)
                return BadRequest($"Cannot delete. Certification '{certCode}' is already in use.");

            _context.VendorCertificationSetups.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(List<VendorCertificationSetup> input)
        {
            if (input.Any(x => x.ShowLookupFl != "Y" && x.ShowLookupFl != "N"))
                return BadRequest("ShowLookupFl must be 'Y' or 'N'");

            if (input == null || !input.Any())
                return BadRequest("Input cannot be empty");

            // ✅ Remove duplicates
            input = input
                .GroupBy(x => x.CertCode)
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var certCodes = input.Select(x => x.CertCode).ToList();

                var existing = await _context.VendorCertificationSetups
                    .Where(x => certCodes.Contains(x.CertCode))
                    .ToListAsync();

                var dict = existing.ToDictionary(x => x.CertCode);

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    if (dict.TryGetValue(item.CertCode, out var db))
                    {
                        // ✅ UPDATE
                        db.CertName = item.CertName;
                        db.ShowLookupFl = item.ShowLookupFl;
                        db.PrimeAgencyId = item.PrimeAgencyId;
                        db.ProfOrgId = item.ProfOrgId;
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = today;

                        updated++;
                    }
                    else
                    {
                        // ✅ INSERT
                        item.TimeStamp = today;
                        await _context.VendorCertificationSetups.AddAsync(item);

                        inserted++;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    message = "Bulk upsert successful",
                    inserted,
                    updated
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorCertificationSetup> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Input cannot be empty");

            if (input.Any(x => x.ShowLookupFl != "Y" && x.ShowLookupFl != "N"))
                return BadRequest("ShowLookupFl must be 'Y' or 'N'");

            input = input
                .GroupBy(x => x.CertCode)
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var existing = await _context.VendorCertificationSetups.ToListAsync();

                var dict = existing.ToDictionary(x => x.CertCode);

                var keySet = input.Select(x => x.CertCode).ToHashSet();

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    if (dict.TryGetValue(item.CertCode, out var db))
                    {
                        db.CertName = item.CertName;
                        db.ShowLookupFl = item.ShowLookupFl;
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = today;
                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = today;
                        await _context.VendorCertificationSetups.AddAsync(item);
                        inserted++;
                    }
                }

                // ❌ DELETE (only if NOT USED)
                var toDelete = existing
                    .Where(x => !keySet.Contains(x.CertCode))
                    .ToList();

                foreach (var item in toDelete)
                {
                    var isUsed = await _context.VendorCertifications
                        .AnyAsync(x => x.CertCd == item.CertCode);

                    if (!isUsed)
                        _context.VendorCertificationSetups.Remove(item);
                }

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
