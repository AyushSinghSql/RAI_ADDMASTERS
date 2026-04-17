using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-certifications")]
    [ApiController]
    public class VendorCertificationController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCertificationController(MydatabaseContext context)
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
            var query = _context.VendorCertifications.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CertStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        [HttpGet("GetCertificationsByVendor/{VendID}/{companyId}")]
        public async Task<IActionResult> GetCertificationsByVendor(
        string VendID,
        string companyId)
        {
            var entity = await _context.VendorCertifications.Where(x => x.VendId == VendID && x.CompanyId == companyId).ToListAsync();

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ GET BY KEY
        [HttpGet("{certCd}/{certSeqNo}/{companyId}/{certStartDate}")]
        public async Task<IActionResult> Get(
            string certCd,
            decimal certSeqNo,
            string companyId,
            DateOnly certStartDate)
        {
            var entity = await _context.VendorCertifications.FindAsync(
                certCd, certSeqNo, companyId, certStartDate);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorCertification dto)
        {
            // 🔴 Validation
            if (dto.CertEndDate.HasValue && dto.CertEndDate < dto.CertStartDate)
                return BadRequest("End date cannot be before start date");

            dto.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorCertifications.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{certCd}/{certSeqNo}/{companyId}/{certStartDate}")]
        public async Task<IActionResult> Update(
            string certCd,
            decimal certSeqNo,
            string companyId,
            DateOnly certStartDate,
            VendorCertification dto)
        {
            var entity = await _context.VendorCertifications.FindAsync(
                certCd, certSeqNo, companyId, certStartDate);

            if (entity == null)
                return NotFound();

            if (dto.CertEndDate.HasValue && dto.CertEndDate < dto.CertStartDate)
                return BadRequest("End date invalid");

            _context.Entry(entity).CurrentValues.SetValues(dto);
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{certCd}/{certSeqNo}/{companyId}/{certStartDate}")]
        public async Task<IActionResult> Delete(
            string certCd,
            decimal certSeqNo,
            string companyId,
            DateOnly certStartDate)
        {
            var entity = await _context.VendorCertifications.FindAsync(
                certCd, certSeqNo, companyId, certStartDate);

            if (entity == null)
                return NotFound();

            _context.VendorCertifications.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorCertification> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.CertCd, x.CertSeqNo, x.CompanyId, x.CertStartDate })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var companyIds = input.Select(x => x.CompanyId).Distinct().ToList();

                var existing = await _context.VendorCertifications
                    .Where(x => companyIds.Contains(x.CompanyId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.CertCd}|{x.CertSeqNo}|{x.CompanyId}|{x.CertStartDate}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.CertCd}|{item.CertSeqNo}|{item.CompanyId}|{item.CertStartDate}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        db.CertStatusCd = item.CertStatusCd;
                        db.CertLevelCd = item.CertLevelCd;
                        db.CertEndDate = item.CertEndDate;
                        db.CertUrl = item.CertUrl;
                        db.CertNotes = item.CertNotes;
                        db.AddrDc = item.AddrDc;
                        db.VendId = item.VendId;
                        db.VendProspectId = item.VendProspectId;
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        await _context.VendorCertifications.AddAsync(item);
                        inserted++;
                    }
                }

                // ❌ DELETE (sync)
                var keySet = input
                    .Select(x => $"{x.CertCd}|{x.CertSeqNo}|{x.CompanyId}|{x.CertStartDate}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains(
                        $"{x.CertCd}|{x.CertSeqNo}|{x.CompanyId}|{x.CertStartDate}"))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorCertifications.RemoveRange(toDelete);

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
